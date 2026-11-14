using MyToDo.Common;
using MyToDo.Shared;
using MyToDo.Shared.Dtos;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using static MaterialDesignThemes.Wpf.Theme;

namespace MyToDo.Service
{
    public class HttpRestClient
    {
        private readonly string ApiUrl;
        protected readonly RestClient client;

        public Func<string, Task<(string? AccessToken, string? RefreshToken)>>? RefreshTokenAsync { get; set; }

        //防止并发刷新
        private readonly SemaphoreSlim refreshLock = new(1, 1);

        public HttpRestClient(string apiUrl)
        {
            ApiUrl = apiUrl;
            client = new RestClient();
        }

        public async Task<ApiResponse> ExecuteAsync(BaseRequest baseRequest)
        {
            //var request = BuildRequest(baseRequest);
            //var response = await client.ExecuteAsync(request);
            var response = await SendWithRetryAsync(baseRequest);

            if (!response.IsSuccessful)
            {
                return new ApiResponse
                {
                    Message = string.IsNullOrWhiteSpace(response.ErrorMessage) ? "未知错误" : response.ErrorMessage,
                    IsSuccess = false,
                    // Result = null
                };
            }

            if (string.IsNullOrWhiteSpace(response.Content))
            {
                return new ApiResponse
                {
                    Message = "执行成功，但未获取到返回内容",
                    IsSuccess = true
                    // Result = null
                };
            }

            try
            {
                var result = JsonConvert.DeserializeObject<ApiResponse>(response.Content);
                return result ?? new ApiResponse
                {
                    IsSuccess = false,
                    Message = "返回内容反序列化失败"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = $"返回内容解析失败：{ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<T>> ExecuteAsync<T>(BaseRequest baseRequest)
        {
            try
            {
                //var request = BuildRequest(baseRequest);
                //var response = await client.ExecuteAsync(request);
                var response = await SendWithRetryAsync(baseRequest);
                if (!response.IsSuccessful)
                {
                    return new ApiResponse<T>
                    {
                        IsSuccess = false,
                        //Result = default,
                        Message = string.IsNullOrWhiteSpace(response.ErrorMessage) ? "未知错误" : response.ErrorMessage
                    };
                }

                if (string.IsNullOrWhiteSpace(response.Content))
                {
                    return new ApiResponse<T>
                    {
                        IsSuccess = true,
                        //Result = default,
                        Message = "执行成功，但未获取到返回内容"
                    };
                }

                try
                {
                    var result = JsonConvert.DeserializeObject<ApiResponse<T>>(response.Content);
                    return result ?? new ApiResponse<T>
                    {
                        IsSuccess = false,
                        Message = "返回内容反序列化失败"
                    };
                }
                catch (Exception ex)
                {
                    return new ApiResponse<T>
                    {
                        IsSuccess = false,
                        Message = $"执行失败：{ex.Message}"
                    };
                }
            }
            catch (Exception exp)
            {
                return new ApiResponse<T>
                {
                    IsSuccess = false,
                    Message = $"返回内容解析失败：{exp.Message}"
                };
            }
        }

        private RestRequest BuildRequest(BaseRequest baseRequest)
        {
            var fullUrl = $"{ApiUrl.TrimEnd('/')}/{baseRequest.Route.TrimStart('/')}";
            var request = new RestRequest(fullUrl, baseRequest.Method);
            if (!string.IsNullOrWhiteSpace(baseRequest.ContentType))
                request.AddHeader("Content-Type", baseRequest.ContentType);
            if (!string.IsNullOrWhiteSpace(baseRequest.Token))
            {
                request.AddParameter("Authorization", "Bearer " + baseRequest.Token, ParameterType.HttpHeader);
                //request.AddHeader("Authorization", "Bearer " + baseRequest.Token);
            }
            if (baseRequest.Parameter != null)
                request.AddJsonBody(JsonConvert.SerializeObject(baseRequest.Parameter));
            return request;
        }

        private async Task<RestResponse> SendWithRetryAsync(BaseRequest baseRequest, bool retryOn401 = true)
        {
            var request = BuildRequest(baseRequest);
            var response = await client.ExecuteAsync(request);
            if (response.StatusCode != System.Net.HttpStatusCode.Unauthorized || !retryOn401)
            {
                return response;
            }
            await refreshLock.WaitAsync();

            try
            {
                var lastToken = AppSession.Token;
                if (!string.IsNullOrEmpty(lastToken) && lastToken != baseRequest.Token)
                {
                    baseRequest.Token = lastToken;

                    return await SendWithRetryAsync(baseRequest, false);
                }
                if (string.IsNullOrEmpty(AppSession.RefreshToken))
                {
                    return response;
                }
                if (RefreshTokenAsync == null || string.IsNullOrEmpty(AppSession.RefreshToken))
                {
                    return response;
                }
                var (newAccess, newRefresh) = await RefreshTokenAsync(AppSession.RefreshToken);
                if (string.IsNullOrEmpty(newAccess))
                {
                    return response;
                }
                AppSession.Token = newAccess;
                if (!string.IsNullOrEmpty(newRefresh))
                {
                    AppSession.RefreshToken = newRefresh;
                }
                baseRequest.Token = newAccess;
                return await SendWithRetryAsync(baseRequest, false);
            }
            finally { refreshLock.Release(); }
        }
    }
}
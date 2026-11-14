using MyToDo.Shared;
using MyToDo.Shared.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyToDo.Service
{
    public class LoginService : ILoginService
    {
        public HttpRestClient client;
        private readonly string serviceName = "Login";

        public LoginService(HttpRestClient client)
        {
            this.client = client;
        }

        public async Task<ApiResponse<LoginUserDto>> LoginAsync(LoginUserDto userDto)
        {
            BaseRequest request = new BaseRequest();
            request.Method = RestSharp.Method.Post;
            request.Route = $"api/{serviceName}/Login";
            request.Parameter = userDto;
            return await client.ExecuteAsync<LoginUserDto>(request);
        }

        public async Task<ApiResponse> RegisterAsync(UserDto userDto)
        {
            BaseRequest request = new BaseRequest();
            request.Method = RestSharp.Method.Post;
            request.Route = $"api/{serviceName}/Register";
            request.Parameter = userDto;
            return await client.ExecuteAsync(request);
        }

        public async Task<ApiResponse> ResetPwdAsync(UserDto userDto)
        {
            BaseRequest request = new BaseRequest();
            request.Method = RestSharp.Method.Post;
            request.Route = $"api/{serviceName}/Reset";
            request.Parameter = userDto;
            return await client.ExecuteAsync(request);
        }

        public async Task<ApiResponse> RefreshTokenAsync(RefreshRequest refreshRequest)
        {
            BaseRequest request = new BaseRequest();
            request.Method = RestSharp.Method.Post;
            request.Route = $"api/{serviceName}/RefreshMyToken";
            request.Parameter = refreshRequest;
            return await client.ExecuteAsync(request);
        }
    }
}
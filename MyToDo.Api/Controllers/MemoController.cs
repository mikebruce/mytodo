using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyToDo.Api.Extensions;
using MyToDo.Api.Service;
using MyToDo.Shared;
using MyToDo.Shared.Dtos;
using MyToDo.Shared.Parameters;

namespace MyToDo.Api.Controllers
{
    /// <summary>
    /// 备忘录
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class MemoController : Controller
    {
        public IMemoService Service;

        public MemoController(IMemoService service)
        {
            Service = service;
        }

        /// <summary>
        /// 获取单条备忘录信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        public async Task<ApiResponse> Get(int id)
        {
            var userId = User.GetUserId();
            if (userId <= 0)
            {
                return new ApiResponse { Message = "没有检测到用户登录状态,请先登录！" };
            }
            return await Service.GetSingleAsync(id, userId);
        }

        /// <summary>
        /// 备忘列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        public async Task<ApiResponse> GetAll([FromQuery] QueryParameter query)
        {
            var userId = User.GetUserId();
            if (userId <= 0)
            {
                return new ApiResponse { Message = "没有检测到用户登录状态,请先登录！" };
            }
            query.UserId = userId;
            return await Service.GetAllAsync(query);
        }

        /// <summary>
        /// 新增备忘信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public async Task<ApiResponse> Add([FromBody] MemoDto model)
        {
            var userId = User.GetUserId();
            if (userId <= 0)
            {
                return new ApiResponse { Message = "没有检测到用户登录状态,请先登录！" };
            }
            model.UserId = userId;
            return await Service.AddAsync(model);
        }

        /// <summary>
        /// 更新备忘信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public async Task<ApiResponse> Update([FromBody] MemoDto model)
        {
            var userId = User.GetUserId();
            if (userId <= 0)
            {
                return new ApiResponse { Message = "没有检测到用户登录状态,请先登录！" };
            }
            model.UserId = userId;
            return await Service.UpdateAsync(model);
        }

        /// <summary>
        /// 删除备忘信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        [HttpDelete]
        public async Task<ApiResponse> Delete(int id)
        {
            var userId = User.GetUserId();
            if (userId <= 0)
            {
                return new ApiResponse { Message = "没有检测到用户登录状态,请先登录！" };
            }
            return await Service.DeleteAsync(id, userId);
        }
    }
}
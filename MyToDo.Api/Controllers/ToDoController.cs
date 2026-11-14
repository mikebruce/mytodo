using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyToDo.Api.Extensions;
using MyToDo.Api.Service;
using MyToDo.Shared;
using MyToDo.Shared.Dtos;

using MyToDo.Shared.Parameters;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace MyToDo.Api.Controllers
{
    /// <summary>
    /// 待办事项
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ToDoController : Controller
    {
        public IToDoService Service;

        public ToDoController(IToDoService service)
        {
            Service = service;
        }

        /// <summary>
        /// 获取单个待办信息
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
        /// 获取待办数据
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        public async Task<ApiResponse> GetAll([FromQuery] ToDoParameter query)
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
        /// 首页汇总信息
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        public async Task<ApiResponse> Summary()
        {
            var userId = User.GetUserId();
            if (userId <= 0)
            {
                return new ApiResponse { Message = "没有检测到用户登录状态,请先登录！" };
            }
            return await Service.Summary(userId);
        }

        /// <summary>
        /// 新增待办
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public async Task<ApiResponse> Add([FromBody] ToDoDto model)
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
        /// 更新待办
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public async Task<ApiResponse> Update([FromBody] ToDoDto model)
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
        /// 删除待办
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
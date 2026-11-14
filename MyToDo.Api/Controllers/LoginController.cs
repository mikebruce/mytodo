using Microsoft.AspNetCore.Mvc;
using MyToDo.Api.Service;
using MyToDo.Shared.Dtos;
using MyToDo.Shared;

namespace MyToDo.Api.Controllers
{
    /// <summary>
    /// 登录相关
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class LoginController : Controller
    {
        public ILoginService Service;

        public LoginController(ILoginService service)
        {
            Service = service;
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResponse> Login([FromBody] LoginUserDto user) => await Service.LoginAsync(user.Account, user.PassWord);

        /// <summary>
        /// 注册
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResponse> Register([FromBody] UserDto model) => await Service.RegisterAsync(model);

        /// <summary>
        /// 重置密码
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResponse> Reset([FromBody] UserDto model) => await Service.ResetAsync(model);

        /// <summary>
        /// 刷新Token
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResponse> RefreshMyToken([FromBody] RefreshRequest request)
        {
            return await Service.RefreshAsync(request.RefreshToken);
        }
    }
}
using MyToDo.Shared;
using MyToDo.Shared.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyToDo.Service
{
    public interface ILoginService
    {
        Task<ApiResponse<LoginUserDto>> LoginAsync(LoginUserDto userDto);

        Task<ApiResponse> RegisterAsync(UserDto userDto);

        Task<ApiResponse> ResetPwdAsync(UserDto userDto);

        Task<ApiResponse> RefreshTokenAsync(RefreshRequest refreshRequest);
    }
}
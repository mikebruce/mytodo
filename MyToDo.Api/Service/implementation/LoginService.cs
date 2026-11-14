using Arch.EntityFrameworkCore.UnitOfWork;
using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using MyToDo.Api.Context;
using MyToDo.Shared;
using MyToDo.Shared.Dtos;
using MyToDo.Shared.Extensions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MyToDo.Api.Service.implementation
{
    public class LoginService : ILoginService
    {
        public IUnitOfWork work;
        public IMapper mapper;
        private readonly IConfiguration configuration;

        public LoginService(IUnitOfWork work, IMapper mapper, IConfiguration configuration)
        {
            this.work = work;
            this.mapper = mapper;
            this.configuration = configuration;
        }

        public async Task<ApiResponse> LoginAsync(string Account, string Password)
        {
            try
            {
                Password = Password.Encrypt();
                var model = await work.GetRepository<User>().GetFirstOrDefaultAsync(predicate:
                     (u => u.Account.Equals(Account) &&
                      u.PassWord.Equals(Password)));
                if (model == null)
                {
                    return new ApiResponse { Message = "账号或密码错误" };
                }
                //构建 Claims
                var claims = new[] {
                 new Claim(JwtRegisteredClaimNames.Sub,model.Id.ToString()),
                 new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                 new Claim(ClaimTypes.Role,"Admin"),
                 new Claim(ClaimTypes.NameIdentifier,model.Account)
                };
                //生成签名凭证
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: configuration["Jwt:Issuer"],
                    audience: configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(30),
                    signingCredentials: creds);

                var refreshToken = await CreateRefreshTokenAsync(model.Id);
                return new ApiResponse { IsSuccess = true, Result = model, Token = new JwtSecurityTokenHandler().WriteToken(token), RefreshToken = refreshToken };
            }
            catch (Exception exp)
            {
                return new ApiResponse { Message = exp.Message };
            }
        }

        public async Task<ApiResponse> RegisterAsync(UserDto user)
        {
            try
            {
                var model = mapper.Map<User>(user);
                var service = work.GetRepository<User>();
                var userInfo = await service.GetFirstOrDefaultAsync(predicate: x => x.Account.Equals(model.Account));
                if (userInfo != null)
                {
                    return new ApiResponse { Message = $"要注册的账号{model.Account}已存在" };
                }
                model.CreateDate = model.UpdateDate = DateTime.Now;
                //密码加密
                model.PassWord = model.PassWord.Encrypt();
                await service.InsertAsync(model);
                if (await work.SaveChangesAsync() > 0)
                {
                    return new ApiResponse { IsSuccess = true, Result = model };
                }
                return new ApiResponse { Message = "添加失败" };
            }
            catch (Exception exp)
            {
                return new ApiResponse { Message = exp.Message };
            }
        }

        public async Task<ApiResponse> ResetAsync(UserDto user)
        {
            try
            {
                var model = mapper.Map<User>(user);
                var service = work.GetRepository<User>();
                var userInfo = await service.GetFirstOrDefaultAsync(predicate: x => x.Account.Equals(model.Account));
                if (userInfo == null)
                {
                    return new ApiResponse { Message = $"要重置密码的账号{model.Account}不存在" };
                }
                model.UpdateDate = DateTime.Now;
                //密码加密
                model.PassWord = model.PassWord.Encrypt();
                model.Id = userInfo.Id;
                model.UserName = userInfo.UserName;
                service.Update(model);
                if (await work.SaveChangesAsync() > 0)
                {
                    return new ApiResponse { IsSuccess = true, Result = model };
                }
                return new ApiResponse { Message = "重置失败" };
            }
            catch (Exception exp)
            {
                return new ApiResponse { Message = exp.Message };
            }
        }

        private async Task<string> CreateRefreshTokenAsync(int UserId)
        {
            var refreshToken = new RefreshToken
            {
                UserId = UserId,
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                ExpiresAt = DateTime.UtcNow.AddDays(7),   // Refresh Token 有效期 7 天
                CreatedAt = DateTime.UtcNow
            };
            await work.GetRepository<RefreshToken>().InsertAsync(refreshToken);
            await work.SaveChangesAsync();
            return refreshToken.Token;
        }

        public async Task<ApiResponse> RefreshAsync(string refreshToken)
        {
            try
            {
                var repo = work.GetRepository<RefreshToken>();
                var stored = await repo.GetFirstOrDefaultAsync(predicate: r => r.Token == refreshToken);

                if (stored == null || !stored.IsActive)
                    return new ApiResponse { Message = "Refresh Token 无效或已过期" };

                // 可选：轮换 Refresh Token（吊销旧的，签发新的），更安全
                stored.RevokedAt = DateTime.UtcNow;
                repo.Update(stored);

                var user = await work.GetRepository<User>().GetFirstOrDefaultAsync(predicate: u => u.Id == stored.UserId);
                if (user == null)
                    return new ApiResponse { Message = "用户不存在" };

                // 重新生成 Access Token（复用你原有的 claims 构建逻辑）
                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Role, "Admin"),
                    new Claim(ClaimTypes.NameIdentifier, user.Account)
                };
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var newToken = new JwtSecurityToken(
                    issuer: configuration["Jwt:Issuer"],
                    audience: configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(30),   // Access Token 建议 15~30 分钟
                    signingCredentials: creds);

                await work.SaveChangesAsync();

                return new ApiResponse
                {
                    IsSuccess = true,
                    Token = new JwtSecurityTokenHandler().WriteToken(newToken),
                    RefreshToken = await CreateRefreshTokenAsync(user.Id)   // 轮换新 Refresh Token
                };
            }
            catch (Exception exp)
            {
                return new ApiResponse { Message = exp.Message };
            }
        }
    }
}
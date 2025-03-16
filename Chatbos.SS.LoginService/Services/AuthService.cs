using Chatbos.SS.LoginService.Models;
using Chatbos.SS.LoginService.Token;
using Chatbos.SS.LoginService.ViewModels;
using Chatbot.SS.AI.Entities.Database;
using Chatbot.SS.AI.Entities.Models;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Security.Claims;

namespace Chatbos.SS.LoginService.Services
{
    public class AuthService : IAuthService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ITokenTool _tokenTool;
        private readonly string? MasterPassword;

        public AuthService(IHttpContextAccessor httpContextAccessor, AppDbContext context, IConfiguration configuration,
           ITokenTool tokenTool)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _configuration = configuration;
            _tokenTool = tokenTool;
            MasterPassword = _configuration["AppSettings:MasterPassword"];
        }

        public async Task<ResponseBase<Res_AuthVM>> Auth()
        {
            try
            {
                string? username = _httpContextAccessor.HttpContext?.User.Identity?.Name;

                string? role = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role);

                if (string.IsNullOrEmpty(username))
                    throw new Exception("Invalid token");

                User? user = await IsUserExist(username);

                var selected_role = new ViewModels.Res_AuthLoginRoleVM();
                var currentToken = GetCurrentToken();
                var userToken = GetRefreshToken(user.Id);

                ViewModels.Res_AuthVM outputUser = new ViewModels.Res_AuthVM
                {
                    Id = user.Id,
                    Username = user.UserName,
                    Role = user.Role,
                    AccessToken = currentToken,
                    RefreshToken = userToken.Result.RefreshToken
                };      

                return new ResponseBase<ViewModels.Res_AuthVM> { Status = true, Message = "OK", Data = outputUser };
            }
            catch (Exception ex)
            {
                return new ResponseBase<ViewModels.Res_AuthVM> { Status = false, Message = ex.Message };
            }
        }

        public async Task<ResponseBase<Res_AuthLoginRoleVM>> GetRoles()
        {
            try
            {
                string? username = _httpContextAccessor.HttpContext?.User.Identity?.Name;

                if (string.IsNullOrEmpty(username))
                    throw new Exception("Invalid token");

                User? user = await IsUserExist(username);

                var data = new ViewModels.Res_AuthLoginRoleVM
                {
                    Id = user.Id,
                    Role = user.Role
                };

                return new ResponseBase<ViewModels.Res_AuthLoginRoleVM> { Status = true, Message = "OK", Data = data };
            }
            catch (Exception ex)
            {
                return new ResponseBase<ViewModels.Res_AuthLoginRoleVM> { Status = false, Message = ex.Message };
            }
        }

        public async Task<ResponseBase<Res_AuthVM>> Login(Req_AuthLoginVM data)
        {
            try
            {
                if (data == null)
                    throw new Exception("Invalid body");

                if (string.IsNullOrEmpty(data.Username))
                    throw new Exception("Username cannot be empty");

                if (string.IsNullOrEmpty(data.Password))
                    throw new Exception("Password cannot be empty");

                var user = await IsUserExist(data.Username, data.Password)
                    ?? throw new Exception("Username does not exist");

                var generatedRefreshToken = _tokenTool.GenerateRefreshToken();

                var userToken = new UserToken
                {
                    Id = ObjectId.GenerateNewId(),
                    UserId = ObjectId.Parse(user.Id),
                    RefreshToken = generatedRefreshToken,
                    Ip = data.IP,
                    CreatedAt = DateTime.UtcNow,
                    ExpiredAt = DateTime.UtcNow.AddDays(7),
                    User = new User
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        Role = user.Role
                    }
                };

                user.LastAccess = DateTime.UtcNow;

                var update = Builders<User>.Update.Set(u => u.LastAccess, user.LastAccess);
                await _context.Users.UpdateOneAsync(u => u.Id == user.Id, update);

                await _context.UserToken.InsertOneAsync(userToken);

                var generatedAccessToken = _tokenTool.GenerateAccessToken(new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.UserName)
                    });

                return new ResponseBase<Res_AuthVM>
                {
                    Status = true,
                    Message = "OK",
                    Data = new Res_AuthVM
                    {
                        Id = user.Id,
                        Username = user.UserName,
                        Role = user.Role,
                        AccessToken = generatedAccessToken,
                        RefreshToken = generatedRefreshToken
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseBase<Res_AuthVM> { Status = false, Message = ex.Message };
            }
        }


        public async Task<ResponseBase<string>> Logout()
        {
            try
            {
                var currentToken = GetCurrentToken();
                if (currentToken == null)
                {
                    return new ResponseBase<string> { Status = false, Message = "No token found" };
                }

                _tokenTool.InvalidateToken(currentToken);

                return new ResponseBase<string> { Status = true, Message = "Logout successful" };
            }
            catch (Exception ex)
            {
                return new ResponseBase<string> { Status = false, Message = ex.Message };
            }
        }

        public async Task<ResponseBase<Res_AuthRefreshTokenVM>> RefreshToken(Req_AuthRefreshTokenVM data)
        {
            try
            {
                if (data == null)
                    throw new Exception("Invalid body");

                var principal = _tokenTool.GetPrincipalFromExpiredToken(data.AccessToken);
                var principalUsername = principal.Identity?.Name;

                string? principalRole = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Role);

                // Query user with refresh token validation
                var user = await _context.Users.Find(u => u.UserName == principalUsername).FirstOrDefaultAsync();

                if (user == null || user.UserName != principalUsername)
                    throw new Exception("Invalid token");

                var userObjectId = ObjectId.Parse(user.Id);

                var userToken = await _context.UserToken
                    .Find(ut => ut.UserId == userObjectId && ut.RefreshToken == data.RefreshToken && ut.ExpiredAt >= DateTime.UtcNow)
                    .FirstOrDefaultAsync();

                if (userToken == null)
                    throw new Exception("Invalid or expired refresh token");

                var selected_role = new ViewModels.Res_AuthLoginRoleVM();
                List<Claim> userClaim = new List<Claim> { new Claim(ClaimTypes.Name, user.UserName) };

                if (!string.IsNullOrEmpty(principalRole))
                {
                    var userRole = await _context.Users
                        .Find(ur => ur.Id == user.Id && ur.Role == principalRole)
                        .FirstOrDefaultAsync();

                    if (userRole != null)
                    {
                        var role = await _context.Users
                            .Find(r => r.Id == userRole.Role)
                            .SortBy(r => r.Role)
                            .FirstOrDefaultAsync();

                        if (role != null)
                        {
                            selected_role = new ViewModels.Res_AuthLoginRoleVM
                            {
                                Id = role.Id,
                                Role = role.Role
                            };
                            userClaim.Add(new Claim(ClaimTypes.Role, selected_role.Id.ToString()));
                        }
                    }
                }

                var generatedAccessToken = _tokenTool.GenerateAccessToken(userClaim);

                return new ResponseBase<ViewModels.Res_AuthRefreshTokenVM>
                {
                    Status = true,
                    Message = "OK",
                    Data = new ViewModels.Res_AuthRefreshTokenVM
                    {
                        AccessToken = generatedAccessToken,
                        RefreshToken = data.RefreshToken
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseBase<ViewModels.Res_AuthRefreshTokenVM> { Status = false, Message = ex.Message };
            }

        }

        public async Task<ResponseBase<Res_AuthSetRoleVM>> SetRoleToToken(Req_AuthSetRoleVM data)
        {
            try
            {
                string? username = _httpContextAccessor.HttpContext?.User.Identity?.Name;

                if (string.IsNullOrEmpty(username))
                    throw new Exception("Invalid token");

                User? user = await IsUserExist(username);

                if (data == null)
                    throw new Exception("Invalid body");

                if (data.Role == null || data.Role.IsNullOrEmpty())
                    throw new Exception("Invalid body. Empty role");

                var role = await _IsRoleUserAuthorized(user.Id, data.Role);

                var generatedAccessToken = _tokenTool.GenerateAccessToken(new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Role, role.Id.ToString())
                });

                return new ResponseBase<ViewModels.Res_AuthSetRoleVM>
                {
                    Status = true,
                    Message = "OK",
                    Data = new ViewModels.Res_AuthSetRoleVM
                    {
                        AccessToken = generatedAccessToken
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseBase<ViewModels.Res_AuthSetRoleVM> { Status = false, Message = ex.Message };
            }
        }

        private async Task<ViewModels.Res_AuthLoginRoleVM> _IsRoleUserAuthorized(string userId, string role)
        {
            var data = new ViewModels.Res_AuthLoginRoleVM()
            {
                Id = userId,
                Role = role
            };

            return await Task.Run(() => data);
        }

        private async Task<User?> IsUserExist(string userName, string password)
        {
            return await _context.Users
                .Find(u => u.UserName == userName && u.Password == password)
                .FirstOrDefaultAsync();
        }


        private async Task<User> IsUserExist(string userName)
        {
            return await _context.Users
                .Find(u => u.UserName == userName)
                .FirstOrDefaultAsync();
        }

        private async Task<UserToken> GetRefreshToken(string id)
        {
            return await _context.UserToken
                .Find(u => u.Id == ObjectId.Parse(id))
                .FirstOrDefaultAsync();
        }

        private string? GetCurrentToken()
        {
            var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault();
            if (authHeader != null && authHeader.StartsWith("Bearer "))
            {
                return authHeader.Substring("Bearer ".Length).Trim();
            }
            return null;
        }

        private bool IsUserHasRole(string role)
        {
            return _context.Users
                .Find(u => u.Role.Contains(role))
                .Any();
        }

    }
}

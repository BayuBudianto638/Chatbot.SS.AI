using Chatbos.SS.LoginService.Models;
using Chatbos.SS.LoginService.Services;
using Chatbos.SS.LoginService.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Chatbos.SS.LoginService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly string? MasterPassword;
        private readonly IAuthService _authService;

        public AuthController(IConfiguration configuration, IAuthService authService)
        {
            _authService = authService;
            _configuration = configuration;
            MasterPassword = _configuration["AppSettings:MasterPassword"];
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(ViewModels.Req_AuthLoginVM data)
        {
            try
            {
                ResponseBase<Res_AuthVM> result = new ResponseBase<Res_AuthVM>();

                result = await _authService.Login(data);

                var output = new ResponseBase<ViewModels.Res_AuthVM> { Status = result.Status, Message = result.Message, Data = result.Data };

                if (result.Status)
                    return Ok(output);

                return BadRequest(output);
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseBase<ViewModels.Res_AuthVM> { Status = false, Message = ex.Message });
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Auth()
        {
            try
            {
                var result = await _authService.Auth();

                var output = new ResponseBase<ViewModels.Res_AuthVM> { Status = result.Status, Message = result.Message, Data = result.Data };

                if (result.Status)
                    return Ok(output);
                else
                    return BadRequest(output);
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseBase<ViewModels.Res_AuthVM> { Status = false, Message = ex.Message });
            }
        }

        [HttpPost("refresh_token")]
        [Authorize]
        public async Task<IActionResult> RefreshToken(ViewModels.Req_AuthRefreshTokenVM data)
        {
            try
            {
                var result = await _authService.RefreshToken(data);

                var output = new ResponseBase<ViewModels.Res_AuthRefreshTokenVM> { Status = result.Status, Message = result.Message, Data = result.Data };

                if (result.Status)
                    return Ok(output);
                else
                    return BadRequest(output);
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseBase<ViewModels.Res_AuthRefreshTokenVM> { Status = false, Message = ex.Message });
            }
        }

        [HttpGet("roles")]
        [Authorize]
        public async Task<IActionResult> GetRoles()
        {
            try
            {
                var result = await _authService.GetRoles();

                var output = new ResponseBase<ViewModels.Res_AuthLoginRoleVM> { Status = result.Status, Message = result.Message, Data = result.Data };

                if (result.Status)
                    return Ok(output);
                else
                    return BadRequest(output);
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseBase<ViewModels.Res_AuthLoginRoleVM> { Status = false, Message = ex.Message });
            }
        }

        [HttpPost("roles")]
        [Authorize]
        public async Task<IActionResult> SetRole(ViewModels.Req_AuthSetRoleVM data)
        {
            try
            {
                var result = await _authService.SetRoleToToken(data);

                var output = new ResponseBase<ViewModels.Res_AuthSetRoleVM> { Status = result.Status, Message = result.Message, Data = result.Data };

                if (result.Status)
                    return Ok(output);
                else
                    return BadRequest(output);
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseBase<ViewModels.Res_AuthSetRoleVM> { Status = false, Message = ex.Message });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _authService.Logout();

                return Ok(new ResponseBase<string>
                {
                    Status = true,
                    Message = "Logout Successfull"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseBase<string>
                {
                    Status = false,
                    Message = ex.Message
                });
            }
        }
    }
}

using Microsoft.AspNetCore.Mvc;

namespace Chatbos.SS.LoginService.ViewModels
{
    [ModelBinder]
    public class Req_AuthLoginVM
    {
        public string UserDomain { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string? IP { get; set; }
        public string? UserAgent { get; set; }
    }
}

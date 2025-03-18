using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatbot.SS.AI.AuthorizationLib.ViewModels
{
    public class AuthorizationVM
    {
        public bool Auth { get; set; }
        public ObjectId? UserId { get; set; }
        public string UserName { get; set; }
        public string? Role { get; set; }
        public string? Message { get; set; }
    }
}

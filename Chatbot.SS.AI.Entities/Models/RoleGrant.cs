using System.Data;

namespace Chatbot.SS.AI.Entities.Models
{
    public class RoleGrant
    {
        public int Id { get; set; }

        public string? Role { get; set; }

        public bool? Create { get; set; }

        public bool? Read { get; set; }

        public bool? Update { get; set; }

        public bool? Delete { get; set; }
    }
}

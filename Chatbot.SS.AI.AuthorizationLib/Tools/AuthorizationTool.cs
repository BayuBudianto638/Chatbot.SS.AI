using Chatbot.SS.AI.AuthorizationLib.Enums;
using Chatbot.SS.AI.Entities.Database;
using Chatbot.SS.AI.Entities.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Chatbot.SS.AI.AuthorizationLib.Tools
{
    public class AuthorizationTool(AppDbContext context)
    {
        private readonly AppDbContext _context = context;

        public async Task<ViewModels.AuthorizationVM> IsAuthorized(ClaimsPrincipal user, Enums.AuthGrantEnum? grantType)
        {
            try
            {
                var username = user.Identity?.Name;

                if (username is null)
                    throw new Exception("Invalid identity");

                User? authedUser = await _context.Users
                    .Find(x => x.UserName == username)
                    .FirstOrDefaultAsync() ?? throw new Exception("Invalid identity. Username not found");


                var currentRole = user.FindFirstValue(ClaimTypes.Role) ?? throw new Exception("Invalid role");

                RoleGrant? roleGrant = await _context.RoleGrants
                         .Find(x => x.Role == authedUser.Role)
                         .FirstOrDefaultAsync() ?? throw new Exception("Role has no any permission");

                if (grantType == AuthGrantEnum.CREATE)
                    if (roleGrant == null || roleGrant.Create == null || roleGrant.Create == false)
                        throw new Exception("Unauthorized user role");

                if (grantType == AuthGrantEnum.READ)
                    if (roleGrant == null || roleGrant.Read == null || roleGrant.Read == false)
                        throw new Exception("Unauthorized user role");

                if (grantType == AuthGrantEnum.UPDATE)
                    if (roleGrant == null || roleGrant.Update == null || roleGrant.Update == false)
                        throw new Exception("Unauthorized user role");

                if (grantType == AuthGrantEnum.DELETE)
                    if (roleGrant == null || roleGrant.Delete == null || roleGrant.Delete == false)
                        throw new Exception("Unauthorized user role");

                return new ViewModels.AuthorizationVM
                {
                    Auth = true,
                    UserId = new ObjectId(authedUser.Id),
                    UserName = authedUser.UserName,
                    Role = authedUser.Role,
                    Message = "OK"
                };
            }
            catch (Exception ex)
            {
                return new ViewModels.AuthorizationVM { Auth = false, Message = ex.Message, UserId = new ObjectId("0") };
            }
        }
    }
}

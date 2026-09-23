using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CrmApp.Domain.DTO;
using CrmApp.Infrastructure.Identity;

namespace CrmApp.Api.Controllers
{
    [ApiController]
    [Route("api/mail")]
    public class MailController : ControllerBase
    {
        private readonly UsersManager _usrManager;
        public MailController(UsersManager usrManager)
        {
            _usrManager = usrManager;
        }

        [HttpPost("send-mail")]
        [Authorize]
        public async Task<ActionResult<bool>> SendEmail([FromBody] SendMailDTO mailDTO, CancellationToken ct = default)
        {
            var loggedUser = await _usrManager.Me(User);
            throw new NotImplementedException();
        }
    }
}

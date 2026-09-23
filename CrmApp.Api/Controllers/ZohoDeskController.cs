using Microsoft.AspNetCore.Mvc;
using CrmApp.Application.Abstractions.Services;
using CrmApp.Domain.DTO;
using CrmApp.Domain.DTO.TasksDTOs;
using CrmApp.Domain.Entities;
using CrmApp.Infrastructure.Identity;

namespace CrmApp.Api.Controllers
{
    [Route("api/zoho-desk")]
    [ApiController]
    public class ZohoDeskController : ControllerBase
    {
        private readonly IZohoDeskService _zohoDeskService;
        private readonly UsersManager _usersManager;
        public ZohoDeskController(IZohoDeskService taskService, UsersManager usersManager)
        {
            _zohoDeskService = taskService;
            _usersManager = usersManager;
        }
        [HttpGet("tickets")]
        public async Task<ResultDTO<List<TaskDTO>>> GetTickets()
        {
            var (userId, rolesIds) = await _usersManager.LoggedUserRoles(User);
            List<TaskDTO> tasks = await _zohoDeskService.GetTickets(userId, rolesIds);
            //zapisać do naszej bazy
            return new SuccessResultDTO<List<TaskDTO>>(data: tasks);
        }
        [HttpGet("ticket")]
        public async Task<ResultDTO<string>> GetTicket()
        {
            //await _zohoDeskService.GetTicket();
            return new SuccessResultDTO<string>(data: "");
        }
        [HttpPut("ticket")]
        public async Task<ResultDTO<TaskDTO>> UpdateTicket([FromBody] TaskDTO task)
        {
            var users = await _usersManager.GetAllUsersAsync();
            var (userId, rolesIds) = await _usersManager.LoggedUserRoles(User);
            return await _zohoDeskService.UpdateTicket(task, users, userId, rolesIds);
        }
        [HttpPut("comment-on-ticket")]
        public async Task<ResultDTO<string>> AddCommentToTicket()
        {
            await _zohoDeskService.AddCommentToTicket();
            return new SuccessResultDTO<string>(data: "");
        }
        [HttpPut("send-reply")]
        public async Task<ResultDTO<string>> SendReply()
        {
            await _zohoDeskService.SendReply();
            return new SuccessResultDTO<string>(data: "");
        }
    } 
}

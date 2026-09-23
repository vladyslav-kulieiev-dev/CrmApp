using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CrmApp.Application.Abstractions.Services;
using CrmApp.Domain.DTO;
using CrmApp.Domain.DTO.Lists;
using CrmApp.Domain.DTO.TasksDTOs;
using CrmApp.Infrastructure.Identity;

namespace CrmApp.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/tasks")]
    public class TasksController(
        ITasksService service, UsersManager userManager) : ControllerBase
    {
        private readonly ITasksService _service = service;
        private readonly UsersManager _usersManager = userManager;

        [HttpPost("paged")]
        [Authorize]
        public async Task<ActionResult<PagedResult<TaskListItemDTO>>> GetPagedTasks([FromBody] TaskPagedRequest request, CancellationToken ct)
        {
            var (userId, rolesIds) = await _usersManager.LoggedUserRoles(User);
           return Ok(await _service.GetPagedAsync(request, userId, rolesIds, ct));
        }

        [HttpPost("my")]
        [Authorize]
        public async Task<ActionResult<PagedResult<TaskListItemDTO>>> GetMyPagesTasks([FromBody] TaskPagedRequest request, CancellationToken ct)
        {
            var (userId, rolesIds) = await _usersManager.LoggedUserRoles(User);
            request.AssignedTo = userId;
            return Ok(await _service.GetPagedAsync(request, userId, rolesIds, ct));
        }

        [HttpGet("task/{id}")]
        [Authorize]
        public async Task<ActionResult<ResultDTO<TaskDTO>>> GeyById(int id, CancellationToken ct)
        {
            var (userId, rolesIds) = await _usersManager.LoggedUserRoles(User);
            var res = await _service.GetByIdAsync(id, userId, rolesIds, ct);
            return Ok(res);
        }

        [HttpPost("task")]
        [Authorize]
        public async Task<ActionResult<ResultDTO<TaskDTO>>> Add([FromBody] TaskDTO dto, CancellationToken ct)
        {
            var (userId, rolesIds) = await _usersManager.LoggedUserRoles(User);
            var res = await _service.Add(dto, userId, rolesIds, ct);
            return Ok(res);
        }

        [HttpPut("task")]
        [Authorize]
        public async Task<ActionResult<ResultDTO<TaskDTO>>> Update([FromBody] TaskDTO dto, CancellationToken ct)
        {
            var (userId, rolesIds) = await _usersManager.LoggedUserRoles(User);
            var res = await _service.Update(dto, userId, rolesIds, ct);
            return Ok(res);
        }

        [HttpDelete("task/{id}")]
        [Authorize]
        public async Task<ActionResult<ResultDTO<TaskDTO>>> Delete(int id, CancellationToken ct)
        {
            var (userId, rolesIds) = await _usersManager.LoggedUserRoles(User);
            return Ok(await _service.Delete(id, userId, rolesIds, ct));
        }

        [HttpPost("comment")]
        [Authorize]
        public async Task<ActionResult<ResultDTO<TaskCommentDTO>>> CommentTask([FromBody] TaskCommentDTO dto, CancellationToken ct)
        {
            var (userId, _) = await _usersManager.LoggedUserRoles(User);
            return Ok(await _service.AddComment(dto, userId, ct));
        }

        [HttpPut("comment")]
        [Authorize]
        public async Task<ActionResult<ResultDTO<TaskCommentDTO>>> EditComment([FromBody] TaskCommentDTO dto, CancellationToken ct)
        {
            var (userId, _) = await _usersManager.LoggedUserRoles(User);
            return Ok(await _service.EditComment(dto, userId, ct));
        }

        [HttpDelete("comment/{id}")]
        [Authorize]
        public async Task<ActionResult<ResultDTO<TaskCommentDTO>>> DeleteComment(int id, CancellationToken ct)
        {
            var (userId, _) = await _usersManager.LoggedUserRoles(User);
            return Ok(await _service.DeleteComment(id, userId, ct));
        }
    }
}

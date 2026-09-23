using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CrmApp.Application.Abstractions.Services;
using CrmApp.Application.Services;
using CrmApp.Domain.DTO;
using CrmApp.Domain.Entities;
using CrmApp.Domain.Identity;

namespace CrmApp.Api.Controllers
{
    [ApiController]
    [Route("api/projects")]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectsService _projectsService;
        public ProjectsController(IProjectsService projectsService)
        {
            _projectsService = projectsService;
        }

        [HttpGet("project/{id}")]
        [Authorize(Policy = "perm:" + AppClaims.ProjectsView)]
        public async Task<ActionResult<ResultDTO<Projects>>> GetProject(int id, CancellationToken ct = default)
        {
            var project = await _projectsService.GetById(id, ct);
            return Ok(project);
        }

        [HttpGet("contractor-projects/{contractorId}")]
        [Authorize(Policy = "perm:" + AppClaims.ProjectsView)]
        public async Task<ActionResult<IReadOnlyList<Projects>>> GetContractorProjects(int contractorId, CancellationToken ct = default)
        {
            var project = await _projectsService.GetByContractorId(contractorId, ct);
            return Ok(project);
        }

        [HttpGet("projects")]
        [Authorize]
        public async Task<ActionResult<IReadOnlyList<Projects>>> GetProjects(CancellationToken ct = default)
        {
            var projects = await _projectsService.GetProjects(ct);
            return Ok(projects);
        }

        [HttpPost("project")]
        [Authorize(Policy = "perm:" + AppClaims.ProjectsCreate)]
        public async Task<ActionResult<ResultDTO<Projects>>> AddProject([FromBody] Projects project, CancellationToken ct = default)
        {
            var result = await _projectsService.Add(project, ct);
            return Ok(result);
        }

        [HttpPut("project")]
        [Authorize(Policy = "perm:" + AppClaims.ProjectsUpdate)]
        public async Task<ActionResult<ResultDTO<Projects>>> UpdateProject([FromBody] Projects project, CancellationToken ct = default)
        {
            var result = await _projectsService.Update(project, ct);
            return Ok(result);
        }

        [HttpPut("project-start")]
        [Authorize(Policy = "perm:" + AppClaims.ProjectsUpdate)]
        public async Task<ActionResult<ResultDTO<Projects>>> StartProject([FromBody] int projectId, CancellationToken ct = default)
        {
            var result = await _projectsService.StartProject(projectId, ct);
            return Ok(result);
        }

        [HttpPut("project-close")]
        [Authorize(Policy = "perm:" + AppClaims.ProjectsUpdate)]
        public async Task<ActionResult<ResultDTO<Projects>>> CloseProject([FromBody] int projectId, CancellationToken ct = default)
        {
            var result = await _projectsService.CloseProject(projectId, ct);
            return Ok(result);
        }

        [HttpPut("project-cancel")]
        [Authorize(Policy = "perm:" + AppClaims.ProjectsUpdate)]
        public async Task<ActionResult<ResultDTO<Projects>>> CancelProject([FromBody] int projectId, CancellationToken ct = default)
        {
            var result = await _projectsService.CancelProject(projectId, ct);
            return Ok(result);
        }

        [HttpDelete("project/{id}")]
        [Authorize(Policy = "perm:" + AppClaims.ProjectsDelete)]
        public async Task<ActionResult<ResultDTO<Projects>>> DeleteProject(int id, CancellationToken ct = default)
        {
            var result = await _projectsService.Delete(id, ct);
            return Ok(result);
        }
    }
}

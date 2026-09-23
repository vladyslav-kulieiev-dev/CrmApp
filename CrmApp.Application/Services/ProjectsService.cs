using CrmApp.Application.Abstractions.Persistence;
using CrmApp.Application.Abstractions.Services;
using CrmApp.Domain.DTO;
using CrmApp.Domain.Entities;
using CrmApp.Domain.Enums;
using CrmApp.Domain.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Application.Services
{
    public sealed class ProjectsService : IProjectsService
    {
        private readonly IProjectsRepository _repo;

        public ProjectsService(IProjectsRepository projectsRepository)
        {
            _repo = projectsRepository;
        }

        public async Task<ResultDTO<Projects>> GetById(int projectId, CancellationToken ct)
        {
            var project = await _repo.GetReadOnlyAsync(projectId, ct);
            if (project == null) return new ErrorResultDTO<Projects>(["Nie znaleziono projektu w bazie"]);
            return new SuccessResultDTO<Projects>(project, projectId.ToString());
        }

        public Task<IReadOnlyList<Projects>> GetByContractorId(int contractorId, CancellationToken ct) => _repo.ListByContractorIdAsync(contractorId, ct);
        public Task<IReadOnlyList<Projects>> GetProjects(CancellationToken ct) => _repo.ListAllAsync(ct);

        public async Task<ResultDTO<Projects>> Add(Projects project, CancellationToken ct)
        {
            if (await _repo.ExistsWithNameForContractorAsync(project.Name, project.ContractorId, ct))
            {
                return new ErrorResultDTO<Projects>(["Istnieje już projekt o nazwie " + project.Name + 
                    (project.ContractorId != null ? " dla wybranego kontrahenta" : "")]);
            }

            project.Id = 0;
            project.MembersIds ??= [];
            project.CreatedAt = DateTime.Now;
            project.StartDate = project.StartDateStr.DateTimeFromOptString();
            project.EndDate = project.EndDateStr.DateTimeFromOptString();

            await _repo.AddAsync(project, ct);
            await _repo.UpdateProjectsMembers(project, project.MembersIds ?? [], project.ProjectManagerId , ct);
            await _repo.SaveChangesAsync(ct);

            return new SuccessResultDTO<Projects>(project.Id.ToString());
        }

        public async Task<ResultDTO<Projects>> Update(Projects project, CancellationToken ct)
        {
            var projectToUpdate = await _repo.GetAsync(project.Id, ct);
            if (projectToUpdate == null) return new ErrorResultDTO<Projects>(["Nie znaleziono projektu w bazie"]);

            if (await _repo.ExistsWithNameForContractorAsync(project.Id, project.Name, project.ContractorId, ct))
            {
                return new ErrorResultDTO<Projects>(["Istnieje już projekt o nazwie " + project.Name +
                    (project.ContractorId != null ? " dla wybranego kontrahenta" : "")]);
            }

            projectToUpdate.Name = project.Name;
            projectToUpdate.Description = project.Description;
            projectToUpdate.StartDate = project.StartDateStr.DateTimeFromOptString();
            projectToUpdate.EndDate = project.EndDateStr.DateTimeFromOptString();
            projectToUpdate.CreatedAt = project.CreatedAt;
            projectToUpdate.CreatedBy = project.CreatedBy;
            projectToUpdate.ProjectManagerId = project.ProjectManagerId;
            projectToUpdate.ModifiedBy = project.ModifiedBy;
            projectToUpdate.ModifiedAt = DateTime.Now;

            await _repo.UpdateAsync(projectToUpdate, ct);
            await _repo.UpdateProjectsMembers(projectToUpdate, project.MembersIds ?? [], project.ProjectManagerId, ct);
            await _repo.SaveChangesAsync(ct);

            return new SuccessResultDTO<Projects>(projectToUpdate.Id.ToString());
        }

        public async Task<ResultDTO<Projects>> Delete(int projectId, CancellationToken ct)
        {
            var projectToUpdate = await _repo.GetAsync(projectId, ct);
            if (projectToUpdate == null) return new ErrorResultDTO<Projects>(["Nie znaleziono projektu w bazie"]);
            if (projectToUpdate.State == EProjectState.Closed || projectToUpdate.State == EProjectState.Active) 
                return new ErrorResultDTO<Projects>(["Możesz usuwać projekty które są w nierozpoczęte lub anulowane"]);

            await _repo.RemoveProjectMembers(projectId);
            await _repo.Remove(projectToUpdate);
            await _repo.SaveChangesAsync(ct);

            return new SuccessResultDTO<Projects>(projectId.ToString());
        }

        public async Task<ResultDTO<Projects>> StartProject(int projectId, CancellationToken ct = default)
        {
            var projectToUpdate = await _repo.GetAsync(projectId, ct);
            if (projectToUpdate == null) return new ErrorResultDTO<Projects>(["Nie znaleziono projektu w bazie"]);
            if (projectToUpdate.State == EProjectState.Cancelled) return new ErrorResultDTO<Projects>(["Nie możesz rozpocząć projektu który został anulowany"]);
            if (projectToUpdate.State == EProjectState.Closed) return new ErrorResultDTO<Projects>(["Nie możesz rozpocząć projektu który został zamknięty"]);
            if (projectToUpdate.State == EProjectState.Active) return new ErrorResultDTO<Projects>(["Projekt jest już rozpoczęty"]);

            projectToUpdate.State = EProjectState.Active;
            await _repo.UpdateAsync(projectToUpdate, ct);
            await _repo.SaveChangesAsync(ct);

            // TO DO SEND EMAILS

            return new SuccessResultDTO<Projects>(projectId.ToString());
        }

        public async Task<ResultDTO<Projects>> CloseProject(int projectId, CancellationToken ct = default)
        {
            var projectToUpdate = await _repo.GetAsync(projectId, ct);
            if (projectToUpdate == null) return new ErrorResultDTO<Projects>(["Nie znaleziono projektu w bazie"]);
            if (projectToUpdate.State == EProjectState.Cancelled) return new ErrorResultDTO<Projects>(["Nie możesz zamknąć projektu który został anulowany"]);
            if (projectToUpdate.State == EProjectState.Created) return new ErrorResultDTO<Projects>(["Projekt nie został rozpoczęty"]);
            if (projectToUpdate.State == EProjectState.Closed) return new ErrorResultDTO<Projects>(["Projekt jest już zamknięty"]);

            projectToUpdate.State = EProjectState.Closed;
            await _repo.UpdateAsync(projectToUpdate, ct);
            await _repo.SaveChangesAsync(ct);

            // TO DO SEND EMAILS

            return new SuccessResultDTO<Projects>(projectId.ToString());
        }
        public async Task<ResultDTO<Projects>> CancelProject(int projectId, CancellationToken ct = default)
        {
            var projectToUpdate = await _repo.GetAsync(projectId, ct);
            if (projectToUpdate == null) return new ErrorResultDTO<Projects>(["Nie znaleziono projektu w bazie"]);
            if (projectToUpdate.State == EProjectState.Cancelled) return new ErrorResultDTO<Projects>(["Projekt jest już anulowany"]);

            projectToUpdate.State = EProjectState.Cancelled;
            await _repo.UpdateAsync(projectToUpdate, ct);
            await _repo.SaveChangesAsync(ct);

            // TO DO SEND EMAILS

            return new SuccessResultDTO<Projects>(projectId.ToString());
        }
    }
}

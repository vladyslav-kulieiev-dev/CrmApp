using CrmApp.Domain.DTO;
using CrmApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Application.Abstractions.Services
{
    public interface IProjectsService
    {
        Task<ResultDTO<Projects>> GetById(int projectId, CancellationToken ct);
        Task<IReadOnlyList<Projects>> GetByContractorId(int contractorId, CancellationToken ct);
        Task<IReadOnlyList<Projects>> GetProjects(CancellationToken ct);
        Task<ResultDTO<Projects>> Add(Projects project, CancellationToken ct);
        Task<ResultDTO<Projects>> Update(Projects project, CancellationToken ct);
        Task<ResultDTO<Projects>> Delete(int projectId, CancellationToken ct);
        Task<ResultDTO<Projects>> StartProject(int projectId, CancellationToken ct = default);
        Task<ResultDTO<Projects>> CloseProject(int projectId, CancellationToken ct = default);
        Task<ResultDTO<Projects>> CancelProject(int projectId, CancellationToken ct = default);
    }
}

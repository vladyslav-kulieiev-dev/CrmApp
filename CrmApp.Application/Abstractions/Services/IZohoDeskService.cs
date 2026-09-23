using CrmApp.Domain.DTO;
using CrmApp.Domain.DTO.TasksDTOs;

namespace CrmApp.Application.Abstractions.Services
{
    public interface IZohoDeskService
    {
        Task<List<TaskDTO>> GetTickets(int userId, List<string> rolesIds, CancellationToken ct = default);
        Task<ResultDTO<TaskDTO>> UpdateTicket(TaskDTO task, IReadOnlyList<UsersDTO> users, int userId, List<string> rolesIds, CancellationToken ct = default);
        Task AddCommentToTicket(CancellationToken ct = default);
        Task SendReply(CancellationToken ct = default);
    }
}

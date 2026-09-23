using CrmApp.Domain.DTO.Lists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Domain.DTO.TasksDTOs
{
    public class TaskPagedRequest : PagedRequest
    {
        public string? TaskNumber { get; set; }
        public string? Title { get; set; }
        public string? ImportedTaskNumber { get; set; }
        public string? Description { get; set; }
        public string? Notes { get; set; }
        public string? State { get; set; }
        public string? Priority { get; set; }
        public int? AssignedTo { get; set; }
        public int? ProjectId { get; set; }
        public int? ContractorId { get; set; }
        public int? ContractorContactId { get; set; }
        public bool? IsOverdue { get; set; }
    }
}

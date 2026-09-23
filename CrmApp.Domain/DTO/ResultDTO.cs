using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Domain.DTO
{
    public class ResultDTO<T>
    {
        public bool Succeeded { get; set; }
        public string[]? Errors { get; set; }
        public string[]? Messages { get; set; }
        public string? ObjectId { get; set; }
        public T? Data { get; set; }
    }

    public class ErrorResultDTO<T> : ResultDTO<T>
    {
        public ErrorResultDTO(string[] errors)
        {
            Succeeded = false;
            Errors = errors;
            Messages = Array.Empty<string>();
        }
    }

    public class SuccessResultDTO<T> : ResultDTO<T>
    {
        public SuccessResultDTO(string? objectId = null, string[]? messages = null)
        {
            Succeeded = true;
            Errors = Array.Empty<string>();
            Messages = messages ?? Array.Empty<string>();
            ObjectId = objectId;
        }
        public SuccessResultDTO(T data, string? objectId = null, string[]? messages = null)
        {
            Succeeded = true;
            Data = data;
            Errors = Array.Empty<string>();
            Messages = messages ?? Array.Empty<string>();
            ObjectId = objectId;
        }
    }
}

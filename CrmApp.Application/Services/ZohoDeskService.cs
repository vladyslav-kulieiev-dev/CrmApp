using Microsoft.AspNetCore.Identity;
using ModelContextProtocol.Client;
using CrmApp.Application.Abstractions.Services;
using CrmApp.Domain.Configuration;
using CrmApp.Domain.DTO;
using CrmApp.Domain.DTO.TasksDTOs;
using CrmApp.Domain.Entities;
using CrmApp.Domain.Enums;
using System.Net.Sockets;
using System.Text.Json;

namespace CrmApp.Application.Services
{
    public sealed class ZohoDeskService(ITasksService _tasksService, ISettingsService _settingsService,
        IDictionariesService _dictionariesService) : IZohoDeskService
    {
        private McpClient? _mcpClient;
        private string? mcpOrgId;
        private string? mcpDepartmentId;

        private async Task<McpClient> GetMcpClientAsync(CancellationToken ct)
        {
            if (_mcpClient is not null)
                return _mcpClient;

            var mcpName = await _settingsService.GetSettingValueByKey(SettingsKeys.ZohoDeskMcpName, ct);
            var mcpUri = await _settingsService.GetSettingValueByKey(SettingsKeys.ZohoDeskMcpUri, ct);
            mcpOrgId = await _settingsService.GetSettingValueByKey(SettingsKeys.ZohoDeskOrganizationId, ct) ?? "";
            mcpDepartmentId = await _settingsService.GetSettingValueByKey(SettingsKeys.ZohoDeskDepartmentId, ct) ?? "";
            if (string.IsNullOrWhiteSpace(mcpUri) || string.IsNullOrWhiteSpace(mcpOrgId) || string.IsNullOrWhiteSpace(mcpDepartmentId))
                throw new InvalidOperationException("Skonfiguruj MCP URI dla Zoho Desk");

            var transport = new HttpClientTransport(new()
            {
                Endpoint = new
                    Uri(mcpUri),
                Name = mcpName ?? "ZohoDesk"
            });

            _mcpClient = await McpClient.CreateAsync(transport, cancellationToken: ct);
            return _mcpClient;
        }

        public async Task<List<TaskDTO>> GetTickets(int userId, List<string> rolesIds, CancellationToken ct = default)
        {
            var mcpClient = await GetMcpClientAsync(ct);
            var result = await mcpClient.CallToolAsync("ZohoDesk_getTickets",
            new Dictionary<string, object?>
            {
                ["query_params"] = new Dictionary<string, object?>
                {
                    ["departmentId"] = long.Parse(mcpDepartmentId),//235799000000007061L
                    ["orgId"] = mcpOrgId //"20113531098"
                }
            },
            cancellationToken: ct);
            List<TaskDTO> tasksFromZoho = new List<TaskDTO>();
            foreach (var content in result.Content)
            {
                var json = JsonSerializer.Serialize(content);
                var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("text", out var textEl))
                {
                    string s = textEl.GetString() ?? "";
                    var d = JsonDocument.Parse(s);
                    if (d.RootElement.TryGetProperty("data", out var data))
                    {
                        foreach (var ticket in data.EnumerateArray())
                        {
                            ImportedTasks task = new ImportedTasks()
                            {
                                ExternalId = ticket.GetProperty("id").GetString() ?? string.Empty,
                                Number = ticket.GetProperty("ticketNumber").GetString() ?? string.Empty,
                                State = ticket.GetProperty("status").GetString() ?? string.Empty,
                                Subject = ticket.GetProperty("subject").GetString() ?? string.Empty,
                                Category = ticket.GetProperty("category").GetString() ?? null,
                                Url = ticket.GetProperty("webUrl").GetString() ?? null,
                                CreatedAt = ticket.TryGetProperty("createdTime", out JsonElement createdTime) && createdTime.ValueKind != JsonValueKind.Null
                                ? createdTime.GetDateTime().ToLocalTime() : DateTime.Now,
                                DueDate = ticket.TryGetProperty("dueDate", out JsonElement dueDate) && dueDate.ValueKind != JsonValueKind.Null
                                ? dueDate.GetDateTime().ToLocalTime() : null,
                                Channel = ticket.GetProperty("channel").GetString() ?? null,
                                Priority = ticket.GetProperty("priority").GetString() ?? null,
                                ResponseDueDate = ticket.TryGetProperty("responseDueDate", out JsonElement responseDueDate) && responseDueDate.ValueKind != JsonValueKind.Null
                                ? responseDueDate.GetDateTime() : null,
                                ClosedAt = ticket.TryGetProperty("closedTime", out JsonElement closedTime) && closedTime.ValueKind != JsonValueKind.Null
                                ? closedTime.GetDateTime().ToLocalTime() : null,
                                IsDeleted = false
                            };
                            task.Description = (await GetTicket(task.ExternalId, ct)).Description;
                            //task.ProductName = await GetProductName(ticket.GetProperty("productId").GetString() ?? null, ct);
                            //Contact? contact = await GetContact(ticket.GetProperty("contactId").GetString() ?? null, ct);
                            //if (contact != null)
                            //{
                            //    task.ContactFirstName = contact.FirstName;
                            //    task.ContactLastName = contact.LastName;
                            //    task.ContactPhoneNumber = contact.PhoneNumber;
                            //    task.ContactEmail = contact.Email;
                            //}
                            Assignee? assignee = await GetAssignee(ticket.GetProperty("assigneeId").GetString() ?? null, ct);
                            if (assignee != null)
                            {
                                task.AssigneeFirstName = assignee.FirstName;
                                task.AssigneeLastName = assignee.LastName;
                                task.AssigneeEmail = assignee.Email;
                            }
                            var newTask = (await _tasksService.AddTaskFromImported(task, userId, rolesIds, ct)).Data;
                            if(newTask != null) tasksFromZoho.Add(newTask);
                        }
                    }
                }
            }
            await MarkDeletedTickets(tasksFromZoho, userId, rolesIds, ct);
            return new List<TaskDTO>();
        }
        private async Task MarkDeletedTickets(List<TaskDTO> tasksFromZoho, int userId, List<string> rolesIds, CancellationToken ct) //przekazać świeżo zaimportowane numery zadań
        {
            var tasksWithImported = await _tasksService.ListAllWithImportedAsync();
            foreach (var task in tasksWithImported)
            {
                if (tasksFromZoho.FirstOrDefault(t => t.Id == task.Id) == null)
                {
                    await _tasksService.Delete(task.Id, userId, rolesIds, ct);
                }
            }
        }
        private async Task<TaskDTO> GetTicket(string externalTicketId, CancellationToken ct = default)
        {
            var mcpClient = await GetMcpClientAsync(ct);
            var result = await mcpClient.CallToolAsync("ZohoDesk_getTicket",
            new Dictionary<string, object?>
            {
                ["path_variables"] = new Dictionary<string, string>
                {
                    ["ticketId"] = externalTicketId
                },
                ["query_params"] = new Dictionary<string, object?>
                {
                    ["orgId"] = mcpOrgId
                }
            },
            cancellationToken: ct);
            TaskDTO task = new TaskDTO();
            foreach (var content in result.Content)
            {
                var json = JsonSerializer.Serialize(content);
                var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("text", out var textEl))
                {
                    string s = textEl.GetString() ?? "";
                    var d = JsonDocument.Parse(s);
                    var description = d.RootElement.TryGetProperty("description", out var descEl)
                        ? descEl.GetString() : "";
                    task.Description = description;
                }
            }
            return task;
        }
        private async Task<string?> GetProductName(string? productId, CancellationToken ct)
        {
            if(productId == null)
            {
                return null;
            }
            var mcpClient = await GetMcpClientAsync(ct);
            var result = await mcpClient.CallToolAsync("ZohoDesk_searchProducts",
            new Dictionary<string, object?>
            {
                ["query_params"] = new Dictionary<string, object?>
                {
                    ["orgId"] = mcpOrgId
                }
            },
            cancellationToken: ct);
            string? productName = null;
            foreach (var content in result.Content)
            {
                var json = JsonSerializer.Serialize(content);
                var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("text", out var textEl))
                {
                    string s = textEl.GetString() ?? "";
                    var d = JsonDocument.Parse(s);
                    if (d.RootElement.TryGetProperty("data", out var data))
                    {
                        foreach (var product in data.EnumerateArray())
                        {
                            if(product.GetProperty("id").GetString() == productId)
                            {
                                productName = product.GetProperty("productName").GetString();
                                break;
                            }
                        }
                    }
                }
            }
            return productName;
        }
        private async Task<Contact?> GetContact(string? contactId, CancellationToken ct)
        {
            if (contactId == null)
            {
                return null;
            }
            var mcpClient = await GetMcpClientAsync(ct);
            var result = await mcpClient.CallToolAsync("ZohoDesk_getContact",
            new Dictionary<string, object?>
            {
                ["path_variables"] = new Dictionary<string, object?>
                {
                    ["contactId"] = contactId
                },
                ["query_params"] = new Dictionary<string, object?>
                {
                    ["orgId"] = mcpOrgId
                }
            },
            cancellationToken: ct);
            Contact? contact = null;
            if (result.IsError == false)
            {
                contact = new Contact();
                foreach (var content in result.Content)
                {
                    var json = JsonSerializer.Serialize(content);
                    var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("text", out var textEl))
                    {
                        string s = textEl.GetString() ?? "";
                        var d = JsonDocument.Parse(s);
                        contact.FirstName = d.RootElement.TryGetProperty("firstName", out var firstName) ? firstName.GetString() : null;
                        contact.LastName = d.RootElement.TryGetProperty("lastName", out var lastName) ? lastName.GetString() : null;
                        contact.PhoneNumber = d.RootElement.TryGetProperty("phone", out var phone) ? phone.GetString() : null;
                        contact.Email = d.RootElement.TryGetProperty("email", out var email) ? email.GetString() : null;
                    }
                }
            }
            return contact;
        }
        private async Task<Assignee?> GetAssignee(string? assigneeId, CancellationToken ct)
        {
            if (assigneeId == null)
            {
                return null;
            }
            var mcpClient = await GetMcpClientAsync(ct);
            var result = await mcpClient.CallToolAsync("ZohoDesk_getAgent",
            new Dictionary<string, object?>
            {
                ["path_variables"] = new Dictionary<string, object?>
                {
                    ["agentId"] = assigneeId
                },
                ["query_params"] = new Dictionary<string, object?>
                {
                    ["orgId"] = mcpOrgId
                }
            },
            cancellationToken: ct);
            Assignee? assignee = null;
            if (result.IsError == false)
            {
                assignee = new Assignee();
                foreach (var content in result.Content)
                {
                    var json = JsonSerializer.Serialize(content);
                    var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("text", out var textEl))
                    {
                        string s = textEl.GetString() ?? "";
                        var d = JsonDocument.Parse(s);
                        assignee.FirstName = d.RootElement.TryGetProperty("firstName", out var firstName) ? firstName.GetString() : null;
                        assignee.LastName = d.RootElement.TryGetProperty("lastName", out var lastName) ? lastName.GetString() : null;
                        assignee.Email = d.RootElement.TryGetProperty("emailId", out var email) ? email.GetString() : null;
                    }
                }
            }
            return assignee;
        }
        private async Task<string?> GetAgentIdByEmail(string email, string ticketId, CancellationToken ct)
        {
            var mcpClient = await GetMcpClientAsync(ct);
            var result = await mcpClient.CallToolAsync("ZohoDesk_getAgents",
            new Dictionary<string, object?>
            {
                ["query_params"] = new Dictionary<string, object?>
                {
                    ["orgId"] = mcpOrgId
                }
            },
            cancellationToken: ct);
            foreach (var content in result.Content)
            {
                var json = JsonSerializer.Serialize(content);
                var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("text", out var textEl))
                {
                    string s = textEl.GetString() ?? "";
                    var d = JsonDocument.Parse(s);
                    if (d.RootElement.TryGetProperty("data", out var data))
                    {
                        foreach (var agent in data.EnumerateArray())
                        {
                            if(agent.GetProperty("emailId").GetString() == email)
                            {
                                try
                                {
                                    return agent.GetProperty("id").GetString();
                                }
                                catch (Exception e) { }
                                return null;
                            }
                        }
                    }
                }
            }
            return null;
        }
        public async Task<ResultDTO<TaskDTO>> UpdateTicket(TaskDTO task, IReadOnlyList<UsersDTO> users,
            int userId, List<string> rolesIds, CancellationToken ct = default)
        {
            if (task == null) return new ErrorResultDTO<TaskDTO>(["Task jest null"]);
            var res = await _tasksService.GetByIdAsync(task.Id, userId, rolesIds, ct);
            if (res == null) return new ErrorResultDTO<TaskDTO>(["Task jest null"]);
            TaskDTO taskFromDb = res.Data!;
            if (taskFromDb == null) return new ErrorResultDTO<TaskDTO>(["Task nie istnieje w bazie"]);
            if (taskFromDb.ImportedTaskId == null)
            {
                return await CreateTicket(taskFromDb, users, userId, rolesIds, ct);
            }
            var mcpClient = await GetMcpClientAsync(ct);

            // tą linijkę trzeba przetestować z kilkoma agentami z ZohoDesk
            var assigneeId = await GetAgentIdByEmail(users.FirstOrDefault(user => task.AssignedTo == user.Id)?.Email ?? String.Empty, task.ImportedTask.ExternalId, ct);
            var dueDate = task.DueDate;
            string dueDateString = string.Empty;
            if(dueDate != null)
            {
                dueDateString = dueDate.Value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            }
            var result = await mcpClient.CallToolAsync("ZohoDesk_updateTicket",
            new Dictionary<string, object?>
            {
                ["path_variables"] = new Dictionary<string, string>
                {
                    ["ticketId"] = task.ImportedTask.ExternalId
                },
                ["query_params"] = new Dictionary<string, object?>
                {
                    ["orgId"] = mcpOrgId
                },
                ["body"] = new Dictionary<string, object?>
                {
                    ["subject"] = task.Title,
                    ["description"] = task.Description,
                    ["dueDate"] = dueDateString,
                    ["assigneeId"] = assigneeId,
                    ["priority"] = await MapPriorityToExternal(task.Priority, ct),
                    ["status"] = await MapStateToExternal(task.State, ct),
                    ["channel"] = await MapSourceToExternal(task.Source, ct)
                }
            },
            cancellationToken: ct);
            return await _tasksService.GetByIdAsync(taskFromDb.Id, userId, rolesIds, ct);
        }
        private async Task<string?> MapStateToExternal(string? internalState, CancellationToken ct)
            => await MapToExternal(internalState, EDictionaryType.CrmTaskStates, ct);

        private async Task<string?> MapSourceToExternal(string? internalSource, CancellationToken ct)
            => await MapToExternal(internalSource, EDictionaryType.TaskSource, ct);

        private async Task<string?> MapPriorityToExternal(string? internalPriority, CancellationToken ct)
            => await MapToExternal(internalPriority, EDictionaryType.CrmTasksPriorities, ct);

        private async Task<string?> MapToExternal(string? internalValue, EDictionaryType type, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(internalValue)) return null;
            var dictionary = await _dictionariesService.GetByType(type, ct);
            if (dictionary == null)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[MapToExternal] WARN: brak słownika {type}, nie można zmapować '{internalValue}'.");
                return null;
            }
            var elem = dictionary.DictionariesElements
                .FirstOrDefault(e => e.Key == internalValue);
            if (string.IsNullOrWhiteSpace(elem?.AlternativeValuesForMapping))
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[MapToExternal] WARN: brak AlternativeValuesForMapping dla {type} Key='{internalValue}'.");
                return null;
            }
            return elem.AlternativeValuesForMapping;
        }
        public async Task AddCommentToTicket(CancellationToken ct = default)
        {
            var mcpClient = await GetMcpClientAsync(ct);
            var result = await mcpClient.CallToolAsync("ZohoDesk_createTicketComment",
            new Dictionary<string, object?>
            {
                ["path_variables"] = new Dictionary<string, string>
                {
                    ["ticketId"] = "235799000000360478" //potem będzie parametr
                },
                ["query_params"] = new Dictionary<string, object?>
                {
                    ["orgId"] = mcpOrgId
                },
                ["body"] = new Dictionary<string, object?>
                {
                    ["attachmentIds"] = { },
                    ["content"] = "To jest zawartość komentarza"
                }
            },
            cancellationToken: ct);
            foreach (var content in result.Content)
            {
                var json = JsonSerializer.Serialize(content);
                var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("text", out var textEl))
                {
                    string s = textEl.GetString() ?? "";
                    var d = JsonDocument.Parse(s);
                    var description = d.RootElement.TryGetProperty("description", out var descEl)
                        ? descEl.GetString() : "";
                    System.Diagnostics.Debug.WriteLine($"Description: {description}");
                }
            }
        }
        public async Task SendReply(CancellationToken ct = default)
        {
            var mcpClient = await GetMcpClientAsync(ct);
            var result = await mcpClient.CallToolAsync("ZohoDesk_sendReply",
            new Dictionary<string, object?>
            {
                ["path_variables"] = new Dictionary<string, string>
                {
                    ["ticketId"] = "235799000000360478" // potem będzie parametr
                },
                ["query_params"] = new Dictionary<string, object?>
                {
                    ["orgId"] = mcpOrgId
                },
                ["body"] = new Dictionary<string, object?>
                {
                    ["channel"] = "EMAIL",
                    ["fromEmailAddress"] = "support@example.zohodesk.eu",
                    ["content"] = "To jest mail wysłany dla testu integracji z ZohoDesk",
                    ["to"] = "support@example.com"
                }
            },
            cancellationToken: ct);
            foreach (var content in result.Content)
            {
                var json = JsonSerializer.Serialize(content);
                var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("text", out var textEl))
                {
                    string s = textEl.GetString() ?? "";
                    var d = JsonDocument.Parse(s);
                    var description = d.RootElement.TryGetProperty("description", out var descEl)
                        ? descEl.GetString() : "";
                    System.Diagnostics.Debug.WriteLine($"Description: {description}");
                }
            }
        }
        public async Task<ResultDTO<TaskDTO>> CreateTicket(TaskDTO task, IReadOnlyList<UsersDTO> users,
            int userId, List<string> rolesIds, CancellationToken ct = default)
        {
            var mcpClient = await GetMcpClientAsync(ct);

            // tą linijkę trzeba przetestować z kilkoma agentami z ZohoDesk
            var assigneeId = await GetAgentIdByEmail(
                users.FirstOrDefault(user => task.AssignedTo == user.Id)?.Email ?? string.Empty,
                string.Empty, ct);

            var dueDate = task.DueDate;
            string dueDateString = string.Empty;
            if (dueDate != null)
            {
                dueDateString = dueDate.Value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            }
            var body = new Dictionary<string, object?>
            {
                ["subject"] = task.Title,
                ["departmentId"] = long.Parse(mcpDepartmentId!),
                ["status"] = "Open", // TODO: zmapować status TaskDTO na status ZohoDesk
                ["contact"] = new Dictionary<string, object?>
                {
                    // TODO: podstawić rzeczywiste dane kontaktu zgłaszającego z TaskDTO
                    ["lastName"] = "Kowalski",
                    ["email"] = "example@gmail.com"
                },
                ["description"] = task.Description
            };

            if (!string.IsNullOrWhiteSpace(dueDateString))
                body["dueDate"] = dueDateString;
            if (!string.IsNullOrWhiteSpace(assigneeId))
                body["assigneeId"] = assigneeId;
            var channel = await MapSourceToExternal(task.Source, ct);
            if (!string.IsNullOrWhiteSpace(channel))
                body["channel"] = channel;
            var priority = await MapPriorityToExternal(task.Priority, ct);
            if (!string.IsNullOrWhiteSpace(priority))
                body["priority"] = priority;

            var result = await mcpClient.CallToolAsync("ZohoDesk_createTicket",
            new Dictionary<string, object?>
            {
                ["query_params"] = new Dictionary<string, object?>
                {
                    ["orgId"] = mcpOrgId
                },
                ["body"] = body
            },
            cancellationToken: ct);

            foreach (var content in result.Content)
            {
                var json = JsonSerializer.Serialize(content);
                var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("text", out var textEl))
                {
                    string s = textEl.GetString() ?? "";
                    var d = JsonElement.Parse(s);
                    var it = CreateImportedTask(d);
                    var importedTaskInDbId = await _tasksService.AddImportedTask(it);
                    await _tasksService.AssignImportedTaskToTask(task.Id, importedTaskInDbId, ct);
                }
            }
            return await _tasksService.GetByIdAsync(task.Id, userId, rolesIds, ct);
        }
        private ImportedTasks CreateImportedTask(JsonElement ticket)
        {
            return new ImportedTasks()
            {
                ExternalId = ticket.GetProperty("id").GetString() ?? string.Empty,
                Number = ticket.GetProperty("ticketNumber").GetString() ?? string.Empty,
                State = ticket.GetProperty("status").GetString() ?? string.Empty,
                Subject = ticket.GetProperty("subject").GetString() ?? string.Empty,
                Category = ticket.GetProperty("category").GetString() ?? null,
                Url = ticket.GetProperty("webUrl").GetString() ?? null,
                CreatedAt = ticket.TryGetProperty("createdTime", out JsonElement createdTime) && createdTime.ValueKind != JsonValueKind.Null
                                ? createdTime.GetDateTime().ToLocalTime() : DateTime.Now,
                DueDate = ticket.TryGetProperty("dueDate", out JsonElement dueDate) && dueDate.ValueKind != JsonValueKind.Null
                                ? dueDate.GetDateTime().ToLocalTime() : null,
                Channel = ticket.GetProperty("channel").GetString() ?? null,
                Priority = ticket.GetProperty("priority").GetString() ?? null,
                ResponseDueDate = ticket.TryGetProperty("responseDueDate", out JsonElement responseDueDate) && responseDueDate.ValueKind != JsonValueKind.Null
                                ? responseDueDate.GetDateTime() : null,
                ClosedAt = ticket.TryGetProperty("closedTime", out JsonElement closedTime) && closedTime.ValueKind != JsonValueKind.Null
                                ? closedTime.GetDateTime().ToLocalTime() : null,
                IsDeleted = false
            };
        }
        private class Contact
        {
            public string? FirstName{ get; set; }
            public string? LastName{ get; set; }
            public string? PhoneNumber { get; set; }
            public string? Email { get; set; }
        }
        private class Assignee
        {
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            public string? Email { get; set; }
        }
    }
}

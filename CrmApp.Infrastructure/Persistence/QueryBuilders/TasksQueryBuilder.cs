using Dapper;
using CrmApp.Domain.DTO.TasksDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Infrastructure.Persistence.QueryBuilders
{
    public class TasksQueryBuilder
    {
        private readonly TaskPagedRequest _req;
        private readonly List<int> _fieldIds;
        private readonly DynamicParameters _params = new();
        private readonly List<string> _joins = new();
        private readonly List<string> _wheres = new();

        public TasksQueryBuilder(
            TaskPagedRequest req,
            List<int> fieldIds)
        {
            _req = req;
            _fieldIds = fieldIds;
        }

        public (string dataSql, string countSql, DynamicParameters parameters) Build()
        {
            BuildJoins();
            BuildWheres();

            var select = BuildSelect();
            var from = BuildFrom();
            var where = _wheres.Count > 0
                ? "WHERE " + string.Join("\n  AND ", _wheres)
                : string.Empty;
            var orderBy = BuildOrderBy();
            var paging = $"OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            _params.Add("Offset", (_req.Page - 1) * _req.PageSize);
            _params.Add("PageSize", _req.PageSize);

            var dataSql = $"""
                {select}
                {from}
                {string.Join("\n", _joins)}
                {where}
                {orderBy}
                {paging}
                """;

            var countSql = $"""
                SELECT COUNT(DISTINCT t.Id)
                {from}
                {string.Join("\n", _joins)}
                {where}
                """;

            return (dataSql, countSql, _params);
        }

        private string BuildSelect()
        {
            var sb = new StringBuilder();
            sb.AppendLine("SELECT DISTINCT");
            sb.AppendLine(@" t.Id, t.TaskNumber, t.Title, t.Priority, t.State, t.Progress, t.DueDate,
                t.AssignedTo, CONCAT(au.FirstName, ' ', au.LastName) AS AssignedToFullName, t.CreatedAt,
                t.ContractorId, c.DisplayName AS ContractorName,   
                t.ProjectId, p.Name AS ProjectName,
                t.ContractorContactId, cc.DisplayName AS ContractorContactName,
                t.Description, t.Notes, it.Number AS ImportedTaskNumber, it.Url AS ImportedTaskUrl, t.IsDeleted
            ");

            foreach (var fieldId in _fieldIds)
            {
                sb.AppendLine($"""
                      ,(
                        SELECT TOP 1
                          COALESCE(de.Value, v.FieldValue)
                        FROM TablesAdditionalFieldsValues v
                        LEFT JOIN DictionariesElements de
                          ON de.Id = v.DictionaryElementId
                        WHERE v.TableName = 'Tasks'
                          AND v.RowId = CAST(t.Id AS NVARCHAR)
                          AND v.TableAdditionalFieldId = {fieldId}
                      ) AS [af_{fieldId}]
                    """);
            }

            var result = sb.ToString().TrimEnd();
            return result;
        }

        private string BuildFrom() => "FROM Tasks t";

        private void BuildJoins()
        {
            _joins.Add(@"LEFT JOIN dbo.UsersProfiles au ON au.Id = t.AssignedTo");
            _joins.Add(@"LEFT JOIN dbo.Contractors c ON c.Id = t.ContractorId");
            _joins.Add(@"LEFT JOIN dbo.ContractorContacts cc ON cc.Id = t.ContractorContactId");
            _joins.Add(@"LEFT JOIN dbo.Projects p ON p.Id = t.ProjectId");
            _joins.Add(@"LEFT JOIN dbo.ImportedTasks it ON it.Id = t.ImportedTaskId");

            if (_req.CustomFieldFilters?.Count > 0)
            {
                var idx = 0;
                foreach (var (fieldId, _) in _req.CustomFieldFilters)
                {
                    var alias = $"af_f{idx}";
                    _joins.Add($"""
                        LEFT JOIN TablesAdditionalFieldsValues {alias}
                          ON {alias}.TableName = 'Tasks'
                          AND {alias}.RowId = CAST(t.Id AS NVARCHAR)
                          AND {alias}.TableAdditionalFieldId = {fieldId}
                        """);
                    idx++;
                }
            }
        }

        private void BuildWheres()
        {
            if (!string.IsNullOrWhiteSpace(_req.Search))
            {
                _params.Add("Search", $"%{_req.Search.Trim()}%");
                _wheres.Add("""
                    (
                      t.TaskNumber  LIKE @Search OR
                      t.Title       LIKE @Search OR
                      t.Notes       LIKE @Search
                    )
                    """);
            }
            if (!string.IsNullOrWhiteSpace(_req.Title))
            {
                _params.Add("Title", $"%{_req.Title.Trim()}%");
                _wheres.Add("(t.Title LIKE @Title)");
            }
            if (!string.IsNullOrWhiteSpace(_req.TaskNumber))
            {
                _params.Add("TaskNumber", $"%{_req.TaskNumber.Trim()}%");
                _wheres.Add("(t.TaskNumber LIKE @TaskNumber)");
            }
            if (!string.IsNullOrWhiteSpace(_req.ImportedTaskNumber))
            {
                _params.Add("ImportedTaskNumber", $"%{_req.ImportedTaskNumber.Trim()}%");
                _wheres.Add("(it.Number LIKE @ImportedTaskNumber)");
            }
            if (!string.IsNullOrWhiteSpace(_req.Description))
            {
                _params.Add("Description", $"%{_req.Description.Trim()}%");
                _wheres.Add("(t.Description LIKE @Description)");
            }
            if (!string.IsNullOrWhiteSpace(_req.Notes))
            {
                _params.Add("Notes", $"%{_req.Notes.Trim()}%");
                _wheres.Add("(t.Notes LIKE @Notes)");
            }
            if (!string.IsNullOrWhiteSpace(_req.State))
            {
                _params.Add("State", $"%{_req.State.Trim()}%");
                _wheres.Add("(t.State LIKE @State)");
            }
            if (!string.IsNullOrWhiteSpace(_req.Priority))
            {
                _params.Add("Priority", $"%{_req.Priority.Trim()}%");
                _wheres.Add("(t.Priority LIKE @Priority)");
            }

            if (_req.AssignedTo.HasValue)
            {
                _params.Add("AssignedTo", _req.AssignedTo.Value);
                _wheres.Add("t.AssignedTo = @AssignedTo");
            }

            if (_req.ProjectId.HasValue)
            {
                _params.Add("ProjectId", _req.ProjectId.Value);
                _wheres.Add("t.ProjectId = @ProjectId");
            }

            if (_req.ContractorId.HasValue)
            {
                _params.Add("ContractorId", _req.ContractorId.Value);
                _wheres.Add("t.ContractorId = @ContractorId");
            }

            if (_req.ContractorContactId.HasValue)
            {
                _params.Add("ContractorContactId", _req.ContractorContactId.Value);
                _wheres.Add("t.ContractorContactId = @ContractorContactId");
            }

            if (_req.IsOverdue.HasValue)
            {
                _wheres.Add("t.DueDate <= GETDATE()");
            }

            if (_req.CustomFieldFilters?.Count > 0)
            {
                var idx = 0;
                foreach (var (fieldId, filterValue) in _req.CustomFieldFilters)
                {
                    if (string.IsNullOrWhiteSpace(filterValue)) { idx++; continue; }

                    var alias = $"af_f{idx}";
                    var paramName = $"afv_{idx}";

                    if (int.TryParse(filterValue, out var elemId))
                    {
                        _params.Add(paramName, elemId);
                        _wheres.Add($"{alias}.DictionaryElementId = @{paramName}");
                    }
                    else
                    {
                        _params.Add(paramName, $"%{filterValue}%");
                        _wheres.Add($"{alias}.FieldValue LIKE @{paramName}");
                    }

                    idx++;
                }
            }
        }

        private string BuildOrderBy()
        {
            var dir = _req.SortDescending ? "DESC" : "ASC";

            if (_req.SortBy?.StartsWith("af_") == true
             && int.TryParse(_req.SortBy[3..], out var fieldId))
            {
                return $"""
                    ORDER BY (
                      SELECT TOP 1
                        COALESCE(de.Value, v.FieldValue)
                      FROM TablesAdditionalFieldsValues v
                      LEFT JOIN DictionariesElements de
                        ON de.Id = v.DictionaryElementId
                      WHERE v.TableName = 'Tasks'
                        AND v.RowId = CAST(t.Id AS NVARCHAR)
                        AND v.TableAdditionalFieldId = {fieldId}
                    ) {dir}
                    """;
            }

            var column = _req.SortBy switch
            {
                "taskNumber" => "t.TaskNumber",
                "title" => "t.Title",
                "description" => "t.Description",
                "notes" => "t.Notes",
                "state" => "t.State",
                "priority" => "t.Priority",
                null => "t.CreatedAt",
                _ => "t.CreatedAt"
            };

            return $"ORDER BY {column} {dir}";
        }
    }
}

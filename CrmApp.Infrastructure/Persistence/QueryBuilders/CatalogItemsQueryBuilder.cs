using Dapper;
using CrmApp.Domain.DTO.CatalogItemsDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Infrastructure.Persistence.QueryBuilders
{
    public class CatalogItemsQueryBuilder
    {
        private readonly CatalogItemPagedRequest _req;
        private readonly List<int> _fieldIds;
        private readonly DynamicParameters _params = new();
        private readonly List<string> _joins = new();
        private readonly List<string> _wheres = new();

        public CatalogItemsQueryBuilder(
            CatalogItemPagedRequest req,
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
                SELECT COUNT(DISTINCT c.Id)
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
            sb.AppendLine(@"  c.Id, c.Name, c.Code, c.Description, c.Type, c.CategoryId, c.UnitName, c.BillingUnitName, 
                c.Price, c.VatRate, c.Currency, c.ParentItemId, c.TechnicalSupervisorId, c.ImplementationManagerId, c.IsActive ");

            foreach (var fieldId in _fieldIds)
            {
                sb.AppendLine($"""
                      ,(
                        SELECT TOP 1
                          COALESCE(de.Value, v.FieldValue)
                        FROM TablesAdditionalFieldsValues v
                        LEFT JOIN DictionariesElements de
                          ON de.Id = v.DictionaryElementId
                        WHERE v.TableName = 'CatalogItems'
                          AND v.RowId = CAST(c.Id AS NVARCHAR)
                          AND v.TableAdditionalFieldId = {fieldId}
                      ) AS [af_{fieldId}]
                    """);
            }

            var result = sb.ToString().TrimEnd();
            return result;
        }

        private string BuildFrom() => "FROM CatalogItems c";

        private void BuildJoins()
        {
            if (_req.CustomFieldFilters?.Count > 0)
            {
                var idx = 0;
                foreach (var (fieldId, _) in _req.CustomFieldFilters)
                {
                    var alias = $"af_f{idx}";
                    _joins.Add($"""
                        LEFT JOIN TablesAdditionalFieldsValues {alias}
                          ON {alias}.TableName = 'CatalogItems'
                          AND {alias}.RowId = CAST(c.Id AS NVARCHAR)
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
                      c.Name        LIKE @Search OR
                      c.Code        LIKE @Search OR
                      c.Description LIKE @Search
                    )
                    """);
            }
            if (!string.IsNullOrWhiteSpace(_req.Name))
            {
                _params.Add("Name", $"%{_req.Name.Trim()}%");
                _wheres.Add("(c.Name LIKE @Name)");
            }
            if (!string.IsNullOrWhiteSpace(_req.Code))
            {
                _params.Add("Code", $"%{_req.Code.Trim()}%");
                _wheres.Add("(c.Code LIKE @Code)");
            }

            if (_req.CategoryId.HasValue)
            {
                _params.Add("CategoryId", _req.CategoryId.Value);
                _wheres.Add("c.CategoryId = @CategoryId");
            }

            if (_req.Type.HasValue)
            {
                _params.Add("Type", _req.Type.Value);
                _wheres.Add("c.Type = @Type");
            }

            if (!_req.ShowNotActive) _wheres.Add("c.IsActive = 1");

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
                      WHERE v.TableName = 'CatalogItems'
                        AND v.RowId = CAST(c.Id AS NVARCHAR)
                        AND v.TableAdditionalFieldId = {fieldId}
                    ) {dir}
                    """;
            }

            var column = _req.SortBy switch
            {
                "name" => "c.Name",
                "code" => "c.Code",
                "description" => "c.Description",
                null => "c.Code",
                _ => "c.Code"
            };

            return $"ORDER BY {column} {dir}";
        }
    }
}

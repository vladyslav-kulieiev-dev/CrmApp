using Dapper;
using CrmApp.Domain.DTO.ContractorsDTO;
using System.Text;

namespace CrmApp.Infrastructure.Persistence.QueryBuilders
{
    public class ContractorsQueryBuilder
    {
        private readonly ContractorsPagedRequest _req;
        private readonly List<int> _fieldIds;
        private readonly DynamicParameters _params = new();
        private readonly List<string> _joins = new();
        private readonly List<string> _wheres = new();

        public ContractorsQueryBuilder(
            ContractorsPagedRequest req,
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
            sb.AppendLine("  c.Id, c.Name, c.Code, c.DisplayName, c.Nip, c.IsXopero,");

            sb.AppendLine("""
                  (
                    SELECT STRING_AGG(CAST(cc.EngagementType AS NVARCHAR), ',')
                    FROM ContractorContracts cc
                    WHERE cc.ContractorId = c.Id
                      AND cc.ValidFrom <= GETDATE()
                      AND (cc.ValidTo IS NULL OR cc.ValidTo > GETDATE())
                  ) AS ActiveEngagementTypes,
                """);

            sb.AppendLine("""
                  (
                    SELECT SUM(cc2.HoursLimit)
                    FROM ContractorContracts cc2
                    WHERE cc2.ContractorId = c.Id
                      AND cc2.EngagementType = 3
                      AND cc2.ValidFrom <= GETDATE()
                      AND (cc2.ValidTo IS NULL OR cc2.ValidTo > GETDATE())
                  ) AS HoursLimit,
                  (
                    SELECT SUM(hs.HoursUsed)
                    FROM ContractorHoursSnapshots hs
                    INNER JOIN ContractorContracts cc3
                      ON cc3.Id = hs.ContractorContractId
                    WHERE cc3.ContractorId = c.Id
                      AND cc3.EngagementType = 3
                      AND cc3.ValidFrom <= GETDATE()
                      AND (cc3.ValidTo IS NULL OR cc3.ValidTo > GETDATE())
                      AND hs.Id = (
                        SELECT TOP 1 Id FROM ContractorHoursSnapshots
                        WHERE ContractorContractId = cc3.Id
                        ORDER BY SnapshotDate DESC
                      )
                  ) AS HoursUsed,
                """);

            foreach (var fieldId in _fieldIds)
            {
                sb.AppendLine($"""
                      (
                        SELECT TOP 1
                          COALESCE(de.Value, v.FieldValue)
                        FROM TablesAdditionalFieldsValues v
                        LEFT JOIN DictionariesElements de
                          ON de.Id = v.DictionaryElementId
                        WHERE v.TableName = 'Contractors'
                          AND v.RowId = CAST(c.Id AS NVARCHAR)
                          AND v.TableAdditionalFieldId = {fieldId}
                      ) AS [af_{fieldId}],
                    """);
            }

            var result = sb.ToString().TrimEnd().TrimEnd(',');
            return result;
        }

        private string BuildFrom() => "FROM Contractors c";

        private void BuildJoins()
        {
            if (!string.IsNullOrWhiteSpace(_req.Search)
             || !string.IsNullOrWhiteSpace(_req.Nip))
            {
                _joins.Add("""
                    LEFT JOIN ContractorsNips cn
                      ON cn.ContractorId = c.Id
                    """);
            }

            if (_req.EngagementTypes?.Count > 0
             || _req.HasHoursRemaining.HasValue)
            {
                _joins.Add($"""
                    LEFT JOIN ContractorContracts cc_f
                      ON cc_f.ContractorId = c.Id
                      AND cc_f.ValidFrom <= GETDATE()
                      AND (cc_f.ValidTo IS NULL OR cc_f.ValidTo > GETDATE())
                    """);
            }

            if (_req.CustomFieldFilters?.Count > 0)
            {
                var idx = 0;
                foreach (var (fieldId, _) in _req.CustomFieldFilters)
                {
                    var alias = $"af_f{idx}";
                    _joins.Add($"""
                        LEFT JOIN TablesAdditionalFieldsValues {alias}
                          ON {alias}.TableName = 'Contractors'
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
                      c.DisplayName LIKE @Search OR
                      c.Nip         LIKE @Search OR
                      cn.Nip        LIKE @Search
                    )
                    """);
            }

            if (!string.IsNullOrWhiteSpace(_req.Name))
            {
                _params.Add("Name", $"%{_req.Name.Trim()}%");
                _wheres.Add("c.Name LIKE @Name OR c.DisplayName LIKE @Name");
            }

            if (!string.IsNullOrWhiteSpace(_req.Code))
            {
                _params.Add("Code", $"%{_req.Code.Trim()}%");
                _wheres.Add("c.Code LIKE @Code");
            }

            if (!string.IsNullOrWhiteSpace(_req.Nip))
            {
                var nip = _req.Nip.Replace("-", "").Replace(" ", "").Trim();
                _params.Add("Nip", $"%{nip.Trim()}%");
                _wheres.Add("""
                    (
                      REPLACE(REPLACE(c.Nip, '-', ''), ' ', '') LIKE @Nip OR
                      REPLACE(REPLACE(cn.Nip, '-', ''), ' ', '') LIKE @Nip
                    )
                    """);
            }

            if (_req.IsXopero.HasValue)
            {
                _params.Add("IsXopero", _req.IsXopero.Value);
                _wheres.Add("c.IsXopero = @IsXopero");
            }

            if (_req.EngagementTypes?.Count > 0)
            {
                _params.Add("EngagementTypes", _req.EngagementTypes);
                _wheres.Add("cc_f.EngagementType IN @EngagementTypes");
            }

            if (_req.HasHoursRemaining.HasValue)
            {
                if (_req.HasHoursRemaining.Value)
                    _wheres.Add("""
                        EXISTS (
                          SELECT 1 FROM ContractorContracts cc_h
                          WHERE cc_h.ContractorId = c.Id
                            AND cc_h.EngagementType = 3
                            AND cc_h.ValidFrom <= GETDATE()
                            AND (cc_h.ValidTo IS NULL OR cc_h.ValidTo > GETDATE())
                            AND cc_h.HoursLimit > ISNULL((
                              SELECT TOP 1 HoursUsed
                              FROM ContractorHoursSnapshots
                              WHERE ContractorContractId = cc_h.Id
                              ORDER BY SnapshotDate DESC
                            ), 0)
                        )
                        """);
                else
                    _wheres.Add("""
                        NOT EXISTS (
                          SELECT 1 FROM ContractorContracts cc_h
                          WHERE cc_h.ContractorId = c.Id
                            AND cc_h.EngagementType = 3
                            AND cc_h.ValidFrom <= GETDATE()
                            AND (cc_h.ValidTo IS NULL OR cc_h.ValidTo > GETDATE())
                        )
                        """);
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
                      WHERE v.TableName = 'Contractors'
                        AND v.RowId = CAST(c.Id AS NVARCHAR)
                        AND v.TableAdditionalFieldId = {fieldId}
                    ) {dir}
                    """;
            }

            var column = _req.SortBy switch
            {
                "name" => "c.Name",
                "code" => "c.Code",
                "nip" => "c.Nip",
                _ => "c.DisplayName"
            };

            return $"ORDER BY {column} {dir}";
        }
    }
}
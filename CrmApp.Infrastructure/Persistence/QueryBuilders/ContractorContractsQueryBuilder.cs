using Dapper;
using CrmApp.Domain.DTO.ContractorsDTO;
using System.Text;

namespace CrmApp.Infrastructure.Persistence.QueryBuilders
{
    public class ContractorContractsQueryBuilder
    {
        private readonly ContractorContractsRequest _req;
        private readonly DynamicParameters _params = new();
        private readonly List<string> _havingFilters = new();

        public ContractorContractsQueryBuilder(ContractorContractsRequest req)
        {
            _req = req;
        }

        public (string dataSql, string countSql, DynamicParameters parameters) Build()
        {
            BuildParams();
            BuildHavingFilters();

            var cte = BuildCte();
            var select = BuildSelect();
            var from = BuildFrom();
            var where = BuildWhere();
            var having = BuildHaving();
            var orderBy = BuildOrderBy();
            var paging = "OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            _params.Add("Offset", (_req.Page - 1) * _req.PageSize);
            _params.Add("PageSize", _req.PageSize);

            var dataSql = $"""
                {cte}
                {select}
                {from}
                {where}
                {having}
                {orderBy}
                {paging}
                """;

            var countSql = $"""
                {cte}
                SELECT COUNT(*)
                FROM ContractorContracts cc
                JOIN Contractors c ON c.Id = cc.ContractorId
                LEFT JOIN SnapOverall      so  ON so.ContractorContractId  = cc.Id AND so.rn  = 1
                LEFT JOIN SnapCurrentMonth scm ON scm.ContractorContractId = cc.Id AND scm.rn = 1
                LEFT JOIN SnapPrevMonth    spm ON spm.ContractorContractId = cc.Id AND spm.rn = 1
                {where}
                {having}
                """;

            return (dataSql, countSql, _params);
        }

        private static string BuildCte() => """
            WITH
            SnapOverall AS (
                SELECT
                    ContractorContractId,
                    HoursUsed,
                    HoursRemaining,
                    ROW_NUMBER() OVER (
                        PARTITION BY ContractorContractId
                        ORDER BY SnapshotDate DESC
                    ) AS rn
                FROM ContractorHoursSnapshots
            ),
            SnapCurrentMonth AS (
                SELECT
                    ContractorContractId,
                    HoursUsed,
                    ROW_NUMBER() OVER (
                        PARTITION BY ContractorContractId
                        ORDER BY SnapshotDate DESC
                    ) AS rn
                FROM ContractorHoursSnapshots
                WHERE YEAR(SnapshotDate)  = YEAR(GETDATE())
                  AND MONTH(SnapshotDate) = MONTH(GETDATE())
            ),
            SnapPrevMonth AS (
                SELECT
                    ContractorContractId,
                    HoursUsed,
                    ROW_NUMBER() OVER (
                        PARTITION BY ContractorContractId
                        ORDER BY SnapshotDate DESC
                    ) AS rn
                FROM ContractorHoursSnapshots
                WHERE YEAR(SnapshotDate)  = YEAR(DATEADD(MONTH, -1, GETDATE()))
                  AND MONTH(SnapshotDate) = MONTH(DATEADD(MONTH, -1, GETDATE()))
            )
            """;

        private string BuildSelect()
        {
            var sb = new StringBuilder();
            sb.AppendLine("SELECT");

            sb.AppendLine("""
                  cc.Id,
                  cc.ContractorId,
                  cc.ContractNumber,
                  cc.EngagementType,
                  cc.ValidFrom,
                  cc.ValidTo,
                  cc.HoursLimit,
                  cc.BillingType,
                  cc.BillingAmount,
                  cc.AllowOverLimit,
                  cc.RenewalType,
                  cc.RenewalDate,
                  cc.CreatedAt,
                  cc.CreatedBy,
                  cc.ModifiedAt,
                  cc.ModifiedBy,
                """);

            sb.AppendLine("""
                  c.Name AS ContractorName,
                  c.Code AS ContractorCode,
                """);

            sb.AppendLine("""
                  ISNULL(so.HoursUsed,      0) AS HoursUsed,
                  ISNULL(so.HoursRemaining, 0) AS HoursRemaining,

                  ISNULL(scm.HoursUsed, 0)                          AS HoursUsedCurrentMonth,
                  cc.HoursLimit - ISNULL(scm.HoursUsed, 0)          AS HoursRemainingCurrentMonth,

                  CASE
                      WHEN spm.HoursUsed IS NULL         THEN NULL
                      WHEN spm.HoursUsed > cc.HoursLimit THEN -(spm.HoursUsed - cc.HoursLimit)
                      ELSE 0
                  END AS HoursFromPrevMonth
                """);

            return sb.ToString();
        }

        private static string BuildFrom() => """
            FROM ContractorContracts cc
            JOIN Contractors c ON c.Id = cc.ContractorId
            LEFT JOIN SnapOverall      so  ON so.ContractorContractId  = cc.Id AND so.rn  = 1
            LEFT JOIN SnapCurrentMonth scm ON scm.ContractorContractId = cc.Id AND scm.rn = 1
            LEFT JOIN SnapPrevMonth    spm ON spm.ContractorContractId = cc.Id AND spm.rn = 1
            """;

        private string BuildWhere()
        {
            var conditions = new List<string>();

            conditions.Add("(@ContractorId IS NULL OR cc.ContractorId = @ContractorId)");

            if (_req.EngagementTypes?.Count > 0)
                conditions.Add("cc.EngagementType IN @EngagementTypes");

            if (_req.ActiveOnly == true)
                conditions.Add("""
                    (cc.ValidFrom <= GETDATE()
                     AND (cc.ValidTo IS NULL OR cc.ValidTo > GETDATE()))
                    """);

            return conditions.Count > 0
                ? "WHERE " + string.Join("\n  AND ", conditions)
                : string.Empty;
        }

        private void BuildHavingFilters()
        {
            if (_req.HasHoursDebt == true)
                _havingFilters.Add("""
                    CASE
                        WHEN spm.HoursUsed IS NULL         THEN NULL
                        WHEN spm.HoursUsed > cc.HoursLimit THEN -(spm.HoursUsed - cc.HoursLimit)
                        ELSE 0
                    END < 0
                    """);

            if (_req.IsOverLimitCurrentMonth == true)
                _havingFilters.Add("ISNULL(scm.HoursUsed, 0) > cc.HoursLimit");
        }

        private string BuildHaving() =>
            _havingFilters.Count > 0
                ? "HAVING " + string.Join("\n  AND ", _havingFilters)
                : string.Empty;

        private string BuildOrderBy()
        {
            var dir = _req.SortDescending ? "DESC" : "ASC";

            var column = _req.SortBy switch
            {
                "contractNumber" => "cc.ContractNumber",
                "contractorName" => "c.Name",
                "engagementType" => "cc.EngagementType",
                "validFrom" => "cc.ValidFrom",
                "validTo" => "cc.ValidTo",
                "hoursLimit" => "cc.HoursLimit",
                "hoursUsedCurrentMonth" => "ISNULL(scm.HoursUsed, 0)",
                "hoursRemainingCurrentMonth" => "cc.HoursLimit - ISNULL(scm.HoursUsed, 0)",
                "hoursFromPrevMonth" => "CASE WHEN spm.HoursUsed > cc.HoursLimit THEN -(spm.HoursUsed - cc.HoursLimit) ELSE 0 END",
                _ => "cc.ValidFrom"
            };

            return $"ORDER BY {column} {dir}, cc.Id {dir}";
        }

        private void BuildParams()
        {
            _params.Add("ContractorId", _req.ContractorId);
            _params.Add("EngagementTypes", _req.EngagementTypes);
        }
    }
}
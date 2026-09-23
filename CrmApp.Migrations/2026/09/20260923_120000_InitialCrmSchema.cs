using FluentMigrator;

namespace CrmApp.Migrations._2026._09
{
    [Migration(20260923120000)]
    public class _20260923_120000_InitialCrmSchema : ForwardOnlyMigration
    {
        public override void Up()
        {
            if (Schema.Table("Tasks").Exists())
                return;

            Execute.EmbeddedScript("20260923_120000_InitialCrmSchema.sql");
        }
    }
}

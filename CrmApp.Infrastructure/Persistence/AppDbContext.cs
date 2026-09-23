using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using CrmApp.Application.Abstractions.Persistence;
using CrmApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Infrastructure.Persistence
{
    public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext
    {
        public DbSet<CatalogItems> CatalogItems => Set<CatalogItems>();
        public DbSet<CatalogItemSupportedSystems> CatalogItemSupportedSystems => Set<CatalogItemSupportedSystems>();
        public DbSet<Contractors> Contractors => Set<Contractors>();
        public DbSet<ContractorsNips> ContractorsNips => Set<ContractorsNips>();
        public DbSet<ContractorContacts> ContractorContacts => Set<ContractorContacts>();
        public DbSet<ContractorContracts> ContractorContracts => Set<ContractorContracts>();
        public DbSet<ContractorHoursSnapshots> ContractorHoursSnapshots => Set<ContractorHoursSnapshots>();
        public DbSet<ContractorLicenses> ContractorLicenses => Set<ContractorLicenses>();
        public DbSet<ContractorLicenseHistory> ContractorLicenseHistory => Set<ContractorLicenseHistory>();
        public DbSet<Dictionaries> Dictionaries => Set<Dictionaries>();
        public DbSet<DictionariesElements> DictionariesElements => Set<DictionariesElements>();
        public DbSet<ImportedTasks> ImportedTasks => Set<ImportedTasks>();
        public DbSet<UsersProfiles> UsersProfiles => Set<UsersProfiles>();
        public DbSet<Projects> Projects => Set<Projects>();
        public DbSet<ProjectsMembers> ProjectsMembers => Set<ProjectsMembers>();
        public DbSet<Settings> Settings => Set<Settings>();
        public DbSet<SettingsValuesDictionary> SettingsValuesDictionary => Set<SettingsValuesDictionary>();
        public DbSet<TablesAdditionalFields> TablesAdditionalFields => Set<TablesAdditionalFields>();
        public DbSet<TablesAdditionalFieldsValues> TablesAdditionalFieldsValues => Set<TablesAdditionalFieldsValues>();
        public DbSet<TablesAdditionalFieldsPermissions> TablesAdditionalFieldsPermissions => Set<TablesAdditionalFieldsPermissions>();
        public DbSet<Tasks> Tasks => Set<Tasks>();
        public DbSet<TasksComments> TasksComments => Set<TasksComments>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            b.Entity<UsersProfiles>(e =>
            {
                e.ToTable("UsersProfiles");
                e.HasKey(x => x.Id);
                e.Property(x => x.UserId).HasMaxLength(450).IsRequired();
                e.Property(x => x.DisplayName).HasMaxLength(256).IsRequired();
                e.HasIndex(x => x.UserId).IsUnique();
            });

            b.Entity<CatalogItems>(e => e.HasKey(x => x.Id));
            b.Entity<Contractors>(e => e.HasKey(x => x.Id));
            b.Entity<ContractorContacts>(e => e.HasKey(x => x.Id));
            b.Entity<ContractorContracts>(e => e.HasKey(x => x.Id));
            b.Entity<ContractorHoursSnapshots>(e => e.HasKey(x => x.Id));
            b.Entity<ContractorLicenses>(e => e.HasKey(x => x.Id));
            b.Entity<ContractorLicenseHistory>(e => e.HasKey(x => x.Id));
            b.Entity<ContractorsNips>(e => e.HasKey(x => x.Id));
            b.Entity<Dictionaries>(e => e.HasKey(x => x.Id));
            b.Entity<DictionariesElements>(e => e.HasKey(x => x.Id));
            b.Entity<ImportedTasks>(e => e.HasKey(x => x.Id));
            b.Entity<Projects>(e => e.HasKey(x => x.Id));
            b.Entity<ProjectsMembers>(e => e.HasKey(x => x.Id));
            b.Entity<Settings>(e => e.HasKey(x => x.Id));
            b.Entity<SettingsValuesDictionary>(e => e.HasKey(x => x.Id));
            b.Entity<TablesAdditionalFields>(e => e.HasKey(x => x.Id));
            b.Entity<TablesAdditionalFieldsValues>(e => e.HasKey(x => x.Id));
            b.Entity<TablesAdditionalFieldsPermissions>(e => e.HasKey(x => x.Id));
            b.Entity<Tasks>(e => e.HasKey(x => x.Id));
            b.Entity<TasksComments>(e => e.HasKey(x => x.Id));

            b.Entity<TablesAdditionalFieldsPermissions>(entity =>
            {
                entity.HasIndex(e => e.TableAdditionalFieldId);
                entity.HasIndex(e => new { e.TableAdditionalFieldId, e.RoleId });
                entity.HasIndex(e => new { e.TableAdditionalFieldId, e.UserId });

                entity.ToTable(t => t.HasCheckConstraint(
                      "CK_Permissions_RoleOrUser",
                      "[RoleId] IS NOT NULL OR [UserId] IS NOT NULL"  
                  ));
            });
            b.Entity<TablesAdditionalFieldsPermissions>()
                .HasOne(p => p.TableAdditionalField)
                .WithMany(f => f.Permissions)
                .HasForeignKey(p => p.TableAdditionalFieldId)
                .OnDelete(DeleteBehavior.Cascade); ;

            b.Entity<TablesAdditionalFieldsValues>(entity =>
            {
                entity.HasIndex(e => new { e.TableName, e.RowId });
                entity.HasIndex(e => new { e.TableName, e.TableAdditionalFieldId });
            });
        }
    }
}

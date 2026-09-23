using Microsoft.EntityFrameworkCore;
using CrmApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Application.Abstractions.Persistence
{
    public interface IAppDbContext
    {
        public DbSet<CatalogItems> CatalogItems { get; }
        public DbSet<Contractors> Contractors { get; }
        public DbSet<ContractorsNips> ContractorsNips { get; }
        public DbSet<ContractorContacts> ContractorContacts { get; }
        public DbSet<ContractorContracts> ContractorContracts { get; }
        public DbSet<ContractorHoursSnapshots> ContractorHoursSnapshots { get; }
        public DbSet<ContractorLicenses> ContractorLicenses { get; }
        public DbSet<ContractorLicenseHistory> ContractorLicenseHistory { get; }
        public DbSet<Dictionaries> Dictionaries { get; }
        public DbSet<DictionariesElements> DictionariesElements { get; }
        public DbSet<ImportedTasks> ImportedTasks { get; }
        public DbSet<TablesAdditionalFields> TablesAdditionalFields { get; }
        public DbSet<TablesAdditionalFieldsValues> TablesAdditionalFieldsValues { get; }
        public DbSet<TablesAdditionalFieldsPermissions> TablesAdditionalFieldsPermissions { get; }
        public DbSet<Tasks> Tasks { get; }
        public DbSet<TasksComments> TasksComments { get; }
        public DbSet<UsersProfiles> UsersProfiles { get; }
        public DbSet<Projects> Projects { get; }
        public DbSet<ProjectsMembers> ProjectsMembers { get; }
        public DbSet<Settings> Settings { get; }
        public DbSet<SettingsValuesDictionary> SettingsValuesDictionary { get; }
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}

using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using CrmApp.Application.Abstractions.Persistence;
using CrmApp.Domain.Configuration;
using CrmApp.Domain.DTO.AdditionalFields;
using CrmApp.Domain.DTO.ContractorsDTO;
using CrmApp.Domain.DTO.Lists;
using CrmApp.Domain.DTO.TasksDTOs;
using CrmApp.Domain.Entities;
using CrmApp.Domain.Enums;
using CrmApp.Infrastructure.Persistence.Errors;
using CrmApp.Infrastructure.Persistence.QueryBuilders;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Infrastructure.Persistence.Repositories
{
    public sealed class TasksRepository : ITasksRepository
    {
        private readonly AppDbContext _db;
        public TasksRepository(AppDbContext db) => _db = db;

        public Task AddAsync(Tasks item, CancellationToken ct = default) =>
            _db.Tasks.AddAsync(item, ct).AsTask();

        public Task<Tasks?> GetAsync(int id, CancellationToken ct = default) =>
            _db.Tasks.FirstOrDefaultAsync(n => n.Id == id, ct);

        public Task<Tasks?> GetReadOnlyAsync(int id, CancellationToken ct = default) =>
            _db.Tasks.AsNoTracking()
            .Include(x => x.CreatedByUser)
            .Include(x => x.AssignedToUser)
            .Include(x => x.Contractor)
            .Include(x => x.ContractorContact)
            .FirstOrDefaultAsync(n => n.Id == id, ct);

        public async Task<int> GetNextNumberAsync(CancellationToken ct)
        {
            var lastNumber = await _db.Tasks.AsNoTracking()
                .Where(x => x.CreatedAt.Month == DateTime.Now.Month && x.CreatedAt.Year == DateTime.Now.Year)
                .Select(x => x.Autonumeration)
                .OrderByDescending(x => x)
                .FirstOrDefaultAsync(ct);
            return lastNumber + 1;
        }
        public async Task<Tasks?> GetTaskByImportedTaskId(int importedTaskId, CancellationToken ct) =>
            await _db.Tasks.AsNoTracking().Where(t => t.ImportedTaskId == importedTaskId).FirstOrDefaultAsync(ct);

        public async Task<Tasks?> GetTaskByNumber(int number, CancellationToken ct) => 
            await _db.Tasks.AsNoTracking().Where( t => t.Autonumeration == number).FirstOrDefaultAsync(ct);

        public async Task<IReadOnlyList<Tasks>> ListAllAsync(CancellationToken ct = default)
        {
            return await _db.Tasks
                .AsNoTracking()
                .OrderBy(n => n.CreatedAt)
                .ToListAsync(ct);
        }

        public Task Remove(Tasks item)
        {
            _db.Tasks.Remove(item);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Tasks item, CancellationToken ct = default)
        {
            _db.Tasks.Update(item);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken ct = default)
        {
            try { return _db.SaveChangesAsync(ct); }
            catch (DbUpdateException ex) { throw DbExceptionTranslator.Translate(ex); }
        }
        public async Task<IDbContextTransaction> BeginTransaction(CancellationToken ct = default)
        {
            try { return await _db.Database.BeginTransactionAsync(ct); }
            catch (SqlException ex) { throw DbExceptionTranslator.Translate(ex); }
            catch (DbException ex) { throw DbExceptionTranslator.Translate(ex); }
        }

        public async Task CommitTransaction(IDbContextTransaction transaction, CancellationToken ct = default)
        {
            try { await transaction.CommitAsync(ct); }
            catch (SqlException ex) { throw DbExceptionTranslator.Translate(ex); }
            catch (DbException ex) { throw DbExceptionTranslator.Translate(ex); }
        }

        public async Task RollbackTransaction(IDbContextTransaction transaction, CancellationToken ct = default)
        {
            try { await transaction.RollbackAsync(ct); }
            catch (SqlException ex) { throw DbExceptionTranslator.Translate(ex); }
            catch (DbException ex) { throw DbExceptionTranslator.Translate(ex); }
        }

        public async Task<PagedResult<TaskListItemDTO>> GetPagedAsync(
            TaskPagedRequest request, List<int> additionalFieldIds, CancellationToken ct = default)
        {
            var (dataSql, countSql, parameters) =
                new TasksQueryBuilder(request, additionalFieldIds).Build();

            await using var connection = new SqlConnection(
                _db.Database.GetConnectionString());

            await connection.OpenAsync(ct);
            var rawRows = (await connection.QueryAsync(dataSql, parameters)).ToList();
            var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

            var items = rawRows.Select(row =>
            {
                var dict = (IDictionary<string, object?>)row;
                
                return new TaskListItemDTO
                {
                    Id = (int)(dict["Id"] ?? "0"),
                    TaskNumber = (string)(dict["TaskNumber"] ?? ""),
                    Title = (string)(dict["Title"] ?? ""),
                    Priority = (string)(dict["Priority"] ?? ""),
                    State = (string)(dict["State"] ?? ""),
                    Progress = (int)(dict["Progress"] ?? "0"),
                    CreatedAt = dict["CreatedAt"] != null ? (DateTime)dict["CreatedAt"] : DateTime.Now,
                    DueDate = dict["DueDate"] as DateTime?,
                    AssignedTo = dict["AssignedTo"] as int?,
                    AssignedToFullName = dict["AssignedToFullName"] as string,
                    ContractorId = dict["ContractorId"] as int?,
                    ContractorName = dict["ContractorName"] as string,
                    ProjectId = dict["ProjectId"] as int?,
                    ProjectName = dict["ProjectName"] as string,
                    Description = dict["Description"] as string,
                    Notes = dict["Notes"] as string,
                    ImportedTaskNumber = dict["ImportedTaskNumber"] as string,
                    ImportedTaskUrl = dict["ImportedTaskUrl"] as string,
                    IsDeleted = (bool?)dict["IsDeleted"] ?? false,

                    AdditionalFieldValues = additionalFieldIds
                        .Select(fieldId => new AdditionalFieldValueDTO
                        {
                            TableAdditionalFieldId = fieldId,
                            DictionaryElementValue = dict.TryGetValue(
                                $"af_{fieldId}", out var v) ? v?.ToString() : null
                        })
                        .Where(v => v.DictionaryElementValue != null)
                        .ToList()
                };
            }).ToList();

            return new PagedResult<TaskListItemDTO>
            {
                Items = items,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }
    }
}

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using CrmApp.Application.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Infrastructure.Persistence.Errors
{
    public class DbExceptionTranslator
    {
        // Existing path for SaveChanges (DbUpdateException)
        public static Exception Translate(DbUpdateException ex)
        {
            if (ex is DbUpdateConcurrencyException)
                return new ConcurrencyException("Konflikt współbieżności.", ex);

            if (ex.InnerException is SqlException sql)
                return Translate(sql);

            return ex;
        }

        // New: translate raw SqlException (useful for transactions)
        public static Exception Translate(SqlException sql)
        {
            switch (sql.Number)
            {
                case 1205:
                    return new ConcurrencyException("Wykryto zakleszczenie (deadlock). Spróbuj ponowić operację.", sql);

                case 1222:
                    return new ConcurrencyException("Przekroczono czas oczekiwania na blokadę rekordu/tabeli.", sql);

                case -2:
                    return new TimeoutException("Przekroczono limit czasu polecenia SQL.", sql);

                case 2601:
                case 2627:
                    return new ConflictException("Naruszenie unikalności.", sql);

                case 547:
                    return new ConflictException("Naruszenie więzów integralności (klucz obcy).", sql);

                default:
                    return sql; // fallback: rethrow the original
            }
        }

        // New: catch-all for DbException (e.g., providers other than SQL Server)
        public static Exception Translate(DbException dbEx)
        {
            // SQL Server path
            if (dbEx is SqlException sql) return Translate(sql);

            // Fallback for unknown providers: keep the original exception
            return dbEx;
        }

        // Optional convenience to pass any Exception
        public static Exception Translate(Exception ex)
        {
            return ex switch
            {
                DbUpdateConcurrencyException => new ConcurrencyException("Konflikt współbieżności.", ex),
                DbUpdateException dbu => Translate(dbu),
                SqlException sql => Translate(sql),
                DbException db => Translate(db),
                _ => ex
            };
        }
    }
}

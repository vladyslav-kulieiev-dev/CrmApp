using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CrmApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Infrastructure.Identity
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UsersProfiles>(e =>
            {
                e.HasKey(p => p.Id);
                e.Property(p => p.DisplayName).HasMaxLength(256);
                e.HasIndex(p => p.UserId).IsUnique();
                e.Property(p => p.ForeignSystemType).HasConversion<int?>();
            });
        }
    }
}

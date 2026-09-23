using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Application.Abstractions;
public interface IIdentitySeeder
{
    Task SeedAsync(CancellationToken ct = default);
}
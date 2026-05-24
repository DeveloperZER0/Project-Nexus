using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Nexus.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<NexusDbContext>
{
    public NexusDbContext CreateDbContext(string[] args)
    {
        var dbPath = System.IO.Path.Combine(AppContext.BaseDirectory, "nexus.db");
        var optionsBuilder = new DbContextOptionsBuilder<NexusDbContext>();
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
        return new NexusDbContext(optionsBuilder.Options);
    }
}

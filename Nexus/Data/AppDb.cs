using System;
using Microsoft.EntityFrameworkCore;

namespace Nexus.Data;

public static class AppDb
{
    public static string DbPath => System.IO.Path.Combine(AppContext.BaseDirectory, "nexus.db");

    public static DbContextOptions<NexusDbContext> CreateOptions()
    {
        var optionsBuilder = new DbContextOptionsBuilder<NexusDbContext>();
        optionsBuilder.UseSqlite($"Data Source={DbPath}");
        return optionsBuilder.Options;
    }
}

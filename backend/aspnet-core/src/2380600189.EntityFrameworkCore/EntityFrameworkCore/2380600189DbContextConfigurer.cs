using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace TOEICReading.EntityFrameworkCore;

public static class TOEICReadingDbContextConfigurer
{
    public static void Configure(DbContextOptionsBuilder<TOEICReadingDbContext> builder, string connectionString)
    {
        builder.UseSqlServer(connectionString);
    }

    public static void Configure(DbContextOptionsBuilder<TOEICReadingDbContext> builder, DbConnection connection)
    {
        builder.UseSqlServer(connection);
    }
}

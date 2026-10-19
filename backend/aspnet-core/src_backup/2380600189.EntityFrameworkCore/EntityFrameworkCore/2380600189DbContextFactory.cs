using 2380600189.Configuration;
using 2380600189.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace 2380600189.EntityFrameworkCore;

/* This class is needed to run "dotnet ef ..." commands from command line on development. Not used anywhere else */
public class 2380600189DbContextFactory : IDesignTimeDbContextFactory<2380600189DbContext>
{
    public 2380600189DbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<2380600189DbContext>();

        /*
         You can provide an environmentName parameter to the AppConfigurations.Get method. 
         In this case, AppConfigurations will try to read appsettings.{environmentName}.json.
         Use Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") method or from string[] args to get environment if necessary.
         https://docs.microsoft.com/en-us/ef/core/cli/dbcontext-creation?tabs=dotnet-core-cli#args
         */
        var configuration = AppConfigurations.Get(WebContentDirectoryFinder.CalculateContentRootFolder());

        2380600189DbContextConfigurer.Configure(builder, configuration.GetConnectionString(2380600189Consts.ConnectionStringName));

        return new 2380600189DbContext(builder.Options);
    }
}

[assembly: Microsoft.Azure.Functions.Extensions.DependencyInjection.FunctionsStartup(typeof(FlexKids.AzureFunction.Startup))]

namespace FlexKids.AzureFunction
{
    using System.IO;
    using FlexKids.Core.Startup;
    using Microsoft.Azure.Functions.Extensions.DependencyInjection;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public class Startup : FunctionsStartup
    {
        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            FunctionsHostBuilderContext context = builder.GetContext();

            _ = builder.ConfigurationBuilder
                       .AddJsonFile(Path.Combine(context.ApplicationRootPath, "appsettings.json"), true, false)
                       .AddJsonFile(Path.Combine(context.ApplicationRootPath, $"appsettings.{context.EnvironmentName}.json"), true, false)
                       .AddEnvironmentVariables();

#if DEBUG
            if ("development".Equals(context.EnvironmentName, System.StringComparison.CurrentCultureIgnoreCase))
            {
                _ = builder.ConfigurationBuilder.AddUserSecrets<Startup>();
            }
#endif
        }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            IConfiguration configuration = builder.GetContext().Configuration;
            _ = builder.Services.AddLogging();
            _ = builder.Services.AddSingleton<IConfiguration>(configuration);
            _ = builder.Services.AddSingleton<Executor>();
            _ = builder.Services.AddSingleton<UpdateFlexKidsScheduleCommandAzureFunction>();
        }
    }
}
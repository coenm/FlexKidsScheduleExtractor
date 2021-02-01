[assembly: Microsoft.Azure.Functions.Extensions.DependencyInjection.FunctionsStartup(typeof(FlexKids.AzureFunction.Startup))]

namespace FlexKids.AzureFunction
{
    using System;
    using FlexKids.Core.Startup;
    using Microsoft.Azure.Functions.Extensions.DependencyInjection;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            FunctionsHostBuilderContext context = builder.GetContext();

            if ("development".Equals(context.EnvironmentName, StringComparison.CurrentCultureIgnoreCase))
            {
                var executor = new Executor(configurationBuilder => configurationBuilder.AddUserSecrets<Startup>());
                _ = builder.Services.AddSingleton<Executor>(executor);
            }
            else
            {
                _ = builder.Services.AddSingleton<Executor>(services => new Executor());
            }

            _ = builder.Services.AddSingleton<UpdateFlexKidsScheduleCommandAzureFunction>();
        }
    }
}
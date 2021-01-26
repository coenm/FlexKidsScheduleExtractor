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
            EnvironmentSetting environmentSetting;

            if ("development".Equals(context.EnvironmentName, StringComparison.CurrentCultureIgnoreCase))
            {
                environmentSetting = new EnvironmentSetting(
                    configurationBuilder => configurationBuilder.AddUserSecrets<Startup>());
            }
            else
            {
                environmentSetting = new EnvironmentSetting(_ => { });
            }

            _ = builder.Services.AddSingleton<Executor>(services => new Executor(services.GetService<EnvironmentSetting>().BuilderAction));
            _ = builder.Services.AddSingleton<EnvironmentSetting>(environmentSetting);
            _ = builder.Services.AddSingleton<UpdateFlexKidsScheduleCommandAzureFunction>();
        }
    }
}
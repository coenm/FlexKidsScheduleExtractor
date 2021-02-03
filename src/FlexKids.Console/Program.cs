namespace FlexKids.Console
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using FlexKids.Core.Commands;
    using FlexKids.Core.Startup;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Serilog;
    using Serilog.Core;
    using Serilog.Events;

    public class Program
    {
        protected Program()
        {
        }

        public static async Task Main()
        {
            IConfiguration builder = SetupConfiguration();
            ILoggerFactory loggerFactory = CreateLoggerFactory(builder);
            var executor = new Executor(builder, loggerFactory);

            using (executor)
            {
                await executor.ProcessAsync(new UpdateFlexKidsScheduleCommand(), CancellationToken.None);
            }

            Console.WriteLine("END");
            Console.WriteLine(DateTime.Now);
        }

        /// <summary>
        /// Determines the working environment as IHostingEnvironment is unavailable in a console application.
        /// </summary>
        /// <returns><c>true</c> When the <c>NETCORE_ENVIRONMENT</c>c variable equals <c>development</c>, <c>false</c> otherwise.</returns>
        private static bool IsDevelopment()
        {
            var environmentVariable = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT");
            return "development".Equals(environmentVariable, StringComparison.CurrentCultureIgnoreCase);
        }

        private static IConfiguration SetupConfiguration()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                                            .SetBasePath(Directory.GetCurrentDirectory())
                                            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                                            .AddJsonFile("logging.json", optional: true, reloadOnChange: false)
                                            .AddEnvironmentVariables();

            if (IsDevelopment())
            {
                _ = builder.AddUserSecrets<Program>();
            }

            return builder.Build();
        }

        private static ILoggerFactory CreateLoggerFactory(IConfiguration config)
        {
            ILoggerFactory loggerFactory = new LoggerFactory();

            // https://stackoverflow.com/questions/41243485/simple-injector-register-iloggert-by-using-iloggerfactory-createloggert
            LoggerConfiguration loggerConfiguration = new LoggerConfiguration()
                                                      .ReadFrom.Configuration(config)
                                                      .WriteTo.Console(LogEventLevel.Verbose)
                                                      /*.WriteTo.File()*/;

            Logger logger = loggerConfiguration.CreateLogger();

            _ = loggerFactory.AddSerilog(logger);

            return loggerFactory;
        }
    }
}
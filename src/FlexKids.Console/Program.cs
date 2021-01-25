namespace FlexKids.Console
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Mail;
    using System.Threading.Tasks;
    using FlexKids.Console.Configuration;
    using FlexKids.Core.FlexKidsClient;
    using FlexKids.Core.Interfaces;
    using FlexKids.Core.Parser;
    using FlexKids.Core.Repository;
    using FlexKids.Core.Scheduler;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Reporter.Email;
    using Reporter.GoogleCalendar;
    using Serilog;
    using Serilog.Events;
    using SimpleInjector;
    using SimpleInjector.Lifestyles;
    using ILogger = Microsoft.Extensions.Logging.ILogger;

    public class Program
    {
        private static readonly Container _container = new Container();
        private static IConfigurationRoot _config;
        private static ILogger<Program> _logger = NullLogger<Program>.Instance;

        protected Program()
        {
        }

        public static async Task Main()
        {
            _container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            _config = SetupConfiguration();

            ILoggerFactory loggerFactory = CreateLoggerFactory();
            _logger = loggerFactory.CreateLogger<Program>();

            SetupDependencyContainer(loggerFactory);

            try
            {
                _container.Verify();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Cannot verify the dependency injection container");
                return;
            }

            _logger.LogInformation("Dependencies registered");

            await using Scope scope = AsyncScopedLifestyle.BeginScope(_container);

            FlexKidsContext ctx = _container.GetInstance<FlexKidsContext>();
            _ = await ctx.Database.EnsureCreatedAsync();

            Scheduler scheduler = _container.GetInstance<Scheduler>();

            IReportScheduleChange[] allHandlers = _container.GetAllInstances<IReportScheduleChange>().ToArray();

            async Task DelegateScheduleChangedToReporters(object sender, ScheduleChangedEventArgs changedArgs)
            {
                foreach (IReportScheduleChange handler in allHandlers)
                {
                    var handlerType = handler.GetType().Name;
                    try
                    {
                        _logger.LogInformation($"Start handling using {handlerType}");
                        _ = await handler.HandleChange(changedArgs.Diff, changedArgs.UpdatedWeekSchedule);
                        _logger.LogInformation($"Done handling using {handlerType}");
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, $"Handling using {handlerType} failed.");
                    }
                }
            }

            scheduler.ScheduleChanged += DelegateScheduleChangedToReporters;
            _logger.LogInformation("Start scheduler");
            try
            {
                _ = await scheduler.ProcessAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                _logger.LogInformation("Finished scheduler");
                scheduler.ScheduleChanged -= DelegateScheduleChangedToReporters;
            }

            scheduler.Dispose();
            await _container.DisposeAsync();

            Console.WriteLine("END");
            Console.WriteLine(DateTime.Now);
        }

        private static IConfigurationRoot SetupConfiguration()
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

        /// <summary>
        /// Determines the working environment as IHostingEnvironment is unavailable in a console application.
        /// </summary>
        /// <returns><c>true</c> When the <c>NETCORE_ENVIRONMENT</c>c variable equals <c>development</c>, <c>false</c> otherwise.</returns>
        private static bool IsDevelopment()
        {
            var value = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT");
            return "development".Equals(value, StringComparison.CurrentCultureIgnoreCase);
        }

        private static void SetupDependencyContainer(ILoggerFactory loggerFactory)
        {
            RegisterSettings();
            RegisterLogging(loggerFactory);

            _container.Register<Scheduler>(Lifestyle.Scoped);
            _container.Register<IEmailService, EmailService>();
            _container.Register<IKseParser, FlexKidsHtmlParser>();
            RegisterFlexKidsConnection(_container);

            _container.RegisterSingleton<FlexKidsContext>();
            _container.RegisterSingleton<DbContextOptions<FlexKidsContext>>(
                () =>
                    {
                        var connectionString = _config.GetConnectionString("FlexKidsContext");
                        DbContextOptionsBuilder<FlexKidsContext> result = new DbContextOptionsBuilder<FlexKidsContext>()
                            .UseLoggerFactory(_container.GetInstance<ILoggerFactory>()).EnableSensitiveDataLogging()
                            .UseSqlServer(connectionString);

                        return result.Options;
                    });
            _container.Register<IScheduleRepository, EntityFrameworkScheduleRepository>();
            _container.Collection.Register<IReportScheduleChange>(
                typeof(EmailReportScheduleChange),
                typeof(CalendarReportScheduleChange));
        }

        private static ILoggerFactory CreateLoggerFactory()
        {
            /*
             ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
                _ = builder.AddFilter((category, filter) =>
                    category == DbLoggerCategory.Database.Command.Name
                    &&
                    filter == LogLevel.Information));
            */

            ILoggerFactory loggerFactory = new LoggerFactory();

            // https://stackoverflow.com/questions/41243485/simple-injector-register-iloggert-by-using-iloggerfactory-createloggert
            Serilog.Core.Logger loggerConfiguration = new LoggerConfiguration()
                                                      .ReadFrom.Configuration(_config)
                                                      .WriteTo.Console(LogEventLevel.Verbose)
                                                      /*.WriteTo.RollingFile()*/
                                                      .CreateLogger();

            _ = loggerFactory.AddSerilog(loggerConfiguration);

            return loggerFactory;
        }

        private static void RegisterLogging(ILoggerFactory loggerFactory)
        {
            _container.RegisterInstance<ILoggerFactory>(loggerFactory);
            _container.RegisterSingleton(typeof(ILogger<>), typeof(Logger<>));

            _container.RegisterConditional(
                typeof(ILogger),
                c => typeof(Logger<>).MakeGenericType(c.Consumer.ImplementationType),
                Lifestyle.Singleton,
                _ => true);
        }

        private static void RegisterSettings()
        {
            FlexKids flexKidsConfig = _config.GetSection("FlexKids").Get<FlexKids>();
            GoogleCalendar googleCalendarConfig = _config.GetSection("GoogleCalendar").Get<GoogleCalendar>();
            Smtp smtpConfig = _config.GetSection("SMTP").Get<Smtp>();
            NotificationSubscriptions notificationSubscriptions = _config.GetSection("NotificationSubscriptions").Get<NotificationSubscriptions>();

            var staticEmailServerConfig = new EmailServerConfig(
                smtpConfig.Host,
                smtpConfig.Port,
                smtpConfig.Username,
                smtpConfig.Password,
                smtpConfig.Secure);
            _container.RegisterInstance(staticEmailServerConfig);

            var staticGoogleCalendarConfig = new GoogleCalendarConfig(
                googleCalendarConfig.Account,
                googleCalendarConfig.CalendarId,
                System.Convert.FromBase64String(googleCalendarConfig.KeyFileContent));
            _container.RegisterInstance(staticGoogleCalendarConfig);

            var staticEmailConfig = new EmailConfig(
                new MailAddress(notificationSubscriptions.From.Email, "FlexKids rooster"),
                notificationSubscriptions.To.Select(x => new MailAddress(x.Email, x.Name)).ToArray());
            _container.RegisterInstance(staticEmailConfig);

            var staticFlexKidsHttpClientConfig = new FlexKidsHttpClientConfig(
                flexKidsConfig.Host,
                flexKidsConfig.Username,
                flexKidsConfig.Password);
            _container.RegisterInstance(staticFlexKidsHttpClientConfig);
        }

        private static void RegisterFlexKidsConnection(Container container)
        {
            // todo, fix this
            // https://github.com/simpleinjector/SimpleInjector/issues/668
            // https://github.com/simpleinjector/SimpleInjector/issues/654
            container.Register<HttpClient>(() => new HttpClient(), Lifestyle.Scoped);

            container.Register<IFlexKidsClient, HttpFlexKidsClient>(Lifestyle.Scoped);
            container.Register<WriteToDiskOptions>(() => new WriteToDiskOptions { Directory = $"C:\\temp\\{DateTime.Now:yyyyMMddHHmmss}", });
            container.RegisterDecorator(typeof(IFlexKidsClient), typeof(WriteToDiskFlexKidsClientDecorator), Lifestyle.Scoped);
        }
    }
}
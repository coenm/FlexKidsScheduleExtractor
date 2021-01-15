namespace FlexKids.Console
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using FlexKids.Console.Configuration;
    using FlexKidsConnection;
    using FlexKidsParser;
    using FlexKidsScheduler;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using NLog;
    using Reporter.Email;
    using Reporter.GoogleCalendar;
    using Repository;
    using Repository.EntityFramework;
    using SimpleInjector;
    using SimpleInjector.Lifestyles;

    public class Program
    {
        private static readonly Container _container = new Container();
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private static IConfigurationRoot _config;

        public static async Task Main()
        {
            _logger.Info("Starting.. ");

            _container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            IConfigurationBuilder builder = new ConfigurationBuilder()
                                            .SetBasePath(Directory.GetCurrentDirectory())
                                            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                                            .AddEnvironmentVariables();

            if (IsDevelopment())
            {
                _ = builder.AddUserSecrets<Program>();
            }

            _config = builder.Build();

            SetupDependencyContainer();

            try
            {
                _container.Verify();
            }
            catch (Exception e)
            {
                _logger.Error(e, "Cannot verify the dependency injection container");
                return;
            }

            _logger.Info("Dependencies registered");

            await using Scope scope = AsyncScopedLifestyle.BeginScope(_container);

            Scheduler scheduler = _container.GetInstance<Scheduler>();
            IEnumerable<IReportScheduleChange> allHandlers = _container.GetAllInstances<IReportScheduleChange>();
            scheduler.ScheduleChanged += (sender, changedArgs) =>
                {
                    foreach (IReportScheduleChange handler in allHandlers)
                    {
                        _ = handler.HandleChange(changedArgs.Diff);
                    }
                };

            _logger.Info("Start scheduler");
            _ = await scheduler.GetChanges();
            _logger.Info("Finished scheduler");

            scheduler.Dispose();

            Console.WriteLine("END");
            Console.WriteLine(DateTime.Now);
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

        private static void SetupDependencyContainer()
        {
            RegisterSettings(_container);

            _container.Register<Scheduler>(Lifestyle.Scoped);

            _container.RegisterInstance(Sha1Hash.Instance);
            _container.RegisterInstance(DateTimeProvider.Instance);

            _container.Register<IEmailService, EmailService>();
            _container.Register<IKseParser, FlexKidsHtmlParser>();
            RegisterFlexKidsConnection(_container);

            _container.RegisterSingleton<FlexKidsContext>();
            _container.RegisterSingleton<DbContextOptions<FlexKidsContext>>(
                () =>
                    {
                        var connectionString = _config.GetConnectionString("FlexKidsContext");
                        var result = new DbContextOptionsBuilder<FlexKidsContext>()
                            .UseSqlServer(connectionString);

                        return result.Options;
                    });
            _container.Register<IScheduleRepository, EntityFrameworkScheduleRepository>();
            _container.Collection.Register<IReportScheduleChange>(
                typeof(EmailReportScheduleChange),
                typeof(CalendarReportScheduleChange));
        }

        private static void RegisterSettings(Container container)
        {
            FlexKids flexKidsConfig = _config.GetSection("FlexKids").Get<FlexKids>();
            GoogleCalendar googleCalendarConfig = _config.GetSection("GoogleCalendar").Get<GoogleCalendar>();
            Smtp smtpConfig = _config.GetSection("Smtp").Get<Smtp>();
            NotificationSubscriptions notificationSubscriptions = _config.GetSection("NotificationSubscriptions").Get<NotificationSubscriptions>();

            var staticFlexKidsConfig = new StaticFlexKidsConfig(
                notificationSubscriptions.From.Email,
                notificationSubscriptions.To[1].Email,
                notificationSubscriptions.To[1].Name,
                notificationSubscriptions.To[0].Email,
                notificationSubscriptions.To[0].Name,
                smtpConfig.Host,
                smtpConfig.Port,
                smtpConfig.Username,
                smtpConfig.Password,
                googleCalendarConfig.Account,
                googleCalendarConfig.CalendarId,
                System.Convert.FromBase64String(googleCalendarConfig.KeyFileContent),
                smtpConfig.Secure);

            _container.RegisterInstance<IFlexKidsConfig>(staticFlexKidsConfig);

            var flexKidsCookieConfig = new FlexKidsCookieConfig(
                flexKidsConfig.Host,
                flexKidsConfig.Username,
                flexKidsConfig.Password);

            container.RegisterInstance(flexKidsCookieConfig);
        }

        private static void RegisterFlexKidsConnection(Container container)
        {
            container.Register<IWeb, WebClientAdapter>(Lifestyle.Scoped);
            container.Register<IFlexKidsConnection, FlexKidsCookieWebClient>(Lifestyle.Scoped);
            container.Register<WriteToDiskOptions>(() => new WriteToDiskOptions { Directory = $"C:\\temp\\{DateTime.Now:yyyyMMddHHmmss}", });
            container.RegisterDecorator(typeof(IFlexKidsConnection), typeof(WriteToDiskDecorator), Lifestyle.Scoped);
        }
    }
}
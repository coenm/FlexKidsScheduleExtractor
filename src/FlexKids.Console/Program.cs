namespace FlexKids.Console
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using FlexKids.Console.Configuration;
    using FlexKidsConnection;
    using FlexKidsParser;
    using FlexKidsScheduler;
    using Microsoft.Extensions.Configuration;
    using NLog;
    using Reporter.GoogleCalendar;
    using Repository;
    using SimpleInjector;
    using SimpleInjector.Lifestyles;

    public class Program
    {
        private static readonly Container _container = new Container();
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            _logger.Info("Starting.. ");

            _container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            // validate configurations.

            // AcceptAllCertificates();
            _logger.Info("Certificate validation disabled.");

            SetupDependencyContainer();
            try
            {
                _container.Verify();
            }
            catch (Exception e)
            {
                _logger.Error("Cannot verify the dependency injection container", e);
                return;
            }

            _logger.Info("Dependencies registered");

            using Scope scope = AsyncScopedLifestyle.BeginScope(_container);

            var scheduler = _container.GetInstance<Scheduler>();
            scheduler.ScheduleChanged += (_, changedArgs) =>
                {
                    IEnumerable<IReportScheduleChange> allHandlers = _container.GetAllInstances<IReportScheduleChange>();
                    foreach (IReportScheduleChange handler in allHandlers)
                    {
                        _ = handler.HandleChange(changedArgs.Diff);
                    }
                };

            _logger.Info("Start scheduler");
            _ = scheduler.GetChanges();
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

            _container.Register<IScheduleRepository, DummyScheduleRepository>();
            _container.Collection.Register<IReportScheduleChange>(typeof(CalendarReportScheduleChange));
            // typeof(EmailReportScheduleChange),
            // typeof(ConsoleReportScheduleChange),
        }

        private static void RegisterSettings(Container container)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                                            .SetBasePath(Directory.GetCurrentDirectory())
                                            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                                            .AddEnvironmentVariables();

            if (IsDevelopment())
            {
                _ = builder.AddUserSecrets<Program>();
            }

            IConfigurationRoot config = builder.Build();
            FlexKids flexKidsConfig = config.GetSection("FlexKids").Get<FlexKids>();
            GoogleCalendar googleCalendarConfig = config.GetSection("GoogleCalendar").Get<GoogleCalendar>();
            Smtp smtpConfig = config.GetSection("Smtp").Get<Smtp>();
            NotificationSubscriptions notificationSubscriptions = config.GetSection("NotificationSubscriptions").Get<NotificationSubscriptions>();

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
                googleCalendarConfig.KeyFileContent);

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
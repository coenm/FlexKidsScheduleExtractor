namespace FlexKids.Core.Startup
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Mail;
    using System.Threading;
    using System.Threading.Tasks;
    using FlexKids.Core.Commands;
    using FlexKids.Core.FlexKidsClient;
    using FlexKids.Core.Interfaces;
    using FlexKids.Core.Parser;
    using FlexKids.Core.Repository;
    using FlexKids.Core.Scheduler;
    using FlexKids.Core.Startup.Configuration;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Reporter.Email;
    using Reporter.GoogleCalendar;
    using SimpleInjector;
    using SimpleInjector.Lifestyles;

    public class Executor : IDisposable
    {
        private readonly Container _container = new Container();
        private readonly IConfiguration _config;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<Executor> _logger;

        public Executor(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _config = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = loggerFactory.CreateLogger<Executor>();

            _container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            try
            {
                SetupDependencyContainer();
                _container.Verify();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Cannot verify the dependency injection container");
                throw;
            }

            _logger.LogInformation("Dependencies registered");
        }

        public async Task ProcessAsync(ICommand command, CancellationToken ct)
        {
            _logger.LogInformation("Processing");
            await using Scope scope = AsyncScopedLifestyle.BeginScope(_container);

            Type type = typeof(ICommandHandler<>).MakeGenericType(command.GetType());

            if (_container.GetInstance(type) is not ICommandHandler handler)
            {
                throw new ApplicationException("Something is wrong. All typed ICommandHandler should also implement ICommandHandler.");
            }

            await handler.HandleAsync(command, ct);
        }

        public void Dispose()
        {
            _container.Dispose();
        }

        private void SetupDependencyContainer()
        {
            RegisterSettings();
            RegisterLogging();

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

            _container.Register(typeof(ICommandHandler<>), typeof(ICommandHandler<>).Assembly, typeof(Executor).Assembly);
        }

        private void RegisterLogging()
        {
            _container.RegisterInstance<ILoggerFactory>(_loggerFactory);
            _container.RegisterSingleton(typeof(ILogger<>), typeof(Logger<>));

            _container.RegisterConditional(
                typeof(ILogger),
                c => typeof(Logger<>).MakeGenericType(c.Consumer.ImplementationType),
                Lifestyle.Singleton,
                _ => true);
        }

        private void RegisterSettings()
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
                googleCalendarConfig.PrivateKey);
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

        private void RegisterFlexKidsConnection(Container container)
        {
            // https://github.com/simpleinjector/SimpleInjector/issues/668
            // https://github.com/simpleinjector/SimpleInjector/issues/654
            container.Register<HttpClient>(() => new HttpClient(), Lifestyle.Scoped);

            container.Register<IFlexKidsClient, HttpFlexKidsClient>(Lifestyle.Scoped);

            var configKey = "WriteToDiskEnabled";
            var isWriteToDiskEnabled = _config.GetValue<bool>(configKey);
            if (!isWriteToDiskEnabled)
            {
                _logger.LogTrace($"No {nameof(WriteToDiskFlexKidsClientDecorator)} enabled because of config '{configKey}'.");
                return;
            }

            configKey = "WriteToDiskPath";
            var writeToDiskPath = _config.GetValue<string>(configKey);
            if (string.IsNullOrWhiteSpace(writeToDiskPath))
            {
                _logger.LogTrace($"No {nameof(WriteToDiskFlexKidsClientDecorator)} enabled because of config '{configKey}'.");
                return;
            }

            container.Register<WriteToDiskOptions>(
                () => new WriteToDiskOptions
                    {
                        Directory = Path.Combine(writeToDiskPath, $"{DateTime.Now:yyyyMMddHHmmss}"),
                    },
                Lifestyle.Scoped);
            container.RegisterDecorator(typeof(IFlexKidsClient), typeof(WriteToDiskFlexKidsClientDecorator), Lifestyle.Scoped);
        }
    }
}

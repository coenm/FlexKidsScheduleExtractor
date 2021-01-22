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

        protected Program()
        {
        }

        public static async Task Main()
        {
            // _logger.Info("Starting.. ");

            _container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            IConfigurationBuilder builder = new ConfigurationBuilder()
                                            .SetBasePath(Directory.GetCurrentDirectory())
                                            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                                            .AddJsonFile("logging.json", optional: true, reloadOnChange: false)
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
                // _logger.Error(e, "Cannot verify the dependency injection container");
                return;
            }

            // _logger.Info("Dependencies registered");

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
                        // _logger.Info($"Start handling using {handlerType}");
                        _ = await handler.HandleChange(changedArgs.Diff);
                        // _logger.Info($"Done handling using {handlerType}");
                    }
                    catch (Exception e)
                    {
                        // _logger.Error(e, $"Handling using {handlerType} failed.");
                    }
                }
            }

            scheduler.ScheduleChanged += DelegateScheduleChangedToReporters;
            // _logger.Info("Start scheduler");
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
                // _logger.Info("Finished scheduler");
                scheduler.ScheduleChanged -= DelegateScheduleChangedToReporters;
            }

            scheduler.Dispose();
            await _container.DisposeAsync();

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
            RegisterSettings();
            RegisterLogging();

            _container.Register<Scheduler>(Lifestyle.Scoped);
            _container.RegisterInstance(Sha1Hash.Instance);
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

        private static void RegisterLogging()
        {
            // https://stackoverflow.com/questions/41243485/simple-injector-register-iloggert-by-using-iloggerfactory-createloggert
            ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
                _ = builder.AddFilter((category, filter) =>
                    category == DbLoggerCategory.Database.Command.Name
                    &&
                    filter == LogLevel.Information));

            // loggerFactory = new LoggerFactory();
            Serilog.Core.Logger loggerConfiguration = new LoggerConfiguration()
                                                      .ReadFrom.Configuration(_config)
                                                      .WriteTo.Console(LogEventLevel.Verbose)
                                                      //.WriteTo.RollingFile()
                                                      .CreateLogger();
            _ = loggerFactory.AddSerilog(loggerConfiguration);

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
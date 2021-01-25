namespace FlexKids.Core.Startup
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Mail;
    using System.Threading;
    using System.Threading.Tasks;
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
    using Serilog;
    using Serilog.Events;
    using SimpleInjector;
    using SimpleInjector.Lifestyles;
    using ILogger = Microsoft.Extensions.Logging.ILogger;

    public interface ICommand
    {
    }

    public class UpdateFlexKidsScheduleCommand : ICommand
    {
    }

    internal interface ICommandHandler<T>
        where T: ICommand
    {
        Task HandleAsync(T command, CancellationToken ct);
    }

    internal class UpdateFlexKidsScheduleCommandHandler : ICommandHandler<UpdateFlexKidsScheduleCommand>
    {
        public Task HandleAsync(UpdateFlexKidsScheduleCommand command, CancellationToken ct)
        {
            return Task.CompletedTask;
        }
    }

    public class Executor : IDisposable
    {
        private readonly Container _container = new Container();
        private readonly IConfigurationRoot _config;
        private readonly ILogger<Executor> _logger;

        public Executor(Action<IConfigurationBuilder> builderAction)
        {
            _container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            _config = SetupConfiguration(builderAction);

            ILoggerFactory loggerFactory = CreateLoggerFactory();
            _logger = loggerFactory.CreateLogger<Executor>();

            SetupDependencyContainer(loggerFactory);

            _container.Register(
                typeof(ICommandHandler<>),
                typeof(ICommandHandler<>).Assembly);

            try
            {
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
            await using Scope scope = AsyncScopedLifestyle.BeginScope(_container);

            Type type = typeof(ICommandHandler<>).MakeGenericType(command.GetType());
            var h = _container.GetInstance(type) as ICommandHandler<ICommand>;
            await h.HandleAsync(command, ct);

            // ICommandHandler<UpdateFlexKidsScheduleCommand> handler = _container.GetInstance<ICommandHandler<UpdateFlexKidsScheduleCommand>>();
            // await handler.HandleAsync(command, ct);
        }

        public async Task Main()
        {
            await using Scope scope = AsyncScopedLifestyle.BeginScope(_container);

            // FlexKidsContext ctx = _container.GetInstance<FlexKidsContext>();
            // _ = await ctx.Database.EnsureCreatedAsync();

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
                        _ = await handler.HandleChange(changedArgs.Diff);
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

        public void Dispose()
        {
            _container.Dispose();
        }

        private IConfigurationRoot SetupConfiguration(Action<IConfigurationBuilder> builderAction)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                                            .SetBasePath(Directory.GetCurrentDirectory())
                                            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                                            .AddJsonFile("logging.json", optional: true, reloadOnChange: false)
                                            .AddEnvironmentVariables();

            try
            {
                builderAction?.Invoke(builder);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Invoking builder action failed");
            }

            return builder.Build();
        }

        private void SetupDependencyContainer(ILoggerFactory loggerFactory)
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

        private ILoggerFactory CreateLoggerFactory()
        {
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

        private void RegisterLogging(ILoggerFactory loggerFactory)
        {
            _container.RegisterInstance<ILoggerFactory>(loggerFactory);
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

        private void RegisterFlexKidsConnection(Container container)
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
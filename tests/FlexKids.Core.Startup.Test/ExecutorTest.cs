namespace FlexKids.Core.Startup.Test
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Xunit;

    public class ExecutorTest
    {
        [Fact]
        public void VerifyContainer_ShouldNotThrow()
        {
            // arrange
            var settings = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("FlexKids:Host", "x"),
                    new KeyValuePair<string, string>("FlexKids:Username", "x"),
                    new KeyValuePair<string, string>("FlexKids:Password", "x"),
                    new KeyValuePair<string, string>("GoogleCalendar:CalendarId", "x"),
                    new KeyValuePair<string, string>("GoogleCalendar:Account", "x"),
                    new KeyValuePair<string, string>("GoogleCalendar:PrivateKey", "x"),
                    new KeyValuePair<string, string>("Smtp:Host", "x"),
                    new KeyValuePair<string, string>("Smtp:Port", "12"),
                    new KeyValuePair<string, string>("Smtp:Username", "x"),
                    new KeyValuePair<string, string>("Smtp:Secure", "true"),
                    new KeyValuePair<string, string>("NotificationSubscriptions:From:Name", "aa"),
                    new KeyValuePair<string, string>("NotificationSubscriptions:From:Email", "a@b.com"),
                    new KeyValuePair<string, string>("NotificationSubscriptions:To:0:Name", "bb"),
                    new KeyValuePair<string, string>("NotificationSubscriptions:To:0:Email", "b@c.com"),
                    new KeyValuePair<string, string>("ConnectionStrings:FlexKidsContext", "b@c.com"),
                };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();
            var sut = new TestableExecutor(configuration, new NullLoggerFactory());

            // act
            Action act = () => sut.VerifyContainer();

            // assert
            act.Should().NotThrow();
        }

        private class TestableExecutor : Executor
        {
            public TestableExecutor(IConfiguration configuration, ILoggerFactory loggerFactory)
                : base(configuration, loggerFactory)
            {
            }

            public void VerifyContainer()
            {
                _container.Verify();
            }
        }
    }
}
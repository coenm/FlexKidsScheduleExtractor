namespace FlexKids.Console
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using FlexKids.Core.Startup;
    using Microsoft.Extensions.Configuration;

    public class Program
    {
        protected Program()
        {
        }

        public static async Task Main()
        {
            Executor executor = IsDevelopment()
                ? new Executor(builder => _ = builder?.AddUserSecrets<Program>())
                : new Executor(_ => { });

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
    }
}
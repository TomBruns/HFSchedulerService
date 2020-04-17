using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Hangfire;
using Hangfire.MemoryStorage;

using FOS.Paymetric.POC.HFSchedulerService.Shared.Entities;

namespace FOS.Paymetric.POC.HFSchedulerService
{
    /// <summary>
    /// Main entry point for the executable
    /// </summary>
    // https://csharp.christiannagel.com/2019/10/15/windowsservice/
    // https://github.com/dotnet/AspNetCore.Docs/blob/master/aspnetcore/host-and-deploy/windows-service/samples/3.x/WebAppServiceSample/Program.cs
    public class Program
    {
        private static HangfireServiceConfigBE _hangfireConfig;

        public static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                                .SetBasePath(Directory.GetCurrentDirectory())
                                .AddJsonFile("appsettings.json", optional: false)
                                .Build();

            _hangfireConfig = config.GetSection("hangfireConfig").Get<HangfireServiceConfigBE>();

            CreateHostBuilder(args).Build().Run();
        }

        // set this up to both be:
        //  - installed as a windows service 
        //  - host ASP.NET so we expose the Hangfire Dashboard at http://localhost:5000/hangfire/
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls(BuildURLs(_hangfireConfig));
                })
                .UseWindowsService();

        private static string[] BuildURLs (HangfireServiceConfigBE hangfireConfig)
        {
            var urls = new List<string>();

            string protocol = hangfireConfig.IsUseSSL ? @"https" : "http";
            urls.Add($"{protocol}://localhost:{hangfireConfig.DashboardPortNumber}");

            if(hangfireConfig.IsDashboardRemoteAccessEnabled)
            {
                urls.Add($"{protocol}://{Environment.MachineName}:{hangfireConfig.DashboardPortNumber}");
            }

            return urls.ToArray();
        }
    }
}

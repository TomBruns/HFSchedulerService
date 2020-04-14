using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Hangfire;
using Hangfire.MemoryStorage;

namespace FOS.Paymetric.POC.HFSchedulerService
{
    // https://csharp.christiannagel.com/2019/10/15/windowsservice/
    // https://github.com/dotnet/AspNetCore.Docs/blob/master/aspnetcore/host-and-deploy/windows-service/samples/3.x/WebAppServiceSample/Program.cs
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        // set this up to both be:
        //  - installed as a windows service 
        //  - host ASP.NET so we expose the Hangfire Dashboard at http://localhost:5000/hangfire/
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                //.ConfigureServices((hostContext, services) =>
                //{
                //    services.AddHostedService<Worker>();
                //})
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .UseWindowsService();
    }
}

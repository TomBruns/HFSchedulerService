using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;

using Hangfire;
using Hangfire.Console;
using Hangfire.MemoryStorage;
using Serilog;
using Serilog.Extensions.Logging;
using Swashbuckle.AspNetCore.Filters;

using FOS.Paymetric.POC.HFSchedulerService.Shared.Entities;
using FOS.Paymetric.POC.HFSchedulerService.Shared.Interfaces;
using FOS.Paymetric.POC.HFSchedulerService.Shared;
using FOS.Paymetric.POC.HFSchedulerService.Logging;

using FOS.Paymetric.POC.HFSchedulerService.Managers;

namespace FOS.Paymetric.POC.HFSchedulerService
{
    public class Startup
    {
        // this is the subfolder where all the plug-ins will be loaded from
        private const int MAX_DEFAULT_WORKER_COUNT = 20;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }


        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // configure logging
            Log.Logger = new LoggerConfiguration()
                                .WriteTo.Sink(new HangfireConsoleSink())
                                //.WriteTo.Console()
                                .CreateLogger();

            //var logger = new SerilogLoggerProvider(Log.Logger).CreateLogger(nameof(Program));
            //services.AddSingleton(logger);

            services.AddControllers();

            var plugInsConfig = Configuration.GetSection("plugInsConfig").Get<PlugInsConfigBE>(); 

            // ==========================
            // load the hangfire config 
            //  NOTE:  We are jsut using the In-Memory storage for the POC
            //          A real implementation would use a DB backed store
            // ==========================
            var hangfireConfig = Configuration.GetSection("hangfireConfig").Get<HangfireServiceConfigBE>();

            services.AddHangfire(config =>
                config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseDefaultTypeSerializer()
                .UseMemoryStorage()     // config param PollIntervalInSecs does not apply to InMemory Storage
                .UseConsole());

            services.AddHangfireServer(config =>
                config.WorkerCount = hangfireConfig.WorkerCount != -1 ? hangfireConfig.WorkerCount : Math.Min(Environment.ProcessorCount * 5, MAX_DEFAULT_WORKER_COUNT));

            // =========================
            // config swagger
            // =========================
            services.AddMvc(c =>
            {
                c.Conventions.Add(new ApiExplorerGroupPerVersionConvention()); // decorate Controllers to distinguish SwaggerDoc (v1, v2, etc.)
            });

            // all of the entities with sample requests are in the current assembly
            //services.AddSwaggerExamplesFromAssemblyOf<RaftFindAuthXctRequestBE>();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Scheduler Service POC WebAPI",
                    Version = "v1",
                    Description = @"Documentation for Public WebAPI to administer Recurring Jobs is the Scheduler Service. 
### Technologies Leveraged:
* Hangfire as the scheduling engine.
* System.Composition to implement a Plug-In Model.
* Kafka to push tasks to subscribers.

### Important Endpoints:

| Endpoint | Desciption |
|----------|------------|
| `https://localhost:<port>/hangfire` | Hangfire Dashboard |
| `https://localhost:<port>/swagger` | Swagger website for recurring Job WebAPI   |

### Version History:

| Date| Version | Description |
|----------|----------|----------|
| 2020/04/14 | v1.0 | Initial Release |",
                    Contact = new OpenApiContact
                    {
                        Name = "US ESA Team",
                        Email = "tom.bruns@fisglobal.com",
                        Url = new Uri("https://www.fisglobal.com/"),
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Property of FIS Global",
                        Url = new Uri("https://www.fisglobal.com/"),
                    }
                });

                //c.ExampleFilters();
                c.EnableAnnotations();

                // Set the comments path for the Swagger JSON and UI for this assy
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

                // to avoid issue with BEs in two different namespaces that have the same class name
                c.CustomSchemaIds(i => i.FullName);
            });

            // ==========================
            // load the plug-in assys 
            // ==========================
            var plugInsManager = new PlugInsManager(plugInsConfig);
            services.AddSingleton(plugInsManager);

            // ==========================
            // load the kafka config that is available to all plug-ins
            // ==========================
            var kafkaConfig = Configuration.GetSection("kafkaConfig").Get<KafkaServiceConfigBE>();
            services.AddSingleton(kafkaConfig);

            // ==========================
            // on startup, inject the config info and the logger into all of the plug-ins
            // ==========================
            foreach (var plugin in plugInsManager.PlugIns)
            {
                plugin.Value.InjectConfig(kafkaConfig);
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Scheduler Service Public WebAPI v1");
            });

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            var dashboardOptions = new DashboardOptions()
            {

            };
            app.UseHangfireDashboard();
            app.UseHangfireServer(new BackgroundJobServerOptions { WorkerCount = 2 });
        }

        /// <summary>
        /// Implements Grouping in the Swagger UI using the version that is last part of the namespace
        /// </summary>
        /// <seealso cref="Microsoft.AspNetCore.Mvc.ApplicationModels.IControllerModelConvention" />
        private class ApiExplorerGroupPerVersionConvention : IControllerModelConvention
        {
            public void Apply(ControllerModel controller)
            {
                var controllerNamespace = controller.ControllerType.Namespace; // e.g. "Controllers.v1"
                var apiVersion = controllerNamespace?.Split('.').Last().ToLower();

                controller.ApiExplorer.GroupName = apiVersion;
            }
        }

    }
}

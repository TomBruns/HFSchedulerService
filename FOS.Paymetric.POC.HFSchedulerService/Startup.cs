using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
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
using Hangfire.MemoryStorage;
using Serilog;
using Serilog.Extensions.Logging;
using Swashbuckle.AspNetCore.Filters;

using FOS.Paymetric.POC.HFSchedulerService.Shared.Entities;
using FOS.Paymetric.POC.HFSchedulerService.Shared.Interfaces;
using FOS.Paymetric.POC.HFSchedulerService.Shared;
using FOS.Paymetric.POC.HFSchedulerService.Logging;
using Hangfire.Console;

namespace FOS.Paymetric.POC.HFSchedulerService
{
    public class Startup
    {
        // this is the subfolder where all the plug-ins will be loaded from
        private const string PLUGIN_FOLDER = @"Plugins";
        private const int MAX_DEFAULT_WORKER_COUNT = 20;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        // this collection holds the dynamically loaded assys   
        //  IEventPublisher is the common interface that all the sample plug-ins will implement
        //  MessageSenderType has a custom property that will allow us to pick a specific plug-in
        [ImportMany()]
        private static IEnumerable<Lazy<IJobPlugIn, JobPlugInType>> _scheduleTaskPlugIns { get; set; }

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
            Compose();
            services.AddSingleton(_scheduleTaskPlugIns);

            // ==========================
            // load the kafka config that is available to all plug-ins
            // ==========================
            var kafkaConfig = Configuration.GetSection("kafkaConfig").Get<KafkaServiceConfigBE>();
            services.AddSingleton(kafkaConfig);

            // ==========================
            // on startup, inject the config info and the logger into all of the plug-ins
            // ==========================
            foreach (var plugin in _scheduleTaskPlugIns)
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

        #region Helpers

        /// <summary>
        /// Build the composition host from assys that are dynamically loaded from a specific subfolder.
        /// </summary>
        private static void Compose()
        {
            // build the correct path to load the plug-in assys from
            var executableLocation = Assembly.GetEntryAssembly().Location;
            var path = Path.Combine(Path.GetDirectoryName(executableLocation), PLUGIN_FOLDER);

            // get a list of only the managed dlls
            var managedDlls = GetListOfManagedAssemblies(path, SearchOption.AllDirectories);

            // load the assys
            var assemblies = managedDlls
                        .Select(AssemblyLoadContext.Default.LoadFromAssemblyPath)
                        .ToList();

            // build a composition container
            var configuration = new ContainerConfiguration()
                        .WithAssemblies(assemblies);

            try
            {
                // load the plug-in assys that export the correct attribute
                using (var container = configuration.CreateContainer())
                {
                    _scheduleTaskPlugIns = container.GetExports<Lazy<IJobPlugIn, JobPlugInType>>();
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                StringBuilder sb = new StringBuilder();
                foreach (Exception exSub in ex.LoaderExceptions)
                {
                    sb.AppendLine(exSub.Message);
                    FileNotFoundException exFileNotFound = exSub as FileNotFoundException;
                    if (exFileNotFound != null)
                    {
                        if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                        {
                            sb.AppendLine("Fusion Log:");
                            sb.AppendLine(exFileNotFound.FusionLog);
                        }
                    }
                    sb.AppendLine();
                }
                string errorMessage = sb.ToString();
                Console.WriteLine(errorMessage);
            }
        }

        /// <summary>
        /// Gets the list of managed assemblies.
        /// </summary>
        /// <param name="folderPath">The folder path.</param>
        /// <param name="searchOption">The search option.</param>
        /// <returns>List&lt;System.String&gt;.</returns>
        /// <remarks>
        /// Some of the dlls in the target folder may be unmanaged dlls referenced by managed dlls, we need to exclude those
        /// </remarks>
        private static List<string> GetListOfManagedAssemblies(string folderPath, SearchOption searchOption)
        {
            List<string> assyPathNames = new List<string>();

            var files = Directory.GetFiles(folderPath, "*.dll", searchOption);

            foreach (var filePathName in files)
            {
                try
                {
                    // this call will throw an exception if this is not a managed dll
                    System.Reflection.AssemblyName testAssembly = System.Reflection.AssemblyName.GetAssemblyName(filePathName);

                    assyPathNames.Add(filePathName);
                }
                catch
                {
                    // swallow the exception and continue
                }
            }

            return assyPathNames;
        }

        /// <summary>
        /// A research spike into loading each plugin's assys into an isolated AssemblyLoadContext
        /// </summary>
        /// <param name="assysToIgnore">The assys to ignore.</param>
        /// <remarks>
        /// assysToIgnore is a list of assys NOT to load into the plug-in AssemblyLoadContext so the types will
        /// match and the a GetExports call will recognize them as the same type
        /// </remarks>
        private static void ComposeIsolated(List<string> assysToIgnore)
        {
            // build the correct path to load the plug-in assys from
            var executableLocation = Assembly.GetEntryAssembly().Location;
            var path = Path.Combine(Path.GetDirectoryName(executableLocation), PLUGIN_FOLDER);

            // find the names of all the plug-in subfolders
            var plugInFolderPathNames = Directory.GetDirectories(path);

            Dictionary<string, IEnumerable<Lazy<IJobPlugIn, JobPlugInType>>> test = new Dictionary<string, IEnumerable<Lazy<IJobPlugIn, JobPlugInType>>>();

            // loop thru each plug-in subfolder and load the assys into a separate (isolated) load context.
            foreach (var plugInFolderPathName in plugInFolderPathNames)
            {
                var plugInFolderName = Path.GetFileName(plugInFolderPathName);

                var assyLoadContext = new AssemblyLoadContext(plugInFolderName);

                // get a list of all the assys from that path
                var assemblies = Directory
                            .GetFiles(plugInFolderPathName, "*.dll", SearchOption.AllDirectories)
                            .Where(f => !f.Contains(assysToIgnore[0]))
                            .Select(assyLoadContext.LoadFromAssemblyPath)
                            .ToList();

                var configuration = new ContainerConfiguration()
                            .WithAssemblies(assemblies);

                // load the plug-in assys that export the correct attribute
                using (var container = configuration.CreateContainer())
                {
                    _scheduleTaskPlugIns = container.GetExports<Lazy<IJobPlugIn, JobPlugInType>>();
                }

                test.Add(plugInFolderName, _scheduleTaskPlugIns);

                _scheduleTaskPlugIns = (IEnumerable<Lazy<IJobPlugIn, JobPlugInType>>)test.Values.ToList();
            }
        }
        #endregion
    }
}

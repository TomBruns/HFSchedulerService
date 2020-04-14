using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
using Swashbuckle.AspNetCore.Filters;

namespace FOS.Paymetric.POC.HFSchedulerService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddHangfire(config =>
                config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseDefaultTypeSerializer()
                .UseMemoryStorage());

            services.AddHangfireServer();

            services.AddMvc(c =>
            {
                c.Conventions.Add(new ApiExplorerGroupPerVersionConvention()); // decorate Controllers to distinguish SwaggerDoc (v1, v2, etc.)
            });

            // all of the entities with sample requests are in the current assembly
            //services.AddSwaggerExamplesFromAssemblyOf<RaftFindAuthXctRequestBE>();

            // config swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Paymetric Scheduler Service POC WebAPI",
                    Version = "v1",
                    Description = @"Documentation for Public WebAPI that supports Paymetric Scheduler Service. 
This version leverages:
    Hangfire as the scheduling engine
    System.Composition to implement a Plug-In Model
    Kafka to push tasks to subscribers

- 2020/04/14 -v1.0 - Initial Release
                    ",
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

                // to avoid issue with BEs in two different namespaces have the same class name
                c.CustomSchemaIds(i => i.FullName);
            });
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

            app.UseHangfireDashboard();
            app.UseHangfireServer(new BackgroundJobServerOptions { WorkerCount = 2 });
        }

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

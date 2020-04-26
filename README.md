# [HFSchedulerService](https://github.com/TomBruns/HFSchedulerService)

This is a POC of a scheduler service using Hangfire and System.Composition.

---
## Background

This POC is implemented as a Windows Service (that can also be run as a console app) that hosts:
* An instance of Hangfire to schedule and execute background tasks.
* The Hangfire Web Dashboard.
* A WebAPI instance to administer the scheduled recurring jobs.

All of the scheduled tasks are packaged as `plug-ins`  that are dynamically loaded and invoked by the scheduler.  This implies there are NO static, compile-time dependencies between the scheduler and the tasks it executes. 

---
## Technologies Leveraged

|Technology | Description |
|---- | ------------ |
| Windows Service / Console App  | Hosting Process |
| [System.Composition](https://docs.microsoft.com/en-us/dotnet/api/system.composition?view=dotnet-plat-ext-3.1)  | Namespace containing classes to support creating a plug-in architecture|
| [Hangfire](https://www.hangfire.io/) | Open-source framework that helps you to create, process and manage your background jobs |
| [Swashbuckle.AspNetCore](https://www.nuget.org/packages/Swashbuckle.AspNetCore/) | Swagger tools for documenting APIs built on ASP.NET Core |
| [Serilog](https://serilog.net/) | Structured logging |

---
## Hangfire

Hangfire is an OpenSource .NET Core library similar to [Celery](http://www.celeryproject.org/) for python that can add support for background processing to .NET applications. 

In this POC we are leveraging it support recurring (scheduled) jobs.

![Hangfire](./images/hangfire.jpg?raw=true)

> **Note**: This POC used Hangfire's InMemory Storage option.  In real-world scenarios this should be replaced with a persistent datastore (ex. MSSQL Server)

> **Note**: Hangfire is horizontally scaleable by running additional Windows Service instances.  Typically these would be executed on different physical or virtual servers to avoid processer thread contention with this instance.

---
## Solution Structure

![Solution Structure](./images/solutionStructure2.jpg?raw=true)

---
## Steps to compile and run

| step | Description |
| ---- | ----------- |
| 1. Compile `FOS.Paymetric.POC.HFSchedulerService.Shared` |  This is the shared assembly.  A post build step copies this assembly to the `PlugInsShared` solution folder (Note: In real use this could be packaged as a nuget package. |
| 2a. `dotnet publish --runtime win-x64 --self-contained true` | run this command *in each plug-in project folder* to compile the plug-in (and gather all of its dependencies) |
| 2b. `dotnet build -target:CopyToStaging` | run this command *in each plug-in project folder* to copy the output to a subfolder in the `PlugInsStaging` solution folder |
| 3. Compile the `FOS.Paymetric.POC.HFSchedulerService` | Note: This is the hosting EXE.  A post-build step copies all of the plugins from the `PlugInsStaging` solution folder to the `plugins` folder in the `targetdir` |

---
## Useful links when running

Here are some useful link when running the POC locally

| Endpoint | Description |
|----------|------------|
| [https://localhost:5000/hangfire](https://localhost:5000/hangfire) | Hangfire Dashboard |
| [https://localhost:5000/swagger](https://localhost:5000/swagger)| Swagger website for recurring Job WebAPI   |

> **Note**: The HTTP port is set in the hosting exe's `appsettings.json` file 

---
## Hosting Process Config

The hosting process (EXE) appsettings.json file contains configration information for Hangfire and for Kafka.  Kafka can be configured centrally and is passed into each plug-in.

```json
{
  "kafkaConfig": {
    "bootstrapServers": "localhost:9092",
    "schemaRegistry": "localhost:8081"
  },
  "hangfireConfig": {
    "isUseSSL": true,
    "dashboardPortNumber": "5000",
    "isDashboardRemoteAccessEnabled": true,
    "workerCount": -1,
    "pollIntervalInSecs": 15
  },
  "AllowedHosts": "*"
}
```

---
## Implementing Scheduled Plug-In Jobs

Scheduled jobs all implement an interface (located in a shared assy) and are marked-up with a set of attributes that allow the runtime to find them.

> **Note**: The string associated with the ExportMetadata attribute must be globally unique and is used to identify a plug-in 

![Plugin](./images/plugIn.jpg?raw=true)

> **Note**: The plug-in assembly and its dependencies would typically be copied to a subfolder of the hosting EXE.  

Each plug-in supports its own private configuration that can be loaded from a local  appsettings.json file

![PlugIn Config](./images/pluginConfig.jpg?raw=true)

Plug-ins can write log messages directly to the Hangfire Console using standard methods on the ILogger interface that is passed in on the `Execute` method.

![Logging](./images/logging.jpg?raw=true)

---
## Scheduled Job Administration (WebAPI)

The scheduled jobs can be fully administered (add, remove, list) using a webAPI that includes a Swagger Website.

![Swagger](./images/swagger.jpg?raw=true)

Using the `POST` endpoint you can pass the data required to schedule a reoccurring task.

```json
{
  "job_id": "JOB001",
  "job_description": "Check for Invoices every minute",
  "job_plugin_type": "EventTypeA",
  "cron_schedule": "0-59 * * * MON,TUE,WED,THU,FRI",
  "schedule_time_zone": "EST"
}
```
> **Note**: This plug-in is uniquely identified as **EventTypeA** and will execute every minute of every hour Mon-Fri.  Schedules are defined using [CRON](https://en.wikipedia.org/wiki/Cron#CRON_expression) syntax.

---
## Hangfire Dashboard

The scheduled jobs can be viewed on the Hangfire Dashboard on the `Recurring Jobs` tab

![Swagger](./images/hangfireRecurringJobs.jpg?raw=true)

The scheduled jobs execution history can be viewed on the Hangfire Dashboard on the `Jobs` tab

![Swagger](./images/hangfireJob.jpg?raw=true)


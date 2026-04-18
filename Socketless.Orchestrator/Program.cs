using Azure.Identity;
using Azure.ResourceManager;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Socketless.Orchestrator.Interfaces;
using Socketless.Orchestrator.Services;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights()
    // TODO: How to register durable task client?!?
    .AddSingleton(_ => new ArmClient(new DefaultAzureCredential()))
    .AddSingleton<IResourceManager, AzureResourceManager>();

builder.Build().Run();

﻿using System;
using System.IO;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using NServiceBus;
using System.Threading.Tasks;
using NServiceBus.AzureFunctions.ServiceBus;
using NServiceBus.Configuration.AdvancedExtensibility;

public class AzureServiceBusTriggerFunction
{
    private const string EndpointName = "ASBTriggerQueue";

    #region Function

    [FunctionName(EndpointName)]
    public static async Task Run(
        [ServiceBusTrigger(queueName: EndpointName)]
        Message message,
        ILogger logger,
        ExecutionContext executionContext)
    {
        var paths = new
        {
            RelativeSearchPath = AppDomain.CurrentDomain.RelativeSearchPath,
            BaseDirectory = AppDomain.CurrentDomain.BaseDirectory,
            FunctionDirectory = executionContext.FunctionDirectory,
            FunctionAppDirectory = executionContext.FunctionAppDirectory,
            Bin = Path.Combine(executionContext.FunctionAppDirectory, "bin")
        };

        logger.LogWarning($">>><<<< {paths}");

        // var assembly = System.Reflection.Assembly.Load("Handlers, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
        await endpoint.Process(message, executionContext, logger);
    }

    #endregion

    #region EndpointSetup

    private static readonly FunctionEndpoint endpoint = new FunctionEndpoint(executionContext =>
    {
        var configuration = new ServiceBusTriggeredEndpointConfiguration(EndpointName);
        configuration.UseSerialization<NewtonsoftSerializer>();

        // optional: log startup diagnostics using Functions provided logger
        configuration.AdvancedConfiguration.CustomDiagnosticsWriter(diagnostics =>
        {
            executionContext.Logger.LogInformation(diagnostics);
            return Task.CompletedTask;
        });

        configuration.AdvancedConfiguration.AssemblyScanner().ScanAppDomainAssemblies = false;// >> we don't need to scan appdomain <<
        configuration.AdvancedConfiguration.GetSettings().Set("Functions", Path.Combine(executionContext.ExecutionContext.FunctionAppDirectory, "bin"));

        return configuration;
    });

    #endregion EndpointSetup

    #region AlternativeEndpointSetup

    private static readonly FunctionEndpoint autoConfiguredEndpoint = new FunctionEndpoint(executionContext =>
    {
        // endpoint name, logger, and connection strings are automatically derived from FunctionName and ServiceBusTrigger attributes
        var configuration = ServiceBusTriggeredEndpointConfiguration.FromAttributes();

        configuration.UseSerialization<NewtonsoftSerializer>();

        return configuration;
    });

    #endregion
}

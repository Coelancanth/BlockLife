using BlockLife.Core.Application.Behaviors;
using BlockLife.Core.Presentation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BlockLife.Core;

/// <summary>
/// The main bootstrapper for the C# application logic. It configures and initializes
/// the dependency injection container, including logging, MediatR pipelines, and all services.
/// </summary>
public static class GameStrapper
{
    public static IServiceProvider Initialize(
        LoggingLevelSwitch masterSwitch,
        IDictionary<string, LoggingLevelSwitch> categorySwitches,
        ILogEventSink godotConsoleSink,
        ILogEventSink? richTextSink = null)
    {
        var services = new ServiceCollection();
        
        // --- Logger Configuration ---
        var logger = ConfigureAndCreateLogger(masterSwitch, categorySwitches, godotConsoleSink, richTextSink);
        // This extension method correctly registers ILoggerFactory and ILogger<T> for Microsoft abstractions.
        services.AddLogging(builder => builder.AddSerilog(logger));
        // We also add the root logger directly to the container so our own code can resolve ILogger.
        services.AddSingleton<ILogger>(logger);

        // --- MediatR Pipeline Behaviors ---
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        // --- MediatR Registration ---
        var coreAssembly = typeof(GameStrapper).Assembly;
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(coreAssembly));
        
        // --- Register Other Core Services ---
        services.AddSingleton<IPresenterFactory, PresenterFactory>();

        // --- Register Presenters as Transient ---
        var presenterTypes = coreAssembly.GetTypes().Where(t =>
            !t.IsAbstract && t.BaseType is { IsGenericType: true } &&
            t.BaseType.GetGenericTypeDefinition() == typeof(PresenterBase<>));

        foreach (var type in presenterTypes)
        {
            services.AddTransient(type);
        }

        // --- Build Provider with Validation ---
        return services.BuildServiceProvider(new ServiceProviderOptions 
        { 
            ValidateOnBuild = true, 
            ValidateScopes = true 
        });
    }

    private static ILogger ConfigureAndCreateLogger(
        LoggingLevelSwitch masterSwitch,
        IDictionary<string, LoggingLevelSwitch> categorySwitches,
        ILogEventSink godotConsoleSink,
        ILogEventSink? richTextSink)
    {
        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.ControlledBy(masterSwitch)
            .Enrich.FromLogContext()
            .WriteTo.Sink(godotConsoleSink);

        foreach (var (category, levelSwitch) in categorySwitches)
        {
            loggerConfig.MinimumLevel.Override(category, levelSwitch);
        }

        if (richTextSink != null)
        {
            loggerConfig.WriteTo.Sink(richTextSink);
        }

        // Add file sink for release builds
        #if !DEBUG
        loggerConfig.WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day);
        #endif

        return loggerConfig.CreateLogger();
    }
}

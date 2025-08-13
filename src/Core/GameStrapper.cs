using BlockLife.Core.Application.Behaviors;
using BlockLife.Core.Infrastructure.Configuration;
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
        // Load environment variables from .env file
        EnvironmentLoader.LoadDotEnv();
        
        var services = new ServiceCollection();
        
        // --- Logger Configuration ---
        var logger = ConfigureAndCreateLogger(masterSwitch, categorySwitches, godotConsoleSink, richTextSink);
        // This extension method correctly registers ILoggerFactory and ILogger<T> for Microsoft abstractions.
        services.AddLogging(builder => builder.AddSerilog(logger));
        // We also add the root logger directly to the container so our own code can resolve ILogger.
        services.AddSingleton<ILogger>(logger);

        

        // --- MediatR Pipeline Behaviors ---
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        // --- MediatR Registration with License ---
        var coreAssembly = typeof(GameStrapper).Assembly;
        services.AddMediatR(cfg => 
        {
            cfg.RegisterServicesFromAssembly(coreAssembly);
            
            // Try to get license key from environment variable
            var licenseKey = Environment.GetEnvironmentVariable("MEDIATR_LICENSE_KEY");
            if (!string.IsNullOrEmpty(licenseKey))
            {
                cfg.LicenseKey = licenseKey;
            }
        });
        
        // --- Register Other Core Services ---
        services.AddSingleton<IPresenterFactory, PresenterFactory>();
        
        // --- Grid and Block Services ---
        services.AddSingleton<BlockLife.Core.Infrastructure.Services.IGridStateService, BlockLife.Core.Infrastructure.Services.GridStateService>();
        services.AddSingleton<BlockLife.Core.Domain.Block.IBlockRepository, BlockLife.Core.Infrastructure.Block.InMemoryBlockRepository>();
        
        // --- Validation Rules ---
        services.AddTransient<BlockLife.Core.Features.Block.Placement.Rules.IPositionIsValidRule, BlockLife.Core.Features.Block.Placement.Rules.PositionIsValidRule>();
        services.AddTransient<BlockLife.Core.Features.Block.Placement.Rules.IPositionIsEmptyRule, BlockLife.Core.Features.Block.Placement.Rules.PositionIsEmptyRule>();
        services.AddTransient<BlockLife.Core.Features.Block.Placement.Rules.IBlockExistsRule, BlockLife.Core.Features.Block.Placement.Rules.BlockExistsRule>();
        
        // Legacy rules (to be removed after migration)
        services.AddTransient<BlockLife.Core.Features.Block.Rules.PlacementValidationRule>();
        services.AddTransient<BlockLife.Core.Features.Block.Rules.RemovalValidationRule>();
        
        // --- Simulation Manager ---
        services.AddSingleton<BlockLife.Core.Application.Simulation.ISimulationManager, BlockLife.Core.Application.Simulation.SimulationManager>();
        
        // --- Block Management Services ---
        services.AddTransient<BlockLife.Core.Features.Block.Placement.PlaceBlockCommandHandler>();
        services.AddTransient<BlockLife.Core.Features.Block.Placement.RemoveBlockCommandHandler>();
        services.AddTransient<BlockLife.Core.Features.Block.Placement.RemoveBlockByIdCommandHandler>();
        
        // --- Notification Handlers ---
        // Bridge handlers to connect MediatR notifications to presenters
        services.AddTransient<BlockLife.Core.Features.Block.Placement.Effects.BlockPlacementNotificationBridge>();

        // Presenters are NOT registered in the DI container directly
        // They are created via PresenterFactory with view dependencies

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
        
        // Suppress MediatR license messages
        loggerConfig.MinimumLevel.Override("LuckyPennySoftware.MediatR.License", Serilog.Events.LogEventLevel.Fatal);

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

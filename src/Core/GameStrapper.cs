using BlockLife.Core.Application.Behaviors;
using BlockLife.Core.Infrastructure.Configuration;
using BlockLife.Core.Presentation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BlockLife.Core;

/// <summary>
/// **ENHANCED** bootstrapper for the C# application logic with comprehensive safety features.
/// 
/// Configures and initializes the dependency injection container with:
/// - Fallback logger that never crashes the application
/// - Validated DI container with early error detection  
/// - Complete MediatR pipeline with logging behavior
/// - All core services with proper lifetimes
/// 
/// CRITICAL SAFETY: This bootstrapper will never crash the application due to
/// logging failures and provides comprehensive diagnostics for debugging.
/// </summary>
public static class GameStrapper
{
    /// <summary>
    /// **NEW ENHANCED API** - Initialize with LogSettings resource for easier debugging.
    /// This is the recommended approach for new code.
    /// </summary>
    public static IServiceProvider Initialize(object logSettingsObj, ILogEventSink? richTextSink = null)
    {
        // Handle the LogSettings object (passed as object to avoid Godot dependency in Core)
        var logSettings = ExtractLogSettingsData(logSettingsObj);
        
        // Load environment variables from .env file
        EnvironmentLoader.LoadDotEnv();
        
        var services = new ServiceCollection();
        
        // --- CRITICAL: Fallback Logger Configuration with Safety ---
        var logger = ConfigureAndCreateLoggerSafely(logSettings.defaultLevel, logSettings.categoryLevels, 
            logSettings.enableRichText, logSettings.enableFileLogging, richTextSink);
        
        // Register logger in both Microsoft abstractions and Serilog patterns
        services.AddLogging(builder => builder.AddSerilog(logger));
        services.AddSingleton<ILogger>(logger);
        
        // --- MediatR Pipeline Behaviors (MUST be registered before MediatR) ---
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
        
        // Register all other services
        RegisterCoreServices(services);
        
        // --- CRITICAL: Build Provider with Validation ---
        // ValidateOnBuild ensures all dependencies can be resolved at startup
        // ValidateScopes prevents incorrect service lifetimes
        try
        {
            return services.BuildServiceProvider(new ServiceProviderOptions 
            { 
                ValidateOnBuild = true, 
                ValidateScopes = true 
            });
        }
        catch (Exception ex)
        {
            // Log the error and create a minimal service provider for diagnostics
            logger.Fatal(ex, "CRITICAL: Failed to build DI container with validation");
            
            // Create minimal service provider without validation as emergency fallback
            return services.BuildServiceProvider();
        }
    }

    /// <summary>
    /// **LEGACY API** - Maintains backwards compatibility with existing LoggingLevelSwitch approach.
    /// </summary>
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

    /// <summary>
    /// **NEW FALLBACK-SAFE LOGGER** - Never crashes the application if logging fails.
    /// </summary>
    private static ILogger ConfigureAndCreateLoggerSafely(
        LogEventLevel defaultLevel, 
        Dictionary<string, LogEventLevel> categoryLevels, 
        bool enableRichText, 
        bool enableFileLogging, 
        ILogEventSink? richTextSink)
    {
        try
        {
            // Primary logger configuration
            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Is(defaultLevel)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "BlockLife")
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}");

            // Apply category-specific levels
            if (categoryLevels != null)
            {
                foreach (var (category, level) in categoryLevels)
                {
                    loggerConfig.MinimumLevel.Override(category, level);
                }
            }

            // Add rich text sink if provided
            if (richTextSink != null)
            {
                loggerConfig.WriteTo.Sink(richTextSink);
            }

            // Always add fallback file logging for critical errors
            if (enableFileLogging)
            {
                try
                {
                    loggerConfig.WriteTo.File("logs/blocklife-.txt", 
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 7,
                        restrictedToMinimumLevel: LogEventLevel.Warning);
                }
                catch
                {
                    // File logging failure shouldn't crash the app
                }
            }
            
            // Suppress MediatR license messages
            loggerConfig.MinimumLevel.Override("LuckyPennySoftware.MediatR.License", LogEventLevel.Fatal);

            return loggerConfig.CreateLogger();
        }
        catch
        {
            // CRITICAL FALLBACK: Create minimal logger that never fails
            return new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .CreateLogger();
        }
    }

    /// <summary>
    /// Extracts data from LogSettings object without creating Godot dependency in Core.
    /// </summary>
    private static (LogEventLevel defaultLevel, Dictionary<string, LogEventLevel> categoryLevels, 
                   bool enableRichText, bool enableFileLogging) ExtractLogSettingsData(object logSettingsObj)
    {
        if (logSettingsObj == null)
        {
            return (LogEventLevel.Information, new Dictionary<string, LogEventLevel>(), true, true);
        }

        try
        {
            // Use reflection to extract data safely
            var type = logSettingsObj.GetType();
            
            var defaultLevel = (LogEventLevel)(type.GetProperty("DefaultLogLevel")?.GetValue(logSettingsObj) 
                ?? LogEventLevel.Information);
                
            var enableRichText = (bool)(type.GetProperty("EnableRichTextInGodot")?.GetValue(logSettingsObj) ?? true);
            
            var enableFileLogging = (bool)(type.GetProperty("EnableFileLogging")?.GetValue(logSettingsObj) ?? true);
            
            // Extract category levels using the helper method
            var getCategoryLevels = type.GetMethod("GetCategoryLogLevels");
            var categoryLevels = getCategoryLevels?.Invoke(logSettingsObj, null) as Dictionary<string, LogEventLevel>
                ?? new Dictionary<string, LogEventLevel>();

            return (defaultLevel, categoryLevels, enableRichText, enableFileLogging);
        }
        catch
        {
            // Safe fallback if reflection fails
            return (LogEventLevel.Information, new Dictionary<string, LogEventLevel>(), true, true);
        }
    }

    /// <summary>
    /// Registers all core services with proper lifetimes.
    /// Extracted for maintainability and testing.
    /// </summary>
    private static void RegisterCoreServices(IServiceCollection services)
    {
        // --- Register Core Services ---
        services.AddSingleton<IPresenterFactory, PresenterFactory>();
        
        // --- Grid and Block Services ---
        services.AddSingleton<BlockLife.Core.Infrastructure.Services.IGridStateService, 
            BlockLife.Core.Infrastructure.Services.GridStateService>();
        services.AddSingleton<BlockLife.Core.Domain.Block.IBlockRepository, 
            BlockLife.Core.Infrastructure.Block.InMemoryBlockRepository>();
        
        // --- Validation Rules ---
        services.AddTransient<BlockLife.Core.Features.Block.Placement.Rules.IPositionIsValidRule, 
            BlockLife.Core.Features.Block.Placement.Rules.PositionIsValidRule>();
        services.AddTransient<BlockLife.Core.Features.Block.Placement.Rules.IPositionIsEmptyRule, 
            BlockLife.Core.Features.Block.Placement.Rules.PositionIsEmptyRule>();
        services.AddTransient<BlockLife.Core.Features.Block.Placement.Rules.IBlockExistsRule, 
            BlockLife.Core.Features.Block.Placement.Rules.BlockExistsRule>();
        
        // Legacy rules (to be removed after migration)
        services.AddTransient<BlockLife.Core.Features.Block.Rules.PlacementValidationRule>();
        services.AddTransient<BlockLife.Core.Features.Block.Rules.RemovalValidationRule>();
        
        // --- Simulation Manager ---
        services.AddSingleton<BlockLife.Core.Application.Simulation.ISimulationManager, 
            BlockLife.Core.Application.Simulation.SimulationManager>();
        
        // --- Block Management Services ---
        services.AddTransient<BlockLife.Core.Features.Block.Placement.PlaceBlockCommandHandler>();
        services.AddTransient<BlockLife.Core.Features.Block.Placement.RemoveBlockCommandHandler>();
        services.AddTransient<BlockLife.Core.Features.Block.Placement.RemoveBlockByIdCommandHandler>();
        
        // --- Notification Handlers ---
        // Bridge handlers to connect MediatR notifications to presenters
        services.AddTransient<BlockLife.Core.Features.Block.Placement.Effects.BlockPlacementNotificationBridge>();

        // NOTE: Presenters are NOT registered in the DI container directly
        // They are created via PresenterFactory with view dependencies
    }
}

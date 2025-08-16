using Godot;
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using BlockLife.Core.Domain.Common;
using BlockLife.Core.Features.Block.Commands;
using BlockLife.Core.Features.Block.Placement;
using BlockLife.Core.Infrastructure.Services;
using BlockLife.Godot.Features.Block.Placement;
using BlockLife.Godot.Features.Block.Performance;
using BlockLife.Godot.Scenes;
using LanguageExt;
using static LanguageExt.Prelude;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace BlockLife.Godot.Features.Block.Input;

/// <summary>
/// Centralized input manager for all block-related operations.
/// Handles both block placement and movement with configurable key bindings.
/// Replaces click-to-place with key-triggered placement for better control.
/// </summary>
public partial class BlockInputManager : Node
{
    // Configurable key bindings
    [Export] public Key PlaceBlockKey { get; set; } = Key.Space;
    [Export] public Key InspectBlockKey { get; set; } = Key.I;
    [Export] public float DragThreshold { get; set; } = 5.0f;
    [Export] public float DoubleClickTime { get; set; } = 0.3f;
    
    // Drag & Drop state
    private bool _isDragging = false;
    private Option<Guid> _draggedBlockId = None;
    private Option<Vector2Int> _dragStartPosition = None;
    private Option<Vector2Int> _dragCurrentPosition = None;
    
    // Services
    private BlockManagementPresenter? _blockManagementPresenter;
    private IMediator? _mediator;
    private IGridStateService? _gridStateService;
    
    // Controllers
    private GridInteractionController? _interactionController;
    private BlockVisualizationController? _visualizationController;
    
    // Subscriptions
    private IDisposable? _cellClickedSubscription;
    private IDisposable? _cellHoveredSubscription;
    // TODO: Phase 2 - Add drag subscriptions
    // private IDisposable? _dragStartedSubscription;
    // private IDisposable? _dragMovedSubscription;
    // private IDisposable? _dragEndedSubscription;
    
    // Drag timing (for future double-click detection)
    private double _lastClickTime = 0.0;
    
    // Current hover position for placement and inspection
    private Option<Vector2Int> _currentHoverPosition = None;
    
    // Block selection state (for current click-based movement)
    private Option<Guid> _selectedBlockId = None;
    private Option<Vector2Int> _selectedBlockPosition = None;
    
    public override void _Ready()
    {
        var logger = GetNode<SceneRoot>("/root/SceneRoot")?.Logger?.ForContext("SourceContext", "UI");
        logger?.Information("BlockInputManager ready - Place: {PlaceKey}, Inspect: {InspectKey}", 
            PlaceBlockKey, InspectBlockKey);
        
        // Pre-warm Serilog message templates to prevent 282ms first-time delay
        PreWarmSerilogTemplates(logger);
        
        // Pre-warm Godot Tween subsystem to prevent 289ms first-time delay (BF_001)
        PreWarmGodotSubsystems();
        
        // V3: Pre-warm async/await machinery to prevent first-time initialization cost
        // BF_001 FIX: Run pre-warming synchronously to ensure it completes before any user input
        var preWarmTask = PreWarmAsyncAwaitMachinery();
        preWarmTask.Wait(500); // Allow up to 500ms for pre-warming to complete
        
        // Additional pre-warming: Trigger OnCellClicked with a dummy click to warm up the exact code path
        _ = Task.Run(async () =>
        {
            await Task.Delay(10); // Small delay to ensure UI is ready
            CallDeferred(nameof(PreWarmOnCellClicked));
        });
        
        // Get references to controllers
        var gridView = GetParent();
        if (gridView != null)
        {
            _interactionController = gridView.GetNodeOrNull<GridInteractionController>("InteractionController");
            _visualizationController = gridView.GetNodeOrNull<BlockVisualizationController>("VisualizationController");
            
            if (_interactionController != null)
            {
                // Subscribe to cell events for movement and hover tracking
                _cellClickedSubscription = _interactionController.GridCellClicked.Subscribe(OnCellClicked);
                _cellHoveredSubscription = _interactionController.GridCellHovered.Subscribe(OnCellHovered);
                
                // TODO: Subscribe to drag & drop events (Phase 2)
                // _dragStartedSubscription = _interactionController.DragStarted.Subscribe(OnDragStarted);
                // _dragMovedSubscription = _interactionController.DragMoved.Subscribe(OnDragMoved);
                // _dragEndedSubscription = _interactionController.DragEnded.Subscribe(OnDragEnded);
                
                logger?.Information("BlockInputManager subscribed to grid interaction events");
            }
            else
            {
                logger?.Warning("GridInteractionController not found - input will not work");
            }
            
            // Get the presenter from the parent GridView (no reflection needed!)
            if (gridView is GridView gv)
            {
                _blockManagementPresenter = gv.Presenter;
                
                if (_blockManagementPresenter != null)
                {
                    logger?.Information("Successfully obtained BlockManagementPresenter from GridView - no reflection!");
                }
            }
        }
        
        // Get services directly from SceneRoot (no reflection needed!)
        var sceneRoot = GetNode<SceneRoot>("/root/SceneRoot");
        if (sceneRoot?.ServiceProvider != null)
        {
            _mediator = sceneRoot.ServiceProvider.GetService<IMediator>();
            _gridStateService = sceneRoot.ServiceProvider.GetService<IGridStateService>();
            
            logger?.Information("BlockInputManager services initialized successfully - no reflection!");
        }
    }
    
    public override void _UnhandledKeyInput(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (keyEvent.Keycode == PlaceBlockKey)
            {
                HandlePlaceBlockKey();
                GetViewport().SetInputAsHandled();
            }
            else if (keyEvent.Keycode == InspectBlockKey)
            {
                HandleInspectBlockKey();
                GetViewport().SetInputAsHandled();
            }
        }
    }
    
    /// <summary>
    /// Handles the place block key press - places a block at the current hover position.
    /// </summary>
    private async void HandlePlaceBlockKey()
    {
        var logger = GetNode<SceneRoot>("/root/SceneRoot")?.Logger?.ForContext("SourceContext", "UI");
        
        if (_currentHoverPosition.IsSome)
        {
            var position = _currentHoverPosition.Match(Some: p => p, None: () => new Vector2Int(0, 0));
            logger?.Information("🔨 Placing block at {Position} via {Key} key", position, PlaceBlockKey);
            
            if (_mediator != null)
            {
                // Place a basic block at the hovered position
                var command = new PlaceBlockCommand(position, BlockLife.Core.Domain.Block.BlockType.Basic);
                var result = await _mediator.Send(command);
                
                result.Match(
                    Succ: _ =>
                    {
                        logger?.Information("✅ Block placed successfully at {Position}", position);
                    },
                    Fail: error =>
                    {
                        logger?.Warning("❌ Failed to place block at {Position}: {Error}", position, error.Message);
                    }
                );
            }
            else
            {
                logger?.Warning("Cannot place block - IMediator not available");
            }
        }
        else
        {
            logger?.Information("No position hovered - move cursor over grid to place block");
        }
    }
    
    /// <summary>
    /// Handles the inspect block key press - prints detailed information about the block under cursor.
    /// </summary>
    private void HandleInspectBlockKey()
    {
        var logger = GetNode<SceneRoot>("/root/SceneRoot")?.Logger?.ForContext("SourceContext", "UI");
        
        if (_currentHoverPosition.IsSome)
        {
            var position = _currentHoverPosition.Match(Some: p => p, None: () => new Vector2Int(0, 0));
            logger?.Information("🔍 Inspecting position {Position}", position);
            
            if (_gridStateService != null)
            {
                var blockAtPosition = _gridStateService.GetBlockAt(position);
                blockAtPosition.Match(
                    Some: block =>
                    {
                        // Get view information if available
                        var hasView = false;
                        var viewPosition = new Vector2(0, 0);
                        var isVisible = false;
                        
                        if (_visualizationController != null)
                        {
                            // Access block nodes directly (no reflection needed!)
                            if (_visualizationController.BlockNodes.TryGetValue(block.Id, out var node))
                            {
                                hasView = true;
                                viewPosition = node.Position;
                                isVisible = node.Visible;
                            }
                        }
                        
                        // Print comprehensive block information
                        logger?.Information("📋 BLOCK INFO at {Position}:", position);
                        logger?.Information("   BlockId: {BlockId}", block.Id);
                        logger?.Information("   Type: {BlockType}", block.Type);
                        logger?.Information("   Position: {Position}", block.Position);
                        logger?.Information("   CreatedAt: {CreatedAt}", block.CreatedAt);
                        logger?.Information("   LastModifiedAt: {LastModifiedAt}", block.LastModifiedAt);
                        logger?.Information("   Has View: {HasView}", hasView);
                        if (hasView)
                        {
                            logger?.Information("   View Position: ({X}, {Y})", viewPosition.X, viewPosition.Y);
                            logger?.Information("   View Visible: {IsVisible}", isVisible);
                        }
                        logger?.Information("   Is Occupied: True");
                        
                        // Also print to Godot console for easy copying
                        GD.Print($"=== BLOCK INSPECTION ===");
                        GD.Print($"Position: ({position.X}, {position.Y})");
                        GD.Print($"BlockId: {block.Id}");
                        GD.Print($"Type: {block.Type}");
                        GD.Print($"CreatedAt: {block.CreatedAt:yyyy-MM-dd HH:mm:ss}");
                        GD.Print($"LastModifiedAt: {block.LastModifiedAt:yyyy-MM-dd HH:mm:ss}");
                        GD.Print($"HasView: {hasView}");
                        if (hasView)
                        {
                            GD.Print($"ViewPosition: ({viewPosition.X:F1}, {viewPosition.Y:F1})");
                            GD.Print($"ViewVisible: {isVisible}");
                        }
                        GD.Print($"========================");
                    },
                    None: () =>
                    {
                        logger?.Information("📋 EMPTY POSITION at {Position}:", position);
                        logger?.Information("   No block present");
                        logger?.Information("   Is Valid Position: {IsValid}", _gridStateService.IsValidPosition(position));
                        
                        GD.Print($"=== POSITION INSPECTION ===");
                        GD.Print($"Position: ({position.X}, {position.Y})");
                        GD.Print($"Status: Empty");
                        GD.Print($"Valid: {_gridStateService.IsValidPosition(position)}");
                        GD.Print($"===========================");
                    }
                );
            }
            else
            {
                logger?.Warning("Cannot inspect - GridStateService not available");
            }
        }
        else
        {
            logger?.Information("No position hovered - move cursor over grid to inspect");
        }
    }
    
    /// <summary>
    /// Tracks mouse hover for placement and inspection targeting.
    /// </summary>
    private void OnCellHovered(Vector2Int position)
    {
        _currentHoverPosition = Some(position);
        
        // Optional: Show placement preview when hovering if no block is selected
        if (_selectedBlockId.IsNone && _blockManagementPresenter != null)
        {
            // Could add preview logic here later
        }
    }
    
    /// <summary>
    /// Handles cell clicks for block movement only (placement disabled).
    /// </summary>
    private async void OnCellClicked(Vector2Int position)
    {
        PerformanceProfiler.StartTimer("OnCellClicked_Total");
        
        // V3 INVESTIGATION: Track EVERYTHING
        PerformanceProfiler.StartTimer("V3_Method_Entry");
        var entryTime = DateTime.Now;
        PerformanceProfiler.StopTimer("V3_Method_Entry");
        
        // V3 CRITICAL TEST: Is it async/await machinery initialization?
        PerformanceProfiler.StartTimer("V3_FirstAwait_Test");
        await Task.CompletedTask; // Minimal await to trigger async state machine
        PerformanceProfiler.StopTimer("V3_FirstAwait_Test");
        
        // Phase 1 Instrumentation: Track setup operations timing
        PerformanceProfiler.StartTimer("OnCellClicked_Setup");
        var logger = GetNode<SceneRoot>("/root/SceneRoot")?.Logger?.ForContext("SourceContext", "UI");
        PerformanceProfiler.StopTimer("OnCellClicked_Setup");
        
        // Phase 1 Instrumentation: Track state checking operations
        PerformanceProfiler.StartTimer("OnCellClicked_StateCheck");
        var hasSelectedBlock = _selectedBlockId.IsSome;
        PerformanceProfiler.StopTimer("OnCellClicked_StateCheck");
        
        // V3: Track the await pattern match setup
        PerformanceProfiler.StartTimer("V3_PatternMatch_Setup");
        var selectedBlockIdCopy = _selectedBlockId; // Copy to avoid closure issues
        PerformanceProfiler.StopTimer("V3_PatternMatch_Setup");
        
        // Only handle movement logic - no placement via clicks
        await _selectedBlockId.Match(
            Some: async selectedId =>
            {
                // Phase 2 Instrumentation: Detailed move logic profiling
                PerformanceProfiler.StartTimer("OnCellClicked_MoveLogic");
                
                // INVESTIGATION V3: Add granular timing for every single operation
                PerformanceProfiler.StartTimer("V3_Check_HasSelectedBlock");
                var hasBlock = _selectedBlockPosition.IsSome;
                PerformanceProfiler.StopTimer("V3_Check_HasSelectedBlock");
                
                PerformanceProfiler.StartTimer("V3_Check_DifferentPosition");
                var isDifferentPosition = _selectedBlockPosition != Some(position);
                PerformanceProfiler.StopTimer("V3_Check_DifferentPosition");
                
                // We have a selected block - try to move it
                if (hasBlock && isDifferentPosition)
                {
                    PerformanceProfiler.StartTimer("V3_Match_SelectedPosition");
                    var fromPos = _selectedBlockPosition.Match(Some: p => p, None: () => position);
                    PerformanceProfiler.StopTimer("V3_Match_SelectedPosition");
                    
                    PerformanceProfiler.StartTimer("V3_Log_MoveAttempt");
                    logger?.Information("🔄 Moving block {BlockId} from {FromPosition} to {ToPosition}", 
                        selectedId, fromPos, position);
                    PerformanceProfiler.StopTimer("V3_Log_MoveAttempt");
                    
                    // CRITICAL: What happens RIGHT BEFORE MoveBlockAsync?
                    PerformanceProfiler.StartTimer("V3_PreMoveBlock_Call");
                    // Force any potential lazy initialization
                    var dummy = _mediator?.GetHashCode();
                    PerformanceProfiler.StopTimer("V3_PreMoveBlock_Call");
                    
                    PerformanceProfiler.StartTimer("MoveBlock_Pipeline");
                    await MoveBlockAsync(selectedId, position);
                    PerformanceProfiler.StopTimer("MoveBlock_Pipeline", true);
                    
                    // CRITICAL: What happens RIGHT AFTER MoveBlockAsync?
                    PerformanceProfiler.StartTimer("V3_PostMoveBlock_Return");
                    // Check if there's any deferred work
                    await Task.Yield(); // Force async continuation
                    PerformanceProfiler.StopTimer("V3_PostMoveBlock_Return");
                    
                    PerformanceProfiler.StartTimer("PostMove_ClearSelection");
                    await ClearSelectionAsync();
                    PerformanceProfiler.StopTimer("PostMove_ClearSelection", true);
                }
                else
                {
                    // Clicked same position - deselect
                    logger?.Debug("Deselecting block at {Position}", position);
                    
                    PerformanceProfiler.StartTimer("SamePosition_ClearSelection");
                    await ClearSelectionAsync();
                    PerformanceProfiler.StopTimer("SamePosition_ClearSelection", true);
                }
                
                PerformanceProfiler.StopTimer("OnCellClicked_MoveLogic", true);
            },
            None: async () =>
            {
                // Phase 1 Instrumentation: Track selection logic timing
                PerformanceProfiler.StartTimer("OnCellClicked_SelectionLogic");
                
                // No block selected - try to select block at clicked position
                PerformanceProfiler.StartTimer("GetBlockAtPosition");
                var blockAtPosition = await GetBlockAtPositionAsync(position);
                PerformanceProfiler.StopTimer("GetBlockAtPosition");
                
                PerformanceProfiler.StartTimer("OnCellClicked_BlockMatching");
                await blockAtPosition.Match(
                    Some: async blockId =>
                    {
                        logger?.Information("✋ Selected block {BlockId} at position {Position}", blockId, position);
                        
                        PerformanceProfiler.StartTimer("SelectBlockAsync");
                        await SelectBlockAsync(blockId, position);
                        PerformanceProfiler.StopTimer("SelectBlockAsync");
                    },
                    None: () =>
                    {
                        logger?.Debug("No block at position {Position} to select", position);
                        return Task.CompletedTask;
                    }
                );
                PerformanceProfiler.StopTimer("OnCellClicked_BlockMatching");
                
                PerformanceProfiler.StopTimer("OnCellClicked_SelectionLogic");
            }
        );
        
        // V3 CRITICAL: Track what happens AFTER the await completes
        PerformanceProfiler.StartTimer("V3_PostAwait_Completion");
        var exitTime = DateTime.Now;
        var totalTime = (exitTime - entryTime).TotalMilliseconds;
        logger?.Debug("V3_Timing: OnCellClicked total time={TotalMs}ms", totalTime);
        PerformanceProfiler.StopTimer("V3_PostAwait_Completion");
        
        PerformanceProfiler.StopTimer("OnCellClicked_Total", true);
    }
    
    private async Task SelectBlockAsync(Guid blockId, Vector2Int position)
    {
        _selectedBlockId = Some(blockId);
        _selectedBlockPosition = Some(position);
        
        var logger = GetNode<SceneRoot>("/root/SceneRoot")?.Logger?.ForContext("SourceContext", "UI");
        logger?.Information("Block {BlockId} selected for movement at position {Position}", blockId, position);
        
        await Task.CompletedTask;
    }
    
    private async Task ClearSelectionAsync()
    {
        _selectedBlockId = None;
        _selectedBlockPosition = None;
        
        var logger = GetNode<SceneRoot>("/root/SceneRoot")?.Logger?.ForContext("SourceContext", "UI");
        logger?.Debug("Block selection cleared");
        
        await Task.CompletedTask;
    }
    
    private async Task MoveBlockAsync(Guid blockId, Vector2Int toPosition)
    {
        if (_mediator == null)
        {
            var logger = GetNode<SceneRoot>("/root/SceneRoot")?.Logger?.ForContext("SourceContext", "UI");
            logger?.Error("Cannot send MoveBlockCommand - IMediator not available");
            return;
        }
        
        var command = new MoveBlockCommand
        {
            BlockId = blockId,
            ToPosition = toPosition
        };
        
        var logger2 = GetNode<SceneRoot>("/root/SceneRoot")?.Logger?.ForContext("SourceContext", "UI");
        logger2?.Information("Sending MoveBlockCommand for block {BlockId} to position {ToPosition}", 
            blockId, toPosition);
        
        try
        {
            PerformanceProfiler.StartTimer("Mediator_Send_MoveBlock");
            var result = await _mediator.Send(command);
            PerformanceProfiler.StopTimer("Mediator_Send_MoveBlock", true);
            
            // Phase 2: Detailed result processing profiling
            PerformanceProfiler.StartTimer("MoveBlock_ResultProcessing");
            
            PerformanceProfiler.StartTimer("MoveBlock_ResultMatch");
            await result.Match(
                Succ: _ =>
                {
                    PerformanceProfiler.StartTimer("MoveBlock_SuccessLogging");
                    logger2?.Information("✅ Successfully moved block {BlockId} to {ToPosition}", blockId, toPosition);
                    PerformanceProfiler.StopTimer("MoveBlock_SuccessLogging");
                    return Task.CompletedTask;
                },
                Fail: error =>
                {
                    PerformanceProfiler.StartTimer("MoveBlock_FailureLogging");
                    logger2?.Warning("❌ Failed to move block {BlockId}: {Error}", blockId, error.Message);
                    PerformanceProfiler.StopTimer("MoveBlock_FailureLogging");
                    return Task.CompletedTask;
                }
            );
            PerformanceProfiler.StopTimer("MoveBlock_ResultMatch");
            
            PerformanceProfiler.StopTimer("MoveBlock_ResultProcessing", true);
        }
        catch (Exception ex)
        {
            logger2?.Error(ex, "💥 Unexpected error moving block {BlockId}", blockId);
        }
    }
    
    private Task<Option<Guid>> GetBlockAtPositionAsync(Vector2Int position)
    {
        // Phase 1 Instrumentation: Track service access timing
        PerformanceProfiler.StartTimer("GetBlockAtPosition_ServiceCheck");
        var hasService = _gridStateService != null;
        PerformanceProfiler.StopTimer("GetBlockAtPosition_ServiceCheck");
        
        if (_gridStateService != null)
        {
            // Phase 1 Instrumentation: Track grid service call timing  
            PerformanceProfiler.StartTimer("GetBlockAtPosition_GridServiceCall");
            var blockAtPosition = _gridStateService.GetBlockAt(position);
            PerformanceProfiler.StopTimer("GetBlockAtPosition_GridServiceCall");
            
            // Phase 1 Instrumentation: Track mapping operation timing
            PerformanceProfiler.StartTimer("GetBlockAtPosition_Mapping");
            var result = Task.FromResult(blockAtPosition.Map(b => b.Id));
            PerformanceProfiler.StopTimer("GetBlockAtPosition_Mapping");
            
            return result;
        }
        
        return Task.FromResult(Option<Guid>.None);
    }
    
    /// <summary>
    /// Pre-compiles Serilog message templates to prevent 282ms delay on first move operation.
    /// This addresses BF_001 where first-time message template compilation caused significant lag.
    /// </summary>
    private void PreWarmSerilogTemplates(Serilog.ILogger? logger)
    {
        if (logger == null) return;
        
        try
        {
            // Pre-compile all expensive message templates used in move operations
            // These are silently discarded but force Serilog to compile the templates
            var preWarmLogger = logger.ForContext("PreWarm", true);
            
            // Pre-warm the move operation template (the one causing 282ms delay)
            preWarmLogger.Information("🔄 Moving block {BlockId} from {FromPosition} to {ToPosition}", 
                Guid.Empty, Vector2Int.Zero, Vector2Int.One);
            
            // Pre-warm other structured logging templates
            preWarmLogger.Information("✅ Successfully moved block {BlockId} to {ToPosition}", 
                Guid.Empty, Vector2Int.Zero);
            
            preWarmLogger.Information("✋ Selected block {BlockId} at position {Position}", 
                Guid.Empty, Vector2Int.Zero);
            
            preWarmLogger.Warning("❌ Failed to move block {BlockId}: {Error}", 
                Guid.Empty, "test");
            
            // Pre-warm inspect output templates
            preWarmLogger.Information("🔍 Inspecting position {Position}", Vector2Int.Zero);
            preWarmLogger.Information("📋 BLOCK INFO at {Position}:", Vector2Int.Zero);
            preWarmLogger.Information("   BlockId: {BlockId}", Guid.Empty);
            preWarmLogger.Information("   Type: {BlockType}", BlockLife.Core.Domain.Block.BlockType.Basic);
            preWarmLogger.Information("   Position: {Position}", Vector2Int.Zero);
            
            logger?.Debug("Serilog message templates pre-warmed successfully");
        }
        catch (Exception ex)
        {
            // Don't let pre-warming failures affect normal operation
            logger?.Warning(ex, "Failed to pre-warm Serilog templates, first operation may be slow");
        }
    }
    
    /// <summary>
    /// Pre-warms async/await state machine to prevent first-time initialization cost.
    /// V3 investigation for BF_001 - testing if async machinery causes 282ms delay.
    /// </summary>
    private async Task PreWarmAsyncAwaitMachinery()
    {
        var logger = GetNode<SceneRoot>("/root/SceneRoot")?.Logger?.ForContext("SourceContext", "UI");
        
        try
        {
            logger?.Information("Pre-warming async/await machinery...");
            
            // Time the first async operation
            PerformanceProfiler.StartTimer("PreWarm_AsyncAwait_First");
            await Task.CompletedTask; // Minimal async operation
            var firstTime = PerformanceProfiler.StopTimer("PreWarm_AsyncAwait_First");
            
            // BF_001 FIX: Pre-warm the EXACT async pattern that causes 271ms delay
            // The delay happens in the Option<T>.Match async lambda context
            PerformanceProfiler.StartTimer("PreWarm_OptionMatch_Async");
            var testOption = Some(new Vector2Int(0, 0));
            await testOption.Match(
                Some: async pos =>
                {
                    // Simulate the exact pattern from OnCellClicked line 333-334
                    var differentPos = testOption != Some(new Vector2Int(1, 1));
                    await Task.CompletedTask;
                },
                None: async () => await Task.CompletedTask
            );
            var optionMatchTime = PerformanceProfiler.StopTimer("PreWarm_OptionMatch_Async");
            
            // Also pre-warm Task.Delay which showed 268ms in initial logs
            PerformanceProfiler.StartTimer("PreWarm_AsyncAwait_Verify");
            await Task.Delay(1); // Small delay to ensure state machine runs
            var verifyTime = PerformanceProfiler.StopTimer("PreWarm_AsyncAwait_Verify");
            
            logger?.Information("✅ Async/await machinery pre-warmed (first: {FirstMs}ms, optionMatch: {OptionMs}ms, verify: {VerifyMs}ms)",
                firstTime, optionMatchTime, verifyTime);
            
            // If any operation took >100ms, we found the culprit!
            if (firstTime > 100 || optionMatchTime > 100 || verifyTime > 100)
            {
                logger?.Warning("🎯 FOUND IT! Async initialization delays - first: {FirstMs}ms, optionMatch: {OptionMs}ms, verify: {VerifyMs}ms", 
                    firstTime, optionMatchTime, verifyTime);
            }
        }
        catch (Exception ex)
        {
            logger?.Warning(ex, "Failed to pre-warm async/await machinery");
        }
    }
    
    /// <summary>
    /// Pre-warms the OnCellClicked code path to prevent first-click delay.
    /// BF_001 FIX: Exercises the exact async pattern that causes 271ms delay.
    /// </summary>
    private void PreWarmOnCellClicked()
    {
        var logger = GetNode<SceneRoot>("/root/SceneRoot")?.Logger?.ForContext("SourceContext", "UI");
        logger?.Debug("Pre-warming OnCellClicked code path...");
        
        try
        {
            // Simulate a click at an empty position to warm up the async machinery
            // This will go through the selection logic path without side effects
            // OnCellClicked is async void, so we can't await it, but that's fine for pre-warming
            OnCellClicked(new Vector2Int(-1, -1));
            
            logger?.Debug("OnCellClicked pre-warming triggered");
        }
        catch (Exception ex)
        {
            logger?.Debug(ex, "Pre-warming OnCellClicked failed (this is expected and safe)");
        }
    }
    
    /// <summary>
    /// Pre-warms Godot subsystems that have first-time initialization costs.
    /// Addresses BF_001 where Godot Tween system initialization caused 289ms delay.
    /// </summary>
    private void PreWarmGodotSubsystems()
    {
        var logger = GetNode<SceneRoot>("/root/SceneRoot")?.Logger?.ForContext("SourceContext", "UI");
        
        try
        {
            logger?.Information("Pre-warming Godot Tween subsystem...");
            
            // Create and immediately destroy a tween to trigger JIT compilation
            // This moves the ~289ms initialization cost to startup time
            PerformanceProfiler.StartTimer("PreWarm_Tween_FirstCreation");
            var warmupTween = CreateTween();
            PerformanceProfiler.StopTimer("PreWarm_Tween_FirstCreation", true);
            
            // Kill the tween immediately
            warmupTween.Kill();
            
            // Create a second tween to verify pre-warming worked
            PerformanceProfiler.StartTimer("PreWarm_Tween_Verification");
            var verifyTween = CreateTween();
            var verifyTime = PerformanceProfiler.StopTimer("PreWarm_Tween_Verification", true);
            verifyTween.Kill();
            
            // If verification takes >10ms, pre-warming may have failed
            if (verifyTime > 10)
            {
                logger?.Warning("Godot Tween pre-warming may have failed - verification took {Ms}ms", verifyTime);
            }
            else
            {
                logger?.Information("✅ Godot Tween subsystem pre-warmed successfully (verification: {Ms}ms)", verifyTime);
            }
        }
        catch (Exception ex)
        {
            logger?.Warning(ex, "Failed to pre-warm Godot subsystems, first operation may be slow");
        }
    }
    
    public override void _ExitTree()
    {
        _cellClickedSubscription?.Dispose();
        _cellHoveredSubscription?.Dispose();
        ClearSelectionAsync().Wait(100);
        base._ExitTree();
    }
}
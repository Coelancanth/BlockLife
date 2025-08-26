using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BlockLife.Core.Domain.Block;
using BlockLife.Core.Domain.Common;
using BlockLife.Core.Features.Block.Placement;
using BlockLife.godot_project.features.block.performance;
using Godot;
using LanguageExt;
using static LanguageExt.Prelude;

namespace BlockLife.godot_project.features.block.placement;

public partial class BlockVisualizationController : Node2D, IBlockVisualizationView
{
    [Export] public PackedScene? BlockScene { get; set; }
    [Export] public PackedScene? PreviewScene { get; set; }
    [Export] public Node2D? BlockContainer { get; set; }
    [Export] public Control? FeedbackContainer { get; set; }
    [Export] public float CellSize { get; set; } = 64f;

    // Animation settings - exposed for user preference
    [Export] public bool EnableAnimations { get; set; } = true;
    [Export] public float AnimationSpeed { get; set; } = 0.15f; // Default snappy speed

    private readonly Dictionary<Guid, Node2D> _blockNodes = new();
    private Node2D? _previewNode;
    private Control? _feedbackNode;
    private bool _firstTweenCreated = false; // Track first-time tween creation

    // Expose block nodes for inspection (eliminates reflection)
    public IReadOnlyDictionary<Guid, Node2D> BlockNodes => _blockNodes;

    public override void _Ready()
    {
        // Validate required nodes
        if (BlockContainer == null)
        {
            GD.PrintErr("BlockContainer is not assigned!");
        }
        if (FeedbackContainer == null)
        {
            GD.PrintErr("FeedbackContainer is not assigned!");
        }

        // Pre-warm Godot Tween system to prevent first-time initialization delay
        PreWarmTweenSystem();
    }

    /// <summary>
    /// Pre-warms the Godot Tween system to prevent ~289ms first-time initialization cost.
    /// Part of BF_001 fix for eliminating first-move lag.
    /// </summary>
    private void PreWarmTweenSystem()
    {
        try
        {
            // Create and immediately kill a tween to trigger subsystem initialization
            var warmupTween = CreateTween();
            warmupTween.Kill();

            // Mark that we've already created our first tween
            _firstTweenCreated = true;

            // Silent pre-warming - no need to log this
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to pre-warm Tween system: {ex.Message}");
        }
    }

    public Task ShowBlockAsync(Guid blockId, Vector2Int position, BlockType type)
    {
        // Delegate to tier-aware method with default tier 1
        return ShowBlockAsync(blockId, position, type, tier: 1);
    }

    public Task ShowBlockAsync(Guid blockId, Vector2Int position, BlockType type, int tier)
    {
        PerformanceProfiler.StartTimer($"ShowBlock_Total_{blockId}");

        // FIXED: Handle duplicate notifications gracefully
        // If block already exists at this exact position with same ID, it's a duplicate notification
        if (_blockNodes.ContainsKey(blockId))
        {
            // Check if it's at the same position - if so, just ignore the duplicate
            if (_blockNodes.TryGetValue(blockId, out var existingNode))
            {
                var existingPos = WorldToGridPosition(existingNode.Position);
                if (existingPos == position)
                {
                    // Same block, same position - just a duplicate notification, ignore it
                    PerformanceProfiler.StopTimer($"ShowBlock_Total_{blockId}");
                    return Task.CompletedTask;
                }
            }

            // Different position or corrupted state - this is an actual error
            GD.PrintErr($"ERROR: Block {blockId} already exists but at different position!");
            PerformanceProfiler.StopTimer($"ShowBlock_Total_{blockId}");
            return Task.CompletedTask;
        }

        // Create tier-aware block node
        PerformanceProfiler.StartTimer("CreateBlockNode");
        var blockNode = CreateBlockNode(position, type, tier);
        PerformanceProfiler.StopTimer("CreateBlockNode", true);

        if (BlockContainer == null)
        {
            GD.PrintErr($"ERROR: BlockContainer is null! Cannot add block visual.");
            PerformanceProfiler.StopTimer($"ShowBlock_Total_{blockId}");
            return Task.CompletedTask;
        }

        PerformanceProfiler.StartTimer("AddChild_Block");
        BlockContainer.AddChild(blockNode);
        PerformanceProfiler.StopTimer("AddChild_Block", true);

        _blockNodes[blockId] = blockNode;

        // Animate appearance with tier-aware scaling
        PerformanceProfiler.StartTimer("AnimateBlockAppearance");
        AnimateBlockAppearance(blockNode, tier);
        PerformanceProfiler.StopTimer("AnimateBlockAppearance", true);

        PerformanceProfiler.StopTimer($"ShowBlock_Total_{blockId}", true);
        return Task.CompletedTask;
    }

    public async Task ShowMergeAnimationAsync(Guid[] sourceBlockIds, Vector2Int targetPosition, BlockType type, int targetTier)
    {
        if (!EnableAnimations)
        {
            // Instant merge - just remove sources and show result
            foreach (var sourceId in sourceBlockIds)
            {
                await HideBlockAsync(sourceId);
            }
            var instantResultId = Guid.NewGuid();
            await ShowBlockAsync(instantResultId, targetPosition, type, targetTier);
            return;
        }

        // Phase 1: Converge animation - source blocks move to target position
        var convergeTasks = new List<Task>();
        var targetWorldPos = GridToWorldPosition(targetPosition);

        foreach (var sourceId in sourceBlockIds)
        {
            if (_blockNodes.TryGetValue(sourceId, out var sourceNode))
            {
                var convergeTask = Task.Run(async () =>
                {
                    var tween = CreateTween();
                    tween.TweenProperty(sourceNode, "position", targetWorldPos, AnimationSpeed * 0.8f)
                         .SetEase(Tween.EaseType.In)
                         .SetTrans(Tween.TransitionType.Cubic);
                    
                    // Scale down during convergence
                    tween.Parallel().TweenProperty(sourceNode, "scale", Vector2.One * 0.3f, AnimationSpeed * 0.8f)
                         .SetEase(Tween.EaseType.In);
                    
                    await Task.Delay((int)(AnimationSpeed * 800));
                });
                convergeTasks.Add(convergeTask);
            }
        }

        // Wait for all blocks to converge
        await Task.WhenAll(convergeTasks);

        // Phase 2: Flash effect at target position
        await CreateMergeFlashEffect(targetWorldPos);

        // Phase 3: Remove source blocks and show result with special entrance
        foreach (var sourceId in sourceBlockIds)
        {
            if (_blockNodes.TryGetValue(sourceId, out var sourceNode))
            {
                sourceNode.QueueFree();
                _blockNodes.Remove(sourceId);
            }
        }

        // Phase 4: Show result block with enhanced appearance animation
        var resultBlockId = Guid.NewGuid();
        await ShowBlockAsync(resultBlockId, targetPosition, type, targetTier);
        
        // Add special "burst" effect for merge result
        if (_blockNodes.TryGetValue(resultBlockId, out var resultNode))
        {
            await CreateMergeBurstEffect(resultNode);
        }
    }

    public Task HideBlockAsync(Guid blockId)
    {
        if (!_blockNodes.TryGetValue(blockId, out var blockNode))
        {
            GD.PrintErr($"Block {blockId} not found");
            return Task.CompletedTask;
        }

        // Animate disappearance
        AnimateBlockDisappearance(blockNode);

        // Queue free after animation
        if (!EnableAnimations)
        {
            // Instant removal
            blockNode.QueueFree();
            _blockNodes.Remove(blockId);
        }
        else
        {
            var tween = CreateTween();
            tween.TweenCallback(Callable.From(() =>
            {
                blockNode.QueueFree();
                _blockNodes.Remove(blockId);
            })).SetDelay(AnimationSpeed * 0.5f); // Match the disappearance animation
        }

        return Task.CompletedTask;
    }

    public Task UpdateBlockPositionAsync(Guid blockId, Vector2Int newPosition)
    {
        PerformanceProfiler.StartTimer($"UpdateBlockPosition_{blockId}");

        if (!_blockNodes.TryGetValue(blockId, out var blockNode))
        {
            GD.PrintErr($"Block {blockId} not found");
            PerformanceProfiler.StopTimer($"UpdateBlockPosition_{blockId}");
            return Task.CompletedTask;
        }

        PerformanceProfiler.StartTimer("GridToWorldPosition");
        var targetPosition = GridToWorldPosition(newPosition);
        PerformanceProfiler.StopTimer("GridToWorldPosition");

        PerformanceProfiler.StartTimer("AnimateBlockMovement");
        AnimateBlockMovement(blockNode, targetPosition);
        PerformanceProfiler.StopTimer("AnimateBlockMovement", true);

        PerformanceProfiler.StopTimer($"UpdateBlockPosition_{blockId}", true);
        return Task.CompletedTask;
    }

    public Task ShowPlacementPreviewAsync(Vector2Int position, BlockType type)
    {
        HidePlacementPreviewInternal();

        _previewNode = CreatePreviewNode(position, type);
        BlockContainer?.AddChild(_previewNode);

        // Gentle scale animation
        AnimatePreviewAppearance(_previewNode);

        return Task.CompletedTask;
    }

    public Task HidePlacementPreviewAsync()
    {
        HidePlacementPreviewInternal();
        HideFeedback();
        return Task.CompletedTask;
    }

    private void HidePlacementPreviewInternal()
    {
        if (_previewNode != null && IsInstanceValid(_previewNode))
        {
            _previewNode.QueueFree();
            _previewNode = null;
        }
    }

    public Task ShowInvalidPlacementFeedbackAsync(Vector2Int position, string reason)
    {
        return ShowFeedback(position, reason, Colors.Red);
    }

    public Task ShowValidPlacementFeedbackAsync(Vector2Int position)
    {
        return ShowFeedback(position, "Valid placement", Colors.Green);
    }

    public bool IsBlockVisible(Guid blockId) => _blockNodes.ContainsKey(blockId);

    public Option<Vector2Int> GetBlockVisualPosition(Guid blockId) =>
        _blockNodes.TryGetValue(blockId, out var node)
            ? Some(WorldToGridPosition(node.Position))
            : Option<Vector2Int>.None;

    // Helper Methods
    private Node2D CreateBlockNode(Vector2Int gridPos, BlockType type)
    {
        return CreateBlockNode(gridPos, type, tier: 1);
    }

    private Node2D CreateBlockNode(Vector2Int gridPos, BlockType type, int tier)
    {
        Node2D blockNode;

        if (BlockScene != null)
        {
            blockNode = BlockScene.Instantiate<Node2D>();
            
            // FIXED: Apply tier-based scaling to PackedScene instances
            var tierScale = GetTierScale(tier);
            blockNode.Scale = Vector2.One * tierScale;
        }
        else
        {
            // Create a tier-aware colored square as fallback
            blockNode = new Node2D();
            
            // Calculate tier-based size scaling
            var tierScale = GetTierScale(tier);
            var baseSize = CellSize - 4;
            
            var colorRect = new ColorRect
            {
                Size = new Vector2(baseSize, baseSize),
                Position = new Vector2(2, 2), // Small margin
                Color = GetBlockColor(type)
            };
            
            // Apply tier scaling to the entire node instead of individual rect
            blockNode.Scale = Vector2.One * tierScale;
            blockNode.AddChild(colorRect);
            
        }
        
        // ENHANCED: Display both type and tier information for ALL blocks
        AddBlockInfoDisplay(blockNode, type, tier);
        
        // Add tier-specific effects
        AddTierEffects(blockNode, tier);

        blockNode.Position = GridToWorldPosition(gridPos);
        return blockNode;
    }

    private Node2D CreatePreviewNode(Vector2Int gridPos, BlockType type)
    {
        Node2D previewNode;

        if (PreviewScene != null)
        {
            previewNode = PreviewScene.Instantiate<Node2D>();
        }
        else
        {
            // Create a semi-transparent square as fallback
            previewNode = new Node2D();
            var colorRect = new ColorRect
            {
                Size = new Vector2(CellSize - 4, CellSize - 4),
                Position = new Vector2(2, 2),
                Color = GetBlockColor(type) with { A = 0.5f }
            };
            previewNode.AddChild(colorRect);
        }

        previewNode.Position = GridToWorldPosition(gridPos);
        previewNode.Modulate = new Color(1, 1, 1, 0.5f);
        return previewNode;
    }

    private Vector2 GridToWorldPosition(Vector2Int gridPos)
    {
        return new Vector2(gridPos.X * CellSize, gridPos.Y * CellSize);
    }

    private Vector2Int WorldToGridPosition(Vector2 worldPos)
    {
        return new Vector2Int(
            Mathf.RoundToInt(worldPos.X / CellSize),
            Mathf.RoundToInt(worldPos.Y / CellSize)
        );
    }

    private Color GetBlockColor(BlockType type)
    {
        // Use the colors defined in the domain layer for consistency
        var (r, g, b) = type.GetColorRGB();
        return new Color(r / 255f, g / 255f, b / 255f);

        // Alternative implementation using switch for quick reference:
        // return type switch
        // {
        //     BlockType.Basic => new Color(0.502f, 0.502f, 0.502f),      // Gray
        //     BlockType.Work => new Color(0.255f, 0.412f, 0.882f),       // Royal Blue
        //     BlockType.Study => new Color(0.196f, 0.804f, 0.196f),      // Lime Green
        //     BlockType.Relationship => new Color(1.0f, 0.412f, 0.706f), // Hot Pink
        //     BlockType.Health => new Color(1.0f, 0.388f, 0.278f),       // Tomato Red
        //     BlockType.Creativity => new Color(0.576f, 0.439f, 0.859f), // Medium Purple
        //     BlockType.Fun => new Color(1.0f, 0.843f, 0.0f),            // Gold
        //     BlockType.CareerOpportunity => new Color(0.0f, 0.808f, 0.82f), // Dark Turquoise
        //     BlockType.Partnership => new Color(1.0f, 0.078f, 0.576f),  // Deep Pink
        //     BlockType.Passion => new Color(1.0f, 0.549f, 0.0f),        // Dark Orange
        //     _ => new Color(1.0f, 1.0f, 1.0f)                           // White (fallback)
        // };
    }

    private void AnimateBlockAppearance(Node2D blockNode)
    {
        AnimateBlockAppearance(blockNode, tier: 1);
    }

    private void AnimateBlockAppearance(Node2D blockNode, int tier)
    {
        if (!EnableAnimations)
        {
            // Instant appearance
            blockNode.Scale = Vector2.One;
            return;
        }

        blockNode.Scale = Vector2.Zero;

        var tween = CreateTween();
        // OPTIMIZATION: Using configurable AnimationSpeed with tier-aware bounce
        var easeType = tier > 1 ? Tween.EaseType.OutIn : Tween.EaseType.Out;
        tween.TweenProperty(blockNode, "scale", Vector2.One, AnimationSpeed)
             .SetEase(easeType)
             .SetTrans(Tween.TransitionType.Back);
    }

    private void AnimateBlockDisappearance(Node2D blockNode)
    {
        if (!EnableAnimations)
        {
            // Instant disappearance
            blockNode.Scale = Vector2.Zero;
            return;
        }

        var tween = CreateTween();
        // OPTIMIZATION: Half speed for removal (feels snappier)
        tween.TweenProperty(blockNode, "scale", Vector2.Zero, AnimationSpeed * 0.5f)
             .SetEase(Tween.EaseType.In);
    }

    private void AnimateBlockMovement(Node2D blockNode, Vector2 targetPosition)
    {
        if (!EnableAnimations)
        {
            // Instant movement for maximum responsiveness
            blockNode.Position = targetPosition;
            return;
        }

        // Phase 2 Instrumentation: Deep profiling of tween operations
        PerformanceProfiler.StartTimer("CreateTween_Movement");
        PerformanceProfiler.StartTimer("CreateTween_FirstTime_Check");
        var isFirstTween = _firstTweenCreated == false;
        PerformanceProfiler.StopTimer("CreateTween_FirstTime_Check");

        if (isFirstTween)
        {
            PerformanceProfiler.StartTimer("CreateTween_FirstTime");
            _firstTweenCreated = true;
        }

        var tween = CreateTween();

        if (isFirstTween)
        {
            PerformanceProfiler.StopTimer("CreateTween_FirstTime", true);
        }

        PerformanceProfiler.StopTimer("CreateTween_Movement", true);

        PerformanceProfiler.StartTimer("TweenProperty_Setup");
        // OPTIMIZATION: Using configurable AnimationSpeed (default 0.15f)
        // User testing shows 150ms feels responsive while maintaining smooth motion
        tween.TweenProperty(blockNode, "position", targetPosition, AnimationSpeed)
             .SetEase(Tween.EaseType.Out)
             .SetTrans(Tween.TransitionType.Cubic);
        PerformanceProfiler.StopTimer("TweenProperty_Setup", true);
    }

    private void AnimatePreviewAppearance(Node2D previewNode)
    {
        previewNode.Scale = Vector2.One * 0.8f;

        var tween = CreateTween();
        // OPTIMIZATION: Reduced from 0.2f to 0.1f for instant preview feedback
        tween.TweenProperty(previewNode, "scale", Vector2.One, 0.1f)
             .SetEase(Tween.EaseType.Out);
    }

    private Task ShowFeedback(Vector2Int position, string message, Color color)
    {
        HideFeedback();

        if (FeedbackContainer == null) return Task.CompletedTask;

        _feedbackNode = new Label
        {
            Text = message,
            Modulate = color,
            Position = GridToWorldPosition(position) + new Vector2(0, -40)
        };

        FeedbackContainer.AddChild(_feedbackNode);

        // Animate feedback
        var tween = CreateTween();
        tween.TweenProperty(_feedbackNode, "position:y", _feedbackNode.Position.Y - 20, 1.0f);
        tween.Parallel().TweenProperty(_feedbackNode, "modulate:a", 0.0f, 1.0f);
        tween.TweenCallback(Callable.From(() => HideFeedback()));

        return Task.CompletedTask;
    }

    private void HideFeedback()
    {
        if (_feedbackNode != null && IsInstanceValid(_feedbackNode))
        {
            _feedbackNode.QueueFree();
            _feedbackNode = null;
        }
    }

    /// <summary>
    /// Clears all visual blocks and resets internal state.
    /// Used for testing cleanup.
    /// </summary>
    public void ClearAllBlocks()
    {
        // Remove all visual block nodes
        foreach (var blockNode in _blockNodes.Values)
        {
            if (IsInstanceValid(blockNode))
            {
                blockNode.QueueFree();
            }
        }

        // Clear the tracking dictionary
        _blockNodes.Clear();

        // Hide any preview or feedback
        HidePlacementPreviewInternal();
        HideFeedback();
    }

    /// <summary>
    /// Gets the visual scale multiplier for a given tier.
    /// T1=1.0x, T2=1.15x, T3=1.3x, T4=1.5x
    /// </summary>
    private float GetTierScale(int tier) => tier switch
    {
        1 => 1.0f,
        2 => 1.15f,
        3 => 1.3f,
        4 => 1.5f,
        _ => 1.0f + (tier - 1) * 0.15f // Progressive scaling for T5+
    };

    /// <summary>
    /// Adds comprehensive block information display showing both type and tier.
    /// ALL blocks (including T1) now display their information clearly.
    /// </summary>
    private void AddBlockInfoDisplay(Node2D blockNode, BlockType type, int tier)
    {
        
        // Create info container for layering
        var infoContainer = new Node2D
        {
            Name = $"BlockInfo_{type}_T{tier}",
            ZIndex = 10 // Render on top of block
        };
        
        // Background panel for readability
        var backgroundPanel = new ColorRect
        {
            Size = new Vector2(CellSize - 4, 24),
            Position = new Vector2(2, CellSize - 26),
            Color = Colors.Black with { A = 0.85f },
            ShowBehindParent = false
        };
        
        // Block type label (shortened names for space)
        var typeText = GetShortTypeName(type);
        var typeLabel = new Label
        {
            Text = typeText,
            Position = new Vector2(6, CellSize - 24),
            Modulate = Colors.White,
            ZIndex = 11
        };
        typeLabel.AddThemeFontSizeOverride("font_size", 10);
        
        // Tier indicator (all tiers including T1)
        var tierLabel = new Label
        {
            Text = $"T{tier}",
            Position = new Vector2(CellSize - 20, CellSize - 24),
            Modulate = GetTierColor(tier),
            ZIndex = 11
        };
        tierLabel.AddThemeFontSizeOverride("font_size", 10);
        
        // Assembly: Add all elements to container
        infoContainer.AddChild(backgroundPanel);
        infoContainer.AddChild(typeLabel);
        infoContainer.AddChild(tierLabel);
        blockNode.AddChild(infoContainer);
        
    }
    
    /// <summary>
    /// Gets shortened block type names that fit in the display space.
    /// </summary>
    private string GetShortTypeName(BlockType type) => type switch
    {
        BlockType.Work => "Work",
        BlockType.Study => "Study", 
        BlockType.Health => "Health",
        BlockType.Fun => "Fun",
        BlockType.Creativity => "Create",
        BlockType.Relationship => "Relate",
        BlockType.CareerOpportunity => "Career",
        BlockType.Partnership => "Partner",
        BlockType.Passion => "Passion",
        BlockType.Basic => "Basic",
        _ => type.ToString().Substring(0, Math.Min(6, type.ToString().Length))
    };
    
    /// <summary>
    /// Gets tier-specific colors for visual hierarchy.
    /// T1=White, T2=Yellow, T3=Orange, T4=Red
    /// </summary>
    private Color GetTierColor(int tier) => tier switch
    {
        1 => Colors.White,      // T1: Standard/baseline
        2 => Colors.Yellow,     // T2: Enhanced  
        3 => Colors.Orange,     // T3: Advanced
        4 => Colors.Red,        // T4: Elite
        _ => Colors.Magenta     // T5+: Special
    };

    /// <summary>
    /// Legacy tier badge method - kept for backward compatibility.
    /// New code should use AddBlockInfoDisplay instead.
    /// </summary>
    private void AddTierBadge(Node2D blockNode, int tier)
    {
        // Redirect to comprehensive info display
        // This method is deprecated but kept for any existing callers
        GD.PrintErr($"[DEPRECATED] AddTierBadge called - use AddBlockInfoDisplay instead");
    }

    /// <summary>
    /// Adds tier-specific visual effects (T2=pulse, T3=glow, T4=particles).
    /// </summary>
    private void AddTierEffects(Node2D blockNode, int tier)
    {
        switch (tier)
        {
            case 1:
                // No effects for T1 (baseline)
                break;
                
            case 2:
                // T2: Pulse effect
                AddPulseEffect(blockNode);
                break;
                
            case 3:
                // T3: Glow effect
                AddGlowEffect(blockNode);
                break;
                
            case 4:
                // T4: Particle system
                AddParticleEffect(blockNode);
                break;
                
            default:
                // T5+: Combine all effects
                AddPulseEffect(blockNode);
                AddGlowEffect(blockNode);
                AddParticleEffect(blockNode);
                break;
        }
    }

    private void AddPulseEffect(Node2D blockNode)
    {
        // Simple scale pulsing animation
        var tween = CreateTween();
        tween.SetLoops();
        tween.TweenProperty(blockNode, "modulate:a", 0.8f, 1.0f)
             .SetEase(Tween.EaseType.InOut);
        tween.TweenProperty(blockNode, "modulate:a", 1.0f, 1.0f)
             .SetEase(Tween.EaseType.InOut);
    }

    private void AddGlowEffect(Node2D blockNode)
    {
        // Add subtle glow outline
        blockNode.Modulate = Colors.White with { A = 1.2f };
    }

    private void AddParticleEffect(Node2D blockNode)
    {
        // For now, just enhanced glow - proper particles need PackedScene
        blockNode.Modulate = Colors.White with { A = 1.5f };
        // TODO: Add actual GPUParticles2D when particle scenes are available
    }

    /// <summary>
    /// Creates a flash effect at the merge target position.
    /// Visual feedback that merge is occurring.
    /// </summary>
    private async Task CreateMergeFlashEffect(Vector2 worldPosition)
    {
        // Create temporary flash node
        var flashNode = new ColorRect
        {
            Size = new Vector2(CellSize, CellSize),
            Position = worldPosition,
            Color = Colors.White with { A = 0.8f },
            Modulate = Colors.Yellow
        };

        BlockContainer?.AddChild(flashNode);

        // Quick flash animation
        var tween = CreateTween();
        tween.TweenProperty(flashNode, "modulate:a", 0.0f, AnimationSpeed * 0.3f)
             .SetEase(Tween.EaseType.Out);

        // Wait for flash to complete
        await Task.Delay((int)(AnimationSpeed * 300));
        
        // Clean up flash node
        if (IsInstanceValid(flashNode))
        {
            flashNode.QueueFree();
        }
    }

    /// <summary>
    /// Creates a burst effect for the merged result block.
    /// Shows that a higher-tier block has appeared.
    /// </summary>
    private async Task CreateMergeBurstEffect(Node2D resultBlock)
    {
        // Store original scale
        var originalScale = resultBlock.Scale;
        
        // Start with larger scale and "pop" down to normal
        resultBlock.Scale = originalScale * 1.4f;
        resultBlock.Modulate = Colors.White with { A = 1.3f };

        var tween = CreateTween();
        
        // Scale burst effect
        tween.TweenProperty(resultBlock, "scale", originalScale, AnimationSpeed * 0.4f)
             .SetEase(Tween.EaseType.Out)
             .SetTrans(Tween.TransitionType.Back);
             
        // Brightness fade
        tween.Parallel().TweenProperty(resultBlock, "modulate", Colors.White, AnimationSpeed * 0.6f)
             .SetEase(Tween.EaseType.Out);

        // Wait for burst to complete
        await Task.Delay((int)(AnimationSpeed * 600));
    }
}

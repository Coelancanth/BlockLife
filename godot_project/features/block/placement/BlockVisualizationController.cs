using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BlockLife.Core.Features.Block.Placement;
using BlockLife.Core.Domain.Common;
using BlockLife.Core.Domain.Block;
using BlockLife.Godot.Features.Block.Performance;
using LanguageExt;
using static LanguageExt.Prelude;

namespace BlockLife.Godot.Features.Block.Placement;

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
            
            GD.Print("[BlockVisualizationController] Tween system pre-warmed");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to pre-warm Tween system: {ex.Message}");
        }
    }
    
    public Task ShowBlockAsync(Guid blockId, Vector2Int position, BlockType type)
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
        
        // For now, create a simple colored rectangle as a block
        PerformanceProfiler.StartTimer("CreateBlockNode");
        var blockNode = CreateBlockNode(position, type);
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
        
        // Animate appearance
        PerformanceProfiler.StartTimer("AnimateBlockAppearance");
        AnimateBlockAppearance(blockNode);
        PerformanceProfiler.StopTimer("AnimateBlockAppearance", true);
        
        PerformanceProfiler.StopTimer($"ShowBlock_Total_{blockId}", true);
        return Task.CompletedTask;
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
            tween.TweenCallback(Callable.From(() => {
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
        Node2D blockNode;
        
        if (BlockScene != null)
        {
            blockNode = BlockScene.Instantiate<Node2D>();
        }
        else
        {
            // Create a simple colored square as fallback
            blockNode = new Node2D();
            var colorRect = new ColorRect
            {
                Size = new Vector2(CellSize - 4, CellSize - 4),
                Position = new Vector2(2, 2),
                Color = GetBlockColor(type)
            };
            blockNode.AddChild(colorRect);
        }
        
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
        return type switch
        {
            BlockType.Basic => Colors.White,
            BlockType.Work => Colors.Blue,
            BlockType.Study => Colors.Purple,
            BlockType.Relationship => Colors.Pink,
            BlockType.Health => Colors.Green,
            BlockType.Creativity => Colors.Orange,
            BlockType.Fun => Colors.Yellow,
            BlockType.CareerOpportunity => Colors.Cyan,
            BlockType.Partnership => Colors.Magenta,
            BlockType.Passion => Colors.Red,
            _ => Colors.Gray
        };
    }
    
    private void AnimateBlockAppearance(Node2D blockNode)
    {
        if (!EnableAnimations)
        {
            // Instant appearance
            blockNode.Scale = Vector2.One;
            return;
        }
        
        blockNode.Scale = Vector2.Zero;
        
        var tween = CreateTween();
        // OPTIMIZATION: Using configurable AnimationSpeed
        tween.TweenProperty(blockNode, "scale", Vector2.One, AnimationSpeed)
             .SetEase(Tween.EaseType.Out)
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
}
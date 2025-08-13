using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BlockLife.Core.Features.Block.Placement;
using BlockLife.Core.Domain.Common;
using BlockLife.Core.Domain.Block;
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
    
    private readonly Dictionary<Guid, Node2D> _blockNodes = new();
    private Node2D? _previewNode;
    private Control? _feedbackNode;
    
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
    }
    
    public Task ShowBlockAsync(Guid blockId, Vector2Int position, BlockType type)
    {
        if (_blockNodes.ContainsKey(blockId))
        {
            GD.PrintErr($"Block {blockId} already exists");
            return Task.CompletedTask;
        }
        
        // For now, create a simple colored rectangle as a block
        var blockNode = CreateBlockNode(position, type);
        
        BlockContainer?.AddChild(blockNode);
        _blockNodes[blockId] = blockNode;
        
        // Animate appearance
        AnimateBlockAppearance(blockNode);
        
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
        var tween = CreateTween();
        tween.TweenCallback(Callable.From(() => {
            blockNode.QueueFree();
            _blockNodes.Remove(blockId);
        })).SetDelay(0.2f);
        
        return Task.CompletedTask;
    }
    
    public Task UpdateBlockPositionAsync(Guid blockId, Vector2Int newPosition)
    {
        if (!_blockNodes.TryGetValue(blockId, out var blockNode))
        {
            GD.PrintErr($"Block {blockId} not found");
            return Task.CompletedTask;
        }
        
        var targetPosition = GridToWorldPosition(newPosition);
        AnimateBlockMovement(blockNode, targetPosition);
        
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
        blockNode.Scale = Vector2.Zero;
        
        var tween = CreateTween();
        tween.TweenProperty(blockNode, "scale", Vector2.One, 0.3f)
             .SetEase(Tween.EaseType.Out)
             .SetTrans(Tween.TransitionType.Back);
    }
    
    private void AnimateBlockDisappearance(Node2D blockNode)
    {
        var tween = CreateTween();
        tween.TweenProperty(blockNode, "scale", Vector2.Zero, 0.2f)
             .SetEase(Tween.EaseType.In);
    }
    
    private void AnimateBlockMovement(Node2D blockNode, Vector2 targetPosition)
    {
        var tween = CreateTween();
        tween.TweenProperty(blockNode, "position", targetPosition, 0.4f)
             .SetEase(Tween.EaseType.Out)
             .SetTrans(Tween.TransitionType.Cubic);
    }
    
    private void AnimatePreviewAppearance(Node2D previewNode)
    {
        previewNode.Scale = Vector2.One * 0.8f;
        
        var tween = CreateTween();
        tween.TweenProperty(previewNode, "scale", Vector2.One, 0.2f)
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
}
using BlockLife.Core.Domain.Common;
using LanguageExt;
using LanguageExt.Common;
using Serilog;
using System;
using static LanguageExt.Prelude;

namespace BlockLife.Core.Features.Block.Drag.Services
{
    /// <summary>
    /// Implementation of IDragStateService for tracking drag operations.
    /// Maintains the state of the current drag operation.
    /// </summary>
    public class DragStateService : IDragStateService
    {
        private readonly ILogger _logger;
        private DragState _currentState;

        public DragStateService(ILogger logger)
        {
            _logger = logger;
            _currentState = DragState.None;
        }

        public bool IsDragging => _currentState.IsDragging;
        public Guid DraggedBlockId => _currentState.BlockId;
        public Vector2Int OriginalPosition => _currentState.OriginalPosition;
        public Vector2Int CurrentPreviewPosition => _currentState.PreviewPosition;

        public Fin<Unit> StartDrag(Guid blockId, Vector2Int originalPosition)
        {
            if (IsDragging)
            {
                _logger.Warning("Attempted to start drag while another drag is in progress");
                return FinFail<Unit>(Error.New("DRAG_IN_PROGRESS", "Another drag operation is already in progress"));
            }

            _currentState = new DragState
            {
                IsDragging = true,
                BlockId = blockId,
                OriginalPosition = originalPosition,
                PreviewPosition = originalPosition, // Initially preview at original position
                DragStartTime = DateTime.UtcNow
            };

            _logger.Debug("Started drag for block {BlockId} from position {Position}",
                blockId, originalPosition);
            return FinSucc(Unit.Default);
        }

        public Fin<Unit> UpdatePreviewPosition(Vector2Int previewPosition)
        {
            if (!IsDragging)
            {
                _logger.Warning("Attempted to update preview position with no active drag");
                return FinFail<Unit>(Error.New("NO_ACTIVE_DRAG", "No drag operation in progress"));
            }

            _currentState = _currentState with { PreviewPosition = previewPosition };

            _logger.Verbose("Updated drag preview position to {Position} for block {BlockId}",
                previewPosition, _currentState.BlockId);
            return FinSucc(Unit.Default);
        }

        public Fin<Unit> CompleteDrag()
        {
            if (!IsDragging)
            {
                _logger.Warning("Attempted to complete drag with no active drag");
                return FinFail<Unit>(Error.New("NO_ACTIVE_DRAG", "No drag operation in progress"));
            }

            var dragDuration = DateTime.UtcNow - _currentState.DragStartTime;
            _logger.Debug("Completed drag for block {BlockId} after {Duration}ms",
                _currentState.BlockId, dragDuration.TotalMilliseconds);

            _currentState = DragState.None;
            return FinSucc(Unit.Default);
        }

        public Fin<Unit> CancelDrag()
        {
            if (!IsDragging)
            {
                _logger.Warning("Attempted to cancel drag with no active drag");
                return FinFail<Unit>(Error.New("NO_ACTIVE_DRAG", "No drag operation in progress"));
            }

            _logger.Debug("Cancelled drag for block {BlockId}", _currentState.BlockId);
            _currentState = DragState.None;
            return FinSucc(Unit.Default);
        }

        public bool IsWithinDragRange(Vector2Int targetPosition, int maxRange = 3)
        {
            if (!IsDragging)
            {
                return false;
            }

            // Calculate Manhattan distance (grid-based movement)
            var distance = Math.Abs(targetPosition.X - OriginalPosition.X) +
                          Math.Abs(targetPosition.Y - OriginalPosition.Y);

            return distance <= maxRange;
        }

        /// <summary>
        /// Internal state representation for drag operations.
        /// </summary>
        private record DragState
        {
            public bool IsDragging { get; init; }
            public Guid BlockId { get; init; }
            public Vector2Int OriginalPosition { get; init; }
            public Vector2Int PreviewPosition { get; init; }
            public DateTime DragStartTime { get; init; }

            public static DragState None => new()
            {
                IsDragging = false,
                BlockId = Guid.Empty,
                OriginalPosition = Vector2Int.Zero,
                PreviewPosition = Vector2Int.Zero,
                DragStartTime = DateTime.MinValue
            };
        }
    }
}

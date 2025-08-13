using BlockLife.Core.Domain.Block;
using BlockLife.Core.Domain.Common;
using System;

namespace BlockLife.Core.Tests.Builders
{
    /// <summary>
    /// Builder for creating Block entities in tests with clean, readable syntax.
    /// </summary>
    public class BlockBuilder
    {
        private Guid _id = Guid.NewGuid();
        private BlockType _type = BlockType.Work;
        private Vector2Int _position = Vector2Int.Zero;
        private DateTime? _createdAt;
        private DateTime? _lastModifiedAt;

        public BlockBuilder WithId(Guid id)
        {
            _id = id;
            return this;
        }

        public BlockBuilder WithType(BlockType type)
        {
            _type = type;
            return this;
        }

        public BlockBuilder WithPosition(Vector2Int position)
        {
            _position = position;
            return this;
        }

        public BlockBuilder WithPosition(int x, int y)
        {
            _position = new Vector2Int(x, y);
            return this;
        }

        public BlockBuilder WithCreatedAt(DateTime createdAt)
        {
            _createdAt = createdAt;
            return this;
        }

        public BlockBuilder WithLastModifiedAt(DateTime lastModifiedAt)
        {
            _lastModifiedAt = lastModifiedAt;
            return this;
        }

        public Block Build()
        {
            var now = DateTime.UtcNow;
            return new Block
            {
                Id = _id,
                Type = _type,
                Position = _position,
                CreatedAt = _createdAt ?? now,
                LastModifiedAt = _lastModifiedAt ?? now
            };
        }
    }
}
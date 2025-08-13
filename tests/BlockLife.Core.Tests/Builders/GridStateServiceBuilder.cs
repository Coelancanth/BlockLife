using BlockLife.Core.Domain.Block;
using BlockLife.Core.Domain.Common;
using BlockLife.Core.Infrastructure.Services;
using System;
using System.Collections.Generic;

namespace BlockLife.Core.Tests.Builders
{
    /// <summary>
    /// Builder for creating pre-populated GridStateService instances for testing.
    /// </summary>
    public class GridStateServiceBuilder
    {
        private int _width = 10;
        private int _height = 10;
        private readonly List<Block> _blocks = new();

        public GridStateServiceBuilder WithDimensions(int width, int height)
        {
            _width = width;
            _height = height;
            return this;
        }

        public GridStateServiceBuilder WithBlock(Guid id, BlockType type, Vector2Int position)
        {
            var block = new BlockBuilder()
                .WithId(id)
                .WithType(type)
                .WithPosition(position)
                .Build();
            _blocks.Add(block);
            return this;
        }

        public GridStateServiceBuilder WithBlock(BlockType type, Vector2Int position)
        {
            return WithBlock(Guid.NewGuid(), type, position);
        }

        public GridStateServiceBuilder WithBlock(BlockType type, int x, int y)
        {
            return WithBlock(type, new Vector2Int(x, y));
        }

        public GridStateService Build()
        {
            var service = new GridStateService(_width, _height);
            
            foreach (var block in _blocks)
            {
                service.PlaceBlock(block);
            }
            
            return service;
        }
    }
}
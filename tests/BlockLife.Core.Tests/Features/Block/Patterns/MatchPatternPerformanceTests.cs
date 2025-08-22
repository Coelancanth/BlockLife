using BlockLife.Core.Domain.Block;
using BlockLife.Core.Domain.Common;
using BlockLife.Core.Features.Block.Patterns;
using BlockLife.Core.Features.Block.Patterns.Recognizers;
using BlockLife.Core.Infrastructure.Services;
using FluentAssertions;
using System;
using System.Diagnostics;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace BlockLife.Core.Tests.Features.Block.Patterns
{
    /// <summary>
    /// Performance tests for MatchPatternRecognizer, specifically testing the CanRecognizeAt optimization.
    /// </summary>
    public class MatchPatternPerformanceTests
    {
        private readonly ITestOutputHelper _output;

        public MatchPatternPerformanceTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CanRecognizeAt_Optimization_ReducesUnnecessaryFloodFills()
        {
            // Arrange - Create a sparse grid with isolated blocks
            var gridService = new GridStateService(20, 20);
            var recognizer = new MatchPatternRecognizer();
            
            // Place isolated blocks that cannot form matches
            for (int x = 0; x < 20; x += 3)
            {
                for (int y = 0; y < 20; y += 3)
                {
                    var block = new BlockLife.Core.Domain.Block.Block
                    {
                        Id = Guid.NewGuid(),
                        Type = BlockType.Work,
                        Position = new Vector2Int(x, y),
                        CreatedAt = DateTime.Now,
                        LastModifiedAt = DateTime.Now
                    };
                    gridService.PlaceBlock(block);
                }
            }

            // Act - Measure time with CanRecognizeAt optimization
            var sw = Stopwatch.StartNew();
            int recognitionsPerformed = 0;
            int canRecognizeChecks = 0;
            
            for (int x = 0; x < 20; x++)
            {
                for (int y = 0; y < 20; y++)
                {
                    var position = new Vector2Int(x, y);
                    canRecognizeChecks++;
                    
                    // First check with CanRecognizeAt (the optimization)
                    if (recognizer.CanRecognizeAt(gridService, position))
                    {
                        recognitionsPerformed++;
                        var context = PatternContext.CreateDefault(position);
                        var result = recognizer.Recognize(gridService, position, context);
                    }
                }
            }
            
            var optimizedTime = sw.ElapsedMilliseconds;
            sw.Stop();

            // Act - Measure time WITHOUT optimization (always call Recognize)
            sw.Restart();
            int fullRecognitionsPerformed = 0;
            
            for (int x = 0; x < 20; x++)
            {
                for (int y = 0; y < 20; y++)
                {
                    var position = new Vector2Int(x, y);
                    fullRecognitionsPerformed++;
                    var context = PatternContext.CreateDefault(position);
                    var result = recognizer.Recognize(gridService, position, context);
                }
            }
            
            var unoptimizedTime = sw.ElapsedMilliseconds;
            sw.Stop();

            // Assert
            _output.WriteLine($"With optimization: {optimizedTime}ms, {recognitionsPerformed} recognitions out of {canRecognizeChecks} checks");
            _output.WriteLine($"Without optimization: {unoptimizedTime}ms, {fullRecognitionsPerformed} recognitions");
            _output.WriteLine($"Performance improvement: {((double)unoptimizedTime / optimizedTime):F2}x faster");
            
            // The optimization should significantly reduce the number of actual recognitions
            recognitionsPerformed.Should().BeLessThan(fullRecognitionsPerformed);
            
            // Most isolated blocks should be quickly rejected by CanRecognizeAt
            recognitionsPerformed.Should().BeLessThan(canRecognizeChecks / 2);
        }

        [Fact]
        public void Recognize_PerformanceWithinThreshold_ForLargeGrid()
        {
            // Arrange - Create a grid with complex patterns
            var gridSize = 50;
            var gridService = new GridStateService(gridSize, gridSize);
            var recognizer = new MatchPatternRecognizer();
            
            // Create several match patterns
            // Horizontal lines
            for (int y = 0; y < gridSize; y += 5)
            {
                for (int x = 0; x < 5; x++)
                {
                    var block = new BlockLife.Core.Domain.Block.Block
                    {
                        Id = Guid.NewGuid(),
                        Type = BlockType.Health,
                        Position = new Vector2Int(x, y),
                        CreatedAt = DateTime.Now,
                        LastModifiedAt = DateTime.Now
                    };
                    gridService.PlaceBlock(block);
                }
            }
            
            // L-shapes
            for (int baseY = 10; baseY < gridSize; baseY += 10)
            {
                for (int baseX = 10; baseX < gridSize - 3; baseX += 10)
                {
                    // Horizontal part
                    for (int i = 0; i < 3; i++)
                    {
                        var block = new BlockLife.Core.Domain.Block.Block
                        {
                            Id = Guid.NewGuid(),
                            Type = BlockType.Study,
                            Position = new Vector2Int(baseX + i, baseY),
                            CreatedAt = DateTime.Now,
                            LastModifiedAt = DateTime.Now
                        };
                        gridService.PlaceBlock(block);
                    }
                    // Vertical part
                    for (int i = 1; i < 3; i++)
                    {
                        var block = new BlockLife.Core.Domain.Block.Block
                        {
                            Id = Guid.NewGuid(),
                            Type = BlockType.Study,
                            Position = new Vector2Int(baseX + 2, baseY + i),
                            CreatedAt = DateTime.Now,
                            LastModifiedAt = DateTime.Now
                        };
                        gridService.PlaceBlock(block);
                    }
                }
            }

            // Act - Recognize patterns at various trigger positions
            var sw = Stopwatch.StartNew();
            int totalPatterns = 0;
            var testPositions = new[]
            {
                new Vector2Int(0, 0),
                new Vector2Int(10, 10),
                new Vector2Int(25, 25),
                new Vector2Int(40, 40),
                new Vector2Int(2, 5),
                new Vector2Int(12, 15),
                new Vector2Int(22, 20),
                new Vector2Int(32, 30)
            };

            foreach (var position in testPositions)
            {
                var context = PatternContext.CreateDefault(position);
                var result = recognizer.Recognize(gridService, position, context);
                
                result.IsSucc.Should().BeTrue();
                result.Match(
                    Succ: patterns => totalPatterns += patterns.Count,
                    Fail: _ => throw new Exception("Recognition failed")
                );
            }
            
            sw.Stop();

            // Assert
            _output.WriteLine($"Recognized {totalPatterns} patterns in {sw.ElapsedMilliseconds}ms");
            _output.WriteLine($"Average time per recognition: {sw.ElapsedMilliseconds / (double)testPositions.Length:F2}ms");
            
            // Performance requirement: Recognition should be fast enough for 60fps gameplay
            // Each recognition should take less than 16ms (for 60fps)
            var averageTime = sw.ElapsedMilliseconds / (double)testPositions.Length;
            averageTime.Should().BeLessThan(16.0, "Recognition must be fast enough for 60fps gameplay");
            
            // Should find patterns
            totalPatterns.Should().BeGreaterThan(0);
        }

        [Fact]
        public void CanRecognizeAt_IsFasterThanFullRecognition()
        {
            // Arrange
            var gridService = new GridStateService(10, 10);
            var recognizer = new MatchPatternRecognizer();
            
            // Place a single block (no matches possible)
            var block = new BlockLife.Core.Domain.Block.Block
            {
                Id = Guid.NewGuid(),
                Type = BlockType.Work,
                Position = new Vector2Int(5, 5),
                CreatedAt = DateTime.Now,
                LastModifiedAt = DateTime.Now
            };
            gridService.PlaceBlock(block);

            var position = new Vector2Int(5, 5);
            var context = PatternContext.CreateDefault(position);
            
            // Warm up
            recognizer.CanRecognizeAt(gridService, position);
            recognizer.Recognize(gridService, position, context);

            // Act - Measure CanRecognizeAt
            var iterations = 10000;
            var sw = Stopwatch.StartNew();
            
            for (int i = 0; i < iterations; i++)
            {
                recognizer.CanRecognizeAt(gridService, position);
            }
            
            var canRecognizeTime = sw.ElapsedMilliseconds;
            
            // Act - Measure full Recognize
            sw.Restart();
            
            for (int i = 0; i < iterations; i++)
            {
                recognizer.Recognize(gridService, position, context);
            }
            
            var recognizeTime = sw.ElapsedMilliseconds;
            sw.Stop();

            // Assert
            _output.WriteLine($"CanRecognizeAt: {canRecognizeTime}ms for {iterations} iterations");
            _output.WriteLine($"Full Recognize: {recognizeTime}ms for {iterations} iterations");
            _output.WriteLine($"CanRecognizeAt is {(double)recognizeTime / canRecognizeTime:F2}x faster");
            
            // CanRecognizeAt should be significantly faster
            canRecognizeTime.Should().BeLessThan(recognizeTime);
        }
    }
}
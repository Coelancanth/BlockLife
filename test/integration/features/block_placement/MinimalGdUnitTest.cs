using Godot;
using GdUnit4;
using System;
using System.Threading.Tasks;
using BlockLife.Godot.Scenes;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using static GdUnit4.Assertions;

namespace BlockLife.Tests.Integration.Features.BlockPlacement
{
    /// <summary>
    /// Minimal test suite to verify GdUnit4 and SceneTree access
    /// Following the SimpleSceneTest pattern for consistency
    /// </summary>
    [TestSuite]
    public partial class MinimalGdUnitTest : Node
    {
        private SceneTree? _sceneTree;
        private SceneRoot? _sceneRoot;
        private IServiceProvider? _serviceProvider;
        
        [Before]
        public async Task Setup()
        {
            // Get the scene tree from the test node itself
            _sceneTree = GetTree();
            _sceneTree.Should().NotBeNull("scene tree must be available");
            
            // Get SceneRoot autoload - should be available in Godot context
            _sceneRoot = _sceneTree!.Root.GetNodeOrNull<SceneRoot>("/root/SceneRoot");
            
            if (_sceneRoot == null)
            {
                // If SceneRoot doesn't exist, we're not in a proper Godot context
                GD.PrintErr("SceneRoot not found - test must be run from Godot editor with SceneRoot autoload");
                return;
            }
            
            // Get service provider using reflection (following SimpleSceneTest pattern)
            var serviceProviderField = typeof(SceneRoot).GetField("_serviceProvider",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            _serviceProvider = serviceProviderField?.GetValue(_sceneRoot) as IServiceProvider;
            
            // Wait for frame to ensure everything is initialized
            await ToSignal(_sceneTree, SceneTree.SignalName.ProcessFrame);
        }
        
        [After]
        public async Task Cleanup()
        {
            // Wait for cleanup to complete
            if (_sceneTree != null)
            {
                await ToSignal(_sceneTree, SceneTree.SignalName.ProcessFrame);
            }
        }
        
        [TestCase]
        public async Task TestSceneTreeAccess()
        {
            // Get scene tree directly in test
            var sceneTree = GetTree();
            
            // If GetTree() returns null, try alternative approach
            if (sceneTree == null)
            {
                // Try to get from Engine
                var mainLoop = Engine.GetMainLoop();
                sceneTree = mainLoop as SceneTree;
                
                if (sceneTree == null)
                {
                    GD.Print("WARNING: GetTree() returned null and Engine.GetMainLoop() is not SceneTree");
                    GD.Print($"IsInsideTree: {IsInsideTree()}");
                    GD.Print($"GetParent: {GetParent()}");
                    // Skip test if we can't get scene tree
                    AssertThat(sceneTree).IsNotNull();
                    return;
                }
            }
            
            // Assert scene tree is available
            sceneTree.Should().NotBeNull("scene tree should be accessible");
            AssertThat(sceneTree).IsNotNull();
            
            // Verify we're in proper Godot context
            sceneTree!.Root.Should().NotBeNull("scene tree root should exist");
            
            // Log success
            GD.Print($"SUCCESS: Scene tree available with root: {sceneTree.Root.Name}");
            
            await Task.CompletedTask;
        }
        
        [TestCase]
        public async Task TestSceneRootAutoload()
        {
            // Get scene tree and SceneRoot directly in test
            var sceneTree = GetTree();
            if (sceneTree == null)
            {
                GD.Print("Skipping test - not in proper Godot context");
                return;
            }
            
            var sceneRoot = sceneTree.Root.GetNodeOrNull<SceneRoot>("/root/SceneRoot");
            
            // Assert SceneRoot autoload is available
            sceneRoot.Should().NotBeNull("SceneRoot autoload should be accessible");
            AssertThat(sceneRoot).IsNotNull();
            
            // Verify it's the singleton instance
            var path = sceneRoot!.GetPath();
            path.Should().Be("/root/SceneRoot", "SceneRoot should be at expected autoload path");
            
            // Log success
            GD.Print($"SUCCESS: SceneRoot autoload found at: {path}");
            
            await Task.CompletedTask;
        }
        
        [TestCase]
        public async Task TestServiceProviderAccess()
        {
            if (_serviceProvider == null)
            {
                GD.Print("Skipping test - not in proper Godot context");
                return;
            }
            
            // Assert service provider is available
            _serviceProvider.Should().NotBeNull("service provider should be accessible via reflection");
            AssertThat(_serviceProvider).IsNotNull();
            
            // Verify we can get a service
            var mediator = _serviceProvider.GetService<MediatR.IMediator>();
            mediator.Should().NotBeNull("mediator service should be registered");
            
            // Log success
            GD.Print("SUCCESS: Service provider accessible and services available");
            
            await Task.CompletedTask;
        }
        
        [TestCase]
        public async Task TestAsyncOperations()
        {
            // Get scene tree directly in test
            var sceneTree = GetTree();
            
            // If GetTree() returns null, try alternative approach
            if (sceneTree == null)
            {
                // Try to get from Engine
                var mainLoop = Engine.GetMainLoop();
                sceneTree = mainLoop as SceneTree;
                
                if (sceneTree == null)
                {
                    GD.Print("WARNING: Cannot access SceneTree for async operations");
                    GD.Print($"IsInsideTree: {IsInsideTree()}");
                    // Skip test gracefully
                    GD.Print("Skipping test - not in proper Godot context");
                    return;
                }
            }
            
            // Test that async operations work properly
            sceneTree.Should().NotBeNull("scene tree required for async operations");
            
            // Wait a frame using ToSignal
            await ToSignal(sceneTree, SceneTree.SignalName.ProcessFrame);
            
            // Verify we're still in valid context after async operation
            var tree = GetTree() ?? Engine.GetMainLoop() as SceneTree;
            tree.Should().NotBeNull("GetTree() should still work after async operation");
            tree.Should().BeSameAs(sceneTree, "should be same scene tree instance");
            
            // Log success
            GD.Print("SUCCESS: Async operations work correctly");
        }
    }
}
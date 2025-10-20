using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.StateManagement;
using AgValoniaGPS.Services.UndoRedo;
using AgValoniaGPS.Services.UndoRedo.Commands;
using NUnit.Framework;

namespace AgValoniaGPS.Services.Tests.UndoRedo
{
    /// <summary>
    /// Tests for UndoRedoService implementation.
    /// Focused on critical operations: command execution, undo, redo, and stack management.
    /// </summary>
    [TestFixture]
    public class UndoRedoServiceTests
    {
        private UndoRedoService _service = null!;

        [SetUp]
        public void SetUp()
        {
            _service = new UndoRedoService();
        }

        [Test]
        public async Task ExecuteAsync_AddsCommandToUndoStackAndClearsRedoStack()
        {
            // Arrange
            var boundaryPoints = new List<Position>
            {
                new Position { Latitude = 0, Longitude = 0 },
                new Position { Latitude = 1, Longitude = 0 },
                new Position { Latitude = 1, Longitude = 1 },
                new Position { Latitude = 0, Longitude = 1 }
            };
            var command = new CreateBoundaryCommand("TestField", boundaryPoints);

            // Act
            await _service.ExecuteAsync(command);

            // Assert
            Assert.That(_service.CanUndo(), Is.True, "Should be able to undo after executing command");
            Assert.That(_service.CanRedo(), Is.False, "Should not be able to redo after executing new command");
            Assert.That(_service.GetUndoStackDescriptions(), Has.Length.EqualTo(1));
            Assert.That(_service.GetUndoStackDescriptions()[0], Is.EqualTo("Create Boundary 'TestField'"));
        }

        [Test]
        public async Task UndoAsync_MovesCommandFromUndoToRedoStack()
        {
            // Arrange
            var boundaryPoints = new List<Position>
            {
                new Position { Latitude = 0, Longitude = 0 },
                new Position { Latitude = 1, Longitude = 0 },
                new Position { Latitude = 1, Longitude = 1 }
            };
            var command = new CreateBoundaryCommand("TestField", boundaryPoints);
            await _service.ExecuteAsync(command);

            // Act
            var result = await _service.UndoAsync();

            // Assert
            Assert.That(result.Success, Is.True, "Undo should succeed");
            Assert.That(result.CommandDescription, Is.EqualTo("Create Boundary 'TestField'"));
            Assert.That(_service.CanUndo(), Is.False, "Should not be able to undo after single undo");
            Assert.That(_service.CanRedo(), Is.True, "Should be able to redo after undo");
            Assert.That(_service.GetRedoStackDescriptions(), Has.Length.EqualTo(1));
        }

        [Test]
        public async Task RedoAsync_MovesCommandFromRedoToUndoStack()
        {
            // Arrange
            var boundaryPoints = new List<Position>
            {
                new Position { Latitude = 0, Longitude = 0 },
                new Position { Latitude = 1, Longitude = 0 },
                new Position { Latitude = 1, Longitude = 1 }
            };
            var command = new CreateBoundaryCommand("TestField", boundaryPoints);
            await _service.ExecuteAsync(command);
            await _service.UndoAsync();

            // Act
            var result = await _service.RedoAsync();

            // Assert
            Assert.That(result.Success, Is.True, "Redo should succeed");
            Assert.That(result.CommandDescription, Is.EqualTo("Create Boundary 'TestField'"));
            Assert.That(_service.CanUndo(), Is.True, "Should be able to undo after redo");
            Assert.That(_service.CanRedo(), Is.False, "Should not be able to redo after single redo");
            Assert.That(_service.GetUndoStackDescriptions(), Has.Length.EqualTo(1));
        }

        [Test]
        public async Task ExecuteAsync_ClearsRedoStack()
        {
            // Arrange
            var points1 = new List<Position>
            {
                new Position { Latitude = 0, Longitude = 0 },
                new Position { Latitude = 1, Longitude = 0 },
                new Position { Latitude = 1, Longitude = 1 }
            };
            var points2 = new List<Position>
            {
                new Position { Latitude = 2, Longitude = 2 },
                new Position { Latitude = 3, Longitude = 2 },
                new Position { Latitude = 3, Longitude = 3 }
            };
            var command1 = new CreateBoundaryCommand("Field1", points1);
            var command2 = new CreateBoundaryCommand("Field2", points2);

            await _service.ExecuteAsync(command1);
            await _service.UndoAsync();

            // Act - Execute new command should clear redo stack
            await _service.ExecuteAsync(command2);

            // Assert
            Assert.That(_service.CanRedo(), Is.False, "Redo stack should be cleared after new execution");
            Assert.That(_service.GetRedoStackDescriptions(), Has.Length.EqualTo(0));
            Assert.That(_service.GetUndoStackDescriptions(), Has.Length.EqualTo(1));
            Assert.That(_service.GetUndoStackDescriptions()[0], Is.EqualTo("Create Boundary 'Field2'"));
        }

        [Test]
        public async Task UndoRedoStateChanged_RaisedOnExecuteUndoRedo()
        {
            // Arrange
            var boundaryPoints = new List<Position>
            {
                new Position { Latitude = 0, Longitude = 0 },
                new Position { Latitude = 1, Longitude = 0 },
                new Position { Latitude = 1, Longitude = 1 }
            };
            var command = new CreateBoundaryCommand("TestField", boundaryPoints);
            int eventCount = 0;
            UndoRedoStateChangedEventArgs? lastEventArgs = null;

            _service.UndoRedoStateChanged += (sender, args) =>
            {
                eventCount++;
                lastEventArgs = args;
            };

            // Act - Execute
            await _service.ExecuteAsync(command);

            // Assert - After Execute
            Assert.That(eventCount, Is.EqualTo(1), "Event should be raised after execute");
            Assert.That(lastEventArgs?.CanUndo, Is.True);
            Assert.That(lastEventArgs?.CanRedo, Is.False);
            Assert.That(lastEventArgs?.UndoCount, Is.EqualTo(1));
            Assert.That(lastEventArgs?.RedoCount, Is.EqualTo(0));
            Assert.That(lastEventArgs?.LastCommandDescription, Is.EqualTo("Create Boundary 'TestField'"));

            // Act - Undo
            await _service.UndoAsync();

            // Assert - After Undo
            Assert.That(eventCount, Is.EqualTo(2), "Event should be raised after undo");
            Assert.That(lastEventArgs?.CanUndo, Is.False);
            Assert.That(lastEventArgs?.CanRedo, Is.True);
            Assert.That(lastEventArgs?.UndoCount, Is.EqualTo(0));
            Assert.That(lastEventArgs?.RedoCount, Is.EqualTo(1));

            // Act - Redo
            await _service.RedoAsync();

            // Assert - After Redo
            Assert.That(eventCount, Is.EqualTo(3), "Event should be raised after redo");
            Assert.That(lastEventArgs?.CanUndo, Is.True);
            Assert.That(lastEventArgs?.CanRedo, Is.False);
        }

        [Test]
        public async Task ClearAllStacks_ClearsBothStacks()
        {
            // Arrange
            var boundaryPoints = new List<Position>
            {
                new Position { Latitude = 0, Longitude = 0 },
                new Position { Latitude = 1, Longitude = 0 },
                new Position { Latitude = 1, Longitude = 1 }
            };
            var command1 = new CreateBoundaryCommand("Field1", boundaryPoints);
            var command2 = new CreateBoundaryCommand("Field2", boundaryPoints);

            await _service.ExecuteAsync(command1);
            await _service.ExecuteAsync(command2);
            await _service.UndoAsync();

            // Verify setup
            Assert.That(_service.CanUndo(), Is.True, "Should have undo available before clear");
            Assert.That(_service.CanRedo(), Is.True, "Should have redo available before clear");

            // Act
            _service.ClearAllStacks();

            // Assert
            Assert.That(_service.CanUndo(), Is.False, "Should not be able to undo after clear");
            Assert.That(_service.CanRedo(), Is.False, "Should not be able to redo after clear");
            Assert.That(_service.GetUndoStackDescriptions(), Has.Length.EqualTo(0));
            Assert.That(_service.GetRedoStackDescriptions(), Has.Length.EqualTo(0));
        }

        [Test]
        public async Task PerformanceTest_ExecuteUndoRedoCompletesWithin50ms()
        {
            // Arrange
            var boundaryPoints = new List<Position>
            {
                new Position { Latitude = 0, Longitude = 0 },
                new Position { Latitude = 1, Longitude = 0 },
                new Position { Latitude = 1, Longitude = 1 }
            };
            var command = new CreateBoundaryCommand("TestField", boundaryPoints);
            var stopwatch = new Stopwatch();

            // Act & Assert - Execute
            stopwatch.Start();
            await _service.ExecuteAsync(command);
            stopwatch.Stop();
            Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(50),
                $"Execute should complete in <50ms, took {stopwatch.ElapsedMilliseconds}ms");

            // Act & Assert - Undo
            stopwatch.Restart();
            await _service.UndoAsync();
            stopwatch.Stop();
            Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(50),
                $"Undo should complete in <50ms, took {stopwatch.ElapsedMilliseconds}ms");

            // Act & Assert - Redo
            stopwatch.Restart();
            await _service.RedoAsync();
            stopwatch.Stop();
            Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(50),
                $"Redo should complete in <50ms, took {stopwatch.ElapsedMilliseconds}ms");
        }

        [Test]
        public async Task UndoAsync_ReturnsFailure_WhenUndoStackIsEmpty()
        {
            // Act
            var result = await _service.UndoAsync();

            // Assert
            Assert.That(result.Success, Is.False, "Undo should fail when stack is empty");
            Assert.That(result.ErrorMessage, Is.EqualTo("Nothing to undo"));
        }
    }
}

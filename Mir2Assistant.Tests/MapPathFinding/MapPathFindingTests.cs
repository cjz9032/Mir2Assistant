extern alias cqfz;

using System.Collections.Generic;
using Xunit;
using MapPosition = cqfz::Mir2Assistant.Models.MapPathFinding.MapPosition;
using MapConnection = cqfz::Mir2Assistant.Models.MapPathFinding.MapConnection;
using MapPathFindingService = cqfz::Mir2Assistant.Services.MapPathFindingService;

namespace Mir2Assistant.Tests.MapPathFinding
{
    public class MapPathFindingTests
    {
        private readonly MapPathFindingService _pathFinder;
        private readonly List<MapConnection> _connections;

        public MapPathFindingTests()
        {
            // 设置测试数据
            _connections = new List<MapConnection>
            {
                new MapConnection 
                { 
                    From = new MapPosition { MapId = "0101", X = 10, Y = 22 },
                    To = new MapPosition { MapId = "0", X = 287, Y = 296 }
                },
                new MapConnection 
                { 
                    From = new MapPosition { MapId = "0101", X = 18, Y = 14 },
                    To = new MapPosition { MapId = "0", X = 290, Y = 293 }
                },
                new MapConnection 
                { 
                    From = new MapPosition { MapId = "0101", X = 3, Y = 19 },
                    To = new MapPosition { MapId = "0100", X = 11, Y = 13 }
                },
                new MapConnection 
                { 
                    From = new MapPosition { MapId = "0100", X = 11, Y = 14 },
                    To = new MapPosition { MapId = "0101", X = 3, Y = 20 }
                }
            };

            _pathFinder = new MapPathFindingService(_connections);
        }

        [Fact]
        public void FindPath_SameMap_ReturnsEmptyPath()
        {
            // Act
            var path = _pathFinder.FindPath("0101", "0101");

            // Assert
            Assert.NotNull(path);
            Assert.Empty(path);
        }

        [Fact]
        public void FindPath_DirectConnection_ReturnsDirectPath()
        {
            // Act
            var path = _pathFinder.FindPath("0101", "0");

            // Assert
            Assert.NotNull(path);
            Assert.Single(path);
            Assert.Equal("0101", path[0].From.MapId);
            Assert.Equal("0", path[0].To.MapId);
        }

        [Fact]
        public void FindPath_IndirectConnection_ReturnsCorrectPath()
        {
            // Act
            var path = _pathFinder.FindPath("0100", "0");

            // Assert
            Assert.NotNull(path);
            Assert.Equal(2, path.Count);
            Assert.Equal("0100", path[0].From.MapId);
            Assert.Equal("0101", path[0].To.MapId);
            Assert.Equal("0101", path[1].From.MapId);
            Assert.Equal("0", path[1].To.MapId);
        }

        [Fact]
        public void FindPath_NoPath_ReturnsNull()
        {
            // Act
            var path = _pathFinder.FindPath("9999", "0");

            // Assert
            Assert.Null(path);
        }

        [Fact]
        public void FindPath_InvalidMapId_ReturnsNull()
        {
            // Act
            var path = _pathFinder.FindPath("", "0");

            // Assert
            Assert.Null(path);
        }

        [Fact]
        public void FindNearestPath_ExistingPath_ReturnsPathWithUpdatedStartPosition()
        {
            // Arrange
            var start = new MapPosition { MapId = "0100", X = 5, Y = 5 };
            var target = new MapPosition { MapId = "0", X = 287, Y = 296 };

            // Act
            var path = _pathFinder.FindNearestPath(start, target);

            // Assert
            Assert.NotNull(path);
            Assert.Equal(2, path.Count);
            Assert.Equal("0100", path[0].From.MapId);
            Assert.Equal(5, path[0].From.X);
            Assert.Equal(5, path[0].From.Y);
        }

        [Fact]
        public void FindNearestPath_NoPath_ReturnsNull()
        {
            // Arrange
            var start = new MapPosition { MapId = "9999", X = 1, Y = 1 };
            var target = new MapPosition { MapId = "0", X = 287, Y = 296 };

            // Act
            var path = _pathFinder.FindNearestPath(start, target);

            // Assert
            Assert.Null(path);
        }

        [Fact]
        public void FindNearestPath_SameMap_ReturnsEmptyPath()
        {
            // Arrange
            var start = new MapPosition { MapId = "0101", X = 5, Y = 5 };
            var target = new MapPosition { MapId = "0101", X = 10, Y = 10 };

            // Act
            var path = _pathFinder.FindNearestPath(start, target);

            // Assert
            Assert.NotNull(path);
            Assert.Empty(path);
        }
    }
} 
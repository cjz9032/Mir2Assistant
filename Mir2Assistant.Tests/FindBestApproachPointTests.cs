using Mir2Assistant.Common.Functions;
using Mir2Assistant.Common.Models;
using System;
using System.Collections.Concurrent;
using Xunit;
using Xunit.Abstractions;

namespace Mir2Assistant.Tests
{
    public class FindBestApproachPointTests
    {
        private readonly ITestOutputHelper _output;

        public FindBestApproachPointTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TestFindBestApproachPoint_NoObstacles_CharacterOnRight()
        {
            // 场景：角色在怪物右边，周围无障碍物
            var monsterX = 100;
            var monsterY = 100;
            var charX = 105;
            var charY = 100;
            
            var instanceValue = CreateTestInstance(charX, charY);
            var result = CallFindBestApproachPoint(instanceValue, monsterX, monsterY, 1);
            
            _output.WriteLine($"场景: 无障碍物，角色在怪物右边");
            _output.WriteLine($"怪物: ({monsterX}, {monsterY}), 角色: ({charX}, {charY})");
            _output.WriteLine($"返回点: ({result.x}, {result.y})");
            
            // 期望返回怪物右边的点 (101, 100)
            Assert.Equal((101, 100), result);
        }

        [Fact]
        public void TestFindBestApproachPoint_NoObstacles_CharacterOnRightBottom()
        {
            // 场景：角色在怪物右下方，周围无障碍物
            var monsterX = 100;
            var monsterY = 100;
            var charX = 105;
            var charY = 105;
            
            var instanceValue = CreateTestInstance(charX, charY);
            var result = CallFindBestApproachPoint(instanceValue, monsterX, monsterY, 1);
            
            _output.WriteLine($"场景: 无障碍物，角色在怪物右下方");
            _output.WriteLine($"怪物: ({monsterX}, {monsterY}), 角色: ({charX}, {charY})");
            _output.WriteLine($"返回点: ({result.x}, {result.y})");
            
            // 期望返回怪物右下的点 (101, 101)
            Assert.Equal((101, 101), result);
        }

        [Fact]
        public void TestFindBestApproachPoint_WithMonsterBlocking_CharacterOnRight()
        {
            // 场景：角色在怪物右边，但右边有其他怪物阻挡
            var monsterX = 100;
            var monsterY = 100;
            var charX = 105;
            var charY = 100;
            
            var instanceValue = CreateTestInstance(charX, charY);
            
            // 在怪物右边 (101, 100) 添加一个怪物
            AddMonsterAtPosition(instanceValue, 101, 100);
            
            var result = CallFindBestApproachPoint(instanceValue, monsterX, monsterY, 1);
            
            _output.WriteLine($"场景: 右边有怪物阻挡，角色在怪物右边");
            _output.WriteLine($"怪物: ({monsterX}, {monsterY}), 角色: ({charX}, {charY})");
            _output.WriteLine($"阻挡点: (101, 100)");
            _output.WriteLine($"返回点: ({result.x}, {result.y})");
            
            // 右边被阻挡，应该返回右上 (101, 99) 或右下 (101, 101)
            Assert.NotEqual((0, 0), result);
            Assert.True(result.x == 101 && (result.y == 99 || result.y == 101), 
                $"期望返回右上或右下，实际返回 ({result.x}, {result.y})");
        }

        [Fact]
        public void TestFindBestApproachPoint_WithMultipleBlocking_CharacterOnRight()
        {
            // 场景：角色在怪物右边，右边和右上都被阻挡
            var monsterX = 100;
            var monsterY = 100;
            var charX = 105;
            var charY = 100;
            
            var instanceValue = CreateTestInstance(charX, charY);
            
            // 在右边和右上添加怪物
            AddMonsterAtPosition(instanceValue, 101, 100);  // 右
            AddMonsterAtPosition(instanceValue, 101, 99);   // 右上
            
            var result = CallFindBestApproachPoint(instanceValue, monsterX, monsterY, 1);
            
            _output.WriteLine($"场景: 右边和右上都被阻挡");
            _output.WriteLine($"怪物: ({monsterX}, {monsterY}), 角色: ({charX}, {charY})");
            _output.WriteLine($"阻挡点: (101, 100), (101, 99)");
            _output.WriteLine($"返回点: ({result.x}, {result.y})");
            
            // 应该返回右下 (101, 101)
            Assert.Equal((101, 101), result);
        }

        [Fact]
        public void TestFindBestApproachPoint_Radius2_NoObstacles()
        {
            // 场景：searchRadius=2，无障碍物
            var monsterX = 100;
            var monsterY = 100;
            var charX = 105;
            var charY = 100;
            
            var instanceValue = CreateTestInstance(charX, charY);
            var result = CallFindBestApproachPoint(instanceValue, monsterX, monsterY, 2);
            
            _output.WriteLine($"场景: searchRadius=2，无障碍物");
            _output.WriteLine($"怪物: ({monsterX}, {monsterY}), 角色: ({charX}, {charY})");
            _output.WriteLine($"返回点: ({result.x}, {result.y})");
            
            // 期望返回距离2的右边点 (102, 100)
            Assert.Equal((102, 100), result);
            
            // 验证距离确实是2
            int distance = Math.Max(Math.Abs(result.x - monsterX), Math.Abs(result.y - monsterY));
            Assert.Equal(2, distance);
        }

        [Fact]
        public void TestFindBestApproachPoint_Radius2_WithBlocking()
        {
            // 场景：searchRadius=2，距离2的右边被阻挡
            var monsterX = 100;
            var monsterY = 100;
            var charX = 105;
            var charY = 100;
            
            var instanceValue = CreateTestInstance(charX, charY);
            
            // 在距离2的右边添加怪物
            AddMonsterAtPosition(instanceValue, 102, 100);
            
            var result = CallFindBestApproachPoint(instanceValue, monsterX, monsterY, 2);
            
            _output.WriteLine($"场景: searchRadius=2，右边被阻挡");
            _output.WriteLine($"怪物: ({monsterX}, {monsterY}), 角色: ({charX}, {charY})");
            _output.WriteLine($"阻挡点: (102, 100)");
            _output.WriteLine($"返回点: ({result.x}, {result.y})");
            
            // 应该返回距离2的右上或右下
            Assert.NotEqual((0, 0), result);
            int distance = Math.Max(Math.Abs(result.x - monsterX), Math.Abs(result.y - monsterY));
            Assert.Equal(2, distance);
        }

        private MirGameInstanceModel CreateTestInstance(int charX, int charY)
        {
            var instance = new MirGameInstanceModel
            {
                CharacterStatus = new CharacterStatusModel
                {
                    X = charX,
                    Y = charY,
                    MapId = "D2000"
                },
                MonstersByPosition = new ConcurrentDictionary<long, List<MonsterModel>>(),
                AccountInfo = new GameAccountModel
                {
                    role = RoleType.blade
                }
            };
            
            return instance;
        }

        private void AddMonsterAtPosition(MirGameInstanceModel instance, int x, int y)
        {
            var monster = new MonsterModel { X = x, Y = y, Id = new Random().Next() };
            long key = ((long)x << 32) | (uint)y;
            instance.MonstersByPosition[key] = new List<MonsterModel> { monster };
        }

        private (int x, int y) CallFindBestApproachPoint(MirGameInstanceModel instance, int monsterX, int monsterY, int searchRadius)
        {
            var method = typeof(GoRunFunction).GetMethod("FindBestApproachPoint", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            
            return ((int x, int y))method.Invoke(null, new object[] { instance, monsterX, monsterY, searchRadius });
        }
    }
}

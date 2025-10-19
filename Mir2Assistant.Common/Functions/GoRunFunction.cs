using Mir2Assistant.Common.Models;
using Serilog; // 新增Serilog引用
using System.Diagnostics; // 新增Stopwatch引用
using Mir2Assistant.Common.Utils;
using Mir2Assistant.Common.Constants;
using Mir2Assistant.Common.Services;
using Mir2Assistant.Common.Models.MapPathFinding;
using Mir2Assistant.Common.Generated;
using System.Threading.Tasks;
using System.Collections.Concurrent; // 新增Generated命名空间引用

namespace Mir2Assistant.Common.Functions;
/// <summary>
/// 走路、跑路、寻路
/// </summary>
public static class GoRunFunction
{
    public static MapConnectionService mapConnectionService = new MapConnectionService();

    public static async Task DropItem(MirGameInstanceModel instanceValue, ItemModel item)
    {
        var data = StringUtils.GenerateMixedData(
            item.Name,
            item.Id
        );

        SendMirCall.Send(instanceValue, 3032, data);
        instanceValue.memoryUtils.WriteByte(item.addr, 0);
        await Task.Delay(500);

    }
    public static async Task DropBinItems(MirGameInstanceModel instanceValue)
    {
        var curinItems = GameConstants.Items.GetBinItems(instanceValue, instanceValue.CharacterStatus.Level, instanceValue.AccountInfo.role);
        var items = instanceValue.Items.Concat(instanceValue.QuickItems).Where(o => !o.IsEmpty && !o.IsGodly && curinItems.Contains(o.Name)).ToList();
        foreach (var item in items)
        {
            await DropItem(instanceValue, item);
        }
        if (items.Count > 0)
        {
            await NpcFunction.RefreshPackages(instanceValue);
        }

        // 战士扔蓝 太阳
        var mainInstance = GameState.GameInstances[0];
        if (!instanceValue.AccountInfo.IsMainControl && !mainInstance.isHomePreparing)
        {
            var diffFar = Math.Max(Math.Abs(mainInstance.CharacterStatus!.X - instanceValue.CharacterStatus!.X), Math.Abs(mainInstance.CharacterStatus.Y - instanceValue.CharacterStatus.Y));
            if (instanceValue.AccountInfo.role == RoleType.blade && diffFar < 5)
            {
                var mc = mainInstance.Items.Concat(mainInstance.QuickItems).Where(o => !o.IsEmpty).Count();

                var rmc = 46 - mc;
                if (rmc > 6)
                {

                    var items2 = instanceValue.QuickItems.Concat(instanceValue.Items).Where(o => !o.IsEmpty && o.stdMode == 0 && o.Name.Contains("魔法药")).ToList();
                    var mageCount = instanceValue.Items.Concat(instanceValue.QuickItems).Where(o => !o.IsEmpty).Count(o => o.stdMode == 0 && o.Name.Contains("魔法药"));
                    if (mageCount > 0)
                    {
                        // 进位
                        var dropCount = Math.Ceiling(Math.Min(rmc, items2.Count) * 0.5);
                        var dropItems = items2.Take((int)dropCount).ToList();
                        foreach (var item in dropItems)
                        {
                            await DropItem(instanceValue, item);
                        }
                        if (dropItems.Count > 0)
                        {
                            await NpcFunction.RefreshPackages(instanceValue);
                        }
                    }

                    var itemsSuper = instanceValue.QuickItems.Concat(instanceValue.Items).Where(o => !o.IsEmpty && o.stdMode == 0 && o.Name.Contains("太阳水")).ToList();
                    if (itemsSuper.Count > 0)
                    {
                        var dropCount = Math.Ceiling(Math.Min(rmc, itemsSuper.Count) * 0.5);
                        var dropItems = itemsSuper.Take((int)dropCount).ToList();
                        foreach (var item in dropItems)
                        {
                            await DropItem(instanceValue, item);
                        }
                        if (dropItems.Count > 0)
                        {
                            await NpcFunction.RefreshPackages(instanceValue);
                        }
                    }
                }



            }
        }
    }
    public static bool PickupInfoBasicCheck(MirGameInstanceModel instanceValue)
    {
        var enoughBBCanHit = instanceValue.AccountInfo.role != RoleType.blade && instanceValue.Monsters.Values.Where(o => !o.isDead && o.isTeamMons &&
                Math.Max(Math.Abs(o.X - instanceValue.CharacterStatus.X), Math.Abs(o.Y - instanceValue.CharacterStatus.Y)) < 5).Count() > 3;

        var allowMonsters = GameConstants.GetAllowMonsters(instanceValue, instanceValue.CharacterStatus!.Level, instanceValue.AccountInfo.role);
        var existAni2 = instanceValue.Monsters.Values.Where(o => o.stdAliveMon && allowMonsters.Contains(o.Name) &&
        Math.Max(Math.Abs(o.X - instanceValue.CharacterStatus.X), Math.Abs(o.Y - instanceValue.CharacterStatus.Y)) < (enoughBBCanHit ? 3 : 4)).FirstOrDefault();
        if (existAni2 != null)
        {
            return false;
        }
        var isFull = instanceValue.Items.Concat(instanceValue.QuickItems).Where(o => !o.IsEmpty).Count() > 44;
        if (isFull) return false;
        return true;
    }


    public static List<KeyValuePair<int, DropItemModel>>? PreparePickupInfo(MirGameInstanceModel instanceValue)
    {
        var stopwatch = Stopwatch.StartNew();
        if (!PickupInfoBasicCheck(instanceValue))
        {
            return null;
        }
        var mainInstance = GameState.GameInstances[0];
        var isMainFull = mainInstance.Items.Concat(mainInstance.QuickItems).Where(o => !o.IsEmpty).Count() > 44;
        var canLight = CapbilityOfLighting(instanceValue);
        var isMageNeed = instanceValue.AccountInfo.role == RoleType.mage && canLight;

        var CharacterStatus = instanceValue.CharacterStatus!;
        var curinItems = GameConstants.Items.GetBinItems(instanceValue, CharacterStatus.Level, instanceValue.AccountInfo.role);
        var miscs = instanceValue.Items.Concat(instanceValue.QuickItems).Where(o => !o.IsEmpty);
        var mageCount = miscs.Count(o => o.stdMode == 0 && o.Name.Contains("魔法药"));
        var healCount = miscs.Count(o => o.stdMode == 0 && o.Name.Contains("金创药"));
        var isBladeNeed = instanceValue.Skills.FirstOrDefault(o => o.Id == 25) != null;
        var huiCount = miscs.Count(o => o.Name == ("回城卷"));
        var superCount = miscs.Count(o => o.stdMode == 0 && GameConstants.Items.SuperPotions.Contains(o.Name));
        var pickMageRate = instanceValue.AccountInfo.role == RoleType.taoist && CharacterStatus.Level > 7 ? 1.2 :
            isMainFull ? (
                isBladeNeed ? 0.2 : (
                    isMageNeed ? 1.2 : 0
                )
            ) : 0;
        var pickMageCount = GameConstants.Items.mageBuyCount * pickMageRate;
        var pickSuperRate = instanceValue.AccountInfo.role == RoleType.mage ? 3 : 1;
        var pickSuperCount = GameConstants.Items.superPickCount * pickSuperRate;


        var canTemp = CharacterStatus.Level >= 24 && GoRunFunction.CapbilityOfTemptation(instanceValue);
        // 法师不捡武器 最简单
        var weaponCount = miscs.Count(o => o.stdMode == 5 || o.stdMode == 6);
        var weacloRate = instanceValue.AccountInfo.role == RoleType.blade ? 10 : 8;
        var maxWeapon = (instanceValue.AccountInfo.role != RoleType.mage && CharacterStatus.Level > 20) ? GameConstants.Items.getKeepWeaponCount(CharacterStatus.Level, instanceValue.AccountInfo.role) * weacloRate
        : GameConstants.Items.getKeepWeaponCount(CharacterStatus.Level, instanceValue.AccountInfo.role);

        var clothCount = miscs.Count(o => o.stdMode == 10 || o.stdMode == 11);
        var maxCloth = (instanceValue.AccountInfo.role != RoleType.mage && CharacterStatus.Level > 20) ? GameConstants.Items.getKeepClothCount(CharacterStatus.Level, instanceValue.AccountInfo.role) * weacloRate
        : GameConstants.Items.getKeepClothCount(CharacterStatus.Level, instanceValue.AccountInfo.role);

        var isMage = instanceValue.AccountInfo.role == RoleType.mage;
        // 武器表
        // 筛选可捡取的物品
        var preferItems = NpcFunction.preferStdEquipment(instanceValue, EquipPosition.Weapon, 99);
        var otherRole = instanceValue.AccountInfo.role == RoleType.blade ? RoleType.taoist : RoleType.blade;
        var otherPreferItems = NpcFunction.preferStdEquipment(instanceValue, EquipPosition.Weapon, 99, otherRole);

        var drops = instanceValue.DropsItems.Where(o => o.Value.IsGodly || (
                !instanceValue.pickupItemIds.Contains(o.Value.Id) &&
                !curinItems.Contains(o.Value.Name)
            // 不是自己的 但是是别人的 不拿
            && (!(!preferItems.Contains(o.Value.Name) && otherPreferItems.Contains(o.Value.Name)))
            // 普通衣服分类. 超级衣服自然都要了 -- todo 其他狍子gender不对
            &&
            (
                (o.Value.Name.Contains("男") || o.Value.Name.Contains("女")) ?
                    ((clothCount < maxCloth) ?
                    (instanceValue.AccountInfo.Gender == 0 ? !o.Value.Name.Contains("男") : !o.Value.Name.Contains("女"))
                    : false)
                : true
            )
            // 回城卷
            && (o.Value.Name == "回城卷" ? huiCount < 2 : true)
            // 药
            && (!(GameConstants.Items.HealPotions.Contains(o.Value.Name) && healCount > GameConstants.Items.healBuyCount))
            && (o.Value.Name.Contains("魔法药") ? (mageCount < pickMageCount) : true)
            && (GameConstants.Items.SuperPotions.Contains(o.Value.Name) ? superCount < pickSuperCount : true)
            &&
            (
                // todo 更多属性获取drop更高效
                GameConstants.Items.weaponList.Contains(o.Value.Name) ? (isMage ? false : (
                    weaponCount < maxWeapon ? true : false
                )) : true
            )
            &&
            (
                Math.Abs(o.Value.X - CharacterStatus.X) < 18 && Math.Abs(o.Value.Y - CharacterStatus.Y) < 18
            )
            ))
            .OrderBy(o => o.Value.IsGodly ? 0 : 1)
            .ThenBy(o => measureGenGoPath(instanceValue, o.Value.X, o.Value.Y)).ToList();

        stopwatch.Stop();
        if (drops.Count > 0)
        {
            instanceValue.GameDebug($"PreparePickupInfo 执行完成，找到 {drops.Count} 个可拾取物品，耗时: {stopwatch.ElapsedMilliseconds}ms");
        }
        if (drops.Count == 0)
        {
            return null;
        }

        return drops;

    }

    /// <summary>
    /// 通用捡取方法
    /// </summary>
    /// <param name="instanceValue">游戏实例</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否成功捡取到物品</returns>
    public static async Task<bool> PerformPickup(MirGameInstanceModel instanceValue, CancellationToken cancellationToken = default, Func<MirGameInstanceModel, bool>? callback = null)
    {
        if (instanceValue.isPickingWay) return false;
        callback = callback ?? ((instanceValue) => true);
        bool pickedAny = false;
        var allTimes = 0;
        var drops = PreparePickupInfo(instanceValue);
        if (drops == null)
        {
            return false;
        }

        instanceValue.isPickingWay = true;
        while (allTimes < 1)
        {
            if (callback(instanceValue))
            {
                instanceValue.isPickingWay = false;
                return false;
            }
            allTimes++;

            foreach (var drop in drops)
            {
                if (callback(instanceValue))
                {
                    instanceValue.isPickingWay = false;
                    return false;
                }
                if (!PickupInfoBasicCheck(instanceValue))
                {
                    instanceValue.GameDebug("放弃拾取物品，位置: ({X}, {Y})", drop.Value.X, drop.Value.Y);
                    instanceValue.isPickingWay = false;
                    return false;
                }

                instanceValue.GameDebug("准备拾取物品，位置: ({X}, {Y})", drop.Value.X, drop.Value.Y);
                bool pathFound = await PerformPathfinding(cancellationToken, instanceValue, drop.Value.X, drop.Value.Y, "", 0, true, drop.Value.IsGodly ? 15 : 10, 30, 0, callback);

                var triedGoPick = 0;
                var maxTriedGoPick = drop.Value.IsGodly ? 9 : 2;
                while (!pathFound && triedGoPick < maxTriedGoPick)
                {
                    triedGoPick++;
                    pathFound = await PerformPathfinding(cancellationToken, instanceValue, drop.Value.X, drop.Value.Y, "", 0, true, 1, 30, 0, callback);
                }

                var miscs2 = instanceValue.Items.Where(o => !o.IsEmpty);
                // 极品满就扔东西 -- todo 还有 自定义极品
                // if (drop.Value.IsGodly && miscs2.Count() == 40)
                // {
                //     // 扔东西
                //     // 挑选一个扔, 一般扔药
                //     var needDropItem = miscs2.FirstOrDefault(o => GameConstants.Items.HealPotions.Contains(o.Name) ||
                //         GameConstants.Items.MagePotions.Contains(o.Name)
                //     );
                //     if (needDropItem != null)
                //     {
                //         // + 6
                //         NpcFunction.EatIndexItem(instanceValue, needDropItem.Index + 6, true);
                //         await Task.Delay(200);
                //     }
                // }
                if (pathFound)
                {
                    await Task.Delay(200);
                    ItemFunction.Pickup(instanceValue);
                    await Task.Delay(600);
                    if (!(instanceValue.chats.LastOrDefault()?.Contains("时间") ?? true))
                    {
                        instanceValue.pickupItemIds.Add(drop.Value.Id);
                    }
                    pickedAny = true;
                    // 一定时间范围内
                }
                else
                {
                    // instanceValue.pickupItemIds.Add(drop.Value.Id);
                }
            }
        }

        instanceValue.isPickingWay = false;
        return pickedAny;
    }

    /// <summary>
    /// 通用屠挖肉方法
    /// </summary>
    /// <param name="instanceValue">游戏实例</param>
    /// <param name="maxBagCount">背包物品数量上限，默认32</param>
    /// <param name="searchRadius">搜索半径，默认13格</param>
    /// <param name="maxTries">每个尸体最大尝试次数，默认20</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否成功屠宰了尸体</returns>
    public static async Task<bool> PerformButchering(MirGameInstanceModel instanceValue,
        int maxBagCount = 32, int searchRadius = 13, int maxTries = 20, CancellationToken cancellationToken = default)
    {
        var allowButch = new List<string> { "鸡", "鹿", "羊" }; // "毒蜘蛛", "蝎子", "洞蛆",


        var miscs = instanceValue.Items.Where(o => !o.IsEmpty);
        if (miscs.Count() >= maxBagCount)
        {
            return false; // 背包太满，不进行屠宰
        }

        var CharacterStatus = instanceValue.CharacterStatus!;
        var bodys = instanceValue.Monsters.Values.Where(o => o.isDead && allowButch.Contains(o.Name) && !o.isButched
            && Math.Max(Math.Abs(o.X - CharacterStatus.X), Math.Abs(o.Y - CharacterStatus.Y)) < searchRadius)
            .OrderBy(o => Math.Max(Math.Abs(o.X - CharacterStatus.X), Math.Abs(o.Y - CharacterStatus.Y)));

        bool butcheredAny = false;

        foreach (var body in bodys)
        {
            instanceValue.GameDebug("准备屠宰: {Name}, 位置: ({X}, {Y})", body.Name, body.X, body.Y);
            bool pathFound = await PerformPathfinding(cancellationToken, instanceValue, body.X, body.Y, "", 2, true, 1, 30);
            if (pathFound)
            {
                // 要持续屠宰, 直到尸体消失, 最大尝试次数
                var tried = 0;
                while (tried < maxTries)
                {
                    SendMirCall.Send(instanceValue, 3030, new nint[] { (nint)body.X, (nint)body.Y, 0, body.Id });
                    await Task.Delay(500);
                    MonsterFunction.ReadMonster(instanceValue);
                    if (body.isButched)
                    {
                        butcheredAny = true;
                        break;
                    }
                    tried++;
                }
            }
        }

        return butcheredAny;
    }

    /// <summary>
    /// 通用躲避方法
    /// </summary>
    /// <param name="instanceValue">游戏实例</param>
    /// <param name="centerPoint">中心点坐标</param>
    /// <param name="dangerDistance">危险距离阈值，默认为1</param>
    /// <param name="safeDistance">安全距离范围，默认为2-3格</param>
    /// <param name="searchRadius">搜索半径，默认为10</param>
    /// <param name="maxMonstersNearby">身边允许的最大怪物数量，超过此数量才躲避，默认为0（即有怪就躲）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否成功躲避</returns>
    public static async Task<int> PerformEscape(MirGameInstanceModel instanceValue, (int x, int y) centerPoint,
        int dangerDistance = 1, (int min, int max) safeDistance = default, int searchRadius = 10,
        int maxMonstersNearby = 0, CancellationToken cancellationToken = default)
    {
        if (safeDistance == default) safeDistance = (2, 3);

        var escapeStopwatch = System.Diagnostics.Stopwatch.StartNew();

        // 危险点 - 所有活着的怪物
        var dangerPoints = instanceValue.Monsters.Values.Where(o => o.stdAliveMon).Select(o => (o.X, o.Y));

        // 检查身边怪物数量是否超过阈值
        var characterPos = (instanceValue.CharacterStatus!.X, instanceValue.CharacterStatus.Y);
        var nearbyMonstersCount = dangerPoints.Count(dp =>
            Math.Max(Math.Abs(dp.Item1 - characterPos.Item1), Math.Abs(dp.Item2 - characterPos.Item2)) <= dangerDistance);

        if (nearbyMonstersCount <= maxMonstersNearby)
        {
            return -1; // 身边怪物数量未超过阈值，不需要躲避
        }
        // 检查当前位置是否已经是靠墙点，如果是则不需要逃跑
        if (WallPointsUtils.IsWallPoint(instanceValue.CharacterStatus.MapId, instanceValue.CharacterStatus.X, instanceValue.CharacterStatus.Y))
        {
            return 3; // 无需躲避, 因为靠墙, 至少3
        }

        instanceValue.GameInfo($"身边有 {nearbyMonstersCount} 只怪物，超过阈值 {maxMonstersNearby}，开始躲避");

        // 地图障碍点数据
        var (mapWidth, mapHeight, mapObstacles) = retriveMapObstacles(instanceValue.CharacterStatus.MapId);
        var localObstacles = getLocalObstacles(instanceValue.CharacterStatus.MapId, centerPoint.x, centerPoint.y, searchRadius);

        // 其他障碍点数据，比如玩家和怪物
        var actorObstacles = instanceValue.Monsters.Values.Where(o => !o.isDead).Select(o => (o.X, o.Y));
        var obstacles = localObstacles.Concat(actorObstacles).ToHashSet();

        // 计算逃跑点
        var escapePoints = new List<(int x, int y, int distance)>();

        for (int y = centerPoint.y - searchRadius; y <= centerPoint.y + searchRadius; y++)
        {
            for (int x = centerPoint.x - searchRadius; x <= centerPoint.x + searchRadius; x++)
            {
                if (x >= 0 && x < mapWidth && y >= 0 && y < mapHeight && !obstacles.Contains((x, y)))
                {
                    // 计算到所有危险点的最小距离
                    var minDangerDistance = dangerPoints.Select(dp =>
                        Math.Max(Math.Abs(dp.Item1 - x), Math.Abs(dp.Item2 - y))
                    ).Min();

                    if (minDangerDistance >= safeDistance.min && minDangerDistance <= safeDistance.max)
                    {
                        var centerDistance = Math.Max(Math.Abs(x - characterPos.Item1), Math.Abs(y - characterPos.Item2));
                        if (centerDistance < searchRadius)
                        {
                            escapePoints.Add((x, y, centerDistance));
                        }
                    }
                }
            }
        }

        // 按优先级排序：优先距离危险点更远的，然后按到角色距离排序
        var bestEscapePoint = (0, 0, 0);
        var allEscapePoints = escapePoints
            .OrderByDescending(ep => dangerPoints.Select(dp =>
                Math.Max(Math.Abs(dp.Item1 - ep.x), Math.Abs(dp.Item2 - ep.y))
            ).Min())
            .ThenBy(ep => ep.distance);

        foreach (var ep in allEscapePoints)
        {
            var pathDistance = measureGenGoPath(instanceValue, ep.x, ep.y);
            if (pathDistance < searchRadius)
            {
                bestEscapePoint = ep;
                break;
            }
        }

        escapeStopwatch.Stop();

        if (bestEscapePoint != default)
        {
            instanceValue.GameInfo($"躲避到安全点: ({bestEscapePoint.Item1}, {bestEscapePoint.Item2}) [计算耗时: {escapeStopwatch.ElapsedMilliseconds}ms]");
            MonsterFunction.SlayingMonsterCancel(instanceValue!);
            await PerformPathfinding(cancellationToken, instanceValue, bestEscapePoint.Item1, bestEscapePoint.Item2, "", 0, true, 999, 30);
            return 1;
        }
        else
        {
            instanceValue.GameWarning($"未找到合适的逃跑点 [计算耗时: {escapeStopwatch.ElapsedMilliseconds}ms]");
            return 0;
        }
    }

    private static bool CheckNeedPerformEscapeWithSafePts(MirGameInstanceModel instanceValue)
    {
        var preferSafePtsMinMonCount = 10; // 先看看
        var preferSafePtsAreaRadius = 7; // 先看看 人口密度 10/64 
        var currentMonsCount = instanceValue.Monsters.Values.Count(o => o.stdAliveMon && Math.Abs(o.X - instanceValue.CharacterStatus.X) < preferSafePtsAreaRadius && Math.Abs(o.Y - instanceValue.CharacterStatus.Y) < preferSafePtsAreaRadius);
        var isPreferSafe = currentMonsCount > preferSafePtsMinMonCount;
        return isPreferSafe;
    }

    public static async Task<int> PerformEscapeWithSafePts(MirGameInstanceModel instanceValue, CancellationToken _cancellationToken)
    {
        if (CheckIfSurrounded(instanceValue.CharacterStatus.MapId, instanceValue.CharacterStatus.X, instanceValue.CharacterStatus.Y,
                instanceValue.MonstersByPosition
        ))
        {
            return 2; // 无法躲避
        }
        var CharacterStatus = instanceValue.CharacterStatus!;
        var mainChars = GameState.GameInstances[0]!.CharacterStatus!;
        var isMain = instanceValue.AccountInfo!.IsMainControl;
        var maxMonstersNearby = instanceValue.AccountInfo!.role == RoleType.blade ? 4 : 2;
        var isPreferSafe = CheckNeedPerformEscapeWithSafePts(instanceValue);

        // 只跟main搜, 没做main是否中心判断, 因为可能会堵住
        var centerPoint = isMain ? (CharacterStatus.X, CharacterStatus.Y) : (mainChars.X, mainChars.Y);
        if (isPreferSafe)
        {
            // 检查当前位置是否已经是靠墙点，如果是则不需要逃跑
            if (WallPointsUtils.IsWallPoint(instanceValue.CharacterStatus.MapId, instanceValue.CharacterStatus.X, instanceValue.CharacterStatus.Y))
            {
                return 3; // 无需躲避, 因为靠墙, 至少3
            }
            var nearestWallPts = WallPointsUtils.FindAllWallPointsInRange(instanceValue.CharacterStatus.MapId, centerPoint.Item1, centerPoint.Item2, 8);
            var firstPts = nearestWallPts.Where(o =>
            MonsterFunction.GetMonstersByPosition(instanceValue.MonstersByPosition, o.x, o.y).Count == 0
            ).FirstOrDefault();
            if (firstPts != default)
            {
                MonsterFunction.SlayingMonsterCancel(instanceValue!);
                var isSS = await PerformPathfinding(_cancellationToken, instanceValue, firstPts.x, firstPts.y, "", 0, true, 999, 30);
                return isSS ? 0 : -1;
            }
            else
            {
                return -1;
            }
        }
        else
        {
            return await PerformEscape(instanceValue, centerPoint, dangerDistance: 1, safeDistance: (2, 3), searchRadius: 10, maxMonstersNearby: maxMonstersNearby, cancellationToken: _cancellationToken);
        }
    }

    public static (int x, int y) getNextPostion(int x, int y, byte dir, byte steps)
    {

        switch (dir)
        {
            case 5:
                x -= steps;
                y += steps;
                break;
            case 4:
                y += steps;
                break;
            case 3:
                x += steps;
                y += steps;
                break;
            case 6:
                x -= steps;
                break;
            case 2:
                x += steps;
                break;
            case 7:
                x -= steps;
                y -= steps;
                break;
            case 0:
                y -= steps;
                break;
            case 1:
                x += steps;
                y -= steps;
                break;
        }
        return (x, y);
    }


    public static void GoRunAlgorithm(MirGameInstanceModel gameInstance, int x, int y, byte dir, byte steps)
    {
        // gameInstance.GameDebug("执行寻路算法移动，目标: ({X}, {Y}), 方向: {Dir}, 步数: {Steps}", x, y, dir, steps);
        int typePara = 0;
        switch (steps)
        {
            case 1:
                typePara = 0xbc3;
                break;
            case 2:
                typePara = 0xbc5;
                break;
            default:
                typePara = 0xbc3;
                break;
        }
        var (nextX, nextY) = getNextPostion(x, y, dir, steps);

        SendMirCall.Send(gameInstance, 1001, new nint[] { nextX, nextY, dir, typePara, GameState.MirConfig["角色基址"], GameState.MirConfig["UpdateMsg"] });
    }

    public static async Task GoTurn(MirGameInstanceModel gameInstance, byte dir)
    {
        // my
        var myX = gameInstance!.CharacterStatus!.X;
        var myY = gameInstance!.CharacterStatus!.Y;
        SendMirCall.Send(gameInstance, 1000, new nint[] { myX, myY, dir, 0xbc2,
        GameState.MirConfig["角色基址"], GameState.MirConfig["SendMsg"] });
        await Task.Delay(300);
    }

    public static void cici(MirGameInstanceModel gameInstance, byte dir)
    {

        if (gameInstance.AccountInfo.role != RoleType.blade)
        {
            return;
        }
        if (gameInstance.bladeCiciLastTime + 1200 > Environment.TickCount)
        {
            return;
        }
        gameInstance.bladeCiciLastTime = Environment.TickCount;

        // my
        var myX = gameInstance!.CharacterStatus!.X;
        var myY = gameInstance!.CharacterStatus!.Y;

        SendMirCall.Send(gameInstance, 1002, new nint[] { myX, myY, dir, 0xbcb,
        GameState.MirConfig["角色基址"], GameState.MirConfig["SendMsg"] });
    }




    public static void openDoor(MirGameInstanceModel gameInstance, int x, int y)
    {
        SendMirCall.Send(gameInstance, 9020, new nint[] { x, y });
    }

    static private Dictionary<string, (int, int, byte[])> MapBasicInfo { get; set; } = new Dictionary<string, (int, int, byte[])>();

    public static (int width, int height, byte[] obstacles) retriveMapObstacles(string MapId)
    {
        var id = MapId;
        var width = 0;
        var height = 0;
        var obstacles = new byte[0];
        if (!MapBasicInfo.TryGetValue(id, out var data))
        {
            // 读文件获取
            var configPath = Path.Combine(Directory.GetCurrentDirectory(), "config/server-define/unity-config/mapc-out", id + ".mapc");
            // binary
            var bytes = File.ReadAllBytes(configPath);
            width = BitConverter.ToInt32(bytes, 0);
            height = BitConverter.ToInt32(bytes, 4);
            obstacles = new byte[bytes.Length - 8];
            Array.Copy(bytes, 8, obstacles, 0, obstacles.Length);
            // 地图0 有个bug 特殊处理几个点
            if (id == "0")
            {
                var bugPoints = new (int, int)[] { };
                if (GameState.gamePath != "ZC.H.exe")
                {
                    bugPoints = new (int, int)[] {
                    (418,171),
                    (418,172),
                    (417,171),
                    (417,170),
                    (416,170),
                    (419,172)
                    };
                }
                else
                {
                    bugPoints = new (int, int)[] {
                    (266,198),
                    (267,197),
                    (268,196),
                    (265,198),
                    (266,197),
                    (267,196),

                    (271,324),
                    (273,327),
                    (272,326),
                    (271,325),
                    (273,326),
                    (272,325),

                    (399,333),
                    (397,334),
                    (398,333),
                    (399,332),
                    (397,335),
                    (398,334),

                    (418,171),
                    (418,172),
                    (417,171),
                    (417,170),
                    (416,170),
                    (419,172)
                };
                }

                foreach (var point in bugPoints)
                {
                    obstacles[point.Item2 * width + point.Item1] = 1;
                }
            }
            MapBasicInfo[id] = (width, height, obstacles);
        }
        else
        {
            (width, height, var cachedObstacles) = data;
            obstacles = new byte[cachedObstacles.Length];
            Array.Copy(cachedObstacles, 0, obstacles, 0, cachedObstacles.Length);
        }

        // obstacles 前2个int 32是宽高
        return (width, height, (byte[])obstacles.Clone());
    }
    public static List<(int x, int y)> getLocalObstacles(string mapId, int centerX, int centerY, int halfSize)
    {
        var localObstacles = new List<(int x, int y)>();
        var (mapWidth, mapHeight, mapObstacles) = retriveMapObstacles(mapId);
        for (int y = centerY - halfSize; y <= centerY + halfSize; y++)
        {
            for (int x = centerX - halfSize; x <= centerX + halfSize; x++)
            {
                // 检查坐标是否在地图范围内
                if (x >= 0 && x < mapWidth && y >= 0 && y < mapHeight)
                {
                    // 一维数组索引：y * width + x
                    int index = y * mapWidth + x;
                    if (index < mapObstacles.Length && mapObstacles[index] == 1)
                    {
                        localObstacles.Add((x, y));
                    }
                }
            }
        }
        return localObstacles;
    }

    public static List<(byte dir, byte steps, int x, int y)> genGoPath(MirGameInstanceModel gameInstance, int targetX, int targetY,
    int blurRange = 0,
    bool nearBlur = false
    )
    {
        var sw = Stopwatch.StartNew();
        var monsPos = GetMonsPos(gameInstance!.Monsters);
        var (width, height, data) = retriveMapObstacles(gameInstance.CharacterStatus.MapId);
        int myX = gameInstance!.CharacterStatus!.X;
        int myY = gameInstance!.CharacterStatus!.Y;
        // 添加怪物位置作为障碍点
        foreach (var pos in monsPos)
        {
            int monX = pos[0];
            int monY = pos[1];
            if (monX >= 0 && monX < width && monY >= 0 && monY < height)
            {
                data[monY * width + monX] = 1;
            }
        }

        // 检查起点和终点是否为障碍物
        if (blurRange > 0)
        {
            List<(int X, int Y)> candidatePoints = new List<(int X, int Y)>();

            // 以target为中心，在range范围内选取n*n的范围
            for (int y = targetY - blurRange; y <= targetY + blurRange; y++)
            {
                for (int x = targetX - blurRange; x <= targetX + blurRange; x++)
                {
                    if (x >= 0 && x < width && y >= 0 && y < height && data[y * width + x] != 1)
                    {
                        // 计算到起点和终点的距离
                        double distToStart = Math.Sqrt(Math.Pow(x - myX, 2) + Math.Pow(y - myY, 2));
                        double distToTarget = Math.Sqrt(Math.Pow(x - targetX, 2) + Math.Pow(y - targetY, 2));
                        // 综合距离评分：起点距离权重0.7，终点距离权重0.3
                        //double score = nearBlur ?
                        //    (99 * distToStart + 0.3 * distToTarget) :  // 近距离优先
                        //    (0.3 * distToStart + 99 * distToTarget);   // 远距离优先

                        candidatePoints.Add((x, y));
                    }
                }
            }

            // 根据综合距离评分排序
            //candidatePoints = candidatePoints.OrderBy(p => p.Distance).ToList();

            targetX = -1;
            targetY = -1;

            if (candidatePoints.Count > 0)
            {
                if (blurRange == 1)
                {
                    // 基本是单点扩 所以用基本测距
                    var selectedPoint = candidatePoints.OrderBy(ep => Math.Max(Math.Abs(myX - ep.X), Math.Abs(myY - ep.Y))).FirstOrDefault();
                    targetX = selectedPoint.X;
                    targetY = selectedPoint.Y;
                }
                else
                {
                    // 从前3个点中随机选择一个（如果不足3个则在现有点中随机选择）
                    var random = new Random();
                    //var topPoints = candidatePoints.Take(Math.Min(5, candidatePoints.Count)).ToList();
                    var selectedPoint = candidatePoints[random.Next(candidatePoints.Count)];
                    targetX = selectedPoint.X;
                    targetY = selectedPoint.Y;
                }

                // gameInstance.GameDebug($"从{candidatePoints.Count}个最佳点中随机选择目标点: ({targetX}, {targetY})");
            }

            if (targetX == -1)
            {
                sw.Stop();
                if (sw.ElapsedMilliseconds > 10)
                    gameInstance.GameDebug($"无法找到有效的模糊目标点，耗时: {sw.ElapsedMilliseconds}ms");
                return new List<(byte dir, byte steps, int x, int y)>();
            }
        }
        else if (data[targetY * width + targetX] == 1)
        {
            sw.Stop();
            if (sw.ElapsedMilliseconds > 10)
                gameInstance.GameDebug($"目标点不可达，耗时: {sw.ElapsedMilliseconds}ms");
            return new List<(byte dir, byte steps, int x, int y)>();
        }

        // 分距离, 100以内直接a星
        List<(byte dir, byte steps, int x, int y)> path = new List<(byte dir, byte steps, int x, int y)>();
        path = FindPathCoreSmallWithAStar(width, height, data, myX, myY, targetX, targetY);
        // 优化路径 TODO 暂时先不用 费血
        path = OptimizePath(path);
        sw.Stop();
        // gameInstance.GameDebug($"寻路完成: 起点({myX},{myY}) -> 终点({targetX},{targetY}), 路径长度: {path.Count}, 总耗时(含数据准备): {sw.ElapsedMilliseconds}ms");
        return path.Select(p => (p.dir, p.steps, p.x, p.y)).ToList();
    }




    /// <summary>
    /// JPS寻路算法实现
    /// </summary>
    private class Node
    {
        public int X { get; set; }
        public int Y { get; set; }
        public Node? Parent { get; set; }
        public int G { get; set; }
        public int H { get; set; }
        public int F => G + H;
        public byte StepSize { get; set; }
        public byte Direction { get; set; }

        public Node(int x, int y, Node? parent = null)
        {
            X = x;
            Y = y;
            Parent = parent;
            StepSize = 2; // 默认2步移动
        }
    }

    private static byte GetDirectionFromDelta(int dx, int dy)
    {
        if (dx == 0 && dy == -1) return 0;      // 上
        if (dx == 1 && dy == -1) return 1;      // 右上
        if (dx == 1 && dy == 0) return 2;       // 右
        if (dx == 1 && dy == 1) return 3;       // 右下
        if (dx == 0 && dy == 1) return 4;       // 下
        if (dx == -1 && dy == 1) return 5;      // 左下
        if (dx == -1 && dy == 0) return 6;      // 左
        if (dx == -1 && dy == -1) return 7;     // 左上
        return 0;
    }

    private static byte GetDirectionFromDelta2(int dx, int dy)
    {
        if (dx == 0 && dy == -2) return 0;      // 上
        if (dx == 2 && dy == -2) return 1;      // 右上
        if (dx == 2 && dy == 0) return 2;       // 右
        if (dx == 2 && dy == 2) return 3;       // 右下
        if (dx == 0 && dy == 2) return 4;       // 下
        if (dx == -2 && dy == 2) return 5;      // 左下
        if (dx == -2 && dy == 0) return 6;      // 左
        if (dx == -2 && dy == -2) return 7;     // 左上
        return 0;
    }

    private static List<(byte dir, byte steps, int x, int y)> FindPathCoreSmallWithAStar(int width, int height, byte[] obstacles, int startX, int startY, int targetX, int targetY)
    {
        var openSet = new PriorityQueue<Node, int>();
        var closedSet = new HashSet<string>();
        var startNode = new Node(startX, startY);
        startNode.G = 0;
        startNode.H = Math.Abs(targetX - startX) + Math.Abs(targetY - startY);
        openSet.Enqueue(startNode, startNode.F);

        while (openSet.Count > 0)
        {
            var current = openSet.Dequeue();

            if (current.X == targetX && current.Y == targetY)
            {
                return ReconstructPath(current);
            }

            var key = $"{current.X},{current.Y}";
            if (closedSet.Contains(key)) continue;
            closedSet.Add(key);

            // 8个方向
            int[] dx = { 0, 1, 1, 1, 0, -1, -1, -1 };
            int[] dy = { -1, -1, 0, 1, 1, 1, 0, -1 };

            for (int i = 0; i < 8; i++)
            {
                int newX = current.X + dx[i];
                int newY = current.Y + dy[i];

                if (newX < 0 || newX >= width || newY < 0 || newY >= height) continue;
                if (obstacles[newY * width + newX] == 1) continue;

                var neighbor = new Node(newX, newY, current);
                neighbor.Direction = (byte)i;
                neighbor.StepSize = 1;

                // 对角线移动代价和直线移动一样
                neighbor.G = current.G + 1;
                neighbor.H = Math.Abs(targetX - newX) + Math.Abs(targetY - newY);

                if (!closedSet.Contains($"{newX},{newY}"))
                {
                    openSet.Enqueue(neighbor, neighbor.F);
                }
            }
        }

        return new List<(byte dir, byte steps, int x, int y)>();
    }

    private static Node? Jump(int x, int y, int px, int py, int width, int height, byte[] obstacles, int targetX, int targetY)
    {
        if (x < 0 || x >= width || y < 0 || y >= height || obstacles[y * width + x] == 1)
            return null;

        if (x == targetX && y == targetY)
            return new Node(x, y);

        int dx = x - px;
        int dy = y - py;

        // 对角线移动
        if (dx != 0 && dy != 0)
        {
            // 检查强制邻居
            if ((IsWalkable(x - dx, y + dy, width, height, obstacles) && !IsWalkable(x - dx, y, width, height, obstacles)) ||
                (IsWalkable(x + dx, y - dy, width, height, obstacles) && !IsWalkable(x, y - dy, width, height, obstacles)))
            {
                return new Node(x, y);
            }

            // 递归检查水平和垂直方向
            if (Jump(x + dx, y, x, y, width, height, obstacles, targetX, targetY) != null ||
                Jump(x, y + dy, x, y, width, height, obstacles, targetX, targetY) != null)
            {
                return new Node(x, y);
            }
        }
        else // 直线移动
        {
            if (dx != 0) // 水平移动
            {
                // 检查强制邻居
                if ((IsWalkable(x + dx, y + 1, width, height, obstacles) && !IsWalkable(x, y + 1, width, height, obstacles)) ||
                    (IsWalkable(x + dx, y - 1, width, height, obstacles) && !IsWalkable(x, y - 1, width, height, obstacles)))
                {
                    return new Node(x, y);
                }
            }
            else // 垂直移动
            {
                // 检查强制邻居
                if ((IsWalkable(x + 1, y + dy, width, height, obstacles) && !IsWalkable(x + 1, y, width, height, obstacles)) ||
                    (IsWalkable(x - 1, y + dy, width, height, obstacles) && !IsWalkable(x - 1, y, width, height, obstacles)))
                {
                    return new Node(x, y);
                }
            }
        }

        // 如果没有强制邻居，继续在同一方向跳跃
        return Jump(x + dx, y + dy, x, y, width, height, obstacles, targetX, targetY);
    }

    private static bool IsWalkable(int x, int y, int width, int height, byte[] obstacles)
    {
        return x >= 0 && x < width && y >= 0 && y < height && obstacles[y * width + x] != 1;
    }

    private static List<(byte dir, byte steps, int x, int y)> ReconstructPath(Node node)
    {
        var path = new List<(byte dir, byte steps, int x, int y)>();
        var current = node;

        while (current.Parent != null)
        {
            path.Add((current.Direction, current.StepSize, current.X, current.Y));
            current = current.Parent;
        }

        path.Reverse();
        return path;
    }

    private static List<(byte dir, byte steps, int x, int y)> OptimizePath(List<(byte dir, byte steps, int x, int y)> path)
    {
        if (path.Count < 2) return path;

        var optimizedPath = new List<(byte dir, byte steps, int x, int y)>();
        int i = 0;

        while (i < path.Count)
        {
            var current = path[i];

            // 检查是否可以和下一个点合并
            if (i + 1 < path.Count && current.steps == 1)
            {
                var next = path[i + 1];
                bool canMerge = current.dir == next.dir &&
                               next.steps == 1 &&
                               IsConsecutivePoints(current.x, current.y, next.x, next.y, current.dir);

                if (canMerge)
                {
                    var merged = (current.dir, (byte)2, next.x, next.y);
                    optimizedPath.Add(merged);
                    i += 2;
                    continue;
                }
            }
            optimizedPath.Add(current);
            i++;
        }

        return optimizedPath;
    }

    // 检查两个点是否是连续的（基于方向）
    private static bool IsConsecutivePoints(int x1, int y1, int x2, int y2, byte dir)
    {
        // 根据方向计算期望的下一个点的坐标
        var (expectedX, expectedY) = getNextPostion(x1, y1, dir, 1);
        return expectedX == x2 && expectedY == y2;
    }


    public static int[][] GetMonsPos(ConcurrentDictionary<int, MonsterModel> Monsters)
    {
        var monsters = Monsters.Where(o => !o.Value.isDead);
        int[][] monsPos = new int[monsters.Count()][];
        int index = 0;
        foreach (var monster in monsters)
        {
            monsPos[index++] = new int[] {
                monster.Value.X,
                monster.Value.Y
            };
        }
        return monsPos;
    }

    public static (int, int)[] GenMobCleanPairs(MirGameInstanceModel instanceValue, string mapId)
    {
        var CharacterStatus = instanceValue.CharacterStatus!;
        var patrolSteps = 10;
        var portalStartX = 0;
        var portalEndX = 0;
        var portalStartY = 0;
        var portalEndY = 0;
        var fixedPoints = new List<(int, int)>();
        if (mapId == "0")
        {
            // 地图优化
            portalStartX = 200;
            portalEndX = 300;
            portalStartY = 550;
            portalEndY = 620;
            if (CharacterStatus.Level >= GameConstants.NoobLevel)
            {
                portalStartX = 50;
                portalEndX = 250;
                portalStartY = 350;
                portalEndY = 550;
            }


            // 生成矩形区域内的所有点位
            for (int x = portalStartX; x <= portalEndX; x += patrolSteps)
            {
                // 根据x的奇偶性决定y的遍历方向，形成蛇形路线
                var yStart = (x - portalStartX) / patrolSteps % 2 == 0 ? portalStartY : portalEndY;
                var yEnd = (x - portalStartX) / patrolSteps % 2 == 0 ? portalEndY : portalStartY;
                var yStep = (x - portalStartX) / patrolSteps % 2 == 0 ? patrolSteps : -patrolSteps;

                for (int y = yStart; yStep > 0 ? y <= yEnd : y >= yEnd; y += yStep)
                {
                    // 生成点位
                    fixedPoints.Add((x, y));
                    // Log.Debug($"生成巡逻点: ({x}, {y})");
                }
            }
        }
        else
        {
            // 获取该地图数据
            // var (width, height, obstacles) = retriveMapObstacles(instanceValue!);
            var hangPoints = HangPointData.GetHangPoints(mapId);
            foreach (var point in hangPoints)
            {
                fixedPoints.Add((point[0], point[1]));
            }
        }


        instanceValue.GameInfo($"共生成 地图 {CharacterStatus.MapId} 的 {fixedPoints.Count} 个固定巡逻点 from {portalStartX} to {portalEndX} from {portalStartY} to {portalEndY}");

        // 转换为数组
        return fixedPoints.ToArray();
    }

    // 1为中间态 可打可不打 武器爆了也不管
    public static byte whoIsConsumer(MirGameInstanceModel instanceValue)
    {
        if (instanceValue.AccountInfo.IsMainControl) return 2;
        if (instanceValue.AccountInfo.role == RoleType.mage)
        {
            // 法师永远别砍
            return 0;
        }
        // 或者组里有大佬, 且自己很菜
        var mainInstance = GameState.GameInstances[0];
        if (mainInstance.IsAttached)
        {
            // 后期战道可以自己打一点
            var diff = mainInstance.CharacterStatus!.Level - instanceValue.CharacterStatus!.Level;
            if (diff < 10 || instanceValue.CharacterStatus!.Level > 16)
            {
                return 1;
            }
        }
        return 0;
    }

    public static async Task<bool> NormalAttackPoints(MirGameInstanceModel instanceValue, CancellationToken _cancellationToken, bool forceSkip, Func<MirGameInstanceModel, bool> checker, string mapId = "", bool cleanAll = false, int searchRds = 10)
    {

        if (instanceValue.CharacterStatus!.isEnhanceDead)
        {
            instanceValue.GameWarning("角色已死亡，无法执行巡逻攻击");
            return false;
        }
        if (checker(instanceValue!))
        {
            return true;
        }
        if (mapId == "") mapId = instanceValue.CharacterStatus.MapId;
        var patrolPairs = new (int, int)[] { (0, 0) };
        if (!forceSkip)
        {
            patrolPairs = GenMobCleanPairs(instanceValue, mapId);
        }

        if (instanceValue.AccountInfo.IsMainControl)
        {
            instanceValue.GameDebug("开始巡逻攻击，巡逻点数量: {Count}", patrolPairs.Length);
        }


        // 等级高了不打鸡鹿
        var allowMonsters = GameConstants.GetAllowMonsters(instanceValue, instanceValue.CharacterStatus!.Level, instanceValue.AccountInfo.role);
        // 当前巡回
        var curP = 0;
        var direction = 1; // 1表示正向(0->N), -1表示反向(N->0)
        var CharacterStatus = instanceValue.CharacterStatus!;
        if (!forceSkip)
        {
            // 查找离我最近的巡逻点 
            // todo 不是当前地图要找洞口点, 没别的需求是别的点 
            var finalMyX = CharacterStatus.X;
            var finalMyY = CharacterStatus.Y;
            if (CharacterStatus.MapId != mapId)
            {
                var connectionsPath = mapConnectionService.FindPath(CharacterStatus.MapId, mapId);
                if (connectionsPath != null)
                {
                    var last = connectionsPath.Last();
                    finalMyX = last.To.X;
                    finalMyY = last.To.Y;
                }
            }
            curP = patrolPairs
                .Select((p, i) => (i, dis: Math.Max(Math.Abs(p.Item1 - finalMyX), Math.Abs(p.Item2 - finalMyY))))
                .MinBy(x => x.dis)
                .i;
        }
        // 巡逻太多次了 有问题
        var mainInstance = GameState.GameInstances[0];
        var patrolTried = 0;
        var canTemp = CapbilityOfTemptation(instanceValue);
        var canLight = CapbilityOfLighting(instanceValue);
        var canBaolie = CapbilityOfBaolie(instanceValue);
        // 

        while (true)
        {
            if (instanceValue.CharacterStatus!.isEnhanceDead)
            {
                instanceValue.GameWarning("角色已死亡，无法执行巡逻攻击");
                return false;
            }
            // instanceValue.GameDebug("开始巡逻攻击，巡逻点 {CurP}", curP);
            await Task.Delay(100);
            patrolTried++;
            if (whoIsConsumer(instanceValue!) == 2 && patrolTried > 200)
            {
                instanceValue.GameWarning("巡逻攻击失败，巡逻点 {CurP}", curP);
                return false;
            }
            // 不寻路模式, 其实就是只打怪, 需要抽象
            // 
            var followDetectDistance = instanceValue.AccountInfo.role == RoleType.mage ? 7 : 9;
            // 主从模式
            // 主人是点位
            var (px, py) = (0, 0);
            if (checker(instanceValue!))
            {
                return true;
            }
            var isToPoint = false;
            if (!forceSkip)
            {
                // 从是跟随
                if (instanceValue.AccountInfo.IsMainControl)
                {
                    // 主人是点位
                    (px, py) = patrolPairs[curP];
                    bool _whateverPathFound = await PerformPathfinding(_cancellationToken, instanceValue!, px, py, mapId, 4, true, 0, 9999, 0,
                            (instanceValue) =>
                            {
                                if (checker(instanceValue))
                                {
                                    return true;
                                }
                                var slashRemainHP = 80;
                                var slasher = whoIsConsumer(instanceValue!) > 0;
                                var enoughBBCanHit = instanceValue.Monsters.Values.Where(o => !o.isDead && o.isTeamMons &&
                                    Math.Max(Math.Abs(o.X - CharacterStatus.X), Math.Abs(o.Y - CharacterStatus.Y)) < 9).Count() > 3;

                                var temp = GameConstants.GetAllowMonsters(instanceValue, instanceValue.CharacterStatus!.Level, instanceValue.AccountInfo.role);
                                var monsters = instanceValue.Monsters.Where(o => o.Value.stdAliveMon && temp.Contains(o.Value.Name) &&
                                Math.Max(Math.Abs(o.Value.X - instanceValue.CharacterStatus.X), Math.Abs(o.Value.Y - instanceValue.CharacterStatus.Y)) < searchRds
                                && (slasher && o.Value.Appr != 40 ? (enoughBBCanHit ?
                                    (o.Value.CurrentHP > 0
                                        ? ((o.Value.CurrentHP > slashRemainHP) || (o.Value.MaxHP < slashRemainHP) || (o.Value.MaxHP >= 500))
                                    : true) : true)
                                    : true)
                                ).OrderBy(o =>
                                 Math.Max(Math.Abs(o.Value.X - instanceValue.CharacterStatus.X), Math.Abs(o.Value.Y - instanceValue.CharacterStatus.Y))
                                ).FirstOrDefault();

                                if (monsters.Value != null)
                                {
                                    var fm = monsters.Value;
                                    var distance = measureGenGoPath(instanceValue!, fm.X, fm.Y);
                                    if (distance <= 30)
                                    {
                                        return true;
                                    }
                                }
                                // 自定义
                                var drops = PreparePickupInfo(instanceValue);
                                if (drops != null)
                                {
                                    return true;
                                }
                                return false;
                            }
                         );
                    isToPoint = true;
                }
                else
                {
                    (px, py) = (mainInstance.CharacterStatus.X, mainInstance.CharacterStatus.Y);
                    // instanceValue.GameInfo("跟随 in init: {X}, {Y}", px, py);
                    // 判断主任是否太远, 不同图可以认为太远, 或者是距离过长也是
                    var soFarGezi = (instanceValue.CharacterStatus.MapId != mainInstance.CharacterStatus.MapId
                    || Math.Max(Math.Abs(px - CharacterStatus.X), Math.Abs(py - CharacterStatus.Y)) > 30) ? 999 : 30;
                    // 距离之内不要乱跑 但是为了防卡死 偶尔东东
                    if (
                        Math.Max(Math.Abs(px - CharacterStatus.X), Math.Abs(py - CharacterStatus.Y)) < 10 &&
                        new Random().Next(100) < 90
                    )
                    {
                        await Task.Delay(500);
                    }
                    else
                    {
                        if (checker(instanceValue!))
                        {
                            return true;
                        }
                        bool _whateverPathFound = await PerformPathfinding(_cancellationToken, instanceValue!, px, py, mainInstance.CharacterStatus.MapId, 4, true, 12, soFarGezi, 0,
                            (instanceValue) =>
                            {
                                if (checker(instanceValue))
                                {
                                    return true;
                                }
                                // 自定义
                                var OFFSET_TO_COMP = 10;
                                if (Math.Abs(GameState.GameInstances[0].CharacterStatus.X - px) > OFFSET_TO_COMP ||
                                Math.Abs(GameState.GameInstances[0].CharacterStatus.Y - py) > OFFSET_TO_COMP)
                                {
                                    return true;
                                }
                                // 自定义
                                var drops = PreparePickupInfo(instanceValue);
                                if (drops != null)
                                {
                                    return true;
                                }
                                return false;
                            }
                         );
                    }
                }
                await PerformPickup(instanceValue, _cancellationToken, checker);
            }
            if (checker(instanceValue!))
            {
                return true;
            }

            await PerformPickup(instanceValue, _cancellationToken, checker);

            var monsterTried = 0;
            // 无怪退出
            while (true)
            {
                var enoughBBCanHit = instanceValue.Monsters.Values.Where(o => !o.isDead && o.isTeamMons &&
                Math.Max(Math.Abs(o.X - CharacterStatus.X), Math.Abs(o.Y - CharacterStatus.Y)) < 9).Count() > 3;

                CharacterStatus = instanceValue.CharacterStatus;
                if (instanceValue.CharacterStatus!.isEnhanceDead)
                {
                    instanceValue.GameWarning("角色已死亡，无法执行巡逻攻击");
                    return false;
                }
                await Task.Delay(100);
                monsterTried++;
                if (monsterTried > 100)
                {
                    instanceValue.GameWarning("怪物攻击失败，巡逻点 {CurP}", curP);
                    break;
                }
                // 发现活人先停下 并且不是自己人
                // var zijiren = GameState.GameInstances.Select(o => o.CharacterStatus.Name);
                // var otherPeople = instanceValue.Monsters.Values.Where(o => o.TypeStr == "玩家" && !zijiren.Contains(o.Name)).FirstOrDefault();
                // var huorend = 0;
                // if (otherPeople != null && otherPeople.CurrentHP > 0)
                // {
                //     instanceValue.GameInfo($"发现活人{otherPeople.Name}  hp {otherPeople.CurrentHP} level {otherPeople.Level} 停下");
                //     await Task.Delay(1000);
                //     huorend++;
                //     if (huorend > 15)
                //     {
                //         break;
                //     }
                //     continue;
                // }
                // todo 测试是否有效
                // !forceSkip && 测试打怪退出在cleanMobs 这样才能飞走
                if (checker(instanceValue!))
                {
                    // 2层要直接return 
                    // break;
                    return true;
                }
                // 检测距离
                if (!instanceValue.AccountInfo.IsMainControl && !forceSkip)
                {
                    (px, py) = (mainInstance.CharacterStatus.X, mainInstance.CharacterStatus.Y);
                    if (Math.Max(Math.Abs(px - CharacterStatus.X), Math.Abs(py - CharacterStatus.Y)) > followDetectDistance)
                    {
                        // instanceValue.GameInfo("跟随 in monster: {X}, {Y}", px, py);
                        var diffFar = Math.Max(Math.Abs(px - CharacterStatus.X), Math.Abs(py - CharacterStatus.Y));
                        var soFarGezi = (instanceValue.CharacterStatus.MapId != mainInstance.CharacterStatus.MapId
                                || diffFar > 30) ? 999 : 30;
                        var atksThan = diffFar > 12 ? 12 : 0;
                        var isSS = await PerformPathfinding(_cancellationToken, instanceValue!, px, py, mainInstance.CharacterStatus.MapId, 4, true, atksThan, soFarGezi, 0,
                         (instanceValue) =>
                            {
                                if (checker(instanceValue))
                                {
                                    return true;
                                }
                                // 自定义
                                var OFFSET_TO_COMP = 10;
                                if (Math.Abs(GameState.GameInstances[0].CharacterStatus.X - px) > OFFSET_TO_COMP ||
                                Math.Abs(GameState.GameInstances[0].CharacterStatus.Y - py) > OFFSET_TO_COMP)
                                {
                                    return true;
                                }
                                return false;
                            }
                         );

                        if (isSS)
                        {
                            break;
                        }
                    }
                }

                // 保护消费者法师 诱惑/火球
                var consume0 = whoIsConsumer(instanceValue!) == 0;
                var slasher = whoIsConsumer(instanceValue!) > 0;
                var slashRemainHP = 50;
                var drawBBRemainHP = 25;




                // 查看存活怪物 并且小于距离10个格子
                var ani = instanceValue.Monsters.Values.Where(o => o.stdAliveMon &&
                // 暂时取消 看起来没作用
                // !instanceValue.attackedMonsterIds.Contains(o.Id) &&
                // 补刀用
                (slasher && o.Appr != 40 ? (enoughBBCanHit ?
                (o.CurrentHP > 0
                    ? ((o.CurrentHP > slashRemainHP) || (o.MaxHP < slashRemainHP) || (o.MaxHP >= 500))
                : true) : true)
                : true) &&

                (cleanAll || allowMonsters.Contains(o.Name))
                 && Math.Max(Math.Abs(o.X - CharacterStatus.X), Math.Abs(o.Y - CharacterStatus.Y)) < searchRds)
                // 还要把鹿羊鸡放最后
                .Select(o => new { Monster = o, Distance = measureGenGoPath(instanceValue!, o.X, o.Y) })
                .Where(o => o.Distance <= 30)
                .OrderBy(o => o.Monster.MaxHP < 500 ? (o.Monster.Appr != 40 ? (GameConstants.allowM10.Contains(o.Monster.Name) ? 2 : 1) : 0) : -1)
                .ThenBy(o => o.Distance)
                .Select(o => o.Monster)
                .FirstOrDefault();

                if (consume0)
                {
                    // 一直等到无怪,  TODO 测试主从, 优先测从
                    await Task.Delay(200);
                    if (ani == null)
                    {
                        break;
                    }
                    var nearBBCount = instanceValue.Monsters.Values.Count(o => !o.isDead &&
                        o.isMyMons
                        && Math.Max(Math.Abs(o.X - CharacterStatus.X), Math.Abs(o.Y - CharacterStatus.Y)) < 12);
                    var _isFullBB = canTemp ? nearBBCount == (CharacterStatus.Level >= 24 ? 5 : (CharacterStatus.Level >= 18 ? 4 : 3)) : false;
                    var isFullBB = false;
                    if (_isFullBB)
                    {
                        isFullBB = true;
                        instanceValue.mageFullBBtime = Environment.TickCount;
                    }
                    else if (instanceValue.mageFullBBtime > 0 && (Environment.TickCount - instanceValue.mageFullBBtime) < 300_000)
                    {
                        isFullBB = true;
                    }

                    var bossLike = instanceValue.Monsters.Values.Where(o => o.stdAliveMon && (o.Appr == 40 || o.Appr == 121 || o.MaxHP > 500)
                        && Math.Max(Math.Abs(o.X - CharacterStatus.X), Math.Abs(o.Y - CharacterStatus.Y)) < 12)
                        // 还要把鹿羊鸡放最后
                        .FirstOrDefault();


                    var isWallPoint = WallPointsUtils.IsWallPoint(instanceValue.CharacterStatus.MapId, instanceValue.CharacterStatus.X, instanceValue.CharacterStatus.Y);
                    var isPreferSafe = CheckNeedPerformEscapeWithSafePts(instanceValue) && isWallPoint;
                    // 围绕跑一下
                    if (
                        bossLike == null && isFullBB && new Random().Next(100) < 80 && !isPreferSafe)
                    {
                        var isSS = await PerformPathfinding(_cancellationToken, instanceValue!, px, py, mainInstance.CharacterStatus.MapId, 6, true, 0, 30, 0,
                        (instanceValue) =>
                        {
                            if (checker(instanceValue))
                            {
                                return true;
                            }
                            // 自定义
                            var OFFSET_TO_COMP = 10;
                            if (Math.Abs(GameState.GameInstances[0].CharacterStatus.X - px) > OFFSET_TO_COMP ||
                            Math.Abs(GameState.GameInstances[0].CharacterStatus.Y - py) > OFFSET_TO_COMP)
                            {
                                return true;
                            }
                            return false;
                        }
                        );
                    }

                    // 继续诱惑 还是火球
                    var temps = GameConstants.GetAllowTemp(CharacterStatus.Level);

                    // 使用通用躲避方法
                    await PerformEscapeWithSafePts(instanceValue, _cancellationToken);
                    var hadQun = false;
                    if (canBaolie)
                    {
                        // TODO 注意弓箭
                        // 优先群 有boss, 所有怪
                        var qunAnis = instanceValue.Monsters.Values.Where(o => o.stdAliveMon
                        && Math.Max(Math.Abs(o.X - CharacterStatus.X), Math.Abs(o.Y - CharacterStatus.Y)) < 12);

                        var baolieNum = 5; // instanceValue.Monsters.Where(t => t.Value.stdAliveMon).Count() > 20 ? 5 : 5;
                        // 先4爆裂 爆率很高
                        var qunanis = MonsterCoverageUtils.FindOptimal3x3Square(qunAnis.Select(o => (o.X, o.Y)).ToList(), baolieNum);
                        if (qunanis != (-1, -1))
                        {
                            sendSpell(instanceValue!, GameConstants.Skills.baolie, qunanis.x, qunanis.y, 0);
                            hadQun = true;
                        }
                    }
                    if (!hadQun)
                    {
                        // 如果是法师 可以抽陀螺
                        var hasTempedMon = false;
                        if (bossLike == null && canTemp && !isFullBB)
                        {
                            var teamsXY = instanceValue.Monsters.Where(t => t.Value.isTeams).Select(t => (t.Value.X, t.Value.Y)).ToList();
                            // 寻找陀螺
                            var mytop = instanceValue.Monsters.Values.Where(o => o.stdAliveMon
                            && GameConstants.TempMonsterLevels.GetValueOrDefault(o.Name, 99) <= (CharacterStatus.Level + 2)
                            && temps.Contains(o.Name)
                            && ((o.CurrentHP == 0 && o.MaxHP == 0) || o.CurrentHP == o.MaxHP
                            || (
                              // 或者是旁边没人砍 也允许
                              !teamsXY.Any(((int x, int y) t) => Math.Min(Math.Abs(t.x - o.X), Math.Abs(t.y - o.Y)) == 1)
                            ))
                            && Math.Max(Math.Abs(o.X - CharacterStatus.X), Math.Abs(o.Y - CharacterStatus.Y)) < 12)
                            .OrderBy(o => Math.Max(Math.Abs(o.X - CharacterStatus.X), Math.Abs(o.Y - CharacterStatus.Y)))
                            .FirstOrDefault();
                            if (mytop != null)
                            {
                                hasTempedMon = true;
                                sendSpell(instanceValue!, GameConstants.Skills.TemptationSpellId, mytop.X, mytop.Y, mytop.Id);
                            }
                        }
                        if (!hasTempedMon)
                        // 搞
                        {
                            // var hasDJS = instanceValue.Monsters.Any(o => o.Value.stdAliveMon && o.Value.Appr == 40); // djs 会缓慢打 算了
                            // 否则好像不打也行, 有群用群无CD, 没群有BOSS 仍然无CD, 否则好像没什么好搞的, 单体没什么意义, 除了低等级
                            var mageAni = bossLike ?? (!canTemp
                                ? instanceValue.Monsters.Values.Where(o => o.stdAliveMon
                                    && Math.Max(Math.Abs(o.X - CharacterStatus.X), Math.Abs(o.Y - CharacterStatus.Y)) < 12
                                    // CD
                                    // && (!instanceValue.mageDrawAttentionMonsterCD.TryGetValue(o.Id, out var cd) || Environment.TickCount > cd + (hasDJS ? 30_000 : 15_000))
                                    && allowMonsters.Contains(o.Name)
                                    // HP
                                    && ((o.CurrentHP == 0) || (o.CurrentHP > drawBBRemainHP)))
                                    // 还要把鹿羊鸡放最后
                                    .Select(o => new { Monster = o, Distance = measureGenGoPath(instanceValue!, o.X, o.Y) })
                                    .OrderBy(o => GameConstants.allowM10.Contains(o.Monster.Name) ? 2 : 1)
                                    .ThenBy(o => o.Monster.CurrentHP == 0 ? 9999 : o.Monster.CurrentHP)
                                    .Select(o => o.Monster)
                                    .FirstOrDefault()
                                : null);

                            if (CharacterStatus.CurrentHP > CharacterStatus.MaxHP * 0.3 && mageAni != null)
                            {
                                sendSpell(instanceValue!, canLight ? GameConstants.Skills.LightingSpellId : GameConstants.Skills.fireBall, mageAni.X, mageAni.Y, mageAni.Id);
                                // if (Environment.TickCount > instanceValue.mageDrawAttentionGlobalCD + (hasDJS ? 30_000 : 15_000))
                                // {
                                //     // instanceValue.mageDrawAttentionMonsterCD[mageAni.Id] = Environment.TickCount;
                                //     // instanceValue.mageDrawAttentionGlobalCD = Environment.TickCount;
                                // }
                            }
                        }
                    }
                    continue;
                }
                // 保护消费者

                if (ani != null)
                {
                    instanceValue.GameDebug("发现目标怪物: {Name}, 位置: ({X}, {Y}), 距离: {Distance}",
                        ani.Name, ani.X, ani.Y,
                        Math.Max(Math.Abs(ani.X - CharacterStatus.X), Math.Abs(ani.Y - CharacterStatus.Y)));
                    // 持续攻击, 超过就先放弃
                    var monTried = 0;
                    // 等待初始到怪面前的时间 根据初始距离推算 200ms 一格, 保持loop delay一致
                    var INIT_WAIT = Math.Max(Math.Abs(ani.X - CharacterStatus.X), Math.Abs(ani.Y - CharacterStatus.Y));
                    var escapeTried = 0;
                    while (true)
                    {
                        if (instanceValue.CharacterStatus!.isEnhanceDead)
                        {
                            instanceValue.GameWarning("角色已死亡，无法执行巡逻攻击");
                            return false;
                        }
                        // todo !forceSkip &&
                        if (checker(instanceValue!))
                        {
                            instanceValue.GameWarning("测试打怪中途跑路");
                            return true;
                        }
                        enoughBBCanHit = instanceValue.Monsters.Values.Where(o => !o.isDead && o.isTeamMons &&
                         Math.Max(Math.Abs(o.X - CharacterStatus.X), Math.Abs(o.Y - CharacterStatus.Y)) < 9).Count() > 3;
                        if (
                            !(slasher && ani.Appr != 40 ? (enoughBBCanHit ?
                            (ani.CurrentHP > 0
                                ? (ani.CurrentHP > slashRemainHP || ani.MaxHP < slashRemainHP)
                            : true) : true)
                            : true)
                        )
                        {
                            MonsterFunction.SlayingMonsterCancel(instanceValue!);
                            break;
                        }
                        CharacterStatus = instanceValue.CharacterStatus;
                        // 检测距离
                        if (!instanceValue.AccountInfo.IsMainControl && !forceSkip)
                        {
                            (px, py) = (mainInstance.CharacterStatus.X, mainInstance.CharacterStatus.Y);
                            if (Math.Max(Math.Abs(px - CharacterStatus.X), Math.Abs(py - CharacterStatus.Y)) > followDetectDistance)
                            {
                                // instanceValue.GameInfo("跟随 in monster: {X}, {Y}", px, py);
                                var diffFar = Math.Max(Math.Abs(px - CharacterStatus.X), Math.Abs(py - CharacterStatus.Y));
                                var soFarGezi = (instanceValue.CharacterStatus.MapId != mainInstance.CharacterStatus.MapId
                                || diffFar > 30) ? 999 : 30;
                                var atksThan = diffFar > 12 ? 12 : 0;
                                var isSS = await PerformPathfinding(_cancellationToken, instanceValue!, px, py, mainInstance.CharacterStatus.MapId, 4, true, atksThan, soFarGezi, 0,
                                   (instanceValue) =>
                                    {
                                        if (checker(instanceValue))
                                        {
                                            return true;
                                        }
                                        // 自定义
                                        var OFFSET_TO_COMP = 10;
                                        if (Math.Abs(GameState.GameInstances[0].CharacterStatus.X - px) > OFFSET_TO_COMP ||
                                        Math.Abs(GameState.GameInstances[0].CharacterStatus.Y - py) > OFFSET_TO_COMP)
                                        {
                                            return true;
                                        }
                                        return false;
                                    }
                                );
                                if (isSS)
                                {
                                    break;
                                }
                            }
                        }
                        // 主动躲避安全点 怪多, 查看1号主动旁边安全点, 通常也有一格
                        // 配合攻击点, 不出去
                        // 所以要怪阈值, 出去和不出去 躲还是找点
                        // 跟随的人 点不一样
                        if (escapeTried < 2)
                        {
                            var isEscaped = await PerformEscapeWithSafePts(instanceValue, _cancellationToken);
                            if (isEscaped == 1)
                            {
                                break;
                            }
                            else if (isEscaped == 0)
                            {
                                escapeTried++;
                                await Task.Delay(200);
                                continue;
                            }
                            // todo  -1 需要 -2 -3?
                        }
                        monTried++;
                        var isWallPoint = WallPointsUtils.IsWallPoint(instanceValue.CharacterStatus.MapId, instanceValue.CharacterStatus.X, instanceValue.CharacterStatus.Y);
                        var isPreferSafe = CheckNeedPerformEscapeWithSafePts(instanceValue) && isWallPoint;
                        // 这时候可能找不到了就上去, 或者是会跑的少数不用管
                        var diffX = Math.Abs(ani.X - CharacterStatus.X);
                        var diffY = Math.Abs(ani.Y - CharacterStatus.Y);
                        var isCi = instanceValue.AccountInfo.role == RoleType.blade && instanceValue.CharacterStatus.Level > 24 && ((diffX == 0 && diffY == 2) || (diffY == 0 && diffX == 2));
                        if (isCi)
                        {
                            var ciciDir = GetDirectionFromDelta2(ani.X - CharacterStatus.X, ani.Y - CharacterStatus.Y);
                            MonsterFunction.SlayingMonsterCancel(instanceValue!);
                            cici(instanceValue!, ciciDir);
                        }
                        else
                        {
                            // 配合逃跑安全点, 怪多就不走
                            if (!isPreferSafe)
                            {
                                
                                MonsterFunction.SlayingMonster(instanceValue!, ani.Addr);
                            }
                        }
                        if (!isPreferSafe && monTried > INIT_WAIT && Math.Max(diffX, diffY) > 1 && !isCi)
                        {
                            // more intelligent pathfinding
                            MonsterFunction.SlayingMonsterCancel(instanceValue!);
                            await PerformPathfinding(_cancellationToken, instanceValue!, ani.X, ani.Y, "", 1, true, 999, 30, 0, checker);
                            instanceValue.Monsters.TryGetValue(ani.Id, out MonsterModel? ani3);
                            if (ani3 == null)
                            {
                                break;
                            }
                            ani = ani3;
                            // 补刀用
                            if (
                                !(slasher && ani.Appr != 40 ? (enoughBBCanHit ?
                                (ani.CurrentHP > 0
                                    ? (ani.CurrentHP > slashRemainHP || ani.MaxHP < slashRemainHP || ani.MaxHP >= 500)
                                : true) : true)
                                : true)
                            )
                            {
                                MonsterFunction.SlayingMonsterCancel(instanceValue!);
                                break;
                            }
                        }
                        await Task.Delay(200);
                        // todo 优化
                        instanceValue.Monsters.TryGetValue(ani.Id, out MonsterModel? ani2);
                        if (ani2 == null)
                        {
                            break;
                        }
                        ani = ani2;
                        if (ani.isDead || ani.isTeams || monTried > 150)
                        {
                            // instanceValue.attackedMonsterIds.Add(ani.Id);
                            MonsterFunction.SlayingMonsterCancel(instanceValue!);
                            break;
                        }
                        // 补刀用
                        if (
                            !(slasher && ani.Appr != 40 ? (enoughBBCanHit ?
                            (ani.CurrentHP > 0
                                ? (ani.CurrentHP > slashRemainHP || ani.MaxHP < slashRemainHP || ani.MaxHP >= 500)
                            : true) : true)
                            : true)
                        )
                        {
                            MonsterFunction.SlayingMonsterCancel(instanceValue!);
                            break;
                        }
                    }
                    await PerformPickup(instanceValue, _cancellationToken, checker);
                    await PerformButchering(instanceValue, maxBagCount: 32, searchRadius: 13, maxTries: 20, _cancellationToken);
                }
                else
                {
                    break;
                }
                // if (px == 0 && py == 0)
                // {

                // }

            }

            // 使用通用捡取方法
            await PerformPickup(instanceValue, _cancellationToken, checker);

            // 使用通用屠挖肉方法
            await PerformButchering(instanceValue, maxBagCount: 32, searchRadius: 13, maxTries: 20, _cancellationToken);

            // checker 满足条件就跳出循环, checker是参数
            if (checker(instanceValue!))
            {
                break;
            }
            if (isToPoint)
            {
                // 往返循环逻辑：0->1->2->...->N->N-1->N-2->...->1->0
                curP += direction;
                if (curP >= patrolPairs.Length)
                {
                    curP = patrolPairs.Length - 2; // 回到倒数第二个点
                    direction = -1; // 改变方向为反向
                }
                else if (curP < 0)
                {
                    curP = 1; // 回到第二个点
                    direction = 1; // 改变方向为正向
                }
            }
            continue;
        }
        return true;

    }


    public static async Task<bool> SimpleAttackPoints(MirGameInstanceModel instanceValue, CancellationToken _cancellationToken)
    {

        if (instanceValue.CharacterStatus!.isEnhanceDead)
        {
            instanceValue.GameWarning("角色已死亡，无法执行巡逻攻击");
            return false;
        }
        var CharacterStatus = instanceValue.CharacterStatus!;

        while (true)
        {
            if (instanceValue.CharacterStatus!.isEnhanceDead)
            {
                instanceValue.GameWarning("角色已死亡，无法执行巡逻攻击");
                return false;
            }
            await Task.Delay(100);

            if (instanceValue.CharacterStatus!.isEnhanceDead)
            {
                instanceValue.GameWarning("角色已死亡，无法执行巡逻攻击");
                return false;
            }
            await Task.Delay(100);
            var ani = instanceValue.Monsters.Values.Where(o => o.stdAliveMon)
                // 还要把鹿羊鸡放最后
                .Select(o => new { Monster = o, Distance = measureGenGoPath(instanceValue!, o.X, o.Y) })
                .OrderBy(o => o.Distance)
                .Select(o => o.Monster)
                .FirstOrDefault();

            if (ani != null)
            {
                while (true)
                {
                    if (ani == null) break;
                    if (instanceValue.CharacterStatus!.isEnhanceDead)
                    {
                        instanceValue.GameWarning("角色已死亡，无法执行巡逻攻击");
                        return false;
                    }
                    CharacterStatus = instanceValue.CharacterStatus;
                    MonsterFunction.SlayingMonster(instanceValue!, ani.Addr);
                    await Task.Delay(200);
                    if (Math.Max(Math.Abs(ani.X - CharacterStatus.X), Math.Abs(ani.Y - CharacterStatus.Y)) > 1)
                    {
                        await PerformPathfinding(_cancellationToken, instanceValue!, ani.X, ani.Y, "", 1, true, 999, 30);
                        instanceValue.Monsters.TryGetValue(ani.Id, out MonsterModel? ani3);
                        ani = ani3;
                        if (ani == null)
                        {
                            break;
                        }
                    }
                    await Task.Delay(200);
                    instanceValue.Monsters.TryGetValue(ani.Id, out MonsterModel? ani2);
                    ani = ani2;
                    if (ani2 == null)
                    {
                        break;
                    }
                    if (ani2.isDead)
                    {
                        break;
                    }

                }
            }
            else
            {
                break;
            }
        }
        return true;

    }

    public static int measureGenGoPath(MirGameInstanceModel GameInstance, int tx, int ty)
    {
        try
        {
            // 身边不用寻
            int myX = GameInstance!.CharacterStatus!.X;
            int myY = GameInstance!.CharacterStatus!.Y;
            if (Math.Abs(tx - myX) < 2 && Math.Abs(ty - myY) < 2)
            {
                return 0;
            }
            var monsPos = GetMonsPos(GameInstance!.Monsters);
            var res = genGoPath(GameInstance!, tx, ty, 1, true).Count();
            return res == 0 ? 999 : res;
        }
        catch (Exception ex)
        {
            GameInstance.GameError("寻路测距异常" + ex.Message);
        }
        return 999;
    }

    public static async Task cleanMobs(MirGameInstanceModel GameInstance, int attacksThan, bool cleanAll, CancellationToken cancellationToken, Func<MirGameInstanceModel, bool> checker)
    {
        // todo 法师暂时不要砍了 要配合2边一起改
        if (whoIsConsumer(GameInstance!) == 2)
        {
            var searchRds = 7;
            var temp = GameConstants.GetAllowMonsters(GameInstance, GameInstance.CharacterStatus!.Level, GameInstance.AccountInfo.role);
            // 攻击怪物, 太多了 过不去
            var monsters = GameInstance.Monsters.Where(o => o.Value.stdAliveMon && (cleanAll || temp.Contains(o.Value.Name)) &&
             Math.Max(Math.Abs(o.Value.X - GameInstance.CharacterStatus.X), Math.Abs(o.Value.Y - GameInstance.CharacterStatus.Y)) < searchRds
            ).ToList();
            if (monsters.Count > attacksThan)
            {
                await NormalAttackPoints(GameInstance, cancellationToken, true, (instanceValue) =>
                {
                    if (checker(instanceValue))
                    {
                        return true;
                    }
                    // 重读怪物
                    var existsCount = GameInstance.Monsters.Where(o => o.Value.stdAliveMon && (cleanAll || temp.Contains(o.Value.Name)) &&
             Math.Max(Math.Abs(o.Value.X - GameInstance.CharacterStatus.X), Math.Abs(o.Value.Y - GameInstance.CharacterStatus.Y)) < searchRds
            ).Count();
                    // 怪物死了剩余一半就可以通过
                    if (existsCount <= attacksThan / 3)
                    {
                        return true;
                    }
                    return false;
                }, "", cleanAll, searchRds);
            }
        }
    }

    public static List<MonsterModel> findMonsSurrounded(int myX, int myY, ConcurrentDictionary<long, List<MonsterModel>> MonstersByPosition)
    {
        List<MonsterModel> all = new List<MonsterModel>();
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue; // 跳过中心点（角色当前位置）

                var x = myX + dx;
                var y = myY + dy;

                var ms = MonsterFunction.GetMonstersByPosition(MonstersByPosition, x, y);
                all.AddRange(ms);
            }
        }
        return all;
    }

    public static bool CheckIfSurrounded(string MapId, int myX, int myY, ConcurrentDictionary<long, List<MonsterModel>> MonstersByPosition)
    {
        var (width, height, obstacles) = retriveMapObstacles(MapId);

        // 添加怪物位置作为障碍点，就像genGoPath中的处理方式
        // var monsPos = GetMonsPos(Monsters);
        var obstacleData = new byte[obstacles.Length];
        Array.Copy(obstacles, obstacleData, obstacles.Length);


        // foreach (var pos in monsPos)
        // {
        //     int monX = pos[0];
        //     int monY = pos[1];
        //     if (monX >= 0 && monX < width && monY >= 0 && monY < height)
        //     {
        //         obstacleData[monY * width + monX] = 1;
        //     }
        // }

        // 检查周围8个方向是否都是障碍物
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue; // 跳过中心点（角色当前位置）

                var x = myX + dx;
                var y = myY + dy;

                // 边界检查：如果超出地图边界，也算作障碍物
                if (x < 0 || x >= width || y < 0 || y >= height)
                {
                    continue; // 边界算作障碍物，继续检查下一个方向
                }

                var ms = MonsterFunction.GetMonstersByPosition(MonstersByPosition, x, y);
                if (ms.Count() > 0)
                {
                    return false;
                }

                // 如果找到一个非障碍物位置，则未被完全包围
                if (obstacleData[y * width + x] != 1)
                {
                    return false;
                }
            }
        }
        return true; // 所有8个方向都是障碍物，被包围
    }

    public static async Task<bool> PerformPathfinding(CancellationToken cancellationToken, MirGameInstanceModel GameInstance, int tx, int ty, string replaceMap = "",
          int blurRange = 0,
          bool nearBlur = true,
          int attacksThan = 3,
          int maxPathLength = 9999,
          int retries = 0,
          Func<MirGameInstanceModel, bool>? callback = null
        )
    {
        if (cancellationToken.IsCancellationRequested)
        {
            GameInstance.GameDebug("寻路被取消");
            return false;
        }
        if (GameInstance.CharacterStatus!.isEnhanceDead)
        {
            GameInstance.GameWarning("角色已死亡，无法执行寻路");
            await Task.Delay(5_000);
            return false;
        }
        if (callback == null)
        {
            callback = (GameInstance) => false;
        }
        CharacterStatusFunction.GetInfo(GameInstance!);
        MonsterFunction.ReadMonster(GameInstance!);
        // 起点等于终点 结束
        if (GameInstance.CharacterStatus.X == tx && GameInstance.CharacterStatus.Y == ty && replaceMap == "")
        {
            return true;
        }
        replaceMap = replaceMap == "" ? GameInstance.CharacterStatus.MapId : replaceMap;
        // 支持跨多图寻路 返回值要改数组,并且后续数组都是先占位
        var connectionsPath = new List<MapConnection>();
        var originC = new MapConnection()
        {
            From = new MapPosition()
            {
                // 有点tricky 懒得改类型了
                X = tx,
                Y = ty,
                MapId = replaceMap,
            },
            To = new MapPosition()
            {
                X = 999,
                Y = 999,
                MapId = "999",
            }
        };
        var isAcross = replaceMap != GameInstance.CharacterStatus.MapId;
        if (isAcross)
        {
            // 先占位
            connectionsPath = mapConnectionService.FindPath(GameInstance.CharacterStatus.MapId, replaceMap);
            if (connectionsPath == null)
            {
                return false;
            }
            // 默认先加到达后的点 可能是NPC 巡逻点等
            connectionsPath.Add(originC);
        }
        else
        {
            connectionsPath = new List<MapConnection>()
            {
               originC,
            };
        }
        // 以下就变for循环结构了, 因为要1->N个地图寻路, 全部可以泛化成A->B
        for (var i = 0; i < connectionsPath.Count; i++)
        {
            // 所以可得前面的N blur一定是0, 除非count只有一个
            var localBlurRange = isAcross && i < connectionsPath.Count - 1 ? 0 : blurRange;

            var connection = connectionsPath[i];
            // 检查当前是否所在地图 否则说明失效 重新搞
            if (GameInstance.CharacterStatus.MapId != connection.From.MapId)
            {
                // reset
                return await PerformPathfinding(cancellationToken, GameInstance, tx, ty, replaceMap, localBlurRange, nearBlur, attacksThan, maxPathLength, retries + 1, callback);
            }

            var stopwatchTotal = new System.Diagnostics.Stopwatch();
            stopwatchTotal.Start();
            var goNodes = new List<(byte dir, byte steps, int x, int y)>();
            try
            {
                goNodes = genGoPath(GameInstance!, connection.From.X, connection.From.Y, localBlurRange, nearBlur).ToList();
            }
            catch (Exception ex)
            {
                GameInstance!.GameError("寻路异常" + ex.Message);
                await Task.Delay(100);
                if (GameInstance.CharacterStatus!.isEnhanceDead)
                {
                    await Task.Delay(60_000);
                }
                return false;
            }

            stopwatchTotal.Stop();
            if (stopwatchTotal.ElapsedMilliseconds > 100)
            {
                GameInstance.GameDebug("寻路: {Time} 毫秒", stopwatchTotal.ElapsedMilliseconds);
            }


            if (goNodes.Count == 0)
            {
                if (callback(GameInstance))
                {
                    return false;
                }
                // 查看被包围, 8个点都是1障碍
                if (CheckIfSurrounded(GameInstance.CharacterStatus.MapId, GameInstance.CharacterStatus.X, GameInstance.CharacterStatus.Y,
                GameInstance.MonstersByPosition
                ))
                {
                    await cleanMobs(GameInstance, 0, true, cancellationToken, callback);
                }
                await PerformPickup(GameInstance, cancellationToken, callback);
                // 加个重试次数3次
                await Task.Delay(200);

                if (retries < 3)
                {
                    GameInstance.GameWarning("寻路未找到路径，准备第 {Retry} 次重试", retries + 1);
                    return await PerformPathfinding(cancellationToken, GameInstance, tx, ty, replaceMap, localBlurRange + 1, nearBlur, attacksThan, maxPathLength, retries + 1, callback);
                }

                GameInstance.GameWarning("寻路最终未找到路径，已重试 {Retries} 次", retries);
                return false;
            }

            if (goNodes.Count > maxPathLength)
            {
                GameInstance.GameWarning("寻路路径太长:{Retries} ", goNodes.Count);
                return false;
            }
            while (goNodes.Count > 0)
            {
                if (callback(GameInstance))
                {
                    return false;
                }
                if (cancellationToken.IsCancellationRequested)
                {
                    return false;
                }
                if (GameInstance.CharacterStatus!.isEnhanceDead)
                {
                    return false;
                }
                if (CheckIfSurrounded(GameInstance.CharacterStatus.MapId, GameInstance.CharacterStatus.X, GameInstance.CharacterStatus.Y,
                GameInstance.MonstersByPosition))
                {
                    await cleanMobs(GameInstance, 0, false, cancellationToken, callback);
                }
                await PerformPickup(GameInstance, cancellationToken, callback);
                // 寻路会出问题
                // await PerformButchering(GameInstance, maxBagCount: 32, searchRadius: 13, maxTries: 20, cancellationToken);

                var node = goNodes[0];
                var oldX = GameInstance!.CharacterStatus!.X;
                var oldY = GameInstance!.CharacterStatus!.Y;
                var (nextX, nextY) = getNextPostion(oldX, oldY, node.dir, node.steps);

                GoRunAlgorithm(GameInstance, oldX, oldY, node.dir, node.steps);

                var whileList = new List<string>() { "0132" };
                if (isAcross && whileList.Contains(replaceMap))
                {
                    // 注意很多不需要, 用白名单
                    int N = 3; // 你指定的每次尝试的方向数量
                    int maxCount = 9 / N; // 根据 N 动态计算 goNodes.Count 的最大值
                    if (goNodes.Count < maxCount)
                    {
                        var lastN = goNodes[goNodes.Count - 1];
                        // 剩下8个格子 尝试8个方向
                        int[] dx = { 0, 0, 1, 1, 1, 0, -1, -1, -1 };
                        int[] dy = { 0, -1, -1, 0, 1, 1, 1, 0, -1 };

                        // 动态计算起始索引
                        int triIdx = goNodes.Count - 1;

                        // 动态计算尝试的方向
                        for (int iiii = 0; iiii < N; iiii++)
                        {
                            int idx = triIdx * N + iiii; // 当前方向的索引
                            if (idx < dx.Length)
                            {
                                openDoor(GameInstance, lastN.x + dx[idx], lastN.y + dy[idx]);
                            }
                        }
                    }
                }

                var tried = 0;
                var maxed = 12; // 别改
                while (true)
                {
                    if (GameInstance.CharacterStatus!.isEnhanceDead)
                    {
                        return false;
                    }
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return false;
                    }
                    if (callback(GameInstance))
                    {
                        return false;
                    }
                    await Task.Delay(100, cancellationToken);
                    CharacterStatusFunction.GetInfo(GameInstance!);

                    MonsterFunction.ReadMonster(GameInstance!);

                    // 执行后发生了变更
                    var newX = GameInstance!.CharacterStatus.X;
                    var newY = GameInstance!.CharacterStatus.Y;

                    tried++;
                    if (tried > maxed)
                    {
                        // 如果在模糊范围内也算成功
                        if (Math.Abs(tx - newX) <= localBlurRange && Math.Abs(ty - newY) <= localBlurRange)
                        {
                            return true;
                        }
                        // 尝试跳到后面的点
                        var isJumpSuccess = false;
                        foreach (var jumpSteps in new[] { 1, 2, 3, 5, 8, 10, 15 })
                        {
                            await Task.Delay(50, cancellationToken);
                            if (callback(GameInstance))
                            {
                                return false;
                            }
                            if (goNodes.Count <= jumpSteps) continue;

                            // 获取跳跃目标点的坐标
                            var jumpPos = goNodes[jumpSteps];
                            MonsterFunction.ReadMonster(GameInstance!);
                            var jumpPath = new List<(byte dir, byte steps, int x, int y)>();
                            try
                            {
                                jumpPath = genGoPath(GameInstance!, jumpPos.x, jumpPos.y).ToList();
                            }
                            catch (Exception ex)
                            {
                                GameInstance!.GameError("寻路异常" + ex.Message);
                                await Task.Delay(50);
                                if (GameInstance.CharacterStatus!.isEnhanceDead)
                                {
                                    await Task.Delay(60_000);
                                }
                                return false;
                            }

                            if (jumpPath.Count > 0)
                            {
                                if (jumpSteps > 5)
                                {
                                    GameInstance.GameDebug($"尝试跳过{jumpSteps}步，寻路到({jumpPos.x},{jumpPos.y})");
                                }
                                // 先走到跳跃点
                                foreach (var pathNode in jumpPath)
                                {
                                    var (targetX, targetY) = getNextPostion(GameInstance.CharacterStatus.X, GameInstance.CharacterStatus.Y, pathNode.dir, pathNode.steps);
                                    GoRunAlgorithm(GameInstance, GameInstance.CharacterStatus.X, GameInstance.CharacterStatus.Y, pathNode.dir, pathNode.steps);
                                    var localTried = 0;
                                    while (localTried < 3)
                                    {
                                        await Task.Delay(50, cancellationToken);
                                        if (callback(GameInstance))
                                        {
                                            return false;
                                        }
                                        CharacterStatusFunction.GetInfo(GameInstance!);


                                        // 检查是否成功移动到目标位置
                                        if (GameInstance.CharacterStatus.X == targetX && GameInstance.CharacterStatus.Y == targetY)
                                        {
                                            isJumpSuccess = true;
                                            break;
                                        }
                                        localTried++;
                                    }
                                    if (localTried > 3)
                                    {
                                        isJumpSuccess = false;
                                        break;
                                    }
                                }
                                if (callback(GameInstance))
                                {
                                    return false;
                                }
                                if (isJumpSuccess)
                                {
                                    // 移除跳过的路径点和对应的位置信息
                                    goNodes.RemoveRange(0, jumpSteps + 1);
                                    // GameInstance.GameDebug($"成功跳过{jumpSteps}步");
                                    tried = 0;
                                    break;
                                }
                                else
                                {
                                    continue;
                                }
                            }

                        }
                        if (!isJumpSuccess)
                        {
                            // 失败了怎么办, 只能放弃先了
                            GameInstance.GameWarning($"寻路最终未找到 -- 跳点 再次尝试 NB");
                            return await PerformPathfinding(cancellationToken, GameInstance, tx, ty, replaceMap, localBlurRange + 1, nearBlur, attacksThan, maxPathLength, retries + 1, callback);
                            // return false;
                        }
                        else
                        {
                            // 跳出当前点, 并前进了N点, 但是用重装来恢复比较简单
                            return await PerformPathfinding(cancellationToken, GameInstance, tx, ty, replaceMap, localBlurRange, nearBlur, attacksThan, maxPathLength, retries + 1, callback);
                        }
                    }

                    if (oldX != newX || oldY != newY)
                    {
                        if (nextX == newX && nextY == newY)
                        {

                            if (GameState.gamePath == "Client.exe")
                            {
                                await Task.Delay(100, cancellationToken);
                                if (GameInstance.AccountInfo.role == RoleType.mage)
                                {
                                    await Task.Delay(100, cancellationToken);
                                }
                            }
                            // 查看是否反弹
                            CharacterStatusFunction.GetInfo(GameInstance!);

                            if (GameInstance.CharacterStatus!.X != nextX || GameInstance.CharacterStatus.Y != nextY)
                            {
                                await Task.Delay(200, cancellationToken);
                                GameInstance.GameWarning($"反弹");
                                var otherClientMonster = GameState.GameInstances
                                    .Where(t => t.AccountInfo.Account != GameInstance.AccountInfo.Account)
                                    .SelectMany(t => t.Monsters.Values)
                                    .FirstOrDefault(m => m.isTeams && m.Name == GameInstance.AccountInfo.CharacterName);
                                if (otherClientMonster != null)
                                {
                                    CharacterStatusFunction.FastWriteXY(GameInstance, otherClientMonster.X, otherClientMonster.Y);
                                    await Task.Delay(100, cancellationToken);
                                }
                                else
                                {
                                    GoRunAlgorithm(GameInstance, GameInstance.CharacterStatus.X, GameInstance.CharacterStatus.Y, (byte)(tried % 8), 1);
                                    await Task.Delay(400, cancellationToken);
                                }
                                continue;
                            }
                            goNodes.RemoveAt(0);
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    // 否则继续等待
                }
            }
            // 多图就需要等待
            if (isAcross)
            {
                await Task.Delay(1500);
            }
        }

        return true;
    }


    public static async Task RestartByToSelectScene(MirGameInstanceModel gameInstance)
    {
        SendMirCall.Send(gameInstance, 9098, new nint[] { });
        await Task.Delay(8000);
        SendMirCall.Send(gameInstance, 9099, new nint[] { });
        await Task.Delay(7000);
        SendMirCall.Send(gameInstance, 9100, new nint[] { });
        await Task.Delay(4000);
    }

    /// <summary>
    /// 等转到地图
    /// </summary>
    /// <param name="gameInstance"></param>
    /// <param name="text"></param>
    /// <returns></returns>
    public static async Task<bool> WaitMap(MirGameInstanceModel gameInstance, string mapName, int timeout = 50)
    {
        return await TaskWrapper.Wait(() => gameInstance.CharacterStatus!.MapName == mapName, timeout);
    }

    public static bool sendSpell(MirGameInstanceModel GameInstance, int spellId, int x = 0, int y = 0, int targetId = 0)
    {
        // check mp -- spell map
        var cost = GameConstants.MagicSpellMap[spellId];
        if (GameInstance.CharacterStatus!.CurrentMP < cost + 1)
        {
            return false;
        }
        // fs x 2
        var fsBase = GameInstance.AccountInfo.role == RoleType.mage ? 1800 : 1000;
        // fsBase = half ? fsBase / 2 : fsBase;
        if (GameInstance.spellLastTime + fsBase > Environment.TickCount)
        {
            return false;
        }
        GameInstance.spellLastTime = Environment.TickCount;
        // check instance cool time to avoid ban
        SendMirCall.Send(GameInstance, 3100, new nint[] { spellId, x, y, targetId });
        return true;
    }

    public static bool CapbilityOfHeal(MirGameInstanceModel GameInstance)
    {
        return GameInstance.AccountInfo.role == RoleType.taoist && GameInstance.Skills.FirstOrDefault(o => o.Id == 2) != null;
    }


    public static bool CapbilityOfHideSelf(MirGameInstanceModel GameInstance)
    {
        return GameInstance.AccountInfo.role == RoleType.taoist && GameInstance.Skills.FirstOrDefault(o => o.Id == 18) != null;
    }
    public static bool CapbilityOfHideAll(MirGameInstanceModel GameInstance)
    {
        return GameInstance.AccountInfo.role == RoleType.taoist && GameInstance.Skills.FirstOrDefault(o => o.Id == 19) != null;
    }

    public static bool CapbilityOfTemptation(MirGameInstanceModel GameInstance)
    {
        return GameInstance.AccountInfo.role == RoleType.mage && GameInstance.Skills.FirstOrDefault(o => o.Id == 20) != null;
    }

    public static bool CapbilityOfDefUp(MirGameInstanceModel GameInstance)
    {
        return GameInstance.AccountInfo.role == RoleType.taoist && GameInstance.Skills.FirstOrDefault(o => o.Id == GameConstants.Skills.defUp) != null;
    }

    public static bool CapbilityOfFlashMove(MirGameInstanceModel GameInstance)
    {
        return GameInstance.AccountInfo.role == RoleType.mage && GameInstance.Skills.FirstOrDefault(o => o.Id == GameConstants.Skills.flashMove) != null;
    }

    public static bool CapbilityOfMageDefUp(MirGameInstanceModel GameInstance)
    {
        return GameInstance.AccountInfo.role == RoleType.taoist && GameInstance.Skills.FirstOrDefault(o => o.Id == GameConstants.Skills.mageDefup) != null;
    }

    public static bool CapbilityOfSekeleton(MirGameInstanceModel GameInstance)
    {
        return GameInstance.AccountInfo.role == RoleType.taoist && GameInstance.Skills.FirstOrDefault(o => o.Id == GameConstants.Skills.RecallBoneSpellId) != null;
    }


    public static bool CapbilityOfLighting(MirGameInstanceModel GameInstance)
    {
        return GameInstance.AccountInfo.role == RoleType.mage && GameInstance.Skills.FirstOrDefault(o => o.Id == 11) != null;
    }

    public static bool CapbilityOfBaolie(MirGameInstanceModel GameInstance)
    {
        return GameInstance.AccountInfo.role == RoleType.mage && GameInstance.Skills.FirstOrDefault(o => o.Id == 23) != null;
    }

    public static void TryHealPeople(MirGameInstanceModel GameInstance)
    {
        if (!CapbilityOfHeal(GameInstance))
        {
            // GameInstance.GameDebug("角色无法治疗他人，跳过治疗检查");
            return;
        }
        // GameInstance.GameDebug("开始检查需要治疗的目标");
        // pick the needed people
        // 组队成员
        var instances = GameState.GameInstances;

        // 添加别的客户端的怪物信息 -- todo 远程机器信息 开socket连接
        var allMonsInClients = new List<MonsterModel>();
        foreach (var instance in instances)
        {
            // 只取这个实例里对应自己账号的玩家信息和宝宝信息
            var selfMonsters = instance.Monsters.Values.Where(m =>
                m.isSelf ||
                m.isMyMons
            );
            allMonsInClients.AddRange(selfMonsters);
        }

        var ESTIMATED_HEAL = GameInstance.CharacterStatus.Level * 2;
        var cdp = (int)(GameConstants.Skills.HealPeopleCD * (GameInstance.CharacterStatus.Level > 20 ? GameInstance.CharacterStatus.Level / 15.0 : 1));

        var people = allMonsInClients.Where(o =>
            !GameInstance.healCD.TryGetValue(o.Id, out var cd) || Environment.TickCount > cd + cdp &&
            o.CurrentHP > 0 &&
            !o.isDead &&
            ((o.MaxHP - o.CurrentHP) > ESTIMATED_HEAL || (o.CurrentHP < o.MaxHP * GameConstants.Items.commonHealRate)) &&
            Math.Abs(GameInstance.CharacterStatus.X - o.X) < 11 && Math.Abs(GameInstance.CharacterStatus.Y - o.Y) < 11
        )
        // 按优先级排序, 人物总是比宝宝优先, 绝对值低血量优先
        .OrderBy(o => o.TypeStr == "玩家" ? 0 : 1)
        .ThenBy(o => o.CurrentHP)
        .FirstOrDefault();

        if (people == null)
        {
            // GameInstance.GameDebug("未找到需要治疗的目标");
            // 道士回调
            // CharacterStatusFunction.AdjustAttackSpeed(GameInstance, 1100);
            // 延迟回调
            // Task.Delay(10_000).ContinueWith(t =>
            // {
            //     var people = allMonsInClients.Where(o =>
            //     // not in cd
            //     !GameInstance.healCD.TryGetValue(o.Id, out var cd) || Environment.TickCount > cd + cdp &&
            //     // 活着
            //     o.CurrentHP > 0 &&
            //     !o.isDead
            //     // 低血量
            //     && ((o.CurrentHP < o.MaxHP * 0.7) || o.CurrentHP < 10)
            //     // 距离足够
            //     && (Math.Abs(GameInstance.CharacterStatus.X - o.X) < 12
            //     && Math.Abs(GameInstance.CharacterStatus.Y - o.Y) < 12)
            //     )
            //     // 按优先级排序, 人物总是比宝宝优先, 绝对值低血量优先
            //     .OrderBy(o => o.TypeStr == "玩家" ? 0 : 1)
            //     .ThenBy(o => Math.Abs(o.CurrentHP - o.MaxHP * 0.7))
            //     .FirstOrDefault();
            //     // 再次检查
            //     if (people == null)
            //     {
            //         CharacterStatusFunction.AdjustAttackSpeed(GameInstance, 1200);
            //     }
            // });
            return;
        }
        // GameInstance.GameInfo("准备治疗目标: {Name}, HP: {HP}/{MaxHP}", people.Name, people.CurrentHP, people.MaxHP);
        sendSpell(GameInstance, GameConstants.Skills.HealSpellId, people.X, people.Y, people.Id);
        GameInstance.healCD[people.Id] = Environment.TickCount;
        // 道士调整攻速
        // CharacterStatusFunction.AdjustAttackSpeed(GameInstance, 3000);
    }
    public static async Task TryHiddenPeople(MirGameInstanceModel GameInstance)
    {
        if (!CapbilityOfHideSelf(GameInstance))
        {
            return;
        }
        // 目前有个bug 坐标不对 , 所以先用自己的, 这样只有自己出错, 但是低血可以防止问题
        // 待选组人
        var allPeople = GameState.GameInstances.Select(t => t.CharacterStatus).Where(t => !t.isDead && !t.isHidden &&
                Math.Abs(GameInstance.CharacterStatus.X - t.X) < 11 && Math.Abs(GameInstance.CharacterStatus.Y - t.Y) < 11
        );
        CharacterStatusModel? lastFinded;
        // 血太少, 周边有怪, 虽然没被block 但是优先, 没怪的话 先不隐, 怕有别的case
        var lowFind = allPeople.Where(t => t.CurrentHP < t.MaxHP * GameConstants.Items.commonHideRate &&
                    findMonsSurrounded(t.X, t.Y, GameInstance.MonstersByPosition)
                        .Where(t => t.stdAliveMon)
                        .Count() > 3
              ).FirstOrDefault();

        if (lowFind != null)
        {
            lastFinded = lowFind;
        }
        else
        {
            // 纯包围
            lastFinded = allPeople.Where(t => t.CurrentHP < t.MaxHP * 0.6
                // t.Value.CurrentHP < 30 绝对值不管
                &&
                CheckIfSurrounded(GameInstance.CharacterStatus.MapId, t.X, t.Y, GameInstance.MonstersByPosition)
            ).FirstOrDefault();
        }

        if (lastFinded != null)
        {
            sendSpell(GameInstance, lastFinded.Name == GameInstance.AccountInfo.CharacterName ? GameConstants.Skills.smHide : GameConstants.Skills.bigHide, lastFinded.X, lastFinded.Y, 0);
            await Task.Delay(300);
        }
        // 待选组怪? 先不管
    }
    public static async Task CallbackAndBeStatusSlaveIfHas(MirGameInstanceModel GameInstance, bool attack = false)
    {
        if(GameInstance.AccountInfo.role == RoleType.blade)
        {
            return;
        }
        CharacterStatusFunction.ClearChats(GameInstance);
        CharacterStatusFunction.AddChat(GameInstance, "@rest");
        await Task.Delay(800);
        CharacterStatusFunction.ReadChats(GameInstance, true);
        if (GameInstance.chats.Contains("下属：攻击") || GameInstance.chats.Contains("下属：休息"))
        {
            // 开始召回
            for (int i = 0; i < 100; i++)
            {
                CharacterStatusFunction.AddChat(GameInstance, "@rest");
                await Task.Delay(300);
            }
            await Task.Delay(1000);
            CharacterStatusFunction.ReadChats(GameInstance, true);
            var lastChatState = GameInstance.chats.FindLast(o => o.Contains("下属"));
            if (lastChatState?.Contains(attack ? "休息" : "攻击") == true)
            {
                CharacterStatusFunction.AddChat(GameInstance, "@rest");
            }
        }
    }

    public static async Task BeStatusSlaveIfHas(MirGameInstanceModel GameInstance, bool attack = false)
    {
        if(GameInstance.AccountInfo.role == RoleType.blade)
        {
            return;
        }
        CharacterStatusFunction.ClearChats(GameInstance);
        CharacterStatusFunction.AddChat(GameInstance, "@rest");
        await Task.Delay(1000);
        CharacterStatusFunction.ReadChats(GameInstance, true);
        if (GameInstance.chats.Contains("下属：攻击") || GameInstance.chats.Contains("下属：休息"))
        {
            var lastChatState = GameInstance.chats.FindLast(o => o.Contains("下属"));
            if (lastChatState?.Contains(attack ? "休息" : "攻击") == true)
            {
                CharacterStatusFunction.AddChat(GameInstance, "@rest");
            }
        }
    }



    // todo 自动召唤要考虑, 原状态是什么 是不是要恢复, 现在都到攻击状态

    public static async Task TryAliveRecallMob(MirGameInstanceModel GameInstance)
    {
        if (!CapbilityOfSekeleton(GameInstance))
        {
            return;
        }

        var npc = GameInstance.Monsters.Values.FirstOrDefault(o => o.TypeStr == "NPC");
        if (npc != null)
        {
            GameInstance.GameDebug("附近有NPC, 不需要辅助");
            return;
        }
        // 1. 先检查身边
        var myname = $"变异骷髅({GameInstance.AccountInfo.CharacterName})";
        var monster = GameInstance.Monsters.FirstOrDefault(o => !o.Value.isDead && o.Value.Name == myname);
        if (monster.Value != null)
        {
            // GameInstance.GameInfo("身边有召唤兽, 跳过召回");
            return;
        }
        // 2. 再检查命令
        // TODO 这个可能不是每次需要 再说, TODO 还有需要召回需求, 进门需求 回家 很多
        CharacterStatusFunction.ClearChats(GameInstance);
        CharacterStatusFunction.AddChat(GameInstance, "@rest");
        await Task.Delay(1000);
        CharacterStatusFunction.ReadChats(GameInstance, true);
        if (GameInstance.chats.Contains("下属：攻击") || GameInstance.chats.Contains("下属：休息"))
        {
            var lastChatState = GameInstance.chats.FindLast(o => o.Contains("下属"));
            // 如果存在 最后状态要恢复回来
            if (lastChatState?.Contains("休息") == true)
            {
                CharacterStatusFunction.AddChat(GameInstance, "@rest");
                // await Task.Delay(700);
            }
            // 清理
            // CharacterStatusFunction.ClearChats(GameInstance);
            return;
        }
        // 否则说明丢了, 需要召唤
        var isWearSS = await detectAndWearHushen(GameInstance);
        if (!isWearSS)
        {
            return;
        }
        sendSpell(GameInstance, GameConstants.Skills.RecallBoneSpellId, GameInstance.CharacterStatus.X, GameInstance.CharacterStatus.Y, 0);
        await Task.Delay(300);
        // 再自动换回
        await NpcFunction.autoReplaceEquipment(GameInstance, false);
        await Task.Delay(800);
        await NpcFunction.autoReplaceEquipment(GameInstance, false);
    }


    public static async Task TryDefUps(MirGameInstanceModel GameInstance)
    {
        var canDef = CapbilityOfDefUp(GameInstance);
        var canMageDef = CapbilityOfMageDefUp(GameInstance);
        if (!canDef && !canMageDef)
        {
            return;
        }

        canDef = canDef && !GameInstance.CharacterStatus.isDefUp;
        canMageDef = canMageDef && !GameInstance.CharacterStatus.isMagDefUp;
        if (!canDef && !canMageDef)
        {
            return;
        }

        var aMons = GameInstance.Monsters.Values.FirstOrDefault(o => o.stdAliveMon);
        if (aMons == null)
        {
            GameInstance.GameDebug("附近无怪, 不需要辅助");
            return;
        }

        if (canMageDef)
        {
            // 查看是否有怪 电浆40 , 蛾39, 火焰沃玛31
            var apprs = new int[] { 40, 39, 31 };
            canMageDef = GameInstance.Monsters.Values.FirstOrDefault(o => apprs.Contains(o.Appr)) != null;
        }

        if (!canDef && !canMageDef)
        {
            return;
        }

        // 查看有没沪深不然浪费魔法
        var isWearSS = await detectAndWearHushen(GameInstance);
        if (!isWearSS)
        {
            return;
        }

        // 其他actor都接近, 就一起放
        // 查找所有的人
        var myteamMembersPos = GameInstance.Monsters.Values.Where(o => !o.isDead && o.isTeams
        && Math.Abs(o.X - GameInstance.CharacterStatus.X) < 10 && Math.Abs(o.Y - GameInstance.CharacterStatus.Y) < 10)
            .Select(o => (o.X, o.Y)).ToList();

        // 使用最多3个正方形覆盖最多的队友位置
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var optimalSquares = FindOptimalSquareCoverage(myteamMembersPos, 3, 3); // 3格范围的正方形
        stopwatch.Stop();
        GameInstance.GameDebug("最优算法耗时: {ElapsedMs}ms, 队友数: {TeamCount}, 找到正方形: {SquareCount}",
            stopwatch.ElapsedMilliseconds, myteamMembersPos.Count, optimalSquares.Count);
        if (canDef)
        {
            foreach (var square in optimalSquares)
            {
                sendSpell(GameInstance, GameConstants.Skills.defUp, square.CenterX, square.CenterY, 0);
                await Task.Delay(1200);
            }
        }
        if (canMageDef)
        {
            foreach (var square in optimalSquares)
            {
                sendSpell(GameInstance, GameConstants.Skills.mageDefup, square.CenterX, square.CenterY, 0);
                await Task.Delay(1200);
            }
        }
        await Task.Delay(500);
        // 再自动换回
        await NpcFunction.autoReplaceEquipment(GameInstance, false);
    }
    public static async Task tryMagePushBlock(MirGameInstanceModel GameInstance)
    {
        if (GameInstance.AccountInfo.role != RoleType.mage)
        {
            return;
        }
        // random 
        // 转宝宝方向 
        var dir = new Random().Next(0, 7);
        await GoTurn(GameInstance, (byte)dir);
        if (new Random().Next(1, 100) > 50)
        {
            return;
        }
        if (GameInstance.CharacterStatus.Level < 29)
        {
            return;
        }
        sendSpell(GameInstance, GameConstants.Skills.MagePush, GameInstance.CharacterStatus.X, GameInstance.CharacterStatus.Y, 0);
        await Task.Delay(500);
    }
    public static (int, int) CCBBCount(MirGameInstanceModel GameInstance)
    {
        var allMonsIdInClients = new HashSet<int>();
        var all = GameState.GameInstances;
        var exceptNames = all.Where(o => o.AccountInfo.role == RoleType.taoist).Select(o => o.AccountInfo.CharacterName).ToList();
        foreach (var instance in all)
        {
            var selfMonsters = instance.Monsters.Values.Where(m => !m.isDead && m.isTeamMons && !exceptNames.Any(o => m.Name.Contains(o)));
            allMonsIdInClients.UnionWith(selfMonsters.Select(m => m.Id));
        }
        var targetCount = all.Where(o => o.AccountInfo.role == RoleType.mage && o.CharacterStatus.Level >= 24).Count() * 5;

        return (allMonsIdInClients.Count, targetCount);
    }


    private static IEnumerable<int> GetMatchingItemIndices(MirGameInstanceModel GameInstance, string name, bool isBlur = false)
    {
        return GameInstance.Items
            .Select(o => (o, o.Index + 6))
            .Concat(GameInstance.QuickItems
                .Select(o => (o, o.Index)))
            .Where(tuple => isBlur ? tuple.o.Name.Contains(name) : tuple.o.Name == name)
            .Select(tuple => tuple.Item2);
    }

    public static int[]? findIdxInAllItems(MirGameInstanceModel GameInstance, string name, bool isBlur = false)
    {
        // TODO 打包这种还没算
        var allIndices = GetMatchingItemIndices(GameInstance, name, isBlur).ToArray();
        return allIndices.Length > 0 ? allIndices : null;
    }

    public static int findIdxFirstItem(MirGameInstanceModel GameInstance, string name, bool isBlur = false)
    {
        return GetMatchingItemIndices(GameInstance, name, isBlur).FirstOrDefault(-1);
    }

    public static void TryEatDrug(MirGameInstanceModel GameInstance)
    {
        var hp = GameInstance.CharacterStatus.CurrentHP;
        var maxHp = GameInstance.CharacterStatus.MaxHP;
        var mp = GameInstance.CharacterStatus.CurrentMP;
        var maxMp = GameInstance.CharacterStatus.MaxMP;
        // GameInstance.GameDebug("检查是否需要吃药，当前HP: {HP}/{MaxHP}, MP: {MP}/{MaxMP}", hp, maxHp, mp, maxMp);
        // todo 解包再吃
        //  for low hp
        var hpRate = GameConstants.Items.hpProRate;
        var lowHpRate = GameConstants.Items.hpLowRate;
        if (hp < maxHp * hpRate) // 0.5避免浪费治疗
        {
            var veryLow = hp < maxHp * lowHpRate;
            int resIdx = -1;
            if (veryLow)
            {
                var idx = findIdxFirstItem(GameInstance, "太阳水", true);
                if (idx != -1)
                {
                    resIdx = idx;
                }
            }
            if (resIdx == -1)
            {
                var idx = findIdxFirstItem(GameInstance, "金创药", true);
                if (idx != -1)
                {
                    resIdx = idx;
                }
            }
            if (resIdx != -1)
            {
                NpcFunction.EatIndexItem(GameInstance, resIdx);
            }
        }

        // for low mp
        var isNotLowBlade = !(GameInstance.AccountInfo.role == RoleType.blade && GameInstance.CharacterStatus.Level < 28);
        var magePreRate = GameConstants.Items.mpProRate(GameInstance);
        var lowMpRate = GameConstants.Items.mpLowRate(GameInstance);
        if (isNotLowBlade && (mp < maxMp * magePreRate))
        {
            var veryLow = mp < maxMp * lowMpRate;
            int resIdx = -1;
            if (veryLow)
            {
                var idx = findIdxFirstItem(GameInstance, "太阳水", true);
                if (idx != -1)
                {
                    resIdx = idx;
                }
            }
            if (resIdx == -1)
            {
                var idx = findIdxFirstItem(GameInstance, "魔法药", true);
                if (idx != -1)
                {
                    resIdx = idx;
                }
            }
            if (resIdx != -1)
            {
                NpcFunction.EatIndexItem(GameInstance, resIdx);
            }
        }
        // 清理道士蓝
        // if (GameInstance.AccountInfo.role == RoleType.taoist || GameInstance.AccountInfo.role == RoleType.mage)
        // {
        //     var lans = findIdxInAllItems(GameInstance, "魔法药", true);
        //     if (lans != null && lans.Length > GameConstants.Items.mageBuyCount * 1.2)
        //     {
        //         NpcFunction.EatIndexItem(GameInstance, lans[0]);
        //     }
        // }
        // 清理红
        var heals = findIdxInAllItems(GameInstance, "金创药", true);
        if (heals != null && heals.Length > GameConstants.Items.healBuyCount)
        {
            NpcFunction.EatIndexItem(GameInstance, heals[0]);
        }
    }

    public static async Task<bool> detectAndWearHushen(MirGameInstanceModel GameInstance)
    {
        var fuName = GameConstants.Items.getFushen(GameInstance.CharacterStatus.Level);
        var useItem = GameInstance.CharacterStatus.useItems.Where(o => !o.IsEmpty && o.stdMode == 25 && o.Name == fuName).FirstOrDefault();
        // 先自动换符咒
        if (useItem != null)
        {
            return true;
        }

        var itemF = GameInstance.Items.Where(o => !o.IsEmpty && o.stdMode == 25 && o.Name == fuName).FirstOrDefault();
        if (itemF == null)
        {
            return false;
        }
        // 会自动
        var isCS = GameState.gamePath == "Client.exe";
        nint toIndex = (int)(isCS ? EquipPosition.BUJUK : EquipPosition.ArmRingLeft); // 必须左
        nint bagGridIndex = itemF.Index;
        await NpcFunction.takeOn(GameInstance, bagGridIndex + 6, toIndex);
        var useItem2 = GameInstance.CharacterStatus.useItems.Where(o => !o.IsEmpty && o.stdMode == 25 && o.Name == fuName).FirstOrDefault();
        return useItem2 != null;
    }


    public static async Task upgradeBBSkill(MirGameInstanceModel GameInstance)
    {
        if (!CapbilityOfSekeleton(GameInstance)) return;
        GameInstance.GameDebug("升级骨法");
        var level = GameInstance.CharacterStatus.Level;
        var target = level >= 26 ? 3 : (level >= 23 ? 2 : 1);
        while (true)
        {
            var skill = GameInstance.Skills.FirstOrDefault(o => o.Id == GameConstants.Skills.RecallBoneSpellId);
            await Task.Delay(100);
            if (skill == null) continue;
            if (skill.level >= target)
            {
                break;
            }
            // 没钱也退出
            if (GameInstance.CharacterStatus.coin < 50000)
            {
                break;
            }
            var fuName = GameConstants.Items.getFushen(GameInstance.CharacterStatus.Level);
            // 专用商人 更方便, 也就是土的药和F
            var item = GameInstance.Items.Where(o => !o.IsEmpty && o.Name == fuName).FirstOrDefault();
            if (item == null)
            {
                // 购买
                var count = 3;
                GameInstance.GameInfo($"购买{fuName}{count}个");
                var (npcMap, npcName, x, y) = NpcFunction.PickMiscNpcByMap(GameInstance, "SKILLBB");
                bool pathFound = await PerformPathfinding(CancellationToken.None, GameInstance!, x, y, npcMap, 6);
                if (pathFound)
                {
                    await NpcFunction.ClickNPC(GameInstance!, npcName);
                    await NpcFunction.Talk2(GameInstance!, "@buy");

                    nint[] data = MemoryUtils.PackStringsToData(fuName);
                    SendMirCall.Send(GameInstance, 3005, data);
                    await Task.Delay(1000);
                    // 判断是否存在

                    var memoryUtils = GameInstance.memoryUtils!;
                    var menuListLen = memoryUtils.ReadToInt(memoryUtils.GetMemoryAddress(memoryUtils.GetMemoryAddress(GameState.MirConfig["TFrmDlg"],
                    (int)GameState.MirConfig["商店菜单偏移1"], (int)GameState.MirConfig["商店菜单偏移2"])));
                    if (menuListLen > 0)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            var addr = memoryUtils.GetMemoryAddress(GameState.MirConfig["TFrmDlg"], (int)GameState.MirConfig["商店菜单指针偏移"]);
                            memoryUtils.WriteInt(addr, 0);
                            await Task.Delay(600);
                            SendMirCall.Send(GameInstance, 3006, new nint[] { 0 });
                            await Task.Delay(700);
                        }
                    }

                    await NpcFunction.RefreshPackages(GameInstance);
                }
            }
            var item2 = GameInstance.Items.Concat(GameInstance.QuickItems).Where(o => !o.IsEmpty && o.Name.Contains("魔法药")).FirstOrDefault();
            if (item2 == null)
            {
                // 购买
                var count = 10;
                GameInstance.GameInfo($"购买蓝{count}个");
                var (npcMap, npcName, x, y) = NpcFunction.PickDrugNpcByMap(GameInstance, "SKILLBB");
                bool pathFound = await PerformPathfinding(CancellationToken.None, GameInstance!, x, y, npcMap, 6);
                if (pathFound)
                {
                    await NpcFunction.ClickNPC(GameInstance!, npcName);
                    await NpcFunction.Talk2(GameInstance!, "@buy");
                    await Task.Delay(500);
                    // 已经检测过存在了, 只看是否为空先
                    await NpcFunction.BuyImmediate(GameInstance, "魔法药(小量)", count);
                    await NpcFunction.RefreshPackages(GameInstance);

                    await NpcFunction.BuyImmediate(GameInstance, "魔法药(中量)", count);
                    await NpcFunction.RefreshPackages(GameInstance);
                }
            }
            // 检查是否已经在目标位置
            if (GameInstance.CharacterStatus.MapId != "3" ||
             Math.Max(Math.Abs(GameInstance.CharacterStatus.X - 368), Math.Abs(GameInstance.CharacterStatus.Y - 359)) > 3)
            {
                if (!await PerformPathfinding(CancellationToken.None, GameInstance!, 368, 359, "3", 3))
                {
                    return;
                }
            }
            // 查看骷髅是否损失
            while (true)
            {
                var monster = GameInstance.Monsters.Values.FirstOrDefault(o => !o.isDead && o.TypeStr == "(怪)" && o.Name.Contains($"({GameInstance.AccountInfo.CharacterName})"));
                if (monster == null)
                {
                    break;
                }
                await Task.Delay(300);
            }
            var isWearSS = await detectAndWearHushen(GameInstance);
            if (!isWearSS)
            {
                await Task.Delay(1000);
                continue;
            }
            sendSpell(GameInstance, GameConstants.Skills.RecallBoneSpellId, GameInstance.CharacterStatus.X, GameInstance.CharacterStatus.Y, 0);
            await Task.Delay(1000);
        }

    }

    public static List<(int CenterX, int CenterY, int Size)> FindOptimalSquareCoverage(List<(int X, int Y)> points, int maxSquares, int squareSize)
    {
        if (points.Count == 0) return new List<(int CenterX, int CenterY, int Size)>();

        var bestCombination = new List<(int CenterX, int CenterY, int Size)>();
        var maxCoverage = 0;
        int halfSize = squareSize / 2;

        // 生成候选正方形中心位置：考虑每个点周围可能的最优位置
        var candidateSquares = new HashSet<(int X, int Y, int Size)>();

        foreach (var point in points)
        {
            // 为每个点生成可能的正方形中心位置
            for (int dx = -halfSize; dx <= halfSize; dx++)
            {
                for (int dy = -halfSize; dy <= halfSize; dy++)
                {
                    candidateSquares.Add((point.X + dx, point.Y + dy, squareSize));
                }
            }
        }

        var candidateList = candidateSquares.ToList();

        // 搜索所有可能的1到maxSquares个正方形组合
        for (int numSquares = 1; numSquares <= Math.Min(maxSquares, candidateList.Count); numSquares++)
        {
            foreach (var combination in GetCombinations(candidateList, numSquares))
            {
                var coverage = CalculateTotalCoverage(combination, points, squareSize);
                if (coverage > maxCoverage)
                {
                    maxCoverage = coverage;
                    bestCombination = combination.ToList();

                    // 如果已经覆盖所有点，提前退出
                    if (coverage == points.Count)
                    {
                        return bestCombination;
                    }
                }
            }
        }

        return bestCombination;
    }

    public static IEnumerable<IEnumerable<T>> GetCombinations<T>(IEnumerable<T> elements, int k)
    {
        return k == 0 ? new[] { new T[0] } :
            elements.SelectMany((e, i) =>
                GetCombinations(elements.Skip(i + 1), k - 1).Select(c => (new[] { e }).Concat(c)));
    }

    public static int CalculateTotalCoverage(IEnumerable<(int CenterX, int CenterY, int Size)> squares, List<(int X, int Y)> points, int squareSize)
    {
        var coveredPoints = new HashSet<(int X, int Y)>();

        foreach (var point in points)
        {
            foreach (var square in squares)
            {
                if (IsPointInSquare(point, square, squareSize))
                {
                    coveredPoints.Add(point);
                    break; // Point is covered, no need to check other squares
                }
            }
        }

        return coveredPoints.Count;
    }

    public static bool IsPointInSquare((int X, int Y) point, (int CenterX, int CenterY, int Size) square, int squareSize)
    {
        int halfSize = squareSize / 2;
        return point.X >= square.CenterX - halfSize &&
               point.X <= square.CenterX + halfSize &&
               point.Y >= square.CenterY - halfSize &&
               point.Y <= square.CenterY + halfSize;
    }

    public static IEnumerable<(int X, int Y)> GetPointsInSquare((int CenterX, int CenterY, int Size) square, int squareSize)
    {
        for (int x = square.CenterX - squareSize / 2; x <= square.CenterX + squareSize / 2; x++)
        {
            for (int y = square.CenterY - squareSize / 2; y <= square.CenterY + squareSize / 2; y++)
            {
                yield return (x, y);
            }
        }
    }
    public static async Task switchBladeWideSlaying(MirGameInstanceModel instanceValue)
    {
        var isBladeNeed = instanceValue.Skills.FirstOrDefault(o => o.Id == 25) != null;
        if (!isBladeNeed) return;
        // 第一次初始化要先看下状态
        if (instanceValue.CharacterStatus.wideHitEnabled == null)
        {
            // 先初始化
            sendSpell(instanceValue!, GameConstants.Skills.wideHit, instanceValue.CharacterStatus.X, instanceValue.CharacterStatus.Y, 0);
            await Task.Delay(800);
            var lastChatState = instanceValue.chats.FindLast(o => o.Contains("半月剑法"));
            if (lastChatState == null)
            {
                instanceValue.CharacterStatus.wideHitEnabled = null;
            }
            else
            {
                instanceValue.CharacterStatus.wideHitEnabled = lastChatState.Contains("开");
            }
        }
        var turn = instanceValue.CharacterStatus.turn;
        // 查看朝向的怪, 九宫格怪物, 先根据turn得到坐标
        var targetCoords = GetThreeDirectionCoords(turn);
        var targetPositions = targetCoords.Select(o => (instanceValue.CharacterStatus.X + o.x, instanceValue.CharacterStatus.Y + o.y)).ToList();

        var monsters = targetPositions.SelectMany(o => MonsterFunction.GetMonstersByPosition(instanceValue.MonstersByPosition, o.Item1, o.Item2)).ToList();
        var toWideSkillEnabled = monsters.Where(t => t.stdAliveMon).Count() > 2;
        if (instanceValue.CharacterStatus.wideHitEnabled != toWideSkillEnabled)
        {
            sendSpell(instanceValue!, GameConstants.Skills.wideHit, instanceValue.CharacterStatus.X, instanceValue.CharacterStatus.Y, 0);
            await Task.Delay(800);
            var lastChatState = instanceValue.chats.FindLast(o => o.Contains("半月剑法"));
            if (lastChatState == null)
            {
                instanceValue.CharacterStatus.wideHitEnabled = null;
            }
            else
            {
                instanceValue.CharacterStatus.wideHitEnabled = lastChatState.Contains("开");
            }
        }
    }

    // 静态数组，避免重复创建
    private static readonly (int x, int y)[] DirectionOffsets = new (int x, int y)[]
    {
        (0, -1),   // 0: 上
        (1, -1),   // 1: 右上
        (1, 0),    // 2: 右
        (1, 1),    // 3: 右下
        (0, 1),    // 4: 下
        (-1, 1),   // 5: 左下
        (-1, 0),   // 6: 左
        (-1, -1)   // 7: 左上
    };

    /// <summary>
    /// 根据朝向获取前方3个位置的坐标差值
    /// </summary>
    /// <param name="direction">朝向 0-7 (0=上, 顺时针)</param>
    /// <returns>3个坐标差值 (左前, 正前, 右前)</returns>
    private static (int x, int y)[] GetThreeDirectionCoords(int direction)
    {
        // 获取当前朝向的前方3个位置 (左前, 正前, 右前)
        var leftFront = DirectionOffsets[(direction + 7) % 8];  // 左前 (逆时针1格)
        var front = DirectionOffsets[direction];                // 正前
        var rightFront = DirectionOffsets[(direction + 1) % 8]; // 右前 (顺时针1格)
        var rightFront2 = DirectionOffsets[(direction + 2) % 8]; // 右前 (顺时针1格)

        return new[] { leftFront, front, rightFront, rightFront2 };
    }
}

using MonoMod.RuntimeDetour;
using System.Numerics;
using System.Reflection;
using TerraAngel;
using Terraria;
using Terraria.Enums;
using Terraria.ID;
using static MyPlugin.MyPlugin;

namespace MyPlugin;

public class NPCMoveRoomArgs
{
    private static Hook? MoveRoomHook;
    private static Hook? SpawnTownNPCHook;

    #region 注册与卸载
    public static void Register()
    {
        // 获取 WorldGen.moveRoom 方法
        MethodInfo MoveRoomMethod = typeof(WorldGen).GetMethod("moveRoom", BindingFlags.Static | BindingFlags.Public, null, [typeof(int), typeof(int), typeof(int)], null)!;
        MoveRoomHook = new Hook(MoveRoomMethod, OnMoveRoom);

        // 获取 WorldGen.SpawnTownNPC 方法
        MethodInfo SpawnTownNPCMethod = typeof(WorldGen).GetMethod("SpawnTownNPC", BindingFlags.Static | BindingFlags.Public, null, [typeof(int), typeof(int)], null)!;
        SpawnTownNPCHook = new Hook(SpawnTownNPCMethod, OnSpawnTownNPC);
    }

    public static void Dispose()
    {
        MoveRoomHook?.Dispose();
        SpawnTownNPCHook?.Dispose();
    }
    #endregion

    #region 移动住房方法
    private static void OnMoveRoom(Action<int, int, int> orig, int x, int y, int n)
    {
        // 调用原始方法
        orig(x, y, n);

        if (Config.NPCMoveRoomForTeleport)
        {
            // 获取NPC实例
            NPC npc = Main.npc[n];

            // 瞬移NPC到新位置
            Vector2 pos = new Vector2(npc.homeTileX * 16f + 8f - npc.width / 2f, npc.homeTileY * 16f - npc.height);
            npc.Teleport(pos, 8);

            if(Main.netMode is 2)
            {
                byte householdStatus = WorldGen.TownManager.GetHouseholdStatus(npc);
                NetMessage.SendData(MessageID.UpdateNPCHome, -1, -1, null, npc.whoAmI, pos.X, pos.Y, householdStatus);
                NetMessage.SendData(MessageID.TeleportEntity, -1, -1, null, 1, n, pos.X, pos.Y, 8);
            }
        }
    }
    #endregion

    #region 生成城镇NPC方法
    private static TownNPCSpawnResult OnSpawnTownNPC(Func<int, int, TownNPCSpawnResult> orig, int x, int y)
    {
        // 调用原始方法
        var result = orig(x, y);

        if (Config.NPCMoveRoomForTeleport)
        {
            // 检查是否是重新安置NPC
            if (result == TownNPCSpawnResult.RelocatedHomeless)
            {
                // 找到最近被重新安置的NPC
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.homeless && npc.townNPC && npc.homeTileX == WorldGen.bestX && npc.homeTileY == WorldGen.bestY)
                    {
                        // 瞬移NPC到新位置
                        Vector2 pos = new Vector2(npc.homeTileX * 16f + 8f - npc.width / 2f, npc.homeTileY * 16f - npc.height);
                        npc.Teleport(pos, 8);

                        if (Main.netMode is 2)
                        {
                            byte householdStatus = WorldGen.TownManager.GetHouseholdStatus(npc);
                            NetMessage.SendData(MessageID.UpdateNPCHome, -1, -1, null, npc.whoAmI, pos.X, pos.Y, householdStatus);
                            NetMessage.SendData(MessageID.TeleportEntity, -1, -1, null, 1, npc.whoAmI, pos.X, pos.Y, 8);
                        }
                        break;
                    }
                }
            }
        }
        return result;
    }
    #endregion
}
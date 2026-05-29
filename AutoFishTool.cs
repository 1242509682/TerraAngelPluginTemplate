using System.Numerics;
using System.Reflection;
using ImGuiNET;
using Microsoft.Xna.Framework;
using MonoMod.RuntimeDetour;
using TerraAngel;
using TerraAngel.Input;
using TerraAngel.Utility;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.FishDropRules;
using Terraria.ID;
using static MyPlugin.MyPlugin;

namespace MyPlugin;

public class AutoFishTool
{
    private static AutoFishTool? instance;
    private static Hook? FishingCheckHook;
    private static FieldInfo? Context;
    private static MethodInfo? PullFishingBobbersMethod;

    // 实例状态
    private bool WantPullFish = false;
    private bool WantToReCast = false;
    private int FrameCountBeforeActualPullFish = 0;
    private int FrameCountBeforeActualCast = 0;
    private bool HasSpecialPosition = false;
    private Vector2 SpecialPosition = Vector2.Zero;

    // 单例实例
    public static AutoFishTool Instance => instance ??= new AutoFishTool();

    private AutoFishTool() { }

    /// <summary>插件加载时调用此方法注册钩子</summary>
    public static void Load()
    {
        if (FishingCheckHook != null) return;

        // 获取无参方法
        var method = typeof(Projectile).GetMethod("FishingCheck",
            BindingFlags.Public | BindingFlags.Instance,
            null, Type.EmptyTypes, null);

        // 缓存 _context 字段
        Context = typeof(Projectile).GetField("_context",
            BindingFlags.NonPublic | BindingFlags.Static);

            // 获取拉杆方法（私有实例方法，参数 Item）
        PullFishingBobbersMethod = typeof(Player).GetMethod("ItemCheck_PullFishingBobbers",
            BindingFlags.NonPublic | BindingFlags.Instance);

        // 钩子：委托签名为 Action<Projectile>
        FishingCheckHook = new Hook(method!, new Action<Action<Projectile>, Projectile>(FishingCheckDetour));
    }

    // 在 FishingCheckDetour 中获取上下文
    private static void FishingCheckDetour(Action<Projectile> orig, Projectile self)
    {
        orig(self);
        var context = Context?.GetValue(null) as FishingContext;
        if (context == null) return;
        Instance.OnFishingCheck(self, context.Fisher);
    }

    /// <summary>插件卸载时调用此方法卸载钩子</summary>
    public static void Unload()
    {
        FishingCheckHook?.Dispose();
        FishingCheckHook = null;
        instance = null;
    }

    // 每帧更新，由 MyPlugin.Update() 调用
    public void Update()
    {
        if (!Config.AutoFishEnabled) return;

        HasSpecialPosition = Config.AutoFishHasSpecialPosition;

        if (HasSpecialPosition)
        {
            ImDrawListPtr drawList = ImGui.GetBackgroundDrawList();
            drawList.AddCircleFilled(Util.WorldToScreenDynamic(SpecialPosition), 10f, Color.Red.PackedValue);

            if (InputSystem.Ctrl && InputSystem.Alt)
            {
                SpecialPosition = Main.MouseWorld;
            }
        }

        // 拉杆
        if (WantPullFish)
        {
            FrameCountBeforeActualPullFish--;
            if (FrameCountBeforeActualPullFish <= 0)
            {
                // 使用反射调用原版拉杆方法
                if (PullFishingBobbersMethod != null && Main.LocalPlayer?.HeldItem?.fishingPole > 0)
                {
                    PullFishingBobbersMethod.Invoke(Main.LocalPlayer, [Main.LocalPlayer.HeldItem]);
                }

                WantPullFish = false;
                WantToReCast = true;
                FrameCountBeforeActualCast = Main.rand.Next(
                    Config.AutoFishFrameCountRandomizationMin,
                    Config.AutoFishFrameCountRandomizationMax) + 50;
            }
        }

        // 重新抛竿
        if (WantToReCast)
        {
            if (Main.projectile.Any(x => x.active && x.bobber && x.owner == Main.myPlayer))
                return;

            FrameCountBeforeActualCast--;
            if (FrameCountBeforeActualCast <= 0)
            {
                Item heldItem = Main.LocalPlayer!.HeldItem;
                if (heldItem.fishingPole > 0)
                {
                    Main.LocalPlayer.Fishing_GetBait(out int baitPower, out int baitType);
                    if (baitPower > 0 && baitType > 0)
                    {
                        Main.LocalPlayer.controlUseItem = true;
                        int oldMouseX = Main.mouseX;
                        int oldMouseY = Main.mouseY;
                        if (HasSpecialPosition)
                        {
                            Main.mouseX = (int)SpecialPosition.X - (int)Main.screenPosition.X;
                            Main.mouseY = (int)SpecialPosition.Y - (int)Main.screenPosition.Y;
                        }
                        Main.LocalPlayer.ItemCheck();
                        NetMessage.SendData(MessageID.PlayerControls, number: Main.myPlayer);
                        Main.mouseX = oldMouseX;
                        Main.mouseY = oldMouseY;
                    }
                }
                WantToReCast = false;
            }
        }
    }

    // 钩子回调（实例方法）
    public void OnFishingCheck(Projectile bobber, FishingAttempt fish)
    {
        if (!Config.AutoFishEnabled) return;
        if (bobber.owner != Main.myPlayer) return;

        bool wantToCatch = false;

        if (fish.rolledItemDrop > 0)
        {
            if (Config.AutoFishAcceptItems)
            {
                if (Config.AutoFishAcceptAllItems)
                {
                    wantToCatch = true;
                }
                else
                {
                    if (fish.questFish != -1 && Config.AutoFishAcceptQuestFish)
                        wantToCatch = true;
                    if (fish.crate && Config.AutoFishAcceptCrates)
                        wantToCatch = true;
                    if (fish.common && Config.AutoFishAcceptCommon)
                        wantToCatch = true;
                    if (fish.uncommon && Config.AutoFishAcceptUncommon)
                        wantToCatch = true;
                    if (fish.rare && Config.AutoFishAcceptRare)
                        wantToCatch = true;
                    if (fish.veryrare && Config.AutoFishAcceptVeryRare)
                        wantToCatch = true;
                    if (fish.legendary && Config.AutoFishAcceptLegendary)
                        wantToCatch = true;
                    if (!fish.crate && fish.questFish == -1 && !fish.common && !fish.uncommon && !fish.rare && !fish.veryrare && !fish.legendary && Config.AutoFishAcceptNormal)
                        wantToCatch = true;
                }
            }
        }

        if (fish.rolledEnemySpawn > 0 && Config.AutoFishAcceptNPCs)
        {
            wantToCatch = true;
        }

        if (wantToCatch && !WantPullFish)
        {
            WantPullFish = true;
            FrameCountBeforeActualPullFish = Main.rand.Next(
                Config.AutoFishFrameCountRandomizationMin,
                Config.AutoFishFrameCountRandomizationMax);
        }
    }
}
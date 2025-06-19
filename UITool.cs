using ImGuiNET;
using Microsoft.Xna.Framework.Input;
using TerraAngel;
using TerraAngel.Input;
using TerraAngel.Tools;
using static MyPlugin.MyPlugin;

namespace MyPlugin;

public class UITool : Tool
{
    public override string Name => "羽学插件设置";
    public override ToolTabs Tab => ToolTabs.MainTools;

    // 用于临时存储按键编辑状态
    private bool editingHealKey = false;
    private bool editingKillKey = false;

    #region UI与配置文件交互方法
    public override void DrawUI(ImGuiIOPtr io)
    {
        // 使用临时变量中转值
        bool enabled = Config.Enabled;
        bool autoHeal = Config.AutoHeal;
        int autoHealVal = Config.AutoHealVal;
        Keys healKey = Config.HealKey;

        bool mouseStrikeNPC = Config.MouseStrikeNPC;
        int mouseStrikeNPCRange = Config.MouseStrikeNPCRange;

        bool killOrRESpawn = Config.KillOrRESpawn;
        Keys killKey = Config.KillKey;

        ImGui.Checkbox("启用羽学插件", ref enabled);

        if (enabled)
        {
            ImGui.Separator();

            // 快速死亡复活开关
            ImGui.Checkbox("启用快速死亡/复活", ref killOrRESpawn);

            // 自杀/复活按键设置
            ImGui.SameLine();
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 10);
            DrawKeySelector("自杀/复活按键", ref killKey, ref editingKillKey);


            // 回血设置
            ImGui.Separator();
            ImGui.Checkbox("启用回血功能", ref autoHeal);
            ImGui.SameLine(); // 回血按键设置
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 10);
            DrawKeySelector("回血按键", ref healKey, ref editingHealKey);

            if (autoHeal)
            {
                ImGui.Indent();
                ImGui.Text("回血量:");
                ImGui.SameLine();
                ImGui.SetNextItemWidth(150);
                ImGui.SliderInt("##HealAmount", ref autoHealVal, 1, 500, "%d HP");

                ImGui.SameLine();
                ImGui.Text($"{autoHealVal} HP");
                ImGui.Unindent();
            }

            ImGui.Separator();

            // NPC伤害设置
            ImGui.Checkbox("启用鼠标范围伤害NPC", ref mouseStrikeNPC);
            if (mouseStrikeNPC)
            {
                ImGui.Indent();
                ImGui.Text("伤害范围:");
                ImGui.SameLine();
                ImGui.SetNextItemWidth(150);
                ImGui.SliderInt("##StrikeRange", ref mouseStrikeNPCRange, 1, 100, "%d 格");

                ImGui.SameLine();
                ImGui.Text($"{mouseStrikeNPCRange} 格");
                ImGui.Unindent();
            }
        }

        // 更新配置值
        Config.Enabled = enabled;
        Config.AutoHeal = autoHeal;
        Config.AutoHealVal = autoHealVal;
        Config.MouseStrikeNPC = mouseStrikeNPC;
        Config.MouseStrikeNPCRange = mouseStrikeNPCRange;
        Config.KillOrRESpawn = killOrRESpawn;
        Config.HealKey = healKey;
        Config.KillKey = killKey;

        // 保存按钮
        if (ImGui.Button("保存设置"))
        {
            Config.Write();
            ClientLoader.Chat.WriteLine("插件设置已保存", color);
        }

        // 添加重置按钮
        ImGui.SameLine();
        if (ImGui.Button("重置默认"))
        {
            Config.SetDefault();
            ClientLoader.Chat.WriteLine("已重置为默认设置", color);
        }
    } 
    #endregion

    #region 按键选择器辅助方法
    private void DrawKeySelector(string label, ref Keys key, ref bool editing)
    {
        // 显示按键标签和当前按键
        ImGui.Text($"{label}:");
        ImGui.SameLine();

        if (ImGui.Button($"{key}##{label}"))
        {
            editing = !editing;
        }

        // 如果正在编辑，显示提示
        if (editing)
        {
            ImGui.SameLine();
            ImGui.TextColored(new System.Numerics.Vector4(1, 1, 0, 1), "[按下新按键]");

            // 检测所有按键
            foreach (Keys k in Enum.GetValues(typeof(Keys)))
            {
                if (k == Keys.None) continue;

                if (InputSystem.IsKeyPressed(k))
                {
                    key = k;
                    editing = false;
                    break;
                }
            }
        }
    } 
    #endregion
}
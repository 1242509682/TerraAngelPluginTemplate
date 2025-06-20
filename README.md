# MyPlugin TerraAngel插件模板

- 作者: 羽学
- 出处: [TerraAngel-PLUGINS-ZH](https://github.com/UnrealMultiple/TerraAngel/blob/master/PLUGINS.md)
- 这是一个TerraAngel插件模板，集成了配置文件与重载功能、自动回血的控制台指令编写教学。

## 更新日志

```
v1.0.2
加入了自动使用物品功能（将鼠标范围伤害放入其中作为子功能）
加入了物品管理编辑器，方便于修改物品属性
UI代码逻辑简化，容易添加自己需要的物品属性

v1.0.1
新增添加UI方法与自定义按键触发相对功能
给自杀功能加入了死亡判断，死亡时使用则为立即复活
加入了使用物品时对范围内NPC造成手上武器的伤害(可自定义范围格数)
加入个CopyPlugin.cmd小脚本,用于《生成后事件》自动复制插件到插件目录
只需在游戏控制台使用指令即可完成功能重载：#reload_plugins

v1.0.0
实现配置文件重读，添加Reload指令
实现指令启用自动回血功能
```

## 指令

| 语法                             | 指令  |       权限       |                   说明                   |
| -------------------------------- | :---: | :--------------: | :--------------------------------------: |
| heal  | #heal 数值 |  无    |    启用禁用按键回血（可定义回血数值）    |
| kill  | #kill |  无    |    自杀与复活功能    |
| snpc  | #snpc |  无    |    启用鼠标范围伤害(可定义范围格数)    |
| autouse  | #autouse |  无    |    自动使用物品(可定义间隔)    |
| reload  | #reload |   无    |    重载配置文件    |

## 配置
> 配置文件位置：D:\Documents\My Games\Terraria\TerraAngel\MyPlugin.json
```json
{
  "插件开关": true,
  "回血按键": 72,
  "开启快捷键回血": true,
  "回血值": 100,
  "自杀复活按键": 75,
  "快速自杀复活": true,
  "鼠标范围伤害NPC": false,
  "鼠标伤害NPC格数": 44,
  "自动使用按键": 74,
  "自动使用物品": false,
  "使用间隔(毫秒)": 500,
  "物品管理按键": 73,
  "修改物品": [
    {
      "Name": "大师诱饵",
      "Type": 2676,
      "Damage": -1,
      "Defense": 0,
      "Stack": 9999,
      "Prefix": 0,
      "Crit": 0,
      "KnockBack": 0.0,
      "UseTime": 100,
      "UseAnimation": 100,
      "UseStyle": 0,
      "Ammo": 0,
      "bait": 10,
      "HealLife": 0,
      "HealMana": 0,
      "UseAmmo": 0,
      "AutoReuse": false,
      "UseTurn": false,
      "Channel": false,
      "NoMelee": false,
      "NoUseGraphic": false,
      "Shoot": 0,
      "ShootSpeed": 0.0,
      "Melee": false,
      "Magic": false,
      "Ranged": false,
      "Summon": false,
      "Value": 5000
    },
    {
      "Name": "血腥屠刀",
      "Type": 795,
      "Damage": 250,
      "Defense": 0,
      "Stack": 1,
      "Prefix": 81,
      "Crit": 5,
      "KnockBack": 5.75,
      "UseTime": 22,
      "UseAnimation": 22,
      "UseStyle": 1,
      "Ammo": 0,
      "bait": 0,
      "HealLife": 10,
      "HealMana": 0,
      "UseAmmo": 0,
      "AutoReuse": false,
      "UseTurn": false,
      "Channel": false,
      "NoMelee": false,
      "NoUseGraphic": false,
      "Shoot": 0,
      "ShootSpeed": 0.0,
      "Melee": true,
      "Magic": false,
      "Ranged": false,
      "Summon": false,
      "Value": 41829
    }
  ],
  "物品管理": true
}
```
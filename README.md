# MyPlugin TerraAngel插件模板

- 作者: 羽学
- 出处: [TerraAngel-PLUGINS-ZH](https://github.com/UnrealMultiple/TerraAngel/blob/master/PLUGINS.md)
- 这是一个TerraAngel插件模板，集成了配置文件与重载功能、自动回血的控制台指令编写教学。

# TerraAngel v1.0.3 更新说明 | TerraAngel v1.0.3 Update Notes

## 更新功能 | New Features

加入了打开物品管理器时的音效播放  
Added sound effects playback when opening the item manager

优化了物品管理器的模糊搜索功能  
Optimized the fuzzy search function of the item manager

加入了常见的物品修改参数(渔力/镐斧锤力/放置图格或墙ID等)  
Added common item modification parameters (fishing force / axe and hammer and pickaxe power / placement tile or wall ID, etc.)

加入了一键修改饰品栏前缀（仅适配大师难度）  
Added one-click modification of accessory bar prefix (only suitable for master difficulty)

加入了一键收藏背包虚空袋物品  
Added one-click bookmarking of backpack and void bag items

---

## 特别说明 | Special Note

在我完成这个版本的更新时，我的父亲在此刻突然去世，  
At the moment I finished updating this version, my father suddenly passed away,

我不能在第一时间挽留我父亲感到自责，它会作为我人生中很重要的代码更新  
I cannot forgive myself for not being able to stay by his side in his final moments — this will always be a significant code update in my life

因为我把大量时间投入到开发上，导致忽视了对家人的感受，  
because I invested so much time into development, neglecting the feelings of my family,

对于公益插件的开发，我是没有任何收入来源的。  
This is a plugin developed for public good, and I have no source of income from it.

---

## 开发初衷 | Development Motivation

纵然很多人反对这个插件模板的开源，一直有人认为TerraAngel就是外挂或者作弊器的存在。  
Although many people oppose the open-sourcing of this plugin template, some still insist that TerraAngel is nothing but a cheat or hacking tool.

但它始于Terraria，只要它是用在正途上它都是有利于游戏本身的。  
But it all started with Terraria. As long as it's used properly, it can positively contribute to the game itself.

有人问我为什么不去开发MOD，我认为MOD本身也有作弊相关的修改，不应该被区别对待。  
People ask me why I don't develop MODs instead.

作为开发者，价值观不应该跟着别人的思想去走，而是遵循自己的初心去做。  
In my opinion, MODs themselves often involve modifications that could also be considered cheating, so they shouldn't be treated differently.

例如：  
As a developer, one should not let external opinions dictate their values, but rather follow their own 初心 (original intention).

1. 修改物品功能，弥补了我的修改武器插件不能修改其他属性的问题  
   Modifying item functions fills the gap where my previous weapon modification plugin couldn't adjust other properties

2. 一键收藏背包，解决了TShock服务器开启SSC后，每次都需要重新收藏物品的问题  
   One-click favorite backpack solves the issue on TShock servers after enabling SSC, where you had to re-add items to favorites every time

至于其他功能：H键强制回血、一键修改前缀，更多的是借鉴了其他内存修改器功能进行C#代码化还原  
As for other features like forced healing via the H key and one-click prefix changes, they are mostly inspired by similar functionalities found in memory editors, now implemented cleanly in C#.

---

## 最后的话 | Final Words

请不要对这个世界充满恶意，存在即合理，错对只在一念之间，  
Please don’t look at the world with negativity. Existence itself implies reason, and right or wrong lies in the intent behind its use.

即使没有这个插件模板依旧会有人利用作弊途径，开源就是为了互相学习，而不是定义它的优劣性。  
Even without this open-source plugin, people would still find ways to cheat. The purpose of open-sourcing was for mutual learning, not to judge its merits or drawbacks.

现在我已退出Terraria相关插件开发，只想珍惜当下生活，不再对任何代码进行维护，希望谅解。  
I have now stepped away from Terraria-related plugin development entirely. I just want to cherish the life I have now and no longer maintain any code. I hope you can understand.

## 以往更新日志

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
  "鼠标伤害NPC格数": 3,
  "自动使用按键": 74,
  "自动使用物品": false,
  "使用间隔(毫秒)": 500,
  "修改前缀按键": 80,
  "快速收藏按键": 79,
  "物品管理": true,
  "应用修改按键": 73,
  "修改物品": []
}
```
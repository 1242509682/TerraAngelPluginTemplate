# MyPlugin TerraAngel插件模板

- 作者: 羽学
- 出处: [TerraAngel-PLUGINS-ZH](https://github.com/UnrealMultiple/TerraAngel/blob/master/PLUGINS.md)
- 这是一个TerraAngel插件模板，集成了配置文件与重载功能、自动回血的控制台指令编写教学。

## 更新日志

```
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
  "鼠标范围伤害NPC": true,
  "鼠标伤害NPC格数": 10
}
```
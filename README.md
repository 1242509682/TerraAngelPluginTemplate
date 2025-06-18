# MyPlugin TerraAngel插件模板

- 作者: 羽学
- 出处: [TerraAngel-PLUGINS-ZH](https://github.com/UnrealMultiple/TerraAngel/blob/master/PLUGINS.md)
- 这是一个TerraAngel插件模板，集成了配置文件与重载功能、自动回血的控制台指令编写教学。

## 更新日志

```
暂无
```

## 指令

| 语法                             | 指令  |       权限       |                   说明                   |
| -------------------------------- | :---: | :--------------: | :--------------------------------------: |
| heal  | #heal 数值 |  无    |    自动回血（开启关闭与定义回血数值）    |
| reload  | #reload |   无    |    重载配置文件    |

## 配置
> 配置文件位置：D:\Documents\My Games\Terraria\TerraAngel\MyPlugin.json
```json
{
  "插件开关": true,
  "每秒回血": true,
  "回血值": 30
}
```
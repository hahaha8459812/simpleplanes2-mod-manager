# SimplePlanes 2 Mod Manager

[English](README.en.md)

这是一个外置的 `SimplePlanes 2` 插件管理器原型。目标是让后续 SP2 插件不再各自携带 BepInEx，而是统一由管理器安装和维护 BepInEx，再由管理器安装、启用、禁用和卸载各个插件。

当前版本是第一阶段骨架，重点是本地可靠性：

- 选择并保存 `SimplePlanes 2` 游戏目录。
- 检测游戏目录是否有效。
- 检测 BepInEx 5 是否已安装。
- 从本地 BepInEx zip 安装 BepInEx。
- 扫描 `BepInEx\plugins` 下已安装插件。
- 从本地插件 zip 安装插件。
- 启用 / 禁用插件 DLL。
- 卸载插件目录。
- 打开游戏目录、插件目录和配置目录。

## 构建

本机当前没有 .NET SDK，因此第一版使用系统自带的 .NET Framework 编译器构建，避免引入额外工具链。

```powershell
cd E:\Code\simpleplanes2-mod-manager
.\build.ps1 -Release
```

构建产物：

```text
artifacts\SimplePlanes2ModManager.exe
```

## 设计说明

第一版使用：

```text
.NET Framework WinForms
+ 内置 WebBrowser 面板
+ 嵌入式 HTML / CSS / JS
+ C# 后端桥接
```

这样可以先做到一个可直接运行的单文件 exe。后续如果安装 .NET SDK，可以升级为：

```text
.NET 8 WinForms
+ WebView2
+ PublishSingleFile
```

前端只负责显示状态和发起固定动作，所有文件操作都在 C# 后端里完成。

## 安全边界

- 安装 zip 时会校验解压路径，禁止写出游戏目录。
- 游戏运行时拒绝安装、更新、禁用、启用和卸载插件。
- 卸载插件默认只删除插件目录，不删除 `BepInEx\config`。
- 启用 / 禁用通过修改 DLL 扩展名完成，不直接删除 DLL。
- 管理器不会执行插件包里的脚本。


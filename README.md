# SimplePlanes 2 Mod Manager

[English](README.en.md)

这是一个外置的 `SimplePlanes 2` 插件管理器。目标是让后续 SP2 插件不再各自携带 BepInEx，而是统一由管理器安装和维护 BepInEx，再由管理器安装、启用、禁用、更新和卸载各个插件。

当前版本是第一阶段可运行原型，重点是本地可靠性和统一插件规范：

- 选择并保存 `SimplePlanes 2` 游戏目录。
- 检测游戏目录是否有效。
- 检测 BepInEx 5 是否已安装。
- 安装 exe 内置的 `BepInEx 5 Mono x64`。
- 从本地 BepInEx zip 安装 BepInEx，作为备用入口。
- 扫描 `BepInEx\plugins` 下已安装插件。
- 从本地插件 zip 安装插件。
- 从 GitHub 仓库或远程 `index.json` 安装插件。
- 启用 / 禁用插件 DLL。
- 卸载插件目录，默认保留用户配置。
- 打开游戏目录、插件目录和配置目录。

## 构建

本机当前没有 .NET SDK，因此第一版使用系统自带的 .NET Framework 编译器构建，避免引入额外工具链。默认构建会下载并嵌入 `BepInEx_win_x64_5.4.23.2.zip`，最终 exe 自带 BepInEx。

```powershell
cd E:\Code\simpleplanes2-mod-manager
.\build.ps1 -Release
```

如果只想构建不内置 BepInEx 的调试版本：

```powershell
.\build.ps1 -Release -SkipBundledBepInEx
```

构建产物：

```text
artifacts\SimplePlanes2ModManager.exe
```

## 插件维护规范

为了让管理器可以安装和检查更新，后续插件仓库和发行包需要遵循统一约定。

### 发行包

普通插件发行包不应包含 BepInEx，只包含插件本体和必要资源。推荐结构：

```text
SimplePlanes2Example-Plugin.zip
├─ mod.json
└─ BepInEx\
   └─ plugins\
      └─ SimplePlanes2Example\
         └─ SimplePlanes2Example.dll
```

发行包根目录必须包含 `mod.json`，用于描述这个包里是什么插件、主文件在哪里。推荐字段：

```json
{
  "id": "simpleplanes2-example",
  "name": "SimplePlanes 2 Example",
  "version": "0.1.0",
  "description": "Short plugin description.",
  "fileName": "SimplePlanes2Example-Plugin.zip",
  "entryDll": "BepInEx/plugins/SimplePlanes2Example/SimplePlanes2Example.dll",
  "pluginDirectory": "BepInEx/plugins/SimplePlanes2Example",
  "configFiles": [
    "BepInEx/config/com.example.simpleplanes2.example.cfg"
  ]
}
```

要求：

- `id` 必须稳定，不要随版本变化。
- `name` 是显示给用户看的插件名。
- `version` 必须与本次发行版本一致。
- `description` 是简短简介。
- `fileName` 必须与发行包文件名一致。
- `entryDll` 指向插件主 DLL。
- 更新插件时默认不要覆盖用户配置。

### 仓库索引

插件仓库根目录必须包含一个可直接请求的 `index.json`。管理器会通过这个文件获取最新版信息和下载地址，也会用它做后续更新检测。

示例：

```json
{
  "id": "simpleplanes2-example",
  "name": "SimplePlanes 2 Example",
  "version": "0.1.0",
  "description": "Short plugin description.",
  "fileName": "SimplePlanes2Example-Plugin.zip",
  "downloadUrl": "https://github.com/user/simpleplanes2-example/releases/download/v0.1.0/SimplePlanes2Example-Plugin.zip",
  "repository": "https://github.com/user/simpleplanes2-example",
  "entryDll": "BepInEx/plugins/SimplePlanes2Example/SimplePlanes2Example.dll"
}
```

要求：

- 每次发布新插件版本时，必须同步更新 `index.json`。
- `version` 必须是最新版本。
- `fileName` 必须是最新发行包文件名。
- `downloadUrl` 必须是可直接下载 zip 的 http/https 地址。
- `index.json` 应该放在默认分支根目录，确保以下地址可访问：

```text
https://raw.githubusercontent.com/<owner>/<repo>/main/index.json
```

如果仓库默认分支是 `master`，管理器也会尝试：

```text
https://raw.githubusercontent.com/<owner>/<repo>/master/index.json
```

## 从 Git 安装

管理器支持输入 GitHub 仓库地址或直接输入 `index.json` 地址。

支持示例：

```text
https://github.com/hahaha8459812/simpleplanes2-minimap-plugin
https://raw.githubusercontent.com/hahaha8459812/simpleplanes2-minimap-plugin/main/index.json
```

安装流程：

```text
读取 index.json
-> 校验 id/name/version/fileName/downloadUrl
-> 下载发行包 zip
-> 解压到游戏目录
-> 刷新已安装插件列表
```

当前版本只支持 GitHub 仓库 URL 和直接 `index.json` URL。后续可以扩展为插件源列表和自动更新。

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
- 从 Git 安装时只接受 GitHub 仓库地址或直接 `index.json` 地址。

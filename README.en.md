# SimplePlanes 2 Mod Manager

[中文](README.md)

This is an external plugin manager for `SimplePlanes 2`. The goal is to stop shipping BepInEx inside every plugin package. Instead, the manager installs and maintains BepInEx, then installs, enables, disables, updates, and removes individual plugins.

The current version is a first-stage runnable prototype focused on local reliability and a shared plugin packaging convention:

- Select and persist the `SimplePlanes 2` game directory.
- Validate the game directory.
- Detect whether BepInEx 5 is installed.
- Install the BepInEx 5 Mono x64 package bundled inside the exe.
- Install BepInEx from a local BepInEx zip as a fallback path.
- Scan installed plugins under `BepInEx\plugins`.
- Install plugins from local plugin zips.
- Install plugins from a GitHub repository or remote `index.json`.
- Enable / disable plugin DLLs.
- Uninstall plugin directories while keeping user config by default.
- Open the game, plugins, and config directories.

## Build

The current machine does not have the .NET SDK installed, so the first version uses the system .NET Framework compiler to avoid an extra toolchain. The default build downloads and embeds `BepInEx_win_x64_5.4.23.2.zip`, so the final exe carries BepInEx.

```powershell
cd E:\Code\simpleplanes2-mod-manager
.\build.ps1 -Release
```

Debug build without bundled BepInEx:

```powershell
.\build.ps1 -Release -SkipBundledBepInEx
```

Build output:

```text
artifacts\SimplePlanes2ModManager.exe
```

## Plugin Maintenance Rules

To make installation and update checks work consistently, plugin repositories and release packages should follow the same convention.

### Release Package

Regular plugin release packages must not include BepInEx core files. They may only include the plugin itself, required resources, and package metadata. Recommended layout:

```text
SimplePlanes2Example-Plugin.zip
├─ mod.json
└─ BepInEx\
   └─ plugins\
      └─ SimplePlanes2Example\
         └─ SimplePlanes2Example.dll
```

The package root must contain `mod.json`, which describes the plugin and the main file in the package. Recommended fields:

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

Requirements:

- `id` must be stable and must not change between versions.
- `name` is the user-facing plugin name.
- `version` must match the release version.
- `description` is a short plugin description.
- `fileName` must match the release package file name.
- `entryDll` points to the main plugin DLL, must be under `BepInEx/plugins`, and must exist in the release package.
- If `pluginDirectory` is provided, `entryDll` must be inside `pluginDirectory`.
- `configFiles` only declares user config files used by the plugin. Release packages must not include these config files.
- Plugin updates must not overwrite user config.

Release packages must not include:

```text
winhttp.dll
doorstop_config.ini
.doorstop_version
changelog.txt
BepInEx/core/**
BepInEx/patchers/**
BepInEx/config/**
BepInEx/LogOutput.log
```

These files are maintained by the manager or the BepInEx runtime. Regular plugin packages must not overwrite them.

### Repository Index

The plugin repository root must contain a requestable `index.json`. The manager reads this file to find the latest version and package download URL. It will also use this file for update detection later.

Example:

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

Requirements:

- Update `index.json` every time a new plugin version is released.
- `version` must be the latest version.
- `fileName` must be the latest release package file name.
- `downloadUrl` must be a directly downloadable http/https zip URL.
- `id`, `version`, `fileName`, and `entryDll` in `index.json` must match the package `mod.json`.
- `index.json` should live in the default branch root so this URL works:

```text
https://raw.githubusercontent.com/<owner>/<repo>/main/index.json
```

If the default branch is `master`, the manager also tries:

```text
https://raw.githubusercontent.com/<owner>/<repo>/master/index.json
```

## Install From Git

The manager accepts a GitHub repository URL or a direct `index.json` URL.

Supported examples:

```text
https://github.com/hahaha8459812/simpleplanes2-minimap-plugin
https://raw.githubusercontent.com/hahaha8459812/simpleplanes2-minimap-plugin/main/index.json
```

Install flow:

```text
Read index.json
-> validate id/name/version/fileName/downloadUrl
-> download release zip
-> extract into the game directory
-> refresh installed plugins
```

The current version supports GitHub repository URLs and direct `index.json` URLs only. A plugin source list and automatic updates can be added later.

## Design

The first version uses:

```text
.NET Framework WinForms
+ embedded WebBrowser panel
+ embedded HTML / CSS / JS
+ C# backend bridge
```

This allows the manager to ship as a directly runnable single-file exe. After the .NET SDK is available, it can be upgraded to:

```text
.NET 8 WinForms
+ WebView2
+ PublishSingleFile
```

The frontend only displays state and calls fixed actions. All filesystem work is handled by the C# backend.

## Safety Boundaries

- Zip installs validate extraction paths and block writes outside the game directory.
- Plugin install, update, enable, disable, and uninstall actions are blocked while the game is running.
- Uninstall removes the plugin directory only; it does not delete `BepInEx\config`.
- Enable / disable is done by renaming DLL extensions, not deleting DLLs.
- The manager does not execute scripts from plugin packages.
- Git installs only accept GitHub repository URLs or direct `index.json` URLs.

# SimplePlanes 2 Mod Manager

[中文](README.md)

This is an external plugin manager prototype for `SimplePlanes 2`. The goal is to stop shipping BepInEx inside every plugin package. Instead, the manager installs and maintains BepInEx, then installs, enables, disables, and removes individual plugins.

The current version is a first-stage local-management skeleton:

- Select and persist the `SimplePlanes 2` game directory.
- Validate the game directory.
- Detect whether BepInEx 5 is installed.
- Install the BepInEx 5 Mono x64 package bundled inside the exe.
- Install BepInEx from a local BepInEx zip as a fallback path.
- Scan installed plugins under `BepInEx\plugins`.
- Install plugins from local plugin zips.
- Enable / disable plugin DLLs.
- Uninstall plugin directories.
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

param(
    [switch]$Release
)

[Console]::InputEncoding = [System.Text.UTF8Encoding]::new($false)
[Console]::OutputEncoding = [System.Text.UTF8Encoding]::new($false)
$OutputEncoding = [System.Text.UTF8Encoding]::new($false)
chcp 65001 > $null

$ErrorActionPreference = "Stop"

$projectRoot = $PSScriptRoot
$sourceRoot = Join-Path $projectRoot "src\SimplePlanes2ModManager"
$artifactsDir = Join-Path $projectRoot "artifacts"
$outputPath = Join-Path $artifactsDir "SimplePlanes2ModManager.exe"

function Get-CSharpCompilerPath {
    $candidates = @(
        "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe",
        "C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe"
    )

    foreach ($candidate in $candidates) {
        if (Test-Path $candidate) {
            return $candidate
        }
    }

    throw "Unable to find csc.exe from .NET Framework."
}

New-Item -ItemType Directory -Force -Path $artifactsDir | Out-Null

$sourceFiles = Get-ChildItem -Path $sourceRoot -Filter "*.cs" -Recurse | Sort-Object FullName | ForEach-Object { $_.FullName }
$references = @(
    "System.dll",
    "System.Core.dll",
    "System.Drawing.dll",
    "System.IO.Compression.dll",
    "System.IO.Compression.FileSystem.dll",
    "System.Web.Extensions.dll",
    "System.Windows.Forms.dll"
)

$resourceArgs = @(
    "/resource:$(Join-Path $sourceRoot "Web\index.html"),SimplePlanes2ModManager.Web.index.html",
    "/resource:$(Join-Path $sourceRoot "Web\app.css"),SimplePlanes2ModManager.Web.app.css",
    "/resource:$(Join-Path $sourceRoot "Web\app.js"),SimplePlanes2ModManager.Web.app.js"
)

$referenceArgs = $references | ForEach-Object { "/r:$_" }
$optimizeArg = if ($Release) { "/optimize+" } else { "/optimize-" }
$csc = Get-CSharpCompilerPath

& $csc /nologo /target:winexe $optimizeArg /out:$outputPath $referenceArgs $resourceArgs $sourceFiles
if ($LASTEXITCODE -ne 0) {
    throw "C# compilation failed."
}

Write-Host "Built $outputPath"

param(
    [Parameter(Position = 0)]
    [string]$GameDirectory,

    [Parameter(Position = 1)]
    [string]$OutputDirectory = "$PSScriptRoot\decompiled",

    [switch]$All
)

Set-Location "$PSScriptRoot"
$ErrorActionPreference = "Stop"

function Get-DvInstallDirFromTargets {
    $TargetsFile = Join-Path (Get-Location) "Directory.Build.targets"

    if (!(Test-Path $TargetsFile)) {
        return $null
    }

    try {
        [xml]$Xml = Get-Content $TargetsFile

        $DvInstallDir = $Xml.Project.PropertyGroup.DvInstallDir

        if ([string]::IsNullOrWhiteSpace($DvInstallDir)) {
            return $null
        }

        return $DvInstallDir.Trim()
    }
    catch {
        Write-Warning "Failed to parse Directory.Build.targets"
        return $null
    }
}

function ShouldSkipAssembly {
    param(
        [string]$AssemblyName
    )

    $AllowPrefixes = @(
        "DV",
        "Assembly-CSharp"
    )

    foreach ($Prefix in $AllowPrefixes) {
        if ($AssemblyName.StartsWith($Prefix, [System.StringComparison]::OrdinalIgnoreCase)) {
            return $false
        }
    }

    return $true
}

# Resolve game directory
if ([string]::IsNullOrWhiteSpace($GameDirectory)) {
    $GameDirectory = Get-DvInstallDirFromTargets

    if ($GameDirectory) {
        Write-Host "Using DvInstallDir from Directory.Build.targets"
    }
}

if ([string]::IsNullOrWhiteSpace($GameDirectory)) {
    Write-Error "No game directory provided and no DvInstallDir found in Directory.Build.targets"
    exit 1
}

try {
    $GameDirectory = (Resolve-Path $GameDirectory).Path
}
catch {
    Write-Error "Game directory does not exist: $GameDirectory"
    exit 1
}

$ManagedDir = Join-Path $GameDirectory "DerailValley_Data\Managed"

if (!(Test-Path $ManagedDir)) {
    Write-Error "Managed directory not found: $ManagedDir"
    exit 1
}

# Check ilspycmd
$IlspyCmd = Get-Command ilspycmd -ErrorAction SilentlyContinue

if (-not $IlspyCmd) {
    Write-Error "ilspycmd is not installed or not in PATH."
    Write-Host ""
    Write-Host "Install with:"
    Write-Host "dotnet tool install --global ilspycmd"
    exit 1
}

# Resolve output directory
$OutputDirectory = [System.IO.Path]::GetFullPath($OutputDirectory)

# Track whether output directory already existed
$OutputDirectoryExisted = Test-Path $OutputDirectory

# Create output directory
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null

# Only create .gitignore for newly created output folders
if (-not $OutputDirectoryExisted) {
    $GitIgnorePath = Join-Path $OutputDirectory ".gitignore"
    Set-Content -Path $GitIgnorePath -Value "/*"
}

# Find DLLs
$Dlls = Get-ChildItem -Path $ManagedDir -Filter "*.dll" | Sort-Object Name

if ($Dlls.Count -eq 0) {
    Write-Error "No DLLs found in $ManagedDir"
    exit 1
}

# Filter DLLs unless -All is specified
$DllsToProcess = @()

foreach ($Dll in $Dlls) {
    $AssemblyName = [System.IO.Path]::GetFileNameWithoutExtension($Dll.Name)

    if (-not $All -and (ShouldSkipAssembly $AssemblyName)) {
        continue
    }

    $DllsToProcess += $Dll
}

if ($DllsToProcess.Count -eq 0) {
    Write-Error "No DLLs selected for decompilation"
    exit 1
}

Write-Host ""
Write-Host "Game directory:"
Write-Host "  $GameDirectory"
Write-Host ""
Write-Host "Output directory:"
Write-Host "  $OutputDirectory" 

Write-Host ""
Write-Host "Mode:"
if ($All) {
    Write-Host "  Full decompilation (including common libraries)"
}
else {
    Write-Host "  Filtered decompilation (only game-specific assemblies)"
}

Write-Host ""
Write-Host "Assemblies to process: $($DllsToProcess.Count)"
Write-Host ""

$OverallStopwatch = [System.Diagnostics.Stopwatch]::StartNew()

$Total = $DllsToProcess.Count
$Completed = 0
$FailedAssemblies = @()

foreach ($Dll in $DllsToProcess) {

    $AssemblyName = [System.IO.Path]::GetFileNameWithoutExtension($Dll.Name)
    $AssemblyOutputDirectory = Join-Path $OutputDirectory $AssemblyName

    # progress reflects COMPLETED work only
    $percent = [int](($Completed / $Total) * 100)

    # ETA based on completed items
    $ElapsedSeconds = $OverallStopwatch.Elapsed.TotalSeconds

    if ($Completed -gt 0) {
        $avg = $ElapsedSeconds / $Completed
        $remaining = $Total - $Completed
        $secondsRemaining = [int]($avg * $remaining)
    }
    else {
        $secondsRemaining = 0
    }

    Write-Progress `
        -Activity "Decompiling DLLs" `
        -Status "$Completed / $Total - $($Dll.Name)" `
        -CurrentOperation $($Dll.Name) `
        -PercentComplete $percent `
        -SecondsRemaining $secondsRemaining

    New-Item -ItemType Directory -Force -Path $AssemblyOutputDirectory | Out-Null

    & ilspycmd `
        -p `
        --nested-directories `
        --disable-updatecheck `
        -o $AssemblyOutputDirectory `
        $Dll.FullName | Out-Null

    if ($LASTEXITCODE -ne 0) {
        $FailedAssemblies += $Dll.Name
    }

    $Completed++
}

$OverallStopwatch.Stop()

Write-Progress -Activity "Decompiling DLLs" -Completed

$TotalDuration = $OverallStopwatch.Elapsed.ToString("hh\:mm\:ss")

Write-Host "Finished decompilation in $TotalDuration"

if ($FailedAssemblies.Count -gt 0) {
    Write-Host ""
    Write-Host "Failures:"

    foreach ($Failed in $FailedAssemblies) {
        Write-Host "  $Failed"
    }
}

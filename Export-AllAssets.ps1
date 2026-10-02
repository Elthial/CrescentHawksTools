[CmdletBinding()]
param(
    [ValidateSet('Both', 'Inception', 'Revenge')]
    [string]$Game = 'Both',
    [string]$InceptionGameDirectory,
    [string]$RevengeGameDirectory,
    [string]$OutputDirectory = (Join-Path $PSScriptRoot 'ExtractedAssets'),
    [switch]$Force,
    [switch]$SkipBuild
)

$ErrorActionPreference = 'Stop'

function Resolve-InstallationDirectory {
    param([string]$RequestedPath, [string]$EnvironmentVariable, [string]$ExecutableName)

    $candidate = $RequestedPath
    if ([string]::IsNullOrWhiteSpace($candidate)) {
        $candidate = [Environment]::GetEnvironmentVariable($EnvironmentVariable)
    }
    if ([string]::IsNullOrWhiteSpace($candidate)) {
        throw "Supply the game directory or set $EnvironmentVariable."
    }

    $resolved = [IO.Path]::GetFullPath($candidate.Trim().Trim('"'))
    if (-not [IO.Directory]::Exists($resolved)) {
        throw "Game directory does not exist: $resolved"
    }
    $executable = [IO.Directory]::EnumerateFiles($resolved) |
        Where-Object { [IO.Path]::GetFileName($_).Equals($ExecutableName, [StringComparison]::OrdinalIgnoreCase) } |
        Select-Object -First 1
    if ($null -eq $executable) {
        throw "$resolved does not contain $ExecutableName."
    }
    return $resolved
}

function Invoke-Tool {
    param([string]$Tool, [string[]]$Arguments, [switch]$Capture)

    $output = & dotnet $Tool @Arguments 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "Tool failed ($([IO.Path]::GetFileName($Tool)) $($Arguments -join ' ')):`n$($output -join [Environment]::NewLine)"
    }
    if ($Capture) { return $output -join [Environment]::NewLine }
    $output | ForEach-Object { Write-Host $_ }
}

function Write-ArtifactText {
    param([string]$Path, [string]$Text)

    if ([IO.File]::Exists($Path) -and -not $Force) {
        throw "Output already exists: $Path. Pass -Force to overwrite it."
    }
    $parent = [IO.Path]::GetDirectoryName($Path)
    if (-not [string]::IsNullOrEmpty($parent)) {
        [IO.Directory]::CreateDirectory($parent) | Out-Null
    }
    [IO.File]::WriteAllText($Path, $Text, [Text.UTF8Encoding]::new($false))
}

function Get-RawCategory {
    param([string]$GameName, [string]$FileName)

    $extension = [IO.Path]::GetExtension($FileName).ToUpperInvariant()
    if ($extension -eq '.EXE') { return 'program' }
    if ($GameName -eq 'Inception') {
        if ($extension -eq '.ANM') { return 'animations' }
        if ($extension -in @('.CMP', '.ICN')) { return 'graphics' }
        if ($extension -eq '.MTP') { return 'maps' }
        if ($extension -eq '.BLD') { return 'scripts' }
        if ($extension -eq '.SIF') { return 'audio' }
        if ($FileName -match '^GAME[1-6]$') { return 'saves' }
        return 'other'
    }

    if ($extension -in @('.CPS', '.CMP', '.ICN', '.COL', '.FNT')) { return 'graphics' }
    if ($extension -eq '.MAP') { return 'maps' }
    if ($FileName -match '^SCENE.+\.DAT$') { return 'scenes' }
    if ($extension -in @('.MUS', '.BIN')) { return 'audio' }
    if ($FileName.Equals('SAVEGAME.DAT', [StringComparison]::OrdinalIgnoreCase)) { return 'saves' }
    if ($extension -eq '.DAT') { return 'data' }
    return 'other'
}

function Copy-RawAssets {
    param([string]$GameName, [string]$GameDirectory, [string]$Destination)

    $manifest = [Collections.Generic.List[object]]::new()
    foreach ($source in [IO.Directory]::EnumerateFiles($GameDirectory) | Sort-Object) {
        $name = [IO.Path]::GetFileName($source)
        $category = Get-RawCategory $GameName $name
        $relative = [IO.Path]::Combine('raw', $category, $name)
        $target = [IO.Path]::Combine($Destination, $relative)
        [IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($target)) | Out-Null
        if ([IO.File]::Exists($target) -and -not $Force) {
            throw "Output already exists: $target. Pass -Force to overwrite it."
        }
        [IO.File]::Copy($source, $target, $Force.IsPresent)
        $info = [IO.FileInfo]::new($source)
        $manifest.Add([PSCustomObject]@{
            file = $name
            category = $category
            relativePath = $relative.Replace('\', '/')
            length = $info.Length
            sha256 = (Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash.ToLowerInvariant()
        })
    }
    return $manifest
}

function New-ExtractionRoot {
    param([string]$Path)

    $fullPath = [IO.Path]::GetFullPath($Path)
    if ([IO.Directory]::Exists($fullPath) -and -not $Force) {
        $firstItem = [IO.Directory]::EnumerateFileSystemEntries($fullPath) | Select-Object -First 1
        if ($null -ne $firstItem) {
            throw "Extraction directory is not empty: $fullPath. Pass -Force to overwrite generated files."
        }
    }
    [IO.Directory]::CreateDirectory($fullPath) | Out-Null
    return $fullPath
}

function Add-ForceArgument {
    param([string[]]$Arguments)
    if ($Force) { return $Arguments + '--force' }
    return $Arguments
}

function Complete-Extraction {
    param([string]$GameName, [string]$Destination, [object[]]$Manifest)

    Write-ArtifactText (Join-Path $Destination 'metadata/manifest.json') (($Manifest | ConvertTo-Json -Depth 5) + [Environment]::NewLine)
    $readme = @"
$GameName local asset extraction

raw/       Exact files copied from the local game installation, grouped by purpose.
decoded/   Human-readable or directly viewable exports produced by CrescentHawksTools.
metadata/  Installation inventory plus source file lengths and SHA-256 hashes.

This directory contains copyrighted game data derived from your local installation.
Do not commit or redistribute it. The CrescentHawksTools repository ignores ExtractedAssets/.
"@
    Write-ArtifactText (Join-Path $Destination 'README.txt') $readme
    Write-Host "$GameName extraction complete: $Destination"
}

function Export-InceptionAssets {
    param([string]$GameDirectory, [string]$Destination, [string]$Tool)

    Write-Host "Extracting Inception assets to $Destination"
    $manifest = Copy-RawAssets 'Inception' $GameDirectory $Destination
    $metadata = Join-Path $Destination 'metadata'
    $decoded = Join-Path $Destination 'decoded'

    $inventory = Invoke-Tool $Tool @('inventory', '--game-dir', $GameDirectory, '--hash', '--json') -Capture
    Write-ArtifactText (Join-Path $metadata 'inventory.json') ($inventory + [Environment]::NewLine)
    Invoke-Tool $Tool (Add-ForceArgument @('export-images', '--game-dir', $GameDirectory, '--output-dir', (Join-Path $decoded 'graphics')))
    Invoke-Tool $Tool (Add-ForceArgument @('export-mech-spritesheet', '--game-dir', $GameDirectory, '--output', (Join-Path $decoded 'sprites/mechs.png'), '--metadata', (Join-Path $decoded 'sprites/mechs.json')))
    Invoke-Tool $Tool (Add-ForceArgument @('export-maps', '--game-dir', $GameDirectory, '--output-dir', (Join-Path $decoded 'maps')))
    Invoke-Tool $Tool (Add-ForceArgument @('export-animation-gifs', '--game-dir', $GameDirectory, '--output-dir', (Join-Path $decoded 'animations/gif')))

    foreach ($animation in [IO.Directory]::EnumerateFiles($GameDirectory, '*.ANM') | Sort-Object) {
        $name = [IO.Path]::GetFileName($animation)
        $stem = [IO.Path]::GetFileNameWithoutExtension($name)
        Invoke-Tool $Tool (Add-ForceArgument @('export-animation-frames', $name, '--game-dir', $GameDirectory, '--output-dir', (Join-Path $decoded "animations/frames/$stem")))
    }

    Invoke-Tool $Tool (Add-ForceArgument @('export-sif-wav', '--game-dir', $GameDirectory, '--mode', 'pc-speaker', '--output', (Join-Path $decoded 'audio/WWOODBT-pc-speaker.wav')))
    Invoke-Tool $Tool (Add-ForceArgument @('export-sif-wav', '--game-dir', $GameDirectory, '--mode', 'tandy', '--output', (Join-Path $decoded 'audio/WWOODBT-tandy.wav')))
    $effectsJson = Invoke-Tool $Tool @('list-sound-effects', '--json') -Capture
    Write-ArtifactText (Join-Path $decoded 'audio/sound-effects.json') ($effectsJson + [Environment]::NewLine)
    foreach ($effect in ($effectsJson | ConvertFrom-Json)) {
        $waveName = ('{0:D2}-{1}.wav' -f [int]$effect.Id, [string]$effect.Name)
        Invoke-Tool $Tool (Add-ForceArgument @('export-sound-effect-wav', ([string]$effect.Id), '--output', (Join-Path $decoded "audio/sound-effects/$waveName")))
    }

    foreach ($script in [IO.Directory]::EnumerateFiles($GameDirectory, '*.BLD') | Sort-Object) {
        $name = [IO.Path]::GetFileName($script)
        $stem = [IO.Path]::GetFileNameWithoutExtension($name)
        $text = Invoke-Tool $Tool @('disassemble-bld', $name, '--game-dir', $GameDirectory) -Capture
        $json = Invoke-Tool $Tool @('disassemble-bld', $name, '--game-dir', $GameDirectory, '--json') -Capture
        Write-ArtifactText (Join-Path $decoded "scripts/$stem.txt") ($text + [Environment]::NewLine)
        Write-ArtifactText (Join-Path $decoded "scripts/$stem.json") ($json + [Environment]::NewLine)
    }

    Write-ArtifactText (Join-Path $decoded 'data/weapons.json') ((Invoke-Tool $Tool @('dump-weapons', '--json') -Capture) + [Environment]::NewLine)
    foreach ($save in [IO.Directory]::EnumerateFiles($GameDirectory, 'GAME*') | Where-Object { [IO.Path]::GetFileName($_) -match '^GAME[1-6]$' } | Sort-Object) {
        $name = [IO.Path]::GetFileName($save)
        Write-ArtifactText (Join-Path $decoded "saves/$name.json") ((Invoke-Tool $Tool @('dump-save', $name, '--game-dir', $GameDirectory, '--json') -Capture) + [Environment]::NewLine)
        Invoke-Tool $Tool (Add-ForceArgument @('export-save-state', $name, '--game-dir', $GameDirectory, '--output', (Join-Path $decoded "saves/$name.txt")))
    }

    Complete-Extraction 'Inception' $Destination $manifest
}

function Export-RevengeAssets {
    param([string]$GameDirectory, [string]$Destination, [string]$Tool)

    Write-Host "Extracting Revenge assets to $Destination"
    $manifest = Copy-RawAssets 'Revenge' $GameDirectory $Destination
    $metadata = Join-Path $Destination 'metadata'
    $decoded = Join-Path $Destination 'decoded'

    $inventory = Invoke-Tool $Tool @('inventory', '--game-dir', $GameDirectory, '--hash', '--json') -Capture
    Write-ArtifactText (Join-Path $metadata 'inventory.json') ($inventory + [Environment]::NewLine)
    Invoke-Tool $Tool (Add-ForceArgument @('export-images', '--game-dir', $GameDirectory, '--output-dir', (Join-Path $decoded 'graphics/screens')))
    Invoke-Tool $Tool (Add-ForceArgument @('export-icns', '--game-dir', $GameDirectory, '--output-dir', (Join-Path $decoded 'graphics/tile-sheets')))

    foreach ($palette in [IO.Directory]::EnumerateFiles($GameDirectory, '*.COL') | Sort-Object) {
        $name = [IO.Path]::GetFileName($palette)
        $stem = [IO.Path]::GetFileNameWithoutExtension($name)
        Invoke-Tool $Tool (Add-ForceArgument @('export-palette', $name, '--game-dir', $GameDirectory, '--output', (Join-Path $decoded "graphics/palettes/$stem.png")))
    }
    foreach ($font in [IO.Directory]::EnumerateFiles($GameDirectory, '*.FNT') | Sort-Object) {
        $name = [IO.Path]::GetFileName($font)
        $stem = [IO.Path]::GetFileNameWithoutExtension($name)
        Invoke-Tool $Tool (Add-ForceArgument @('export-font', $name, '--game-dir', $GameDirectory, '--output', (Join-Path $decoded "graphics/fonts/$stem.png")))
    }
    foreach ($image in [IO.Directory]::EnumerateFiles($GameDirectory, '*.CMP') | Sort-Object) {
        $name = [IO.Path]::GetFileName($image)
        $stem = [IO.Path]::GetFileNameWithoutExtension($name)
        Invoke-Tool $Tool (Add-ForceArgument @('export-cmp', $name, '--game-dir', $GameDirectory, '--output', (Join-Path $decoded "graphics/cmp/$stem.png")))
    }
    foreach ($map in [IO.Directory]::EnumerateFiles($GameDirectory, '*.MAP') | Sort-Object) {
        $name = [IO.Path]::GetFileName($map)
        $stem = [IO.Path]::GetFileNameWithoutExtension($name)
        Invoke-Tool $Tool (Add-ForceArgument @('export-map', $name, '--game-dir', $GameDirectory, '--output', (Join-Path $decoded "maps/$stem.png")))
    }

    Invoke-Tool $Tool (Add-ForceArgument @('export-scenes', '--game-dir', $GameDirectory, '--output', (Join-Path $decoded 'scenes/manifest.json')))
    Invoke-Tool $Tool (Add-ForceArgument @('export-scene-evidence', '--game-dir', $GameDirectory, '--output-dir', (Join-Path $decoded 'scenes/evidence')))
    foreach ($scene in [IO.Directory]::EnumerateFiles($GameDirectory, 'SCENE*.DAT') | Sort-Object) {
        $name = [IO.Path]::GetFileName($scene)
        $stem = [IO.Path]::GetFileNameWithoutExtension($name)
        Invoke-Tool $Tool (Add-ForceArgument @('export-scene-map', $name, '--game-dir', $GameDirectory, '--output', (Join-Path $decoded "scenes/maps/$stem.png")))
    }

    Write-ArtifactText (Join-Path $decoded 'data/unit-types.json') ((Invoke-Tool $Tool @('dump-unit-types', '--game-dir', $GameDirectory, '--json') -Capture) + [Environment]::NewLine)
    Write-ArtifactText (Join-Path $decoded 'data/unit-types.csv') ((Invoke-Tool $Tool @('dump-unit-types', '--game-dir', $GameDirectory, '--csv') -Capture) + [Environment]::NewLine)
    Write-ArtifactText (Join-Path $decoded 'data/unit-sprites.json') ((Invoke-Tool $Tool @('inspect-unit-sprites', '--game-dir', $GameDirectory, '--json') -Capture) + [Environment]::NewLine)
    Write-ArtifactText (Join-Path $decoded 'data/hit-locations.json') ((Invoke-Tool $Tool @('inspect-hit-locations', '--game-dir', $GameDirectory, '--json') -Capture) + [Environment]::NewLine)
    Write-ArtifactText (Join-Path $decoded 'data/cga-translation.json') ((Invoke-Tool $Tool @('inspect-cga-translation', '--game-dir', $GameDirectory, '--json') -Capture) + [Environment]::NewLine)
    Invoke-Tool $Tool (Add-ForceArgument @('export-weapon-evidence', '--game-dir', $GameDirectory, '--output-dir', (Join-Path $decoded 'data/weapons')))
    Invoke-Tool $Tool (Add-ForceArgument @('export-speaker-effect-evidence', '--game-dir', $GameDirectory, '--output-dir', (Join-Path $decoded 'audio/speaker-effects')))
    Invoke-Tool $Tool (Add-ForceArgument @('export-digital-sounds', '--game-dir', $GameDirectory, '--output-dir', (Join-Path $decoded 'audio/digital-sounds')))

    $savePath = [IO.Directory]::EnumerateFiles($GameDirectory) | Where-Object { [IO.Path]::GetFileName($_).Equals('SAVEGAME.DAT', [StringComparison]::OrdinalIgnoreCase) } | Select-Object -First 1
    if ($null -ne $savePath) {
        Write-ArtifactText (Join-Path $decoded 'saves/SAVEGAME.json') ((Invoke-Tool $Tool @('dump-save', 'SAVEGAME.DAT', '--game-dir', $GameDirectory, '--json') -Capture) + [Environment]::NewLine)
    }

    Complete-Extraction 'Revenge' $Destination $manifest
}

if (-not $SkipBuild) {
    & (Join-Path $PSScriptRoot 'Build.ps1')
    if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }
}

$inceptionTool = Join-Path $PSScriptRoot 'src/InceptionTools/bin/Release/net10.0/InceptionTools.dll'
$revengeTool = Join-Path $PSScriptRoot 'src/RevengeTools/bin/Release/net10.0/RevengeTools.dll'
$root = New-ExtractionRoot $OutputDirectory

if ($Game -in @('Both', 'Inception')) {
    $directory = Resolve-InstallationDirectory $InceptionGameDirectory 'BTCHI_GAME_DIR' 'BTECH.EXE'
    Export-InceptionAssets $directory (Join-Path $root 'Inception') $inceptionTool
}
if ($Game -in @('Both', 'Revenge')) {
    $directory = Resolve-InstallationDirectory $RevengeGameDirectory 'BTCHR_GAME_DIR' 'REVENGE.EXE'
    Export-RevengeAssets $directory (Join-Path $root 'Revenge') $revengeTool
}

Write-Host "Asset extraction root: $root"

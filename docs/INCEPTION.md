# InceptionTools

For decoded game-data tables, see the
[Inception reference section](reference/README.md#the-crescent-hawks-inception).

For original weapon presentation, see
[combat effects, colours, sprites and sound dispatch](reference/INCEPTION_COMBAT_EFFECTS.md).

InceptionTools is a command-line toolkit for inspecting, extracting, converting,
and safely modifying data from a user-supplied installation of the 1988 DOS
game *BattleTech: The Crescent Hawk's Inception*.

The toolkit does not include the original game. It expects the original
external files and is designed so that a future replacement executable can
continue to load those files directly. Extracted images, audio, maps, and edited
saves are local outputs and should not be committed or redistributed.

## Requirements

- .NET 10 SDK, or a published InceptionTools executable.
- A supported game installation containing `BTECH.EXE` and its original data
  files.
- Windows is required only for the two direct audio playback commands. All
  inspection and export commands are portable.

The project has no third-party package dependencies.

## Complete asset extraction

From the repository root, extract every source file plus all supported decoded
forms into a filesystem-browsable directory:

```powershell
.\Export-AllAssets.ps1 -Game Inception -InceptionGameDirectory "D:\Games\BTECH"
```

The default `ExtractedAssets/Inception` tree contains raw files grouped by
purpose, decoded graphics, sprites, map images and metadata, animation GIFs and
individual frames, PC-speaker and Tandy audio, sound effects, BLD
disassemblies, weapon data and save reports. The directory is ignored by Git.

## Build and run

From the repository root:

```powershell
dotnet build src/InceptionTools/InceptionTools.csproj
dotnet run --project src/InceptionTools -- help
```

Place the command and its arguments after the `--` separator:

```powershell
dotnet run --project src/InceptionTools -- inventory --game-dir Chinception
```

After a Release build, the executable can be called directly:

```powershell
dotnet build src/InceptionTools/InceptionTools.csproj --configuration Release
./src/InceptionTools/bin/Release/net10.0/InceptionTools.exe help
```

Running without a command prints the same help summary as `help`.

## Locating the game

Commands that read original files locate the installation in this order:

1. `--game-dir PATH`;
2. the `BTCHI_GAME_DIR` environment variable;
3. the current directory or an ancestor containing `BTECH.EXE`;
4. an ignored `Chinception` directory under the current directory or an
   ancestor.

For example:

```powershell
$env:BTCHI_GAME_DIR = 'D:\Games\BTECH'
dotnet run --project src/InceptionTools -- inventory
```

Arguments such as `GAME1`, `O0.ANM`, and `MAP1.MTP` are installation filenames,
not paths. Lookup is case-insensitive and path traversal is rejected. Output
paths may point anywhere writable and are resolved relative to the current
directory when not absolute.

Numbers may be decimal or use a `0x` hexadecimal prefix.

## Common options

| Option | Meaning |
|---|---|
| `--game-dir PATH` | Select an original game installation explicitly. |
| `--json` | Produce structured JSON instead of the normal text report. |
| `--output FILE` | Select an output file. |
| `--output-dir DIR` | Select a batch or sequence output directory. |
| `--metadata FILE` | Select the JSON sidecar written with a sprite sheet or map. |
| `--force` | Permit replacement of an existing generated output. It never permits the save editor to overwrite its source save. |

Export commands refuse to overwrite files unless `--force` is supplied. Batch
exporters check their inputs and existing output paths before writing the batch.

## Installation and raw-file inspection

### `help`

Print the concise command list.

```powershell
dotnet run --project src/InceptionTools -- help
```

### `inventory`

Check expected executables, assets, maps, scripts, animations, sound data, and
save slots. Hashing is opt-in because it reads every complete file.

```text
inventory [--game-dir PATH] [--hash] [--json] [--output FILE]
```

```powershell
dotnet run --project src/InceptionTools -- inventory --game-dir Chinception
dotnet run --project src/InceptionTools -- inventory --game-dir Chinception --hash --json --output inventory.json
```

The command exits with code `2` when required files are missing or present files
fail known structural checks.

### `inspect`

Display a bounded hexadecimal/ASCII region from any installation file. The
default read is `0x80` bytes and the maximum is `0x1000` bytes.

```text
inspect FILE [--game-dir PATH] [--offset N] [--count N] [--decode-bld]
```

```powershell
dotnet run --project src/InceptionTools -- inspect DEMOFILE --game-dir Chinception --offset 0x20 --count 0x40
dotnet run --project src/InceptionTools -- inspect JAIL.BLD --game-dir Chinception --decode-bld
```

With `--decode-bld`, offsets are relative to decoded BLD payload offset zero;
raw file offset `file+0x02` is therefore decoded offset `0x0000`.

## Saves, characters, mechs, and weapons

These read-only commands expose the currently mapped structures without
changing the save.

### `dump-save`

```text
dump-save FILE [--game-dir PATH] [--json]
```

```powershell
dotnet run --project src/InceptionTools -- dump-save GAME1 --game-dir Chinception
```

### `dump-character`

Select a zero-based character slot. Player and enemy groups each contain slots
`0` through `7`; `player` is the default group.

```text
dump-character FILE SLOT [--group player|enemy] [--game-dir PATH] [--json]
```

```powershell
dotnet run --project src/InceptionTools -- dump-character GAME1 0 --group player --game-dir Chinception --json
```

### `dump-mech`

Select a zero-based mech slot. Player and enemy groups each contain slots `0`
through `3`; `player` is the default group.

```text
dump-mech FILE SLOT [--group player|enemy] [--game-dir PATH] [--json]
```

```powershell
dotnet run --project src/InceptionTools -- dump-mech GAME1 2 --group enemy --game-dir Chinception
```

### `dump-weapons`

Display typed interoperability metadata recovered from the executable-resident
weapon table. Verbatim executable bytes are not distributed. This command does
not require the external game directory.

```text
dump-weapons [--json]
```

```powershell
dotnet run --project src/InceptionTools -- dump-weapons --json
```

## BLD scripts

### `disassemble-bld`

Decode and disassemble a building script with payload offsets, raw instruction
bytes, known operand types, strings, branch tables, sound names, and unresolved
executable-side calls.

```text
disassemble-bld FILE [--game-dir PATH] [--offset N] [--json]
```

```powershell
dotnet run --project src/InceptionTools -- disassemble-bld JAIL.BLD --game-dir Chinception
dotnet run --project src/InceptionTools -- disassemble-bld TRAINING.BLD --game-dir Chinception --offset 0x120 --json
```

`--offset` uses the decoded payload domain, not the two-byte raw container
header. Unknown semantics remain explicitly labelled rather than guessed.

## ANM animations

Animation filenames are `O0.ANM` through `O21.ANM`. Frame indexes are
zero-based. PNG and GIF outputs use the standard EGA palette; GIFs loop
indefinitely using timing derived from the original retrace table.

### `inspect-animation`

```text
inspect-animation FILE [--game-dir PATH] [--json]
```

```powershell
dotnet run --project src/InceptionTools -- inspect-animation O0.ANM --game-dir Chinception
```

### `export-animation-frame`

Export one accumulated animation frame. The default output is
`NAME-frame-NN.png`.

```text
export-animation-frame FILE FRAME [--game-dir PATH] [--output FILE.png] [--force]
```

```powershell
dotnet run --project src/InceptionTools -- export-animation-frame O0.ANM 0 --game-dir Chinception --output artifacts/O0-frame-00.png
```

### `export-animation-frames`

Export every frame as a numbered PNG sequence. The default directory is
`NAME-frames`.

```text
export-animation-frames FILE [--game-dir PATH] [--output-dir DIR] [--force]
```

```powershell
dotnet run --project src/InceptionTools -- export-animation-frames O0.ANM --game-dir Chinception --output-dir artifacts/O0-frames
```

### `export-animation-gif`

Export one complete animation as a looping GIF. The default output is
`NAME.gif`.

```text
export-animation-gif FILE [--game-dir PATH] [--output FILE.gif] [--force]
```

```powershell
dotnet run --project src/InceptionTools -- export-animation-gif O0.ANM --game-dir Chinception --output artifacts/O0.gif
```

### `export-animation-gifs`

Export all 22 original animations. The default directory is `animation-gifs`.

```text
export-animation-gifs [--game-dir PATH] [--output-dir DIR] [--force]
```

```powershell
dotnet run --project src/InceptionTools -- export-animation-gifs --game-dir Chinception --output-dir artifacts/animation-gifs
```

## CMP/ICN graphics and sprites

The maintained decoder supports both original compression formats. CMP files
are exported as 320×200 images; ICN tile sources use a 16×4000 vertical strip
containing 250 sequential 16×16 tiles.

### `inspect-image`

```text
inspect-image FILE [--game-dir PATH] [--json]
```

```powershell
dotnet run --project src/InceptionTools -- inspect-image INFOCOM.CMP --game-dir Chinception
```

### `export-image`

Export one CMP or ICN file as an indexed PNG. The default output uses the source
basename with a `.png` extension.

```text
export-image FILE [--game-dir PATH] [--output FILE.png] [--force]
```

```powershell
dotnet run --project src/InceptionTools -- export-image BTTITLE.CMP --game-dir Chinception --output artifacts/BTTITLE.png
```

### `export-images`

Export all 12 known CMP/ICN files. The default directory is `images`.

```text
export-images [--game-dir PATH] [--output-dir DIR] [--force]
```

```powershell
dotnet run --project src/InceptionTools -- export-images --game-dir Chinception --output-dir artifacts/images
```

### `export-mech-spritesheet`

Export all 376 MECHSHAP source sprites into one transparent, labelled sequence
sheet plus JSON metadata. Rows cover Locust and Commando-style walking, firing,
and kicking sequences followed by effects, missile frames, wreckage, infantry, Jason,
teammates, enemies, and civilian NPC graphics.

Two legacy metadata group labels are misleading: `effects.debris` holds missile
flight sprites `0x82–0x91`, while `infantry.fallen` holds common combat hit
flashes `0x176/0x177`. Their source IDs are correct. Use the
[original direction-specific loops](reference/INCEPTION_COMBAT_EFFECTS.md#srm-and-lrm-flight-sprites)
instead of treating either metadata group as a single sequential animation.

```text
export-mech-spritesheet [--game-dir PATH] [--output FILE.png] [--metadata FILE.json] [--force]
```

Defaults are `MECHSHAP-spritesheet.png` and a same-basename `.json` file.

```powershell
dotnet run --project src/InceptionTools -- export-mech-spritesheet --game-dir Chinception --output artifacts/MECHSHAP-spritesheet.png --metadata artifacts/MECHSHAP-spritesheet.json
```

## MTP maps

The toolkit supports `MAP1.MTP` through `MAP15.MTP`, selects the corresponding
original ICN tile set, and exports both a rendered indexed PNG and a JSON
sidecar. The JSON preserves raw header blocks, source tile IDs, normalized tile
IDs, dimensions, and the current probable storage-order profile.

### `inspect-map`

```text
inspect-map FILE [--game-dir PATH] [--json]
```

```powershell
dotnet run --project src/InceptionTools -- inspect-map MAP15.MTP --game-dir Chinception
```

### `export-map`

```text
export-map FILE [--game-dir PATH] [--output FILE.png] [--metadata FILE.json] [--force]
```

The default outputs use the map basename with `.png` and `.json` extensions.

```powershell
dotnet run --project src/InceptionTools -- export-map MAP1.MTP --game-dir Chinception --output artifacts/MAP1.png --metadata artifacts/MAP1.json
```

### `export-maps`

Export all 15 maps and their JSON sidecars. The default directory is `maps`.

```text
export-maps [--game-dir PATH] [--output-dir DIR] [--force]
```

```powershell
dotnet run --project src/InceptionTools -- export-maps --game-dir Chinception --output-dir artifacts/maps
```

### `export-world-map`

Export a self-contained HTML viewer for the complete 16-by-16 Pacifica
(Chara III) world. The browser page reconstructs the procedural terrain,
renders the original `TINYLAND.CMP` overview tiles, and marks the seven towns
and four other fixed-map regions. It supports scrolling, pixel-perfect zoom,
coordinate inspection, independent world and marker visibility, overlay
filters and location shortcuts. The optional world-vertex overlay displays
the fixed control lattice using its rendered terrain categories (water,
forest, plains, hills, mountain and peak); raw indices and values remain
available on hover. Although it is logically a 17-by-17 corner grid, the
original code uses a 16-byte row stride, so each row's rightmost vertex aliases
the next row's leftmost vertex.

```text
export-world-map [--game-dir PATH] [--output FILE.html] [--seed N] [--force]
```

```powershell
dotnet run --project src/InceptionTools -- export-world-map --game-dir Chinception --output artifacts/inception-world-map.html
dotnet run --project src/InceptionTools -- export-world-map --game-dir Chinception --seed 0x123456 --output artifacts/alternate-world.html
```

Without `--seed`, the viewer opens on the **Pacifica (Chara III)** preset. It
starts the original three-byte RNG from the executable's initial `0x020304`
state and makes the same 256 calls that `Start_Game` uses to fill the
construction table. This fixes a misleading earlier assumption that the
zero-filled executable workspace was itself the terrain seed. A numeric seed
replaces that 24-bit RNG state; it is an editor control and not a field stored
by the original map or save formats. The seed and preset can also be changed
live inside the exported viewer.

The generated HTML embeds pixels decoded from the user's `TINYLAND.CMP` and is
therefore a local extraction output which should not be redistributed. It is a
self-contained browser file: no Node.js installation, package manager, web
server or network connection is required.

## Command-line save editor

The editor uses a readable text representation rather than modifying opaque
bytes directly. Unknown source bytes are retained. Every text export includes
the source save's SHA-256 fingerprint, and import refuses a different source.
The original save can never be selected as the output file.

Always keep an independent backup before testing an edited save in the game.

### `export-save-state`

Export all currently mapped fields to editable UTF-8 text. The default output
is `FILE-state.txt`.

```text
export-save-state FILE [--game-dir PATH] [--output FILE.txt] [--force]
```

```powershell
dotnet run --project src/InceptionTools -- export-save-state GAME1 --game-dir Chinception --output artifacts/GAME1-state.txt
```

Edit values following `=`, leaving the format and fingerprint lines intact.

### `import-save-state`

Apply the edited text over the matching original save and write a new save. The
default output is `FILE.edited`. Every changed byte and its field path is
printed.

```text
import-save-state FILE STATE.txt [--game-dir PATH] [--output FILE] [--force]
```

```powershell
dotnet run --project src/InceptionTools -- import-save-state GAME1 artifacts/GAME1-state.txt --game-dir Chinception --output artifacts/GAME1.edited
```

## SIF music

`WWOODBT.SIF` is the default input. Two arrangements are supported:

- `pc-speaker` reproduces the rapid sequential monophonic playback and is the
  default;
- `tandy` preserves the four logical channels and original Tandy cadence using
  a portable square-wave approximation.

### `inspect-sif`

```text
inspect-sif [FILE] [--game-dir PATH] [--json]
```

```powershell
dotnet run --project src/InceptionTools -- inspect-sif --game-dir Chinception
```

### `export-sif-wav`

Export a mono 16-bit PCM WAV. The default output is
`WWOODBT-MODE.wav`.

```text
export-sif-wav [FILE] [--mode pc-speaker|tandy] [--game-dir PATH] [--output FILE.wav] [--force]
```

```powershell
dotnet run --project src/InceptionTools -- export-sif-wav --mode tandy --game-dir Chinception --output artifacts/WWOODBT-tandy.wav
```

### `play-sif`

Render and synchronously play the music through the Windows multimedia API.

```text
play-sif [FILE] [--mode pc-speaker|tandy] [--game-dir PATH]
```

```powershell
dotnet run --project src/InceptionTools -- play-sif --mode pc-speaker --game-dir Chinception
```

## Executable-resident sound effects

The complete 18-entry sound table is embedded as documented executable data,
so these commands do not require an external game directory. Numeric IDs may be
decimal or hexadecimal; names are case-insensitive.

### `list-sound-effects`

```text
list-sound-effects [--json]
```

```powershell
dotnet run --project src/InceptionTools -- list-sound-effects
```

The current CLI names are listed below. The confirmed combat call sites name
**`0x02` (`mech-energy-weapon`) for mech lasers/PPCs** and
**`0x09` (`personnel-laser`) for personnel lasers**. ACs and machine guns
share `0x03`; SRMs and LRMs share `0x01`. `0x04` is a common hit sound for
mech targets as well as infantry. Flamers select no firing sound in the original
combat effects routine. See the [verified mapping](reference/INCEPTION_COMBAT_EFFECTS.md).

| ID | Name | ID | Name |
|---:|---|---:|---|
| `0x01` | `missile` | `0x0A` | `cache-door` |
| `0x02` | `mech-energy-weapon` | `0x0B` | `bow-string` |
| `0x03` | `repeating-projectile` | `0x0C` | `mech-startup` |
| `0x04` | `infantry-impact` | `0x0D` | `blade-impact` |
| `0x05` | `vibroblade` | `0x0E` | `mech-startup-failed` |
| `0x06` | `single-shot-projectile` | `0x0F` | `map-interaction` |
| `0x07` | `arena-destruction` | `0x10` | `password-accepted` |
| `0x08` | `terrain-damage` | `0x11` | `password-incorrect` |
| `0x09` | `personnel-laser` | `0x12` | `squished-by-mech` |

The incorrect names `mech-kick` and `laser` are no longer accepted; there are
no compatibility aliases. Numeric IDs and sound-table bytes are unchanged.
For example:

```powershell
dotnet run --project src/InceptionTools -- export-sound-effect-wav mech-energy-weapon --output artifacts/mech-laser-ppc.wav
dotnet run --project src/InceptionTools -- export-sound-effect-wav 0x03 --output artifacts/autocannon.wav
```

### `export-sound-effect-wav`

The default output is `sound-NN-NAME.wav`.

```text
export-sound-effect-wav ID|NAME [--output FILE.wav] [--force]
```

```powershell
dotnet run --project src/InceptionTools -- export-sound-effect-wav personnel-laser --output artifacts/personnel-laser.wav
dotnet run --project src/InceptionTools -- export-sound-effect-wav 0x10
```

### `play-sound-effect`

Render and synchronously play one effect through the Windows multimedia API.

```text
play-sound-effect ID|NAME
```

```powershell
dotnet run --project src/InceptionTools -- play-sound-effect cache-door
```

## Current fidelity notes

- CMP/ICN decompression, ANM frame accumulation, map pixels, and MECHSHAP
  rectangles are bounded and verified against the reference installation.
- Map storage-order names remain Probable even though portable exports match all
  existing legacy renderings pixel-for-pixel.
- The Tandy SIF output preserves notes, channels, rests, and cadence but does
  not yet emulate exact SN76489/PIT timbre.
- Sound-effect command streams are transcribed exactly; portable busy-loop
  timing and noise are approximations pending DOSBox-X calibration.
- `DEMOFILE` is recognized as the fixed attract-mode input stream but is not
  translated into a standalone instruction format.

## Exit behavior

Successful commands return exit code `0`. Invalid arguments, unsupported input,
missing files, bounds violations, and refused overwrites print an `Error:` line
to standard error and return exit code `1`. `inventory` uses exit code `2` for
a structurally incomplete or invalid installation report.

## Preservation and distribution

Original game files belong in an ignored local installation such as
`Chinception/`. Generated asset directories should also remain ignored. Source
code may contain documented data recovered from the executable, but external
levels, images, animations, music, saves, and extracted derivatives are not
part of the distributable toolkit.

Additional graphics-format notes are in
[`formats/INCEPTION_GRAPHICS.md`](formats/INCEPTION_GRAPHICS.md). Interpretive
names remain subordinate to bounded decoding behavior and verification.

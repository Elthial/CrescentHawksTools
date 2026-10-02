# RevengeTools

For decoded game-data tables, see the
[Revenge reference section](reference/README.md#the-crescent-hawks-revenge).

RevengeTools is a dependency-free .NET 10 command-line toolkit for inspecting,
extracting, and safely editing data from a user-supplied DOS installation of
*BattleTech: The Crescent Hawks' Revenge*.

The original game is not included. By default the toolkit locates it through
`--game-dir`, the `BTCHR_GAME_DIR` environment variable, or an ignored
`Chrevenge/` directory.

## Complete asset extraction

From the repository root, extract every source file plus all supported decoded
forms into a filesystem-browsable directory:

```powershell
.\Export-AllAssets.ps1 -Game Revenge -RevengeGameDirectory "D:\Games\REVENGE"
```

The default `ExtractedAssets/Revenge` tree contains raw files grouped by
purpose, decoded screens, palettes, fonts, tile sheets, maps, scene evidence,
unit and weapon data, digital samples, speaker-effect traces and save reports.
The directory is ignored by Git.

## Build and verify

From the repository root:

```powershell
dotnet build CrescentHawksTools.sln
dotnet run --project tests/RevengeTools.Verification
dotnet run --project tests/RevengeTools.Verification -- "<save-directory>"
dotnet run --project tests/RevengeTools.Verification -- "<save-directory>" "<game-directory>"
dotnet run --project src/RevengeTools -- help
```

The current dependency-free verification baseline is **82 assertions**.

The first optional argument performs byte-exact no-op save-editor round trips
over every slot in each `SAVEGAME.S??` file found there. A second installation
argument bounded-decodes all CPS files and verifies the observed compression
distribution.

## Implemented commands

```text
inventory [--game-dir PATH] [--hash] [--json] [--output FILE]
inspect FILE [--offset N] [--count N] [--game-dir PATH]
inspect-palette FILE.COL [--json]
export-palette FILE.COL [--output FILE.png] [--force]
inspect-font FILE.FNT [--json]
export-font FILE.FNT [--scale N] [--output FILE.png] [--force]
inspect-image FILE.CPS [--json]
export-image FILE.CPS [--palette FILE.COL] [--output FILE.png] [--force]
export-images [--output-dir DIR] [--force] [--json]
inspect-cmp FILE.CMP [--json]
export-cmp FILE.CMP [--palette FILE.COL] [--output FILE.png] [--force]
inspect-icn FILE.ICN [--json]
inspect-unit-sprites [--json] [--output FILE] [--force]
inspect-cga-translation [--json] [--output FILE] [--force]
export-icn FILE.ICN [--palette FILE.COL] [--columns N] [--output FILE.png] [--force]
export-icns [--columns N] [--output-dir DIR] [--force] [--json]
inspect-map FILE.MAP [--json]
export-map FILE.MAP [--icons FILE.ICN] [--palette FILE.COL] [--output FILE.png] [--force]
inspect-scene SCENEx.DAT [--json]
export-scenes [--output FILE.json] [--force] [--json]
export-scene-evidence [--output-dir DIR] [--force] [--json]
export-scene-map SCENEx.DAT [--palette FILE.COL] [--output FILE.png] [--force]
dump-unit-types [--json|--csv] [--output FILE]
dump-unit-type ID [--json]
inspect-hit-locations [--json]
export-weapon-evidence [--output-dir DIR] [--force] [--json]
inspect-speaker-effect INDEX [--json]
export-speaker-effect-evidence [--output-dir DIR] [--force] [--json]
inspect-digital-sounds [--json]
export-digital-sounds [--output-dir DIR] [--force] [--json]
dump-save FILE [--json]
dump-save-slot FILE SLOT [--json]
compare-save FILE_A SLOT_A FILE_B SLOT_B [--json] [--output FILE] [--force]
analyze-save-set DIRECTORY [--json] [--output FILE] [--force]
export-save-state FILE SLOT [--output FILE.txt] [--force]
import-save-state FILE SLOT STATE.txt [--output FILE] [--dry-run] [--force]
```

`export-weapon-evidence` decodes the 23 fixed weapon definitions embedded in
`REVENGE.EXE`, resolves their far name pointers, and joins their ammunition
family IDs to the fourteen capacity multipliers used by unit records. It also
exports equipment-location frequencies across all 89 `MECHTYPE.DAT` templates
and a per-chassis loadout matrix, plus a vehicle-only record table. Output
defaults to the ignored `RevengeTools.Output/weapons/` directory.

Vehicle analysis uses BattleTech vehicle record-sheet vocabulary—front, left
side, right side, rear, turret, and any motive-system-specific location—when a
raw engine index is proven. The current export deliberately retains numbered
armor/internal indices because the game stores vehicles in the same eight- and
eleven-value arrays as BattleMechs; assigning those indices to vehicle record
sheet locations is the next evidence step rather than a guessed alias.

`inspect-hit-locations` decodes the four directional 2d6 tables and the fixed
infantry table embedded in `REVENGE.EXE`, together with the 16-word
facing/direction selector ring. Location names describe the verified
BattleMech projection. Vehicle records reuse these raw indices, so the command
does not pretend that a BattleMech limb name is a vehicle record-sheet
location.

`inspect-speaker-effect` disassembles one of the 16 PC-speaker weapon-effect
streams embedded in `REVENGE.EXE`. `export-speaker-effect-evidence` writes all
stream traces and a JSON manifest to the ignored
`RevengeTools.Output/speaker-effects/` directory. The older
`inspect-music-stream` and `export-music-evidence` spellings remain accepted as
compatibility aliases. The decoder
requires every pointer-table span to end exactly at its `FFh` terminator, so
unknown opcodes or incorrect command widths fail instead of drifting into the
next stream.

`inspect-digital-sounds` decodes the 22 fixed `DS:5CFC` sound records and
resolves their resource indices through the executable's 18-byte resource
descriptor table. `export-digital-sounds` copies the exact source spans from
`INFOCOM.BIN` and `DPHRASES.BIN` to the ignored
`RevengeTools.Output/digital-sounds/` directory. The recovered timer ISR proves
unsigned 8-bit mono PCM at 8,000 Hz, so the exporter writes both byte-exact
`.bin` slices and canonical `.wav` files without modifying the source assets.
The same pass decodes every NUL-terminated sound-code list at `DS:5DEE-5F6A`.
Each character, including punctuation, is a key into the 22-record table rather
than sequence syntax. Inspection reports the resolved sample order, byte count,
and duration; export records it in `sequences.txt` and `manifest.json`.

Numbers accept decimal or a `0x` prefix.

The CPS commands decode Westwood LZW-12, LZW-14, and RLE files to indexed
320×200 PNGs. Batch export writes a deterministic `manifest.json` containing
the selected palette, compression method, consumed-byte counts, and SHA-256 of
each decoded 64,000-byte pixel buffer. Its default directory is the ignored
`RevengeTools.Output/cps/`.

The CMP commands decode the older four-bit Westwood format used by
`BOOMS.CMP`. Type 2 uses vertical RLE traversal and expands its packed nibbles
to a 320×200 indexed PNG. The default palette is `MAPS.COL`; all but one local
palette share the same first 16 colours used by this asset.

The ICN commands decode 250 sequential 16×16 four-bit tiles per file and
produce contact sheets. Batch output defaults to the ignored
`RevengeTools.Output/icn/` directory and includes a manifest. `ICONSET3.ICN`'s
declared compressed stream is decoded independently from its reported 2,941
physical trailing bytes.

`inspect-unit-sprites` joins `MECHTYPE.DAT:+88h` to the verified tactical
sprite arithmetic. It reports friendly and opposing facing tiles for each
non-infantry 16-tile MECHSET row and the compact two-tile infantry mappings.

`inspect-cga-translation` reads the 32-byte seed table embedded at runtime
`DS:03BCh` and deterministically rebuilds the two 256-byte packed-pixel phase
tables used by the CGA conversion paths. JSON output preserves all three tables
for comparison with a runtime memory capture.

Map export composes the byte-per-tile grid with the ICONSET selected by the
30-file SCENE corpus. `--icons` can override that verified mapping for an
experiment. The eight bytes after the dimensions are not independent metadata:
all eleven files duplicate the first eight grid cells there, and the parser
verifies that invariant. `MAP0.MAP` with `ICONSET0.ICN` has been visually
matched to the supplied stitched map evidence.

SCENE inspection separates the fixed metadata, reserved word, bytecode block,
and 72 bounded message strings. Verified metadata selectors resolve the matching MAP
and ICONSET filenames. `export-scene-map` therefore composes a scenario map
without requiring a manually supplied tile-set guess. Batch summaries default
to the ignored `RevengeTools.Output/scenes/manifest.json`.
`export-scene-evidence` preserves each metadata and instruction region as an
exact `.bin`, adds address-based hex views, a conservative decoded instruction
prefix, a known-opcode control-flow traversal, offset/message listings, and a
cross-scene metadata-column report under the ignored
`RevengeTools.Output/scenes/evidence/` directory.
Message listings label the six 11-entry generic speaker banks and the six
trailing scene-event slots. Instructions `DCh..E1h` include the exact linked
event message in both prefix and control-flow reports.

## Save-editor safety

The text editor currently permits changes to the slot label and the verified
current internal-structure, armor, and ammunition arrays. Campaign fields,
unit identity, maxima, pilot metadata, allegiance, and unknown bytes remain
read-only until controlled game tests establish their write constraints.

Import fingerprints the entire source, preserves all bytes not explicitly
changed, refuses to overwrite its source, supports `--dry-run`, and reports
every changed byte. An unchanged export/import is byte-identical.

`compare-save` accepts arbitrary save-container paths and compares one slot
from each. Every changed byte is grouped and labelled as campaign state,
proposed per-unit map coordinates, lance assignments, pilot availability, or a
named field within one of the 24 live-unit records. Reports default to the ignored
`RevengeTools.Output/save-diffs/` directory.

`analyze-save-set` scans every occupied slot in a directory of `SAVEGAME.S??`
containers. It emits the campaign progression plus per-byte value profiles for
the two 24-byte scenario-state arrays and the 37-byte pilot-availability state.

Unknown or only probable semantics remain labelled in reports and source rather
than being promoted to format guarantees.

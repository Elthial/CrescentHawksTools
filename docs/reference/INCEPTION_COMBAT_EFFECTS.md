# Inception combat graphics and sound

[Reference index](README.md) · [Weapon statistics](INCEPTION_WEAPONS.md) ·
[Maps, animations and sprites](INCEPTION_RESOURCES.md) · [Toolkit commands](../INCEPTION.md)

This page describes the **original combat presentation**, not a proposed remake.
The dispatch was checked against `1AE8:12C7–1E45` assembly, its translated
`Combat_AudioVisual_Effects`, the executable animation bytes and pointer tables,
and the toolkit's MECHSHAP rectangle metadata. IDs and dispatch rules below are
verified; exported RGB values are the standard EGA palette representation.

Weapon-table indices are **zero-based**. Mech component identifiers are one
greater: a PPC is weapon index `18 / 0x12`, component `19 / 0x13`. Do not compare
component IDs directly with the effects routine's weapon argument: older
annotations did this and incorrectly classified an autocannon as a beam and
a flamer as a repeating-projectile sound.

## Laser and PPC beams

Small, Medium and Large Lasers and PPCs (weapon indices `15–18 / 0x0F–0x12`)
all use the **same single-colour, pixel-plotted beam**. There is no distinct
PPC bolt, colour, sound, firing animation or timing branch. Laser pistols and
rifles (indices `12–13 / 0x0C–0x0D`) use the personnel beam branch.

| Beam branch | Shooter | EGA index | Standard exported RGB | Firing sound ID |
| --- | --- | ---: | --- | ---: |
| Mech laser/PPC | Friendly mech | `12 / 0x0C` | Bright red `#FF5555` | `0x02` |
| Mech laser/PPC | Enemy mech | `5 / 0x05` | Magenta `#AA00AA` | `0x02` |
| Personnel laser | Friendly personnel | `14 / 0x0E` | Bright yellow `#FFFF55` | `0x09` |
| Personnel laser | Enemy personnel | `10 / 0x0A` | Bright green `#55FF55` | `0x09` |
| Mech beam, adapter-zero override | Either side | `2 / 0x02` | Green `#00AA00` | `0x02` |
| Personnel beam, adapter-zero override | Either side | `3 / 0x03` | Cyan `#00AAAA` | `0x09` |

The last two rows document the original `GraphicsAdapter == 0` condition,
not a claim of full fidelity for every historical video adapter. The maintained
preservation startup selects the EGA pipeline. These colours are palette
indices chosen by the caller, not colours taken from a beam sprite.

These firing effects assume graphics are enabled and the main characters are
alive. The routine's early gate otherwise skips attack presentation and goes
directly to casualty cleanup; destruction handling has its own sound calls.

The beam is drawn one source pixel at a time with an integer error accumulator
and octant step tables, through `1F3D:031C`. It is not an anti-aliased line,
multi-colour effect or MECHSHAP sprite. Mech-family/direction-specific muzzle
offsets set its origin. After drawing, the game invokes sound `0x02` (mech) or
`0x09` (personnel), then handles the result. If both actors are off-screen, the
routine skips the beam and its firing sound but can still play hit audio.

On a successful visible hit, mech lasers/PPCs also select these cutaways,
subject to the animation-frequency gate in `0800:48B7`:

- Friendly Locust: `O4.ANM`.
- Other friendly mech family: `O7.ANM` (Wasp/generic humanoid firing).
- Enemy non-Locust firing at combatant zero: `O3.ANM` (cockpit hit).

## Autocannons and machine guns

AC/2, AC/5, AC/10 and AC/20 are indices `19–22 / 0x13–0x16`.
All select **sound `0x03`**, also used by the personnel machine gun (index 9)
and mech machine gun (index 23). There are no separate AC calibre sounds.

They use the standard directional firing pose, but the effect classifier leaves
them at `AttackEffect_None`. Consequently there is **no travelling bullet,
shell sprite or beam** drawn between shooter and target. This does not mean
the whole attack is invisible: a successful hit uses the common impact sound
and hit sprites below. There is no AC-specific firing cutaway; separate
critical-damage/arm-loss handlers can still request their own cutaways.

## SRM and LRM flight sprites

LRM-5/10/15/20 and SRM-2/4/6 (indices `25–31 / 0x19–0x1F`) share one missile
effect and **sound `0x01`**. Infantry SR Missile and Inferno (indices 10 and 11)
also select it. Rack size changes damage/cluster resolution, **not** the flight
sprite family or the number of missile sprites drawn simultaneously: this
routine draws one animated projectile along the attack path.

The sprite source is `MECHSHAP.CMP`. Every missile source rectangle is **8×8
pixels**. The eight direction pointers in executable DS `3EDB:2DD8` resolve to
four shared looping streams. Drawn IDs are the returned byte plus `0x68`;
`0x68` itself is a base, **not the first missile frame**.

| Direction indices | Heading | Stream at DS 3EDB | Relative frame loop (decimal) | Absolute sprite loop (hex) |
| --- | --- | --- | --- | --- |
| 0 | North | `3FD8` | `34,35,36,37,36,35` | `8A,8B,8C,8D,8C,8B` |
| 1,2,3 | NE, East, SE | `3FE0` | `26,27,28,29,28,27` | `82,83,84,85,84,83` |
| 4 | South | `3FE8` | `38,39,40,41,40,39` | `8E,8F,90,91,90,8F` |
| 5,6,7 | SW, West, NW | `3FF0` | `30,31,32,33,32,31` | `86,87,88,89,88,87` |

Each stream ends in `FE 07`, which loops to its first frame through the
`0800:1732` animation interpreter; it is not a literal sprite byte or an `FF`
terminator. Flight uses the integer/octant path with primary and secondary
steps scaled by four. Visible EGA missile steps restore the world, draw the
current sprite and present it, preventing a trail of un-erased missile frames.
Missile launch audio is dispatched before the both-actors-off-screen gate.
The routine disables the preliminary target-impact stream for mech missiles;
the later successful-hit impact remains enabled.

### Sprite source rectangles

Coordinates below are in the decoded **320×200 MECHSHAP source image**, not the
exporter's padded animation sheet. All sixteen rectangles are 8×8.

| IDs | Source Y | Source X, in ID order |
| --- | ---: | --- |
| `0x82–0x85` (130–133) | 144 | 32,40,48,56 |
| `0x86–0x89` (134–137) | 152 | 32,40,48,56 |
| `0x8A–0x8D` (138–141) | 160 | 32,40,48,56 |
| `0x8E–0x91` (142–145) | 168 | 32,40,48,56 |

## Flamers

Infantry Flamer (index 14) and mech Flamer (index 24) are **not** classified as
beams or missiles and select **no firing sound** in this routine. They still
use their actor's firing animation and, when successful, the common impact
audio/graphics. There is no dedicated flame jet or flamer cutaway here. The
presence of large/small fire sprites elsewhere does not make them flamer
projectiles.

## Shared impact, destruction and timing

When the caller reports an applied attack, the routine plays **sound `0x04`**
even for mech targets. Its historical toolkit name `infantry-impact` should not
be read as restricting it to infantry. For weapon index >=7 and a visible
target, the post-hit stream at DS `3EDB:41D8` is `7C 7D 7C FF`: add `0xFA` to
each returned frame to obtain **`0x176,0x177,0x176` (374,375,374)**. These are
the common hit flashes/bursts, not missile flight frames or a fallen person.
Their source rectangles are `(0,144,16,11)` and `(16,144,16,11)`.

Keep three sets of IDs distinct:

- `0x7C/0x7D`: large/small fire sprites.
- `0x7E/0x7F`: small/large impact sprites used elsewhere, including persistent
  effects; not the post-hit stream's absolute IDs.
- `0x176/0x177`: common combat post-hit flashes, derived with the `0xFA` base.

Ordinary mech destruction registers wreck `0x80` (Locust) or `0x81` (other
family) and plays sound `0x08`. Destruction of the arena stand-in instead
plays sound `0x07` and enters the arena-damage/reinforcement branch. These are
separate from the weapon's launch and common hit sounds.

The sequence is firing pose → weapon effect/synchronous weapon sound →
successful-hit sound/impact → applicable cutaway → restore shooter pose.
Visible firing frames each wait **five vertical retraces**, and the post-hit
sequence waits five more before requesting a cutaway. At the maintained 70 Hz
retrace model five retraces are about **71.4 ms**. This is not the whole shot
duration: synchronous PC-speaker busy loops and cutaways add time. Missile
travel does not add that same explicit five-retrace wait per step. Do not infer
CPU-independent sound durations from animation ticks or ANM metadata alone.

## Extracting and locating these effects

The toolkit names the two energy-weapon sounds after their confirmed callers:

| Verified use | Sound ID | Current CLI name | Executable table start |
| --- | ---: | --- | --- |
| SRM/LRM/manpack flight | `0x01` | `missile` | `3EDB:5008` |
| Mech laser/PPC beam | `0x02` | `mech-energy-weapon` | `3EDB:501C` |
| AC and machine gun fire | `0x03` | `repeating-projectile` | `3EDB:5030` |
| Common successful-hit noise | `0x04` | `infantry-impact` | `3EDB:5044` |
| Personnel laser beam | `0x09` | `personnel-laser` | `3EDB:50E8` |

The incorrect `mech-kick` and ambiguous `laser` CLI names have been removed,
without aliases. The actual kick weapon (index 32) does **not** enter the beam
branch that plays `0x02`. Numeric IDs, waveform commands and timing are unchanged.
The preservation constants are `Sound_MechEnergyWeapon` and `Sound_PersonnelLaser`.

```powershell
dotnet run --project src/InceptionTools -- export-sound-effect-wav mech-energy-weapon --output artifacts/mech-laser-ppc.wav
dotnet run --project src/InceptionTools -- export-sound-effect-wav 0x03 --output artifacts/autocannon.wav
dotnet run --project src/InceptionTools -- export-sound-effect-wav 0x01 --output artifacts/missile.wav
dotnet run --project src/InceptionTools -- export-sound-effect-wav personnel-laser --output artifacts/personnel-laser.wav
dotnet run --project src/InceptionTools -- export-mech-spritesheet --output artifacts/mechs.png --metadata artifacts/mechs.json
```

Current sprite-sheet metadata groups `0x82–0x91` under **`effects.debris`** and
`0x176/0x177` under **`infantry.fallen`**. Those are legacy descriptive labels,
not their verified combat use. Use `SourceSpriteId` in the JSON sidecar and the
loops above; a sequential sweep through all sixteen missile frames does not
reproduce any one original flight animation. Padded sheet cells are 24×24,
not the source missile dimensions. These sprite-group names have not been renamed.

## Evidence map

See the [Inception preservation source repository](https://github.com/Elthial/crescent-hawks-inception-sdl-port)
for these repository-relative files; no sibling checkout is needed to read this
reference page:

| Evidence | Source location |
| --- | --- |
| Sound predicates | `evidence/assembly/BTECH_1AE8.asm`, `1AE8:1521–1599` |
| Beam/missile classification and colours | same assembly, `1AE8:1599–1680` |
| Projectile path and sprite base | same assembly, `1AE8:1805–19EA` |
| Beam audio and common hit audio/frames | same assembly, `1AE8:19EA–1AD0` |
| Translated presentation routine | `src/Original/BTECH_1AE8_EFFECTS.c` |
| Animation streams, pointer tables and muzzle offsets | `src/Original/BTECH_1AE8_ANIMATION_DATA.c`; `evidence/assembly/BTECH_3EDB.asm` |
| Stream interpreter | `src/Original/BTECH_0800_WALK.c`, `0800:1732–17BA` |
| Sprite draw gateway | `src/Original/BTECH_1631_EFFECT_DRAW.c`, `1631:1F73–1FDE` |
| Speaker gate and interpreter | `0800:19BF`; `1FC5:0002` |
| Toolkit source rectangles | `src/InceptionTools.Core/Graphics/MechShapeSpriteCatalog.cs` |
| Toolkit sheet grouping and palette | `MechSpriteSheetComposer.cs`; `EgaIndexedPngEncoder.cs` in the same graphics directory |

All extraction commands require the user's own game graphics where applicable.
No original art, animation files or exported audio is included with this page.

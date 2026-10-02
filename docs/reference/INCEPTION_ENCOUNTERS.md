# Inception roaming encounters

This page describes the randomly generated roaming encounters in *BattleTech:
The Crescent Hawk's Inception*. Scripted arena, prison, training and story
combats use separate setup routines and are not included here.

The probabilities below assume that the original pseudo-random byte is
uniformly distributed over `0..255`. The implementation uses a mixture of
byte masks, exact `d6` helpers and a byte remainder; those mechanisms are kept
distinct rather than all being described as dice.

## When a roaming patrol is checked

The main exploration loop makes one encounter check per **world tick**. An
accepted input causes one tick after all movement steps associated with that
input. With no input, the idle countdown periodically causes a tick. A long
movement command therefore does not roll separately for every animation step.

A random encounter is eligible only when:

- the Kurita attack on the Citadel has occurred;
- the party is not inside the Star League cache; and
- `(RandomByte & TraitorBattleProbability) == 0`.

`TraitorBattleProbability` is a bit mask, not a percentage. The normal masks
seen in the campaign are:

| Campaign state | Mask | Passing bytes | Chance per eligible world tick | Equivalent |
| --- | ---: | ---: | ---: | --- |
| A recruit has been selected as the traitor | `0x1F` | 8 of 256 | 3.125% | 1 in 32 |
| The traitor has been discovered | `0x7F` | 2 of 256 | 0.78125% | 1 in 128 |

For an arbitrary mask containing `b` set bits, the pass probability is
`1 / 2^b`, provided those tested PRNG bits are uniform. A zero mask would pass
every eligible check, although that is not the ordinary campaign state.

Passing this check starts encounter generation; it does not guarantee that
combat begins. Generated enemies that cannot be placed are discarded, and the
controller requires at least one active enemy plus a successful reachability
probe before showing the combat prompt.

If the player accepts, combat begins. If the player declines, the original
code rolls `RandomByte & 3`: one result out of four forces combat anyway.

| Response | Outcome | Probability |
| --- | --- | ---: |
| Accept | Combat | 100% |
| Decline | Escape | 75% (3 in 4) |
| Decline | “You did not evade them!” and combat | 25% (1 in 4) |

## Enemy infantry count

The generator visits all eight enemy-character records. Each record receives
an independent `RandomByte & 1` coin flip and is populated on the non-zero
result. Before terrain-placement rejection, the infantry count is therefore:

```text
InfantryCount = 8 independent coin flips = Binomial(8, 1/2)
```

| Infantry | Exact probability | Percentage |
| ---: | ---: | ---: |
| 0 | 1/256 | 0.390625% |
| 1 | 8/256 | 3.125% |
| 2 | 28/256 | 10.9375% |
| 3 | 56/256 | 21.875% |
| 4 | 70/256 | 27.34375% |
| 5 | 56/256 | 21.875% |
| 6 | 28/256 | 10.9375% |
| 7 | 8/256 | 3.125% |
| 8 | 1/256 | 0.390625% |

The expected count is 4. At least one infantry record is generated with
probability `255/256`, or 99.609375%.

## Enemy BattleMech count

There are four enemy-Mech records, paired with the four friendly lance slots.
An enemy opportunity exists only when the corresponding friendly Mech record
is occupied. Each opportunity then receives an independent 50% coin flip.

For `N` occupied friendly Mech slots:

```text
EnemyMechCount = Binomial(N, 1/2)
Expected count = N / 2
Chance of at least one = 1 - 1 / 2^N
```

For a full four-Mech lance:

| Enemy Mechs | Exact probability | Percentage |
| ---: | ---: | ---: |
| 0 | 1/16 | 6.25% |
| 1 | 4/16 | 25% |
| 2 | 6/16 | 37.5% |
| 3 | 4/16 | 25% |
| 4 | 1/16 | 6.25% |

At least one enemy Mech is generated against a full lance with probability
`15/16`, or 93.75%, and the expected count is 2.

Before terrain-placement rejection, a generated patrol has no infantry and no
Mechs with probability `1 / 2^(8+N)`. With four friendly Mechs that is
`1/4096`, or 0.024414%; at least one opponent is generated 99.975586% of the
time.

### Chassis selection

Each successful enemy-Mech slot copies one complete stock template selected by
`RandomByte % 3`. This is close to, but not exactly, a fair `d3`: 256 is not
divisible by three, so remainder zero receives one extra byte value.

| Remainder | Chassis | Exact chance | Percentage | Stock armament and ammunition |
| ---: | --- | ---: | ---: | --- |
| 0 | Locust LCT-1V | 86/256 | 33.59375% | 2 Machine Guns (100 rounds each), 1 Medium Laser |
| 1 | Wasp WSP-1A | 85/256 | 33.203125% | 1 Medium Laser, 1 SRM-2 (50 rounds) |
| 2 | Stinger STG-3R | 85/256 | 33.203125% | 2 Machine Guns (100 rounds each), 1 Medium Laser |

The generator copies the complete `0x7D`-byte template, so armour, internal
structure, movement, critical slots, heat sinks and ammunition all begin at
their stock executable values. It does not construct an ad-hoc loadout.

For a full friendly lance, the chance that an eligible world tick both passes
the patrol mask and generates at least one enemy Mech, before placement and
reachability checks, is:

| Campaign mask | Calculation | Chance per eligible world tick |
| --- | ---: | ---: |
| `0x1F` | `1/32 x 15/16` | 15/512 = 2.9296875% |
| `0x7F` | `1/128 x 15/16` | 15/2048 = 0.732421875% |

## Generated infantry statistics

Every populated infantry record is generated independently.

| Field | Original roll | Range | Probability |
| --- | --- | ---: | --- |
| Body | `2d6` | 2..12 | Triangular distribution below |
| Health | `Body x 10` | 20..120 | Inherits Body's distribution |
| Dexterity | `2d6` | 2..12 | Triangular distribution below |
| Each of 7 skills | `RandomByte & 3` | 0..3 | Each value 25% (`1d4-1` equivalent) |
| Armour type | `RandomByte & 3` | 0..3 | Each type 25% |
| Armour value | `2d6 + 2d6` | 4..24 | Exact `4d6` distribution |

The four generated armour types are None, Flak Vest, Flak Suit and Light
Environmental Suit. The rolled armour value is independent of the selected
type; this is the original behavior, even when the roll differs from the
shop's nominal durability for that armour.

The original `d6` helper is exact: it masks a random byte with seven, rejects
six and seven, and maps zero through five to die faces one through six. Thus
the conventional `2d6` distribution is:

| Result | Ways out of 36 | Probability |
| ---: | ---: | ---: |
| 2 or 12 | 1 | 2.777778% each |
| 3 or 11 | 2 | 5.555556% each |
| 4 or 10 | 3 | 8.333333% each |
| 5 or 9 | 4 | 11.111111% each |
| 6 or 8 | 5 | 13.888889% each |
| 7 | 6 | 16.666667% |

The armour-value `4d6` roll has this distribution:

| Value | Ways/1296 | Probability | Value | Ways/1296 | Probability |
| ---: | ---: | ---: | ---: | ---: | ---: |
| 4 | 1 | 0.077160% | 15 | 140 | 10.802469% |
| 5 | 4 | 0.308642% | 16 | 125 | 9.645062% |
| 6 | 10 | 0.771605% | 17 | 104 | 8.024691% |
| 7 | 20 | 1.543210% | 18 | 80 | 6.172840% |
| 8 | 35 | 2.700617% | 19 | 56 | 4.320988% |
| 9 | 56 | 4.320988% | 20 | 35 | 2.700617% |
| 10 | 80 | 6.172840% | 21 | 20 | 1.543210% |
| 11 | 104 | 8.024691% | 22 | 10 | 0.771605% |
| 12 | 125 | 9.645062% | 23 | 4 | 0.308642% |
| 13 | 140 | 10.802469% | 24 | 1 | 0.077160% |
| 14 | 146 | 11.265432% |  |  |  |

## Infantry weapon loadouts

The weapon is not chosen uniformly. The generator sums seven independent
`RandomByte & 3` rolls. In dice notation this is `7d4-7`, producing 0..21,
then looks up that sum in an executable-owned table.

There are `4^7 = 16,384` equally likely roll combinations. Aggregating lookup
entries that name the same weapon gives the actual loadout probabilities:

| Weapon | Triggering sum(s) | Exact chance | Percentage |
| --- | --- | ---: | ---: |
| Cudgel | 4, 16, 17 | 819/16384 | 4.998779% |
| Knife | 6 | 728/16384 | 4.443359% |
| Sword | 15 | 728/16384 | 4.443359% |
| Vibroblade | 7, 14 | 2256/16384 | 13.769531% |
| Shortbow | 13 | 1554/16384 | 9.484863% |
| Longbow | 8 | 1554/16384 | 9.484863% |
| Crossbow | 12 | 1918/16384 | 11.706543% |
| Pistol | 3, 9, 10 | 4130/16384 | 25.207520% |
| Rifle | 5, 11, 18 | 2625/16384 | 16.021729% |
| SMG | 2, 19 | 56/16384 | 0.341797% |
| SRM | 0 | 1/16384 | 0.006104% |
| Inferno | 21 | 1/16384 | 0.006104% |
| Laser pistol | 1 | 7/16384 | 0.042725% |
| Laser rifle | 20 | 7/16384 | 0.042725% |

The extreme SRM and Inferno outcomes require all seven two-bit rolls to be
zero or all seven to be three respectively. Their rarity is intentional in the
table rather than evidence that those weapon IDs are unreachable.

## Placement around the party

The generator chooses X and Y offset magnitudes independently as
`(RandomByte & 7) + 10`, uniformly selecting 10..17. A separate coin flip per
axis selects the sign. Placement therefore starts in one of four quadrants
around the party, 10..17 movement units away on each axis.

From that origin the routine scans for usable ground. Infantry require one
valid cell; Mechs require the candidate and its adjacent cell to be valid. A
generated record is discarded if its final calculated position lies outside
the cached map. The combat controller then checks that at least one active
enemy exists and that the first active enemy is reachable. Consequently the
tables above describe generation before terrain-dependent rejection; the
exact probability of a visible prompt also depends on the local map.

## Preservation evidence

- `0800:0000` (`Main_Game_Loop`): world-tick trigger and mask test.
- `0DAB:0D3D-1466` (`Generate_Random_Encounter_Enemies`): origin,
  infantry, Mech, loadout and placement generation.
- `183B:000A-1481` (`Combat_Run_Encounter`): active-enemy/reachability gate,
  prompt and 25% failed-escape roll.
- Executable table `3EDB:2CF4-2D09`: infantry weapon by seven-roll sum.
- Executable pointer table `3EDB:2DF8`: Locust, Wasp and Stinger templates.


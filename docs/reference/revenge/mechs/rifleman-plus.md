# Rifleman+

[Back to Revenge BattleMech record layout and index](../../REVENGE_UNITS.md)

## Overview

| Field | Value |
| --- | ---: |
| Unit type ID | `0x50` |
| Record size | `0x96` (150 bytes) |
| Tonnage | 60 |
| Walking movement | 4 |
| Jump movement | 0 |
| Damage model | BattleMech |
| Tactical sprite base | `0x40` |

## Armour

| Location | Minimum | Current | Maximum |
| --- | ---: | ---: | ---: |
| Head | 0 | 6 | 6 |
| Left Arm | 0 | 15 | 15 |
| Left Torso | 0 | 15 | 15 |
| Center Torso | 0 | 22 | 22 |
| Right Torso | 0 | 15 | 15 |
| Right Arm | 0 | 15 | 15 |
| Left Leg | 0 | 12 | 12 |
| Right Leg | 0 | 12 | 12 |
| Rear Left Torso | 0 | 8 | 8 |
| Rear Center Torso | 0 | 8 | 8 |
| Rear Right Torso | 0 | 8 | 8 |

## Internal structure

| Location | Minimum | Current | Maximum |
| --- | ---: | ---: | ---: |
| Head | 0 | 3 | 3 |
| Left Arm | 0 | 10 | 10 |
| Left Torso | 0 | 14 | 14 |
| Center Torso | 0 | 20 | 20 |
| Right Torso | 0 | 14 | 14 |
| Right Arm | 0 | 10 | 10 |
| Left Leg | 0 | 14 | 14 |
| Right Leg | 0 | 14 | 14 |

## Systems

| System | Current/template value | Limit / meaning |
| --- | ---: | --- |
| Engine hits | 0 | 3 destroys |
| Sensor hits | 0 | 2 destroys |
| Current heat | 0 | 30 maximum engine scale |
| Engine heat-sink capacity | 16 | stored capacity |
| Critical-slot heat sinks | 4 | equipment entries |
| Gunnery target number | 4 | lower is better |
| Target movement modifier | 0 | stored tactical modifier |

## Special equipment

| Equipment/state | Present |
| --- | --- |
| Improved missiles | False |
| Inferno rockets | False |
| Beagle active probe | False |
| Indirect-fire spotting | True |
| Accuracy upgrade | False |
| Double heat sinks | True |
| CASE | True |

## Weapons

| # | Weapon | Location | Equipment | State | Damage | Heat | Range min/max |
| ---: | --- | --- | ---: | --- | ---: | ---: | --- |
| 0 | Large Laser | Left Arm | `0x05` | Operational | 8 | 8 | 0/15 |
| 1 | AC/10 | Left Arm | `0x0C` | Operational | 10 | 3 | 0/15 |
| 2 | Medium Laser | Left Torso | `0x03` | Operational | 5 | 3 | 0/9 |
| 3 | Medium Laser | Right Torso | `0x03` | Operational | 5 | 3 | 0/9 |
| 4 | Large Laser | Right Arm | `0x05` | Operational | 8 | 8 | 0/15 |
| 5 | AC/10 | Right Arm | `0x0C` | Operational | 10 | 3 | 0/15 |

## Ammunition pools

| Family | Minimum | Current template | Maximum |
| --- | ---: | ---: | --- |
| AC/2 | 0 | 0 | Not stored separately |
| AC/5 | 0 | 0 | Not stored separately |
| AC/10 | 0 | 20 | Not stored separately |
| LB-10X A/C | 0 | 0 | Not stored separately |
| Gauss Rifle | 0 | 0 | Not stored separately |
| AC/20 | 0 | 0 | Not stored separately |
| Machine Gun | 0 | 0 | Not stored separately |
| LRM-5 | 0 | 0 | Not stored separately |
| LRM-10 | 0 | 0 | Not stored separately |
| LRM-15 | 0 | 0 | Not stored separately |
| LRM-20 | 0 | 0 | Not stored separately |
| SRM-2 | 0 | 0 | Not stored separately |
| SRM-4 | 0 | 0 | Not stored separately |
| SRM-6 | 0 | 0 | Not stored separately |

> Revenge stores the current ammunition pool but no separate maximum-ammo array. For a pristine `MECHTYPE.DAT` template, “current template” is the starting load; this page does not invent an unrecorded maximum.

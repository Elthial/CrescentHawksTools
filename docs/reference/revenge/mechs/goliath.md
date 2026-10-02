# Goliath

[Back to Revenge BattleMech record layout and index](../../REVENGE_UNITS.md)

## Overview

| Field | Value |
| --- | ---: |
| Unit type ID | `0x2F` |
| Record size | `0x96` (150 bytes) |
| Tonnage | 80 |
| Walking movement | 4 |
| Jump movement | 0 |
| Damage model | BattleMech |
| Tactical sprite base | `0xB0` |

## Armour

| Location | Minimum | Current | Maximum |
| --- | ---: | ---: | ---: |
| Head | 0 | 9 | 9 |
| Left Arm | 0 | 24 | 24 |
| Left Torso | 0 | 20 | 20 |
| Center Torso | 0 | 30 | 30 |
| Right Torso | 0 | 20 | 20 |
| Right Arm | 0 | 24 | 24 |
| Left Leg | 0 | 30 | 30 |
| Right Leg | 0 | 30 | 30 |
| Rear Left Torso | 0 | 13 | 13 |
| Rear Center Torso | 0 | 19 | 19 |
| Rear Right Torso | 0 | 13 | 13 |

## Internal structure

| Location | Minimum | Current | Maximum |
| --- | ---: | ---: | ---: |
| Head | 0 | 3 | 3 |
| Left Arm | 0 | 13 | 13 |
| Left Torso | 0 | 17 | 17 |
| Center Torso | 0 | 25 | 25 |
| Right Torso | 0 | 17 | 17 |
| Right Arm | 0 | 13 | 13 |
| Left Leg | 0 | 17 | 17 |
| Right Leg | 0 | 17 | 17 |

## Systems

| System | Current/template value | Limit / meaning |
| --- | ---: | --- |
| Engine hits | 0 | 3 destroys |
| Sensor hits | 0 | 2 destroys |
| Current heat | 0 | 30 maximum engine scale |
| Engine heat-sink capacity | 10 | stored capacity |
| Critical-slot heat sinks | 7 | equipment entries |
| Gunnery target number | 4 | lower is better |
| Target movement modifier | 0 | stored tactical modifier |

## Special equipment

| Equipment/state | Present |
| --- | --- |
| Improved missiles | False |
| Inferno rockets | False |
| Beagle active probe | False |
| Indirect-fire spotting | False |
| Accuracy upgrade | False |
| Double heat sinks | False |
| CASE | False |

## Weapons

| # | Weapon | Location | Equipment | State | Damage | Heat | Range min/max |
| ---: | --- | --- | ---: | --- | ---: | ---: | --- |
| 0 | LRM-10 | Left Torso | `0x12` | Operational | 7 | 4 | 6/21 |
| 1 | Mach. Gun | Left Torso | `0x10` | Operational | 2 | 0 | 0/3 |
| 2 | PPC | Right Torso | `0x08` | Operational | 10 | 10 | 3/18 |
| 3 | LRM-10 | Right Torso | `0x12` | Operational | 7 | 4 | 6/21 |
| 4 | Mach. Gun | Right Torso | `0x10` | Operational | 2 | 0 | 0/3 |

## Ammunition pools

| Family | Minimum | Current template | Maximum |
| --- | ---: | ---: | --- |
| AC/2 | 0 | 0 | Not stored separately |
| AC/5 | 0 | 0 | Not stored separately |
| AC/10 | 0 | 0 | Not stored separately |
| LB-10X A/C | 0 | 0 | Not stored separately |
| Gauss Rifle | 0 | 0 | Not stored separately |
| AC/20 | 0 | 0 | Not stored separately |
| Machine Gun | 0 | 200 | Not stored separately |
| LRM-5 | 0 | 0 | Not stored separately |
| LRM-10 | 0 | 24 | Not stored separately |
| LRM-15 | 0 | 0 | Not stored separately |
| LRM-20 | 0 | 0 | Not stored separately |
| SRM-2 | 0 | 0 | Not stored separately |
| SRM-4 | 0 | 0 | Not stored separately |
| SRM-6 | 0 | 0 | Not stored separately |

> Revenge stores the current ammunition pool but no separate maximum-ammo array. For a pristine `MECHTYPE.DAT` template, “current template” is the starting load; this page does not invent an unrecorded maximum.

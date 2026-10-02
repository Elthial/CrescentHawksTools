# Stalker

[Back to Revenge BattleMech record layout and index](../../REVENGE_UNITS.md)

## Overview

| Field | Value |
| --- | ---: |
| Unit type ID | `0x33` |
| Record size | `0x96` (150 bytes) |
| Tonnage | 85 |
| Walking movement | 3 |
| Jump movement | 0 |
| Damage model | BattleMech |
| Tactical sprite base | `0xA0` |

## Armour

| Location | Minimum | Current | Maximum |
| --- | ---: | ---: | ---: |
| Head | 0 | 9 | 9 |
| Left Arm | 0 | 23 | 23 |
| Left Torso | 0 | 25 | 25 |
| Center Torso | 0 | 36 | 36 |
| Right Torso | 0 | 25 | 25 |
| Right Arm | 0 | 23 | 23 |
| Left Leg | 0 | 25 | 25 |
| Right Leg | 0 | 25 | 25 |
| Rear Left Torso | 0 | 7 | 7 |
| Rear Center Torso | 0 | 11 | 11 |
| Rear Right Torso | 0 | 7 | 7 |

## Internal structure

| Location | Minimum | Current | Maximum |
| --- | ---: | ---: | ---: |
| Head | 0 | 3 | 3 |
| Left Arm | 0 | 14 | 14 |
| Left Torso | 0 | 18 | 18 |
| Center Torso | 0 | 27 | 27 |
| Right Torso | 0 | 18 | 18 |
| Right Arm | 0 | 14 | 14 |
| Left Leg | 0 | 18 | 18 |
| Right Leg | 0 | 18 | 18 |

## Systems

| System | Current/template value | Limit / meaning |
| --- | ---: | --- |
| Engine hits | 0 | 3 destroys |
| Sensor hits | 0 | 2 destroys |
| Current heat | 0 | 30 maximum engine scale |
| Engine heat-sink capacity | 10 | stored capacity |
| Critical-slot heat sinks | 10 | equipment entries |
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
| 0 | LRM-10 | Left Arm | `0x12` | Operational | 7 | 4 | 6/21 |
| 1 | Medium Laser | Left Arm | `0x03` | Operational | 5 | 3 | 0/9 |
| 2 | Medium Laser | Left Arm | `0x03` | Operational | 5 | 3 | 0/9 |
| 3 | Large Laser | Left Torso | `0x05` | Operational | 8 | 8 | 0/15 |
| 4 | SRM-6 | Left Torso | `0x17` | Operational | 9 | 4 | 0/9 |
| 5 | Large Laser | Right Torso | `0x05` | Operational | 8 | 8 | 0/15 |
| 6 | SRM-6 | Right Torso | `0x17` | Operational | 9 | 4 | 0/9 |
| 7 | LRM-10 | Right Arm | `0x12` | Operational | 7 | 4 | 6/21 |
| 8 | Medium Laser | Right Arm | `0x03` | Operational | 5 | 3 | 0/9 |
| 9 | Medium Laser | Right Arm | `0x03` | Operational | 5 | 3 | 0/9 |

## Ammunition pools

| Family | Minimum | Current template | Maximum |
| --- | ---: | ---: | --- |
| AC/2 | 0 | 0 | Not stored separately |
| AC/5 | 0 | 0 | Not stored separately |
| AC/10 | 0 | 0 | Not stored separately |
| LB-10X A/C | 0 | 0 | Not stored separately |
| Gauss Rifle | 0 | 0 | Not stored separately |
| AC/20 | 0 | 0 | Not stored separately |
| Machine Gun | 0 | 0 | Not stored separately |
| LRM-5 | 0 | 0 | Not stored separately |
| LRM-10 | 0 | 48 | Not stored separately |
| LRM-15 | 0 | 0 | Not stored separately |
| LRM-20 | 0 | 0 | Not stored separately |
| SRM-2 | 0 | 0 | Not stored separately |
| SRM-4 | 0 | 0 | Not stored separately |
| SRM-6 | 0 | 30 | Not stored separately |

> Revenge stores the current ammunition pool but no separate maximum-ammo array. For a pristine `MECHTYPE.DAT` template, “current template” is the starting load; this page does not invent an unrecorded maximum.

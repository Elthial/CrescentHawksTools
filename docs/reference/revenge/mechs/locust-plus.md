# Locust+

[Back to Revenge BattleMech record layout and index](../../REVENGE_UNITS.md)

## Overview

| Field | Value |
| --- | ---: |
| Unit type ID | `0x45` |
| Record size | `0x96` (150 bytes) |
| Tonnage | 20 |
| Walking movement | 8 |
| Jump movement | 0 |
| Damage model | BattleMech |
| Tactical sprite base | `0x10` |

## Armour

| Location | Minimum | Current | Maximum |
| --- | ---: | ---: | ---: |
| Head | 0 | 7 | 8 |
| Left Arm | 0 | 4 | 4 |
| Left Torso | 0 | 6 | 8 |
| Center Torso | 0 | 7 | 10 |
| Right Torso | 0 | 6 | 8 |
| Right Arm | 0 | 4 | 4 |
| Left Leg | 0 | 7 | 8 |
| Right Leg | 0 | 7 | 8 |
| Rear Left Torso | 0 | 2 | 2 |
| Rear Center Torso | 0 | 2 | 2 |
| Rear Right Torso | 0 | 2 | 2 |

## Internal structure

| Location | Minimum | Current | Maximum |
| --- | ---: | ---: | ---: |
| Head | 0 | 3 | 3 |
| Left Arm | 0 | 3 | 3 |
| Left Torso | 0 | 5 | 5 |
| Center Torso | 0 | 6 | 6 |
| Right Torso | 0 | 5 | 5 |
| Right Arm | 0 | 3 | 3 |
| Left Leg | 0 | 4 | 4 |
| Right Leg | 0 | 4 | 4 |

## Systems

| System | Current/template value | Limit / meaning |
| --- | ---: | --- |
| Engine hits | 0 | 3 destroys |
| Sensor hits | 0 | 2 destroys |
| Current heat | 0 | 30 maximum engine scale |
| Engine heat-sink capacity | 6 | stored capacity |
| Critical-slot heat sinks | 4 | equipment entries |
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
| 0 | Small Laser | Left Torso | `0x01` | Operational | 3 | 1 | 0/3 |
| 1 | Small Laser | Left Torso | `0x01` | Operational | 3 | 1 | 0/3 |
| 2 | Med Pulse | Center Torso | `0x04` | Operational | 7 | 4 | 0/12 |
| 3 | Small Laser | Center Torso | `0x01` | Operational | 3 | 1 | 0/3 |
| 4 | Small Laser | Right Torso | `0x01` | Operational | 3 | 1 | 0/3 |
| 5 | Small Laser | Right Torso | `0x01` | Operational | 3 | 1 | 0/3 |

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
| LRM-10 | 0 | 0 | Not stored separately |
| LRM-15 | 0 | 0 | Not stored separately |
| LRM-20 | 0 | 0 | Not stored separately |
| SRM-2 | 0 | 0 | Not stored separately |
| SRM-4 | 0 | 0 | Not stored separately |
| SRM-6 | 0 | 0 | Not stored separately |

> Revenge stores the current ammunition pool but no separate maximum-ammo array. For a pristine `MECHTYPE.DAT` template, “current template” is the starting load; this page does not invent an unrecorded maximum.

# Wasp - WSP-1A

[Back to Inception BattleMech record layout and index](../../INCEPTION_MECHS.md)

The stock game Wasp matches the [WSP-1A](https://www.sarna.net/wiki/Wasp): a jump-capable 20-ton scout with a Medium Laser and SRM-2. This page records the unmodified executable template.

## Mech-Lube variants

- [Wasp - WSP-1Y](Wasp%20-%20WSP-1Y.md)
- [Wasp - WSP-1Y2](Wasp%20-%20WSP-1Y2.md)

## Overview

| Field | Value |
| --- | ---: |
| Template ID | `1` |
| Record size | `0x7D` (125 bytes) |
| Tonnage | 20 |
| Walking movement | 6 |
| Jump movement | 6 |
| Pilot ID | 0 |
| Rider ID | 255 |
| Upgrade-package base | 1 |
| Upgrade flags | `0x00` |

## Armour

| Location | Minimum | Current | Maximum |
| --- | ---: | ---: | ---: |
| Left Arm | 0 | 4 | 4 |
| Left Torso | 0 | 6 | 6 |
| Left Leg | 0 | 5 | 5 |
| Head | 0 | 4 | 4 |
| Center Torso | 0 | 6 | 6 |
| Right Arm | 0 | 4 | 4 |
| Right Torso | 0 | 6 | 6 |
| Right Leg | 0 | 5 | 5 |
| Rear Left Torso | 0 | 2 | 2 |
| Rear Center Torso | 0 | 4 | 4 |
| Rear Right Torso | 0 | 2 | 2 |

## Internal structure

| Location | Minimum | Current | Maximum |
| --- | ---: | ---: | ---: |
| Left Arm | 0 | 3 | 3 |
| Left Torso | 0 | 5 | 5 |
| Left Leg | 0 | 4 | 4 |
| Head | 0 | 3 | 3 |
| Center Torso | 0 | 6 | 6 |
| Right Arm | 0 | 3 | 3 |
| Right Torso | 0 | 5 | 5 |
| Right Leg | 0 | 4 | 4 |

## Actuators and systems

| System | Current | Maximum / destroyed at |
| --- | ---: | ---: |
| Left leg actuator nibble | 15 | 15 |
| Right leg actuator nibble | 15 | 15 |
| Left arm actuator nibble | 15 | 15 |
| Right arm actuator nibble | 15 | 15 |
| Engine hits | 0 | 3 destroys |
| Gyro hits | 0 | 2 destroys |
| Sensor hits | 0 | 2 destroys |
| Built-in heat sinks | 4 | — |
| Critical-slot heat sinks | 6 | — |
| Life-support state (raw) | 1 | — |

## Weapons

| # | Weapon | Location | Component | State | Damage | Heat | Range S/M/L |
| ---: | --- | --- | ---: | --- | ---: | ---: | --- |
| 0 | Medium Laser | Right Arm | `0x11` | Operational | 5 | 3 | 12/21/30 |
| 1 | SRM-2 | Left Leg | `0x1E` | Operational | 2 | 2 | 12/21/30 |

## Ammunition state

| Weapon ordinal | Weapon | Minimum | Current | Maximum |
| ---: | --- | ---: | ---: | ---: |
| 0 | Medium Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |
| 1 | SRM-2 | 0 | 50 | 50 |
| 2 | Unassigned | 0 | 0 | 0 |
| 3 | Unassigned | 0 | 0 | 0 |
| 4 | Unassigned | 0 | 0 | 0 |
| 5 | Unassigned | 0 | 0 | 0 |
| 6 | Unassigned | 0 | 0 | 0 |
| 7 | Unassigned | 0 | 0 | 0 |
| 8 | Unassigned | 0 | 0 | 0 |
| 9 | Unassigned | 0 | 0 | 0 |

> Unassigned ordinals are retained because all ten bytes are part of the original record. A non-zero value in an unassigned ordinal is preserved data, not automatically usable ammunition.

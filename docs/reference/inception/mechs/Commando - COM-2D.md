# Commando - COM-2D

[Back to Inception BattleMech record layout and index](../../INCEPTION_MECHS.md)

The stock game Commando is the [COM-2D](https://www.sarna.net/wiki/Commando): a 25-ton missile striker armed with a Medium Laser, SRM-4 and SRM-6. This page records the unmodified executable template.

## Mech-Lube variants

- [Commando - COM-7Y](Commando%20-%20COM-7Y.md)
- [Commando - COM-7Y2](Commando%20-%20COM-7Y2.md)

## Overview

| Field | Value |
| --- | ---: |
| Template ID | `3` |
| Record size | `0x7D` (125 bytes) |
| Tonnage | 25 |
| Walking movement | 6 |
| Jump movement | 0 |
| Pilot ID | 0 |
| Rider ID | 255 |
| Upgrade-package base | 3 |
| Upgrade flags | `0x00` |

## Armour

| Location | Minimum | Current | Maximum |
| --- | ---: | ---: | ---: |
| Left Arm | 0 | 6 | 6 |
| Left Torso | 0 | 6 | 6 |
| Left Leg | 0 | 8 | 8 |
| Head | 0 | 6 | 6 |
| Center Torso | 0 | 8 | 8 |
| Right Arm | 0 | 6 | 6 |
| Right Torso | 0 | 6 | 6 |
| Right Leg | 0 | 8 | 8 |
| Rear Left Torso | 0 | 3 | 3 |
| Rear Center Torso | 0 | 4 | 4 |
| Rear Right Torso | 0 | 3 | 3 |

## Internal structure

| Location | Minimum | Current | Maximum |
| --- | ---: | ---: | ---: |
| Left Arm | 0 | 4 | 4 |
| Left Torso | 0 | 6 | 6 |
| Left Leg | 0 | 6 | 6 |
| Head | 0 | 3 | 3 |
| Center Torso | 0 | 8 | 8 |
| Right Arm | 0 | 4 | 4 |
| Right Torso | 0 | 6 | 6 |
| Right Leg | 0 | 6 | 6 |

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
| Built-in heat sinks | 6 | — |
| Critical-slot heat sinks | 4 | — |
| Life-support state (raw) | 1 | — |

## Weapons

| # | Weapon | Location | Component | State | Damage | Heat | Range S/M/L |
| ---: | --- | --- | ---: | --- | ---: | ---: | --- |
| 0 | Medium Laser | Left Arm | `0x11` | Operational | 5 | 3 | 12/21/30 |
| 1 | SRM-4 | Right Arm | `0x1F` | Operational | 2 | 3 | 12/21/30 |
| 2 | SRM-6 | Center Torso | `0x20` | Operational | 2 | 4 | 12/21/30 |

## Ammunition state

| Weapon ordinal | Weapon | Minimum | Current | Maximum |
| ---: | --- | ---: | ---: | ---: |
| 0 | Medium Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |
| 1 | SRM-4 | 0 | 25 | 25 |
| 2 | SRM-6 | 0 | 15 | 15 |
| 3 | Unassigned | 0 | 0 | 0 |
| 4 | Unassigned | 0 | 0 | 0 |
| 5 | Unassigned | 0 | 0 | 0 |
| 6 | Unassigned | 0 | 0 | 0 |
| 7 | Unassigned | 0 | 0 | 0 |
| 8 | Unassigned | 0 | 0 | 0 |
| 9 | Unassigned | 0 | 0 | 0 |

> Unassigned ordinals are retained because all ten bytes are part of the original record. A non-zero value in an unassigned ordinal is preserved data, not automatically usable ammunition.

# Commando - COM-7Y

[Back to Inception BattleMech record layout and index](../../INCEPTION_MECHS.md)

Mech-Lube converts the missile-heavy COM-2D into the better-armoured COM-7Y laser/SRM refit. The designation is cross-checked against [Sarna](https://www.sarna.net/wiki/Commando); the record values below come from the game executable.

## Upgrade

| From | To | Price |
| --- | --- | ---: |
| COM-2D | COM-7Y | 13,800 C-bills |

This is the first package in the chassis upgrade sequence.

### Changes from COM-2D

- Replaces the right-arm SRM-4 with a Medium Laser.
- Adds a Medium Laser in the right torso.
- Moves the retained SRM-6 ammunition from ordinal 2 to ordinal 3.
- Changes ammunition ordinals 1 and 2 to the unlimited energy-weapon value.
- Replaces current and maximum armour with the heavier COM-7Y profile.
- Final armament: three Medium Lasers and one SRM-6.

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
| Upgrade flags | `0x01` |

## Armour

| Location | Minimum | Current | Maximum |
| --- | ---: | ---: | ---: |
| Left Arm | 0 | 8 | 8 |
| Left Torso | 0 | 9 | 9 |
| Left Leg | 0 | 12 | 12 |
| Head | 0 | 9 | 9 |
| Center Torso | 0 | 12 | 12 |
| Right Arm | 0 | 8 | 8 |
| Right Torso | 0 | 9 | 9 |
| Right Leg | 0 | 12 | 12 |
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
| 1 | Medium Laser | Right Arm | `0x11` | Operational | 5 | 3 | 12/21/30 |
| 2 | Medium Laser | Right Torso | `0x11` | Operational | 5 | 3 | 12/21/30 |
| 3 | SRM-6 | Center Torso | `0x20` | Operational | 2 | 4 | 12/21/30 |

## Ammunition state

| Weapon ordinal | Weapon | Minimum | Current | Maximum |
| ---: | --- | ---: | ---: | ---: |
| 0 | Medium Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |
| 1 | Medium Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |
| 2 | Medium Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |
| 3 | SRM-6 | 0 | 15 | 15 |
| 4 | Unassigned | 0 | 0 | 0 |
| 5 | Unassigned | 0 | 0 | 0 |
| 6 | Unassigned | 0 | 0 | 0 |
| 7 | Unassigned | 0 | 0 | 0 |
| 8 | Unassigned | 0 | 0 | 0 |
| 9 | Unassigned | 0 | 0 | 0 |

> Unassigned ordinals are retained because all ten bytes are part of the original record. A non-zero value in an unassigned ordinal is preserved data, not automatically usable ammunition.

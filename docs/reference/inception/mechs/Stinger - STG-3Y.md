# Stinger - STG-3Y

[Back to Inception BattleMech record layout and index](../../INCEPTION_MECHS.md)

Mech-Lube converts the machine-gun STG-3R into the close-range STG-3Y. The designation is cross-checked against [Sarna](https://www.sarna.net/wiki/Stinger); the record values below come from the game executable.

## Upgrade

| From | To | Price |
| --- | --- | ---: |
| STG-3R | STG-3Y | 12,200 C-bills |

This is the first package in the chassis upgrade sequence.

### Changes from STG-3R

- Replaces both Machine Guns and installs four Small Lasers across the arms.
- Changes ammunition ordinals 0 through 4 to the unlimited energy-weapon value.
- Retains the original Medium Laser, armour, movement and jump jets.
- Final armament: one Medium Laser and four Small Lasers.

## Overview

| Field | Value |
| --- | ---: |
| Template ID | `2` |
| Record size | `0x7D` (125 bytes) |
| Tonnage | 20 |
| Walking movement | 6 |
| Jump movement | 6 |
| Pilot ID | 0 |
| Rider ID | 255 |
| Upgrade-package base | 2 |
| Upgrade flags | `0x01` |

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
| 0 | Small Laser | Left Arm | `0x10` | Operational | 3 | 1 | 9/18/27 |
| 1 | Small Laser | Left Arm | `0x10` | Operational | 3 | 1 | 9/18/27 |
| 2 | Medium Laser | Right Arm | `0x11` | Operational | 5 | 3 | 12/21/30 |
| 3 | Small Laser | Right Arm | `0x10` | Operational | 3 | 1 | 9/18/27 |
| 4 | Small Laser | Right Arm | `0x10` | Operational | 3 | 1 | 9/18/27 |

## Ammunition state

| Weapon ordinal | Weapon | Minimum | Current | Maximum |
| ---: | --- | ---: | ---: | ---: |
| 0 | Small Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |
| 1 | Small Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |
| 2 | Medium Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |
| 3 | Small Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |
| 4 | Small Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |
| 5 | Unassigned | 0 | 0 | 0 |
| 6 | Unassigned | 0 | 0 | 0 |
| 7 | Unassigned | 0 | 0 | 0 |
| 8 | Unassigned | 0 | 0 | 0 |
| 9 | Unassigned | 0 | 0 | 0 |

> Unassigned ordinals are retained because all ten bytes are part of the original record. A non-zero value in an unassigned ordinal is preserved data, not automatically usable ammunition.

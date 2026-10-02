# Commando - COM-7Y2

[Back to Inception BattleMech record layout and index](../../INCEPTION_MECHS.md)

The second Mech-Lube package creates the COM-7Y2 “Blazing Inferno,” an all-energy close-range configuration. The designation is cross-checked against [Sarna](https://www.sarna.net/wiki/Commando); the record values below come from the game executable.

## Upgrade

| From | To | Price |
| --- | --- | ---: |
| COM-7Y | COM-7Y2 | 17,600 C-bills |

This is the second package; the first package must already be installed and is paid for separately.

### Changes from COM-7Y

- Replaces the remaining SRM-6 with six Small Lasers across both legs and the center torso.
- Adds a fourth Medium Laser in the head.
- Changes all ten ammunition ordinals to the unlimited energy-weapon value.
- Retains the COM-7Y armour profile.
- Final armament: four Medium Lasers and six Small Lasers.

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
| Upgrade flags | `0x03` |

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
| 3 | Small Laser | Left Leg | `0x10` | Operational | 3 | 1 | 9/18/27 |
| 4 | Small Laser | Left Leg | `0x10` | Operational | 3 | 1 | 9/18/27 |
| 5 | Small Laser | Right Leg | `0x10` | Operational | 3 | 1 | 9/18/27 |
| 6 | Small Laser | Right Leg | `0x10` | Operational | 3 | 1 | 9/18/27 |
| 7 | Small Laser | Center Torso | `0x10` | Operational | 3 | 1 | 9/18/27 |
| 8 | Small Laser | Center Torso | `0x10` | Operational | 3 | 1 | 9/18/27 |
| 9 | Medium Laser | Head | `0x11` | Operational | 5 | 3 | 12/21/30 |

## Ammunition state

| Weapon ordinal | Weapon | Minimum | Current | Maximum |
| ---: | --- | ---: | ---: | ---: |
| 0 | Medium Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |
| 1 | Medium Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |
| 2 | Medium Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |
| 3 | Small Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |
| 4 | Small Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |
| 5 | Small Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |
| 6 | Small Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |
| 7 | Small Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |
| 8 | Small Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |
| 9 | Medium Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |

> Unassigned ordinals are retained because all ten bytes are part of the original record. A non-zero value in an unassigned ordinal is preserved data, not automatically usable ammunition.

# Wasp - WSP-1Y

[Back to Inception BattleMech record layout and index](../../INCEPTION_MECHS.md)

Mech-Lube converts the missile-armed WSP-1A into the all-energy WSP-1Y and adds armour. The designation is cross-checked against [Sarna](https://www.sarna.net/wiki/Wasp); the record values below come from the game executable.

## Upgrade

| From | To | Price |
| --- | --- | ---: |
| WSP-1A | WSP-1Y | 10,400 C-bills |

This is the first package in the chassis upgrade sequence.

### Changes from WSP-1A

- Replaces the left-leg SRM-2 with a Medium Laser.
- Changes ammunition ordinal 1 to the unlimited energy-weapon value.
- Replaces current and maximum armour with the LCT-1V profile.
- Retains 6/9 ground movement and six jump points.
- Final armament: two Medium Lasers.

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
| Upgrade flags | `0x01` |

## Armour

| Location | Minimum | Current | Maximum |
| --- | ---: | ---: | ---: |
| Left Arm | 0 | 4 | 4 |
| Left Torso | 0 | 8 | 8 |
| Left Leg | 0 | 8 | 8 |
| Head | 0 | 8 | 8 |
| Center Torso | 0 | 10 | 10 |
| Right Arm | 0 | 4 | 4 |
| Right Torso | 0 | 8 | 8 |
| Right Leg | 0 | 8 | 8 |
| Rear Left Torso | 0 | 2 | 2 |
| Rear Center Torso | 0 | 2 | 2 |
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
| 1 | Medium Laser | Left Leg | `0x11` | Operational | 5 | 3 | 12/21/30 |

## Ammunition state

| Weapon ordinal | Weapon | Minimum | Current | Maximum |
| ---: | --- | ---: | ---: | ---: |
| 0 | Medium Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |
| 1 | Medium Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |
| 2 | Unassigned | 0 | 0 | 0 |
| 3 | Unassigned | 0 | 0 | 0 |
| 4 | Unassigned | 0 | 0 | 0 |
| 5 | Unassigned | 0 | 0 | 0 |
| 6 | Unassigned | 0 | 0 | 0 |
| 7 | Unassigned | 0 | 0 | 0 |
| 8 | Unassigned | 0 | 0 | 0 |
| 9 | Unassigned | 0 | 0 | 0 |

> Unassigned ordinals are retained because all ten bytes are part of the original record. A non-zero value in an unassigned ordinal is preserved data, not automatically usable ammunition.

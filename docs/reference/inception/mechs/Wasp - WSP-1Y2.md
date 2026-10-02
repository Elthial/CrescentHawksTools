# Wasp - WSP-1Y2

[Back to Inception BattleMech record layout and index](../../INCEPTION_MECHS.md)

The second Mech-Lube package converts the WSP-1Y into the jump-jet-free, five-laser WSP-1Y2. The designation is cross-checked against [Sarna](https://www.sarna.net/wiki/Wasp); the record values below come from the game executable.

## Upgrade

| From | To | Price |
| --- | --- | ---: |
| WSP-1Y | WSP-1Y2 | 14,000 C-bills |

This is the second package; the first package must already be installed and is paid for separately.

### Changes from WSP-1Y

- Removes all six jump jets, reducing jump movement from 6 to 0.
- Adds Medium Lasers in the left arm, center torso and head.
- Changes ammunition ordinals 2 through 4 to the unlimited energy-weapon value.
- Retains the WSP-1Y armour increase.
- Final armament: five Medium Lasers.

## Overview

| Field | Value |
| --- | ---: |
| Template ID | `1` |
| Record size | `0x7D` (125 bytes) |
| Tonnage | 20 |
| Walking movement | 6 |
| Jump movement | 0 |
| Pilot ID | 0 |
| Rider ID | 255 |
| Upgrade-package base | 1 |
| Upgrade flags | `0x03` |

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
| 0 | Medium Laser | Left Arm | `0x11` | Operational | 5 | 3 | 12/21/30 |
| 1 | Medium Laser | Right Arm | `0x11` | Operational | 5 | 3 | 12/21/30 |
| 2 | Medium Laser | Left Leg | `0x11` | Operational | 5 | 3 | 12/21/30 |
| 3 | Medium Laser | Center Torso | `0x11` | Operational | 5 | 3 | 12/21/30 |
| 4 | Medium Laser | Head | `0x11` | Operational | 5 | 3 | 12/21/30 |

## Ammunition state

| Weapon ordinal | Weapon | Minimum | Current | Maximum |
| ---: | --- | ---: | ---: | ---: |
| 0 | Medium Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |
| 1 | Medium Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |
| 2 | Medium Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |
| 3 | Medium Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |
| 4 | Medium Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |
| 5 | Unassigned | 0 | 0 | 0 |
| 6 | Unassigned | 0 | 0 | 0 |
| 7 | Unassigned | 0 | 0 | 0 |
| 8 | Unassigned | 0 | 0 | 0 |
| 9 | Unassigned | 0 | 0 | 0 |

> Unassigned ordinals are retained because all ten bytes are part of the original record. A non-zero value in an unassigned ordinal is preserved data, not automatically usable ammunition.

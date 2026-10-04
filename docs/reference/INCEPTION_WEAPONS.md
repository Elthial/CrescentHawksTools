# Inception weapon and range reference

For presentation rather than damage statistics, see
[combat graphics and sound dispatch](INCEPTION_COMBAT_EFFECTS.md): mech laser/PPC
beam colours, shared AC audio, SRM/LRM flight frames, and flamer behaviour.

The executable table contains 33 records of `0x11` bytes. Each record stores an
eleven-byte name followed by damage, attack/cluster selector, heat/effect,
packed range thresholds, maximum range and skill ID. The table below reports
the decoder's typed values and retains IDs so results can be compared with a
hex dump or the reconstructed C source.

Personnel damage uses a packed dice/bonus byte. Mech range thresholds are
scaled by three by the game; the values shown are effective thresholds.
`Inferno`'s `0xFF` damage is a special effect sentinel. Missile selector values
are cluster-table columns rather than a literal number of projectiles hitting.

| Table | Component | Use | Name | Damage | Selector | Heat | Short | Medium | Max | Skill |
| ---: | ---: | --- | --- | --- | ---: | ---: | ---: | ---: | ---: | --- |
| 0 | — | Infantry | Cudgel | 1d6+1 | 1 | 0 | 1 | 1 | 2 | Bows and blades |
| 1 | — | Infantry | Knife | 1d6 | 1 | 0 | 1 | 1 | 2 | Bows and blades |
| 2 | — | Infantry | Sword | 2d6+2 | 1 | 0 | 1 | 1 | 2 | Bows and blades |
| 3 | — | Infantry | VibroBlade | 3d6 | 1 | 0 | 1 | 1 | 2 | Bows and blades |
| 4 | — | Infantry | Shortbow | 1d6+1 | 1 | 0 | 3 | 6 | 9 | Bows and blades |
| 5 | — | Infantry | Longbow | 1d6+3 | 1 | 0 | 4 | 7 | 13 | Bows and blades |
| 6 | — | Infantry | Crossbow | 2d6+3 | 1 | 0 | 4 | 8 | 14 | Bows and blades |
| 7 | — | Infantry | Pistol | 2d6+3 | 1 | 0 | 3 | 5 | 9 | Pistol |
| 8 | — | Infantry | Rifle | 3d6 | 1 | 0 | 7 | 16 | 31 | Rifle |
| 9 | — | Infantry | MachineGun | 3d6 | 4 | 0 | 4 | 8 | 11 | Rifle |
| 10 | — | Infantry | SR Missile | 2 | 1 | 0 | 21 | 93 | 40 | Gunnery |
| 11 | — | Infantry | Inferno | special `0xFF` | 1 | 0 | 21 | 93 | 40 | Gunnery |
| 12 | — | Infantry | LaserPistl | 4d6 | 1 | 0 | 4 | 7 | 13 | Pistol |
| 13 | — | Infantry | LaserRifle | 4d6+2 | 1 | 0 | 7 | 22 | 43 | Rifle |
| 14 | — | Infantry | Flamer | 2d6 | 1 | 0 | 3 | 5 | 7 | Pistol |
| 15 | `0x10` | Mech | SmallLaser | 3 | 1 | 1 | 6 | 9 | 12 | Gunnery |
| 16 | `0x11` | Mech | Med Laser | 5 | 1 | 3 | 12 | 21 | 30 | Gunnery |
| 17 | `0x12` | Mech | LargeLaser | 8 | 1 | 8 | 18 | 33 | 48 | Gunnery |
| 18 | `0x13` | Mech | PPC | 10 | 1 | 10 | 21 | 39 | 57 | Gunnery |
| 19 | `0x14` | Mech | AutoCann/2 | 2 | 1 | 1 | 21 | 48 | 75 | Gunnery |
| 20 | `0x15` | Mech | AutoCann/5 | 5 | 1 | 1 | 21 | 39 | 57 | Gunnery |
| 21 | `0x16` | Mech | AutoCann10 | 10 | 1 | 3 | 18 | 33 | 48 | Gunnery |
| 22 | `0x17` | Mech | AutoCann20 | 20 | 1 | 7 | 12 | 21 | 30 | Gunnery |
| 23 | `0x18` | Mech | MachineGun | 2 | 1 | 0 | 6 | 9 | 12 | Gunnery |
| 24 | `0x19` | Mech | Flamer | 2 | 1 | 3 | 6 | 9 | 12 | Gunnery |
| 25 | `0x1A` | Mech | LRMissile5 | 1/missile | 4 | 2 | 21 | 45 | 66 | Gunnery |
| 26 | `0x1B` | Mech | LRMissil10 | 1/missile | 6 | 4 | 21 | 45 | 66 | Gunnery |
| 27 | `0x1C` | Mech | LRMissil15 | 1/missile | 7 | 5 | 21 | 45 | 66 | Gunnery |
| 28 | `0x1D` | Mech | LRMissil20 | 1/missile | 8 | 6 | 21 | 45 | 66 | Gunnery |
| 29 | `0x1E` | Mech | SRMissile2 | 2/missile | 2 | 2 | 12 | 21 | 30 | Gunnery |
| 30 | `0x1F` | Mech | SRMissile4 | 2/missile | 3 | 3 | 12 | 21 | 30 | Gunnery |
| 31 | `0x20` | Mech | SRMissile6 | 2/missile | 5 | 4 | 12 | 21 | 30 | Gunnery |
| 32 | `0x21` | Mech | Kick | tonnage ÷ 5 | 1 | 0 | 1 | 1 | 2 | Piloting |

`0x22` is Heat Sink in critical-slot data and is not a weapon-table record.
Run `dump-weapons --json` to retain the raw packed bytes alongside these values.

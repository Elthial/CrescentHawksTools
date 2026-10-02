# Revenge weapon and range reference

The executable contains 23 fixed `0x0F`-byte weapon definitions. Equipment IDs
are one-based. “Max units” is the stored value; the game displays range in
squares by multiplying it by three.

| ID | Weapon | Heat | Damage | Min | Max units | Max squares | Tons | Rack | Ammo/bin | Slots |
| ---: | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| `01` | Small Laser | 1 | 3 | 0 | 1 | 3 | 0.5 | 0 | — | 1 |
| `02` | Sm Pulse | 2 | 3 | 0 | 2 | 6 | 0.5 | 1 | — | 1 |
| `03` | Medium Laser | 3 | 5 | 0 | 3 | 9 | 1 | 0 | — | 1 |
| `04` | Med Pulse | 4 | 7 | 0 | 4 | 12 | 1 | 1 | — | 1 |
| `05` | Large Laser | 8 | 8 | 0 | 5 | 15 | 5 | 0 | — | 2 |
| `06` | Lg Pulse | 10 | 10 | 0 | 6 | 18 | 3 | 1 | — | 2 |
| `07` | ER Laser | 12 | 10 | 0 | 8 | 24 | 2 | 1 | — | 1 |
| `08` | PPC | 10 | 10 | 3 | 6 | 18 | 7 | 0 | — | 3 |
| `09` | ER PPC | 15 | 15 | 0 | 7 | 21 | 3 | 1 | — | 2 |
| `0A` | AC/2 | 1 | 2 | 4 | 8 | 24 | 6 | 0 | 45 | 1 |
| `0B` | AC/5 | 1 | 5 | 3 | 6 | 18 | 8 | 0 | 20 | 4 |
| `0C` | AC/10 | 3 | 10 | 0 | 5 | 15 | 12 | 0 | 10 | 7 |
| `0D` | LB-10X A/C | 2 | 10 | 0 | 6 | 18 | 5 | 1 | 8 | 5 |
| `0E` | Gauss Rifle | 1 | 15 | 2 | 7 | 21 | 6 | 1 | 8 | 6 |
| `0F` | AC/20 | 7 | 20 | 0 | 3 | 9 | 14 | 0 | 5 | 8 |
| `10` | Mach. Gun | 0 | 2 | 0 | 1 | 3 | 0.5 | 0 | 200 | 1 |
| `11` | LRM-5 | 2 | 3 | 6 | 7 | 21 | 2 | 5 | 24 | 1 |
| `12` | LRM-10 | 4 | 7 | 6 | 7 | 21 | 5 | 10 | 12 | 2 |
| `13` | LRM-15 | 5 | 11 | 6 | 7 | 21 | 7 | 15 | 8 | 3 |
| `14` | LRM-20 | 6 | 15 | 6 | 7 | 21 | 10 | 20 | 6 | 5 |
| `15` | SRM-2 | 2 | 3 | 0 | 3 | 9 | 1 | 2 | 50 | 1 |
| `16` | SRM-4 | 3 | 6 | 0 | 3 | 9 | 2 | 4 | 25 | 1 |
| `17` | SRM-6 | 4 | 9 | 0 | 3 | 9 | 3 | 6 | 15 | 2 |

Heat, damage, weight, rack size, ammo-family bridge, and critical-slot count
are verified. Maximum-range scaling is probable but retained in both stored and
displayed forms. `export-weapon-evidence` produces the complete definition,
unit-loadout, equipment-frequency, vehicle, and JSON evidence reports locally.

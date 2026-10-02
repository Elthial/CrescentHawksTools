# Inception CMP and ICN graphics

The original installation contains full-screen images, tile sources, and
sprite data in `.CMP` and `.ICN` containers. `InceptionTools` bounded-decodes
both observed compression formats and exports indexed PNGs.

| Filename | Current interpretation | Palette |
| --- | --- | --- |
| `BTTLTECH.ICN` | Tile set | EGA |
| `BTBORDER.CMP` | Tiny tile set | EGA |
| `ANIMATE.ICN` | Tile set | EGA |
| `STARLEAG.ICN` | Tile set | EGA |
| `DESTRUCT.ICN` | Tile set | EGA |
| `MAP.ICN` | Tile set | EGA |
| `TINYLAND.CMP` | Tiny tile set | EGA |
| `BTSTATS.CMP` | Full-screen image | EGA |
| `BTTITLE.CMP` | Full-screen image | title-specific mapping |
| `INFOCOM.CMP` | Full-screen image | publisher-screen mapping |
| `MECHSHAP.CMP` | Sprite source | EGA |
| `ENDMECH.CMP` | Full-screen image | ending-screen mapping |

Packed image bytes contain two four-bit pixel values. This matches the
original EGA write-mode-2 presentation path and its sixteen-colour output.
Palette substitutions for the title, publisher, and ending screens are kept in
the maintained decoder rather than presented here as a complete file-format
specification.

Use [`export-image`](../INCEPTION.md#export-image) for individual files or
[`export-images`](../INCEPTION.md#export-images) for the known set.

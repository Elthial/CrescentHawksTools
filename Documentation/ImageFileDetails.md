# Image File Details

This is a list of all `.ICN` and .`CMP` files in btech folder.

|Filename|Purpose|Palette|
|----|----|----|
|BTTLTECH.ICN|TileSet|EGA_Palette|
|BTBORDER.CMP|TinyTileSet|EGA_Palette|
|ANIMATE.ICN|TileSet|EGA_Palette|
|STARLEAG.ICN|TileSet|EGA_Palette|
|DESTRUCT.ICN|TileSet|EGA_Palette|
|MAP.ICN|TileSet|EGA_Palette|
|TINYLAND.CMP|TinyTileSet|EGA_Palette|
|BTSTATS.CMP|FullScreen|EGA_Palette|
|BTTITLE.CMP|FullScreen|`BTTITLE_Palette`|
|INFOCOM.CMP|FullScreen|`INFOCOM_Palette`|
|MECHSHAP.CMP|SpriteSheet|EGA_Palette|
|ENDMECH.CMP|FullScreen|`ENDMECH_Palette`|

Some of the images have no standard palettes where the colours are swapped:  
(The code speaks for itself)

`BTTITLE_Palette`
```
BTTITLE_Palette.SwapColour(1, new PaletteColour("Black / Background", 0x00, 0x00, 0x00));
```

`INFOCOM_Palette`
```
INFOCOM_Palette.SwapColour(5, new PaletteColour("Light Blue / Background", 0x55, 0x55, 0xFF));
INFOCOM_Palette.SwapColour(9, new PaletteColour("Dark Blue / Shadow", 0x00, 0x00, 0xAA));
```

`ENDMECH_Palette`
```
ENDMECH_Palette.SwapColour(1, new PaletteColour("Black / Background", 0x00, 0x00, 0x00));        
ENDMECH_Palette.SwapColour(9, new PaletteColour("Dark Blue / Jacket", 0x00, 0x00, 0xAA));
ENDMECH_Palette.SwapColour(13, new PaletteColour("Light Blue / Jacket / Reflection", 0x55, 0x55, 0xFF));    
```

As stated in the reverse engineering repo, each of these images is encoded in either format01 or format02 run-length encoding algorithms.

The file formats are as follows:

|FileSize|RLE Format|Image Data|
|----|----|---|
|`01 d3`|`02`|.. .. .. .. ..|

Each retrieved byte is two `4bp Pixel` divided between low and high values of the byte.  
(This is due to Write Mode 2 EGA encoding and only requiring 16 colours.)

This behaviour is replicated by the toolkit code.

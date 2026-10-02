using System.Text;
using System.Text.Json;
using InceptionTools.Graphics;
using InceptionTools.Installation;

namespace InceptionTools.Maps;

public sealed record WorldMapExportResult(
    string OutputPath,
    int OutputLength,
    string InitialPreset,
    uint? InitialEditorSeed,
    int FixedLocationCount);

/// <summary>Creates a portable, self-contained browser viewer for Pacifica's world map.</summary>
public static class WorldMapHtmlExporter
{
    private const int TinylandTileCount = 66;
    private const int TileSize = 8;

    public static WorldMapExportResult Export(GameInstallation installation, string outputPath,
        uint? editorSeed = null, bool overwrite = false)
    {
        ArgumentNullException.ThrowIfNull(installation);
        if (string.IsNullOrWhiteSpace(outputPath))
            throw new ArgumentException("A world-map HTML output path is required.", nameof(outputPath));
        if (!Path.GetExtension(outputPath).Equals(".html", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("World-map output must use an .html extension.", nameof(outputPath));
        string fullOutputPath = Path.GetFullPath(outputPath);
        if (!overwrite && File.Exists(fullOutputPath))
            throw new IOException("World-map output already exists: " + fullOutputPath +
                ". Pass --force to overwrite it.");

        byte[] tinylandPixels = ExtractTinylandTiles(installation);
        string html = BuildHtml(tinylandPixels, editorSeed);
        string? directory = Path.GetDirectoryName(fullOutputPath);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
        var encoding = new UTF8Encoding(false);
        using (var output = new FileStream(fullOutputPath,
            overwrite ? FileMode.Create : FileMode.CreateNew, FileAccess.Write, FileShare.None))
        using (var writer = new StreamWriter(output, encoding))
            writer.Write(html);
        return new WorldMapExportResult(fullOutputPath, encoding.GetByteCount(html),
            editorSeed.HasValue ? "custom editor seed" : "Pacifica (Chara III)", editorSeed,
            PacificaWorldPreset.FixedLocations.Count);
    }

    internal static string BuildHtml(byte[] tinylandPixels, uint? editorSeed = null)
    {
        ArgumentNullException.ThrowIfNull(tinylandPixels);
        if (tinylandPixels.Length != TinylandTileCount * TileSize * TileSize)
            throw new ArgumentException("TINYLAND tile pixels must contain 66 8x8 tiles.",
                nameof(tinylandPixels));
        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        string vertices = JsonSerializer.Serialize(Array.ConvertAll(
            PacificaWorldPreset.WorldVertices, value => (int)value));
        string tiles = JsonSerializer.Serialize(Array.ConvertAll(tinylandPixels, value => (int)value));
        string locations = JsonSerializer.Serialize(PacificaWorldPreset.FixedLocations, jsonOptions);
        string initialMode = editorSeed.HasValue ? "custom" : "pacifica";
        uint initialSeed = editorSeed ?? PacificaWorldPreset.OriginalRandomSeed;

        return $$$$"""
<!doctype html>
<html lang="en">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width,initial-scale=1">
<title>Pacifica (Chara III) world map</title>
<style>
:root { color-scheme: dark; font-family: system-ui, sans-serif; background:#11151a; color:#e9edf1; }
* { box-sizing:border-box; }
body { margin:0; display:grid; grid-template-rows:auto 1fr; height:100vh; overflow:hidden; }
header { display:flex; flex-wrap:wrap; align-items:center; gap:.7rem; padding:.65rem .8rem; background:#202730; border-bottom:1px solid #3b4654; }
header h1 { font-size:1rem; margin:0 1rem 0 0; }
label { display:flex; align-items:center; gap:.35rem; font-size:.85rem; }
select,input,button { color:#f5f7fa; background:#111820; border:1px solid #526071; border-radius:3px; padding:.3rem .45rem; }
button { cursor:pointer; }
button:hover { background:#273546; }
#layout { min-height:0; display:grid; grid-template-columns:minmax(0,1fr) 18rem; }
#viewport { overflow:auto; background:#05070a; position:relative; }
#map { display:block; image-rendering:pixelated; transform-origin:top left; }
aside { overflow:auto; border-left:1px solid #3b4654; background:#181e25; padding:.8rem; }
aside h2 { font-size:.95rem; margin:.2rem 0 .5rem; }
#status { min-height:4.5rem; padding:.55rem; background:#0f141a; border:1px solid #333f4c; font:12px/1.45 ui-monospace,monospace; }
#locations { list-style:none; margin:.5rem 0 1rem; padding:0; }
#locations button { width:100%; text-align:left; margin:.15rem 0; font-size:.8rem; }
.town { border-left:4px solid #ffe066; }
.special { border-left:4px solid #66e3ff; }
.note { color:#aeb9c6; font-size:.78rem; line-height:1.4; }
@media (max-width:800px) { #layout { grid-template-columns:1fr; } aside { display:none; } }
</style>
</head>
<body>
<header>
  <h1>Pacifica (Chara III) world map</h1>
  <label>Terrain preset
    <select id="preset"><option value="pacifica">Pacifica (original)</option><option value="custom">Custom editor seed</option></select>
  </label>
  <label>Detail/noise seed <input id="seed" value="{{{{initialSeed}}}}" inputmode="numeric" size="12" title="Decimal or 0x-prefixed 24-bit original RNG state"></label>
  <button id="apply">Generate</button>
  <label>Zoom <select id="zoom"><option value="0.5">50%</option><option value="1" selected>100%</option><option value="2">200%</option><option value="4">400%</option></select></label>
  <label><input type="checkbox" id="world" checked> World</label>
  <label><input type="checkbox" id="towns" checked> Towns</label>
  <label><input type="checkbox" id="specials" checked> Other fixed maps</label>
  <label><input type="checkbox" id="vertexOverlay"> World vertices</label>
</header>
<div id="layout">
  <main id="viewport"><canvas id="map" width="1024" height="1024"></canvas></main>
  <aside>
    <h2>Map position</h2><div id="status">Move over the map.</div>
    <h2>Fixed locations</h2><ul id="locations"></ul>
    <p class="note">Yellow squares are village maps; cyan squares are other fixed maps. Region coordinates start at the north-west corner.</p>
    <p class="note">The world-vertex overlay shows the fixed control lattice that defines Pacifica's large-scale geography. Each node is labelled with the terrain category produced by its value; raw table details remain available on hover. It is logically 17 by 17, but the original uses a 16-byte row stride, so the rightmost point of each row aliases the leftmost point of the next row.</p>
    <p class="note">The Pacifica preset starts the original three-byte RNG at executable state 0x020304, then reproduces the 256 calls made by Start_Game to fill the construction table. A custom seed changes that original RNG state; it is an editor control, not a map or save-game field.</p>
    <p class="note" id="invalid"></p>
  </aside>
</div>
<script>
"use strict";
const vertices={{{{vertices}}}};
const tinyland={{{{tiles}}}};
const locations={{{{locations}}}};
const canvas=document.getElementById("map"),ctx=canvas.getContext("2d",{alpha:false});
const viewport=document.getElementById("viewport"),status=document.getElementById("status");
const preset=document.getElementById("preset"),seedInput=document.getElementById("seed");
const palette=[[0,0,0],[0,0,170],[0,170,0],[0,170,170],[170,0,0],[170,0,170],[170,85,0],[170,170,170],[85,85,85],[85,85,255],[85,255,85],[85,255,255],[255,85,85],[255,85,255],[255,255,85],[255,255,255]];
let descriptors=new Uint8Array(128*128),invalidTiles=0;

function editorSeedTable(value){
  let low=value&255,middle=(value>>>8)&255,high=(value>>>16)&255,table=new Uint8Array(256);
  for(let i=0;i<256;i++){
    const carryFromLow=(low>>>1)&1,carryFromHigh=high>>>7,carryFromMiddle=middle>>>7;
    const newHigh=((high<<1)|carryFromLow)&255,newMiddle=((middle<<1)|carryFromHigh)&255;
    const subtraction=((low>>>2)-low-(1-carryFromMiddle))&255;
    const newLow=((low>>>1)|((subtraction&1)<<7))&255;
    low=newLow;middle=newMiddle;high=newHigh;table[i]=(newLow^newMiddle)&255;
  }
  return table;
}
function fillEdge(lattice,seeds,state,index,mean,amplitude){
  if(lattice[index]!==255)return {changed:false,noise:0,value:lattice[index]};
  amplitude&=255; const mask=((amplitude<<1)-1)&255;
  const noise=((seeds[state.i]&mask)-amplitude)&255;
  let value=(mean+noise)&255; lattice[index]=value>=128?0:value; state.i=(state.i+1)&255;
  return {changed:true,noise:noise,value:lattice[index]};
}
function horizontal(lattice,seeds,state,first,last){
  const stack=[[first,last]];
  while(stack.length){[first,last]=stack.pop();const span=last-first;if(span===1)continue;
    const half=span>>1,mid=first+half;stack.push([first,mid],[mid,last]);
    if(lattice[mid]===255){const mean=(lattice[first]+lattice[last])>>1,amp=(half<<1)&255,mask=((amp<<1)-1)&255;
      const noise=((seeds[state.i]&mask)-amp)&255;let value=(mean+noise)&255;lattice[mid]=value>=128?0:value;state.i=(state.i+1)&255;}}
}
function vertical(lattice,seeds,state,first,last){
  const stack=[[first,last]];
  while(stack.length){[first,last]=stack.pop();const span=last-first;if(span===9)continue;
    const half=span>>1,mid=first+half;stack.push([first,mid],[mid,last]);
    if(lattice[mid]===255){const mean=(lattice[first]+lattice[last])>>1;let amp=half>>3;if(amp===9)amp--;amp=(amp<<1)&255;
      const mask=((amp<<1)-1)&255,noise=((seeds[state.i]&mask)-amp)&255;let value=(mean+noise)&255;
      lattice[mid]=value>=128?0:value;state.i=(state.i+1)&255;}}
}
function verticalAmplitude(half){let a=half>>3;if(a===9)a--;return(a<<1)&255;}
function rectangles(lattice,seeds,state){
  const stack=[[0,8,72,80]];
  while(stack.length){const [tl,tr,bl,br]=stack.pop();if(tr-tl===1)continue;
    const centre=tl+((br-tl)>>1);if(lattice[centre]===255)lattice[centre]=(lattice[tl]+lattice[tr]+lattice[bl]+lattice[br])>>2;
    const hw=(tr-tl)>>1,hh=(bl-tl)>>1,top=tl+hw,left=tl+hh,bottom=bl+hw,right=tr+hh;
    let edgeMean=(lattice[tl]+lattice[tr])>>1;fillEdge(lattice,seeds,state,top,edgeMean,hw<<1);
    edgeMean=(lattice[tl]+lattice[bl])>>1;const le=fillEdge(lattice,seeds,state,left,edgeMean,verticalAmplitude(hh));
    if(le.changed)edgeMean=((le.noise<<8)|le.value)&65535;
    edgeMean=((((edgeMean&0xff00)|lattice[bl])+lattice[br])>>1)&65535;
    const be=fillEdge(lattice,seeds,state,bottom,edgeMean,hw<<1);if(be.changed)edgeMean=(edgeMean&0xff00)|be.value;
    edgeMean=((((edgeMean&0xff00)|lattice[tr])+lattice[br])>>1)&65535;
    fillEdge(lattice,seeds,state,right,edgeMean,verticalAmplitude(hw));
    stack.push([tl,top,left,centre],[top,tr,centre,right],[left,centre,bl,bottom],[centre,right,bottom,br]);
  }
}
function buildBlock(corners,seeds){
  const lattice=new Uint8Array(81);lattice.fill(255);lattice[0]=corners[0];lattice[8]=corners[1];lattice[72]=corners[2];lattice[80]=corners[3];
  const state={i:0},edges=[[0,8,1],[72,80,1],[0,72,0],[8,80,0]];
  for(const [first,last,h] of edges){state.i=(lattice[first]+lattice[last])&255;(h?horizontal:vertical)(lattice,seeds,state,first,last);}
  rectangles(lattice,seeds,state);const block=new Uint8Array(64);
  for(let y=0;y<8;y++)for(let x=0;x<8;x++)block[y*8+x]=lattice[y*9+x]&0xf0;return block;
}
function generateWorld(seeds){
  const world=new Uint8Array(128*128);
  for(let ry=0;ry<16;ry++)for(let rx=0;rx<16;rx++){
    const r=ry*16+rx,b=buildBlock([vertices[r],vertices[r+1],vertices[r+16],vertices[r+17]],seeds);
    for(let y=0;y<8;y++)world.set(b.subarray(y*8,y*8+8),(ry*8+y)*128+rx*8);
  }return world;
}
function tileIdAt(x,y){const i=y*128+x,t=descriptors[i];if(t===0x10)return 0x40;let f=0;
  if(y>0&&descriptors[i-128]===t)f|=1;if(x<127&&descriptors[i+1]===t)f|=2;
  if(y<127&&descriptors[i+128]===t)f|=4;if(x>0&&descriptors[i-1]===t)f|=8;
  return (t>=0x20?t-0x10:t)|f;
}
function draw(){
  const image=ctx.createImageData(1024,1024);invalidTiles=0;
  for(let by=0;by<128;by++)for(let bx=0;bx<128;bx++){
    const id=tileIdAt(bx,by),valid=id<66;if(!valid)invalidTiles++;
    for(let py=0;py<8;py++)for(let px=0;px<8;px++){
      const colour=valid?palette[tinyland[id*64+py*8+px]]:[255,0,255];
      const o=((by*8+py)*1024+bx*8+px)*4;image.data[o]=colour[0];image.data[o+1]=colour[1];image.data[o+2]=colour[2];image.data[o+3]=255;
    }
  }if(document.getElementById("world").checked)ctx.putImageData(image,0,0);else{ctx.fillStyle="#111820";ctx.fillRect(0,0,1024,1024);}drawLocations();drawVertices();
  document.getElementById("invalid").textContent=invalidTiles?`${invalidTiles} custom cells produced tile categories outside the original 66-tile overview set and are shown magenta.`:"";
}
function drawLocations(){
  ctx.save();ctx.font="11px system-ui";ctx.lineWidth=2;
  for(const loc of locations){if(loc.isTown&&!document.getElementById("towns").checked)continue;if(!loc.isTown&&!document.getElementById("specials").checked)continue;
    const x=loc.regionX*64,y=loc.regionY*64,w=loc.blockWidth*8,h=loc.blockHeight*8,colour=loc.isTown?"#ffe066":"#66e3ff";
    ctx.strokeStyle=colour;ctx.strokeRect(x+1,y+1,w-2,h-2);ctx.fillStyle="rgba(0,0,0,.75)";const label=`${loc.mapNumber}: ${loc.name}`;
    const tw=ctx.measureText(label).width+6,ly=Math.max(12,y+12);ctx.fillRect(x,ly-11,tw,14);ctx.fillStyle=colour;ctx.fillText(label,x+3,ly);
  }ctx.restore();
}
function drawVertices(){
  if(!document.getElementById("vertexOverlay").checked)return;
  ctx.save();ctx.lineWidth=1;ctx.strokeStyle="rgba(255,120,255,.55)";
  for(let i=0;i<=16;i++){const p=Math.min(1023,i*64)+.5;ctx.beginPath();ctx.moveTo(p,0);ctx.lineTo(p,1024);ctx.stroke();ctx.beginPath();ctx.moveTo(0,p);ctx.lineTo(1024,p);ctx.stroke();}
  ctx.font="9px ui-monospace,monospace";ctx.textBaseline="middle";
  for(let gy=0;gy<=16;gy++)for(let gx=0;gx<=16;gx++){
    const index=gy*16+gx,value=vertices[index],terrain=terrainType(value),rawX=gx*64,rawY=gy*64;
    const x=Math.max(5,Math.min(1018,rawX)),y=Math.max(5,Math.min(1018,rawY));
    ctx.fillStyle=terrain.colour;ctx.beginPath();ctx.arc(x,y,5,0,Math.PI*2);ctx.fill();ctx.strokeStyle="#ffffff";ctx.stroke();
    const label=terrain.name,width=ctx.measureText(label).width+4;
    const lx=gx===16?x-width-5:x+5,ly=gy===16?y-8:y+8;
    ctx.fillStyle="rgba(0,0,0,.82)";ctx.fillRect(lx-2,ly-5,width,10);ctx.fillStyle="#ff9cff";ctx.fillText(label,lx,ly);
  }
  ctx.restore();
}
function terrainType(value){
  const types=[
    {name:"Water",colour:"#4058dc"},{name:"Forest",colour:"#18952a"},
    {name:"Plains",colour:"#68e95d"},{name:"Hills",colour:"#34cfd0"},
    {name:"Mountain",colour:"#a94b22"},{name:"Peak",colour:"#202020"}
  ];
  return types[Math.min(5,(value&0xf0)>>>4)];
}
function parseSeed(){const text=seedInput.value.trim();const value=Number(text);if(!Number.isInteger(value)||value<0||value>0xffffff)throw new Error("Seed must be a decimal or 0x-prefixed 24-bit value (0 to 0xFFFFFF).");return value>>>0;}
function regenerate(){try{const seeds=editorSeedTable(preset.value==="pacifica"?0x020304:parseSeed());descriptors=generateWorld(seeds);seedInput.disabled=preset.value==="pacifica";draw();}catch(error){alert(error.message);}}
function focusLocation(loc){const zoom=Number(document.getElementById("zoom").value),x=(loc.regionX*64+loc.blockWidth*4)*zoom,y=(loc.regionY*64+loc.blockHeight*4)*zoom;viewport.scrollTo({left:Math.max(0,x-viewport.clientWidth/2),top:Math.max(0,y-viewport.clientHeight/2),behavior:"smooth"});}
const list=document.getElementById("locations");for(const loc of locations){const li=document.createElement("li"),button=document.createElement("button");button.className=loc.isTown?"town":"special";button.textContent=`MAP${loc.mapNumber} · ${loc.name} · region ${loc.regionX},${loc.regionY}`;button.onclick=()=>focusLocation(loc);li.appendChild(button);list.appendChild(li);}
canvas.addEventListener("mousemove",event=>{const rect=canvas.getBoundingClientRect(),px=(event.clientX-rect.left)*1024/rect.width,py=(event.clientY-rect.top)*1024/rect.height,bx=Math.floor(px/8),by=Math.floor(py/8),rx=Math.floor(bx/8),ry=Math.floor(by/8),vx=Math.max(0,Math.min(16,Math.round(px/64))),vy=Math.max(0,Math.min(16,Math.round(py/64))),vi=vy*16+vx,vt=terrainType(vertices[vi]);const hit=locations.find(l=>bx>=l.regionX*8&&bx<l.regionX*8+l.blockWidth&&by>=l.regionY*8&&by<l.regionY*8+l.blockHeight),alias=vx===16&&vy<16?` (aliases 0,${vy+1})`:vx===0&&vy>0?` (aliases 16,${vy-1})`:"";status.innerHTML=`region: ${rx}, ${ry}<br>overview block: ${bx}, ${by}<br>local block: ${bx%8}, ${by%8}<br>descriptor: 0x${descriptors[by*128+bx].toString(16).padStart(2,"0")}<br>nearest vertex: ${vx},${vy} · ${vt.name} · [${vi}]=0x${vertices[vi].toString(16).padStart(2,"0")}${alias}${hit?`<br>fixed map: MAP${hit.mapNumber} ${hit.name}`:""}`;});
document.getElementById("apply").onclick=regenerate;preset.onchange=regenerate;
document.getElementById("zoom").onchange=event=>{const z=Number(event.target.value);canvas.style.width=`${1024*z}px`;canvas.style.height=`${1024*z}px`;};
document.getElementById("world").onchange=draw;document.getElementById("towns").onchange=draw;document.getElementById("specials").onchange=draw;document.getElementById("vertexOverlay").onchange=draw;
preset.value="{{{{initialMode}}}}";seedInput.disabled=preset.value==="pacifica";regenerate();
</script>
</body>
</html>
""";
    }

    private static byte[] ExtractTinylandTiles(GameInstallation installation)
    {
        string sourcePath = installation.ResolveFile("TINYLAND.CMP");
        CompressedImage image = CompressedImageDecoder.Decode(File.ReadAllBytes(sourcePath), "TINYLAND.CMP");
        if (image.RemainingPayloadBytes != 0)
            throw new InvalidDataException("TINYLAND.CMP contains trailing compressed payload bytes.");
        byte[] source = image.PaletteIndices;
        var tiles = new byte[TinylandTileCount * TileSize * TileSize];
        for (int id = 0; id < TinylandTileCount; id++)
        {
            int sourceX = id % (CompressedImage.Width / TileSize) * TileSize;
            int sourceY = id / (CompressedImage.Width / TileSize) * TileSize;
            for (int row = 0; row < TileSize; row++)
                Array.Copy(source, (sourceY + row) * CompressedImage.Width + sourceX,
                    tiles, id * TileSize * TileSize + row * TileSize, TileSize);
        }
        return tiles;
    }
}

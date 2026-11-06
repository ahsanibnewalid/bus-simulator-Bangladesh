# Nusantara Heavy art bible — implementation reference
All final assets must be original, purchased with compatible rights, or otherwise licensed for game distribution. The generated test world is replaceable greybox art, not a final-quality visual deliverable.

## UI tokens
| Token | Value |
|---|---|
| Primary / pressed | #D32F2F / #B71C1C |
| Reward amber | #FFC107 |
| Surface / elevated | #1A1A1A at 85% / #2E2E2E |
| Text / secondary | #FFFFFF / #B0B0B0 |
| Comfort good / warning / danger | #4CAF50 / #FF9800 / #F44336 |
| Typefaces | Rajdhani headings; Inter body (supply licensed font files) |
| Corners | Cards 12 dp; buttons 24 dp |
| Touch targets | 48 dp minimum; steering 72 dp minimum |

Landscape reference: 1920×1080. Keep menu top left, currency top centre, passenger card top right, gauge above bottom-left steering buttons, pedals bottom right, horn/door above pedals. Put utilities and pressure display between thumb controls. Scale with safe area. A production portrait layout requires independent usability work.
Idle buttons use red/white; pressed controls scale to .95 and darken; disabled controls grey out. Reward buttons use amber. Do not rely on emoji fonts for essential production controls: replace text placeholders with vector icons.

## Environment palettes
| Role | Urban Jakarta | Rural Java | Toll highway |
|---|---|---|---|
| Sky | #B8C5D6; horizon #E8C9A8 | #8DD3E8 | Sunset #F2A65A |
| Road / edge | #3A3A3C | Edge #8B6B4A | #2B2B2D |
| Vegetation | #2E5D3A | Rice #7CB342 | Sign #1E6B3A |
| Built environment | Concrete #CFC7B8; glass #5B7A99 | Roof #B85C38; mountains #6E6A82 | Steel #9BA0A5; marker #1F3A6E/white |
| Accents | Sign #D64045; traffic #F4A236 | Natural greens | Lane paint #D9D9D9 |

Starter livery: body #B71C1C, stripe #FFFFFF, trim #C9A227. Material controls: body, stripe, metallic trim mask, alpha decal, grime and emissive map. Glass is a separate final-art material; prefer single-sided windows and limited overlap. Inspect transparent overdraw on the lowest-tier device.

## Asset budgets
| Asset | Near tris | Mid tris | Far | Texture |
|---|---:|---:|---|---|
| Bus body | 25,000 | 10,000 | 3,000 | 2048 body atlas |
| Interior | 8,000 | 2,000 | Cull | 1024 |
| Six wheels | 1,200 each | 400 each | Billboard | 512 shared |
| NPC car | 3,000 | 1,500 | 500 | 512 shared atlas |
| NPC truck | 4,500 | 2,000 | 700 | 512 shared atlas |
| Residence | 1,500 | 500 | Cuboid | 512 tileable |
| Tower | 4,000 | 1,200 | Cuboid | 1024 |
| Palm | 800 | 200 | Billboard/cull | 256 alpha clip |
| Road 50 m | 200 | 200 | 100 | 1024 tileable |
| Sign / signal | 40 / 300 | 40 / 150 | Cull | Shared 1024 / 256 |

The original bus sum is **40,200 triangles**, so reduce at least 200 to meet a strict 40,000 cap. These are per-asset targets; assess the complete visible scene, draw calls, overdraw, shadows, bandwidth and memory together.
LODGroup thresholds are screen-relative, not fixed metres. Tune to approximate near 0–30 m and mid 30–100 m across target camera FOVs. Author impostors separately; automatic LOD switches cannot manufacture a correct impostor texture.

## Camera / effects
Chase offset (0,4.5,-9), FOV 65; cockpit 75; hood 80; menu cinematic 50; garage orbit radius about 8 m, FOV 55. Chase SmoothDamp .15 s. Do not use constant interpolation factors as the sole smoothing model.
Bloom intensity .4/threshold 1.2; 3 biome LUTs; cockpit vignette .25. Low tier can disable bloom. No motion blur, DoF, SSAO or SSR.
Exhaust maximum 20 particles; air puff 8; dust only on dirt above 5 km/h. Rain must have its own tier/device budget. Use fake translucent cone meshes for final headlight haze, not volumetric lighting.

## Pipeline
Concept → model → high-to-low normal bake → texture atlas → Unity import → authored LODs → batching/instancing review → optional occlusion validation → Addressable sector build.
Name textures `BUS_[model]_[part]_[size]_[variant].png`, for example `BUS_Intercity_Body_2K_DefaultRed.png`. Publish a UV template with each UGC-capable model. Import maps at appropriate Android compression; verify alpha and mipmaps on-device.

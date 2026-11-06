# Nusantara Heavy — Unity implementation
Unity **6000.0.60f1**, URP **17.0.4**, Android ARM64 / IL2CPP. The original Java prototype lives separately at the repository root.

## Open and play
1. Install the pinned Unity Editor with **Android Build Support, SDK/NDK and OpenJDK** through Unity Hub.
2. Open **UnityProject**, let packages restore, and resolve any Editor/package errors before continuing.
3. Choose **Nusantara → Generate playable project**. This creates Boot, four Addressable scenery scenes, materials, audio placeholders, LUTs, passenger atlas, URP settings and upgrade assets under `Assets/Nusantara/Generated`.
4. Open Generated/Boot, press Play, choose **MULAI KERJA**. The bus starts in the terminal bay.
5. Open DOOR while stationary. Wait for 45 passengers, close DOOR, accelerate, and steer onto the left lane. Reach the BANDUNG bay at z=1030, stop and open DOOR to unload and receive the comfort-adjusted fare.
6. **Nusantara → Build Android APK** builds Addressables then a development APK at `Builds/Android/NusantaraHeavy.apk`. Never distribute a development build as a store release.

Generation rebuilds the generated scenes. Save custom art in a separate directory; do not hand-edit Generated scenes and then regenerate without a backup.

## Build from a phone with Unity Cloud
The Unity Build Automation target is named **Nusantara Heavy Android APK**.
In Unity Dashboard, open the Nusantara Heavy project → Build Automation → Configurations → that target → Build.
The target reads `main`, uses the `UnityProject` subfolder and the version in ProjectVersion.txt, exports an APK with a development keystore, and runs EditMode tests.
Its Pre-Export method is `Nusantara.Editor.ProjectBuilder.PrepareCloudBuild`, which generates Boot and scenery and builds Addressables before UBA exports the player.
Automatic builds and recurring schedules are off. The selected Windows Micro builder is free-tier eligible; check the remaining allowance before starting additional builds.
A finished successful build exposes the APK under its Artifacts tab. Unity Cloud manages its own Editor activation; this route does not use the repository's GameCI activation secrets.

## Controls
- Mobile: simultaneous LEFT/RIGHT + DRIVE/BRAKE; DOOR, HORN, signals, retarder, headlights and camera.
- Editor: W/S = accelerator/brake, A/D = steering, H = horn, O = doors. Mouse can operate touch buttons.
- Manual mode: hold CLUTCH before GEAR +/−. Invalid shifts cut torque and play a placeholder grinding sound.
- Opening doors interlocks propulsion. Changing screens releases inputs; app background/focus loss pauses.
- Garage: select procedural body color; import image by path in the Editor or use GALLERY on Android. Android uses the system document picker without broad storage permissions.

## Implemented scope
| Area | Implementation |
|---|---|
| Physics | 12 t default, 6 WheelColliders, lowered/rear COM, yaw damping, speed-sensitive/hold-ramped steering, air brake smoothing and pressure recovery, retarder, automatic/manual transmission and torque cut |
| Passenger loop | 45 pre-created passenger quads, four-frame walking placeholders, terminal trigger and speed/door gates, automated tickets, destination unloading |
| Comfort / economy | Physics-frame acceleration penalties, overspeed, collisions and potholes, smooth-driving recovery, signal benefit, below-40 zero fare, IDR payout, one trip reward credit |
| Garage | Tint persistence, validated PNG/JPEG imports, 1024 decal resampling, MaterialPropertyBlock, upgrades capped at five levels, three placeholder fleet specifications, multiple saved decal slots and slot purchases |
| Traffic | 20 pre-created kinematic Rigidbody NPCs, cubic Bezier lanes and U-turns, traffic light checks, obstacle SphereCasts, horn response, distant recycling |
| Streaming | 500 m cell addresses, radius-1 preloading/radius-2 retention, async failure retry and handle cleanup; four generated scenery scenes |
| Art | Starter red/white/gold palette, city/rural/toll decorative variants, URP body shader, grime scalar with authored masks, opaque tinted placeholder windows, near/mid/cull vegetation LODs |
| Effects | Procedural horn/diesel/air/grind WAVs, exhaust/puff/rain particles, dirt trails, brake/indicator visual updates, fake mesh headlight haze and two forward spot headlights without shadows |
| Camera / UI | Chase/cockpit/hood, menu/garage orbit, SmoothDamp chase, safe-area landscape/portrait uGUI HUD, analog + digital speed, passenger card, menu/garage/routes/shop/settings/payout |
| Rendering | 30 FPS default; 60 FPS setting, low-quality render scaling/shadow reduction, bloom, biome lookup grading, cockpit vignette |
| Build / tests | Android build entry point, EditMode tests for economy/images/reward callbacks/scene wiring, source validation, manual licensed Unity CI |

## Explicit limits — not a finished commercial game
- **No Unity Editor or Android SDK was available during authoring.** Syntax/source checks are not Unity compilation, Shader compilation, an APK test or device profiling. Run EditMode tests, a clean Android build, and the device checklist before treating this as verified.
- Geometry, passenger sprites, fonts (Unity runtime default) and procedural audio are placeholders, not the requested final Karoseri-quality art. Fleet options currently share the procedural mesh. No licensed bus branding is bundled.
- The route is a fictional straight test corridor. Actual Jakarta/Bandung geography, a road network editor, realistic terminals, detailed interiors and fleet models require authored content.
- Portrait is an implementation-defined stacked layout; it needs device usability approval alongside landscape.
- Decal slots use numbered app-local files and a SLOT button. No layered PSD template or licensed Rajdhani/Inter fonts is bundled.
- The shader is handwritten URP HLSL rather than a Shader Graph asset. There is no runtime UV rebake. A dirt scalar reads authored mask/vertex data. Instruction counts must be measured per target GPU.
- Particle effects use standard ParticleSystem; they do not claim GPU simulation. Passenger flipbooks are simple procedural placeholders. Final exhaust, rain and haze textures still need art.
- Vegetation uses shared instancing-enabled materials and LODs. Static flags are applied to generated props. No occlusion bake is shipped, and boot proxy geometry is not a verified fix for an IL2CPP engine bug.
- Live ads, IAP, receipt verification, purchase restoration, entitlement storage and forced-ad removal are **not configured**. No provider SDK, credentials, product catalogue or store receipts are present. Buttons requiring rewarded ads remain disabled; fake completions are never granted.
- Local career JSON and UTC daily limits are suitable for an offline prototype, not authoritative anti-cheat/accounting. An online service is required for tamper-resistant purchases/rewards and time validation.
- 60/30 FPS are targets, not measurements. The whole driveable road/collision ground stays persistent; the demo streams decorative sectors to avoid dangerous collision gaps. Production route streaming requires loading barriers and a safe spawn policy.

## Tests and build automation
Editor: **Window → General → Test Runner → EditMode → Run All**.
CLI (Editor installed and activated):
```sh
Unity -batchmode -nographics -projectPath UnityProject -runTests -testPlatform EditMode -testResults test-results.xml -logFile tests.log
Unity -batchmode -quit -projectPath UnityProject -buildTarget Android -executeMethod Nusantara.Editor.ProjectBuilder.BuildAndroid -logFile build.log
```
The repository's **Unity Android (licensed)** workflow is manual. Configure GameCI-supported Unity license/account secrets before running it. The existing **Android APK** workflow builds the older Java app, not this Unity game.

See `../docs/ART_DIRECTION.md`, `../docs/DEVICE_ACCEPTANCE.md`, and `../docs/ARCHITECTURE.md`.

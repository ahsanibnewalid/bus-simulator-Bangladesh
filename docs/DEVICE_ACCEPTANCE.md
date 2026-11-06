# Required acceptance before release
Not yet executed in the authoring environment.

## Build gates
- Restore packages in Unity 6000.0.60f1; fix every compiler/shader error.
- Run EditMode tests, including generated scene/reference checks.
- Build Addressables for Android, then IL2CPP ARM64 APK. Install on a clean device and test offline startup.
- Confirm native image picker and callbacks survive cancellation/backgrounding. Pick PNG/JPG at limits; reject oversized or malformed data without a crash.

## Gameplay gates
- Hold steering and a pedal with two fingers; release/cancel one pointer without dropping the other. Enter menus/background during acceleration; ensure controls clear.
- Start stationary at terminal; open doors; wait for boarding; interrupt/reopen boarding; never exceed 45 passengers or charge tickets twice.
- Close doors, drive left lane, use retarder and brakes. Test missed-clutch shifts and every gear. Inspect suspension bottoming and rollover on banking.
- Stop in destination bay and open doors; interrupt/reopen unloading; verify count and fare. Below 40 comfort pays zero. Reopening payout cannot credit twice.
- Buy upgrades, change fleet specifications, import livery, restart app; verify persistence and sufficient-funds checks. Corrupt primary JSON; verify backup recovery.
- Check traffic signals, obstacle queueing, horns within 5 m, and recycling around the camera. Test scene teardown with pending loads and failed Addressable requests.

## Visual / performance gates
- Test 16:9, 20:9, cutouts and system gestures. Measure physical touch target dimensions; the Canvas reference does not by itself guarantee dp sizes on every device.
- Profile Adreno 640 / Mali-G77 for 60 FPS, Adreno 610 for 30 FPS. Run 15-minute thermal sessions with traffic, rain, headlights and sector transitions.
- Record CPU/GPU frame time, GC allocations, peak managed/native/texture memory, batches/SetPass, triangles, transparent overdraw and load stalls. Do not call the target met from a short Editor FPS sample.
- No pink shaders; validate body decal alpha, metallic mask, lights, LUT strips and post-processing. Replace greybox art and default fonts before final visual approval.
- Production monetization requires verified SDK integrations, consent/privacy handling as applicable, store test purchases/restoration, duplicate callbacks, offline failure and fraud-resistant backend checks.

# Nusantara Heavy — Android 3D heavy bus simulation
The active Unity implementation is in **[UnityProject](UnityProject/README.md)**. It implements the approved systems as a connected, generated-scene prototype with replaceable art.

## Start here
1. Install **Unity 6000.0.60f1** with Android Build Support, SDK/NDK and OpenJDK.
2. Open `UnityProject` in Unity Hub and restore packages.
3. Choose **Nusantara → Generate playable project**, then open Generated/Boot and press Play.
4. Choose **Nusantara → Build Android APK** for the Unity development build.

Includes vehicle physics and transmissions, air brakes/retarder, mobile controls, passengers and comfort payouts, garage/livery import, saved upgrades/fleet specifications, pooled spline traffic, Addressable scenery streaming, biome/weather controls, cameras, shader/effect foundations, tests and build tooling.

**Status:** source implementation and greybox prototype. Unity compilation, APK installation and device FPS targets are not yet verified. Final bus models/maps, production UI art and live ad/IAP services remain external/content work. See the [implementation matrix and limits](UnityProject/README.md).

- [Visual art bible](docs/ART_DIRECTION.md)
- [Architecture](docs/ARCHITECTURE.md)
- [Device acceptance checklist](docs/DEVICE_ACCEPTANCE.md)

## Earlier Java prototype
The root Gradle `app/` remains a separate 2D Android arcade prototype. Its **Android APK** workflow and `Bus-Simulator-Bangladesh-debug` artifact do **not** contain the Unity game. The **Unity Android (licensed)** workflow builds Nusantara Heavy and requires an activated Unity license/account configuration.

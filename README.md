# Bus Simulator Bangladesh — Android
An early, offline Android arcade driving prototype. Built with Java and Android Canvas, with no external game assets, accounts, ads, or network permission.

## Playable now
- Landscape perspective road with rural scenery and left-hand traffic.
- Multitouch steering, accelerator, brake and horn.
- 3 km fictional Dhaka-to-countryside route with three distance checkpoints.
- Traffic collisions, vehicle condition and off-road slowdown/damage.
- Pause/resume, restart, speedometer, progress and saved personal best.
- App backgrounding clears controls, stops the bus and pauses the game.

## Get the APK
Open **Actions → Android APK → latest successful run → Artifacts**.
Download **Bus-Simulator-Bangladesh-debug**, unzip and install app-debug.apk on Android 6.0 or newer.
This is a debug build for testing; it is not a signed Play Store release.

## Build
Install Android Studio with Android SDK 35 and JDK 17. Open this directory as a Gradle project.
The build uses Android Gradle Plugin 8.7.3 and Gradle 8.9.
With Gradle 8.9 installed:
```sh
gradle :app:assembleDebug :app:lintDebug
```
Output: app/build/outputs/apk/debug/app-debug.apk.
CI installs Gradle explicitly; no Gradle wrapper binary is committed.

## Device checks
1. Start a route; hold DRIVE and LEFT or RIGHT together.
2. Release each finger independently; that control must release.
3. BRAKE should slow the bus; driving onto the verge should reduce condition.
4. Passing traffic at the same lateral position should cause damage.
5. Pause and background the app while accelerating; resume with pedals released.
6. Complete 3 km, restart, and relaunch to confirm best-score persistence.
7. Check layout and touch targets on wide displays and devices with cutouts.

## Scope and next development
This is a small arcade foundation, not a realistic 3D bus simulator. The route and checkpoint names are fictional; checkpoints are distance milestones, not passenger stops.
No detailed bus models, cockpit, actual geographic maps, passenger boarding, economy, gears, mirrors, physics simulation or multiplayer are implemented.
Those require a larger game design and asset pipeline. All current scenery is drawn in code.
The source has not yet been built with the Android SDK or tested on a physical phone in the authoring environment. GitHub Actions is configured to compile and lint; a successful run is needed before distributing an APK.

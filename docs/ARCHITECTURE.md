# Runtime architecture
`BusController.FixedUpdate` owns forces, WheelColliders, powertrain and brake state at 50 Hz. `MobileUI` owns touch input and samples displayed information; GameEvents publishes telemetry at 10 Hz. Comfort integrates acceleration each physics frame so a short incident cannot be missed by a slower UI poll. All events have unsubscribe paths and static event reset hooks for domain-reload-disabled Editor sessions.

`PassengerManager` uses pre-created objects and interruptible boarding/unloading coroutines. Terminal triggers require stationary speed and open doors. A completed trip credits the career once; advertising can add only the extra fare and is guarded against repeated callbacks. Production SDK adapters must marshal callbacks onto Unity's main thread and verify completion/receipts.

`RoadGraph` stores cubic Bezier segments with next-edge indices. NPCs use kinematic Rigidbodies, MovePosition/MoveRotation, obstacle casts and signal gating. `TrafficSpawner` reuses its serialized pool and searches for non-overlapping points outside the immediate view. The demo only authors a simple loop; a production network needs intersection conflict zones, lane-change rules and speed limits.

`SectorStreamer` owns Addressables scene handles. It retains loading entries, retries failed loads with backoff, releases failures, avoids overlapping unload/reload operations, and unloads successful pending loads after teardown. Sectors are identified explicitly rather than synthesizing addresses that might not exist. Persistent road physics avoids driving into missing collision geometry in the demo.

`LiveryManager` performs bounded file reading on a worker and validates header dimensions before main-thread decode. Unity texture creation, resampling and renderer work stay on the main thread. It stores a normalized app-local PNG, shares materials using property blocks and disposes replaced textures. GalleryBridge uses Android's system picker and bounded ContentResolver copying, without storage-wide permissions.

`Career` stores offline progress in JSON with a backup. Upgrades are ScriptableObjects; reload re-applies stored levels. `Commerce` exposes provider boundaries rather than simulating paid features. These local records are not a server-authoritative economy.

`ProjectBuilder` creates reviewable assets and serialized references from deterministic source, and owns Android/URP configuration and the Addressables-before-player build sequence. Keep real content outside Generated and replace the generator or author production scenes before final asset production.

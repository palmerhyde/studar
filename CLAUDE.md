# studar — AR brick builder for Meta Quest 3

Augmented-reality brick builder for Meta Quest 3, built in Unity 6 on macOS (Apple Silicon).
See the room through passthrough, place compatible bricks on a real table, snap them together
with a controller, and walk around what you build. The name = `stud` + AR (augmented reality).
This is a fresh project, **separate from Brick Fighter** — no shared code or data.

> **Not affiliated with the LEGO Group.** LEGO® is a trademark of the LEGO Group. Use "LEGO"
> only descriptively (adjective + generic noun, e.g. "LEGO® bricks"), never in the project/app
> name, and keep the disclaimer in README/NOTICE intact. See `NOTICE`.

## Source of truth (read these before any work)
- `AR-LEGO-Feasibility-and-Architecture.md` — feasibility report, full architecture, 4-phase roadmap.
- `Phase-0-and-0.5-Technical-Spec.md` — the implementation brief for the first two milestones.
- `Claude-Code-Kickoff.md` — the kickoff prompt + manual on-device checkpoints.

The spec and architecture docs are authoritative for scope. Build strictly to **Phase 0 and
Phase 0.5**; treat the spec's §8 "out of scope" as off-limits until those are proven.

## Current status
**Environment set up; ready to start the Phase 0 spike.** Tooling and device setup per spec §2–3
are complete:
- Unity Hub + **Unity 6.4 (6000.4.10f1, Apple Silicon)** with full Android toolchain
  (Build Support, OpenJDK 17, SDK/NDK — `adb` & `java` verified).
- **MQDH** installed.
- **LDraw** complete library + `3001.dat` and the **LDCad shadow library** staged under
  `third_party/` (gitignored; CC BY 4.0 / CC BY-SA 4.0).
- **Meta developer account + team** created and verified; **Developer Mode on**; Mac↔headset
  **adb authorized** (Quest 3 `eureka`, Horizon OS build 204 — shows as `device`).

Still pending on-device: **Space Setup** (run once in the room so MRUK sees the table) — deferred
until Phase 0 needs it. **Meta XR All-in-One SDK + MRUK** are not installed yet; that happens
inside the Unity project (UPM) as the first task of the Phase 0 spike (spec §5.3).

## Locked decisions
- **Hardware:** Meta Quest 3 (confirmed).
- **Engine:** Unity 6 (6000.x LTS), URP, Apple Silicon editor build, Android build target.
- **SDKs:** Meta XR All-in-One SDK + MR Utility Kit (MRUK).
- **Input:** controllers first; hand-tracking deferred to Phase 1+.
- **Brick model:** snap/constraint-based — **no physics on placed bricks**.
- **2×4 connection points:** hand-authored (not parsed from LDCad) for the spike.
- **Mesh:** pre-convert `3001.dat` (2×4) to OBJ/FBX offline — **no LDraw parser yet**.
  - *Open item:* the `3001_2x4` mesh does not exist yet; how it's produced is parked.

## Critical gotcha — units & coordinates (spec §4)
LDraw and Unity differ. Bake this into `Units.cs` once and never re-derive it:
- 1 LDU = 0.4 mm → multiply LDraw positions by **0.0004** for metres.
- LDraw is **−Y up**, Unity is **+Y up** → **negate Y**.
- Stud pitch = **20 LDU = 8 mm**; brick height = **24 LDU = 9.6 mm**.
- A 2×4 = 40×80 LDU footprint = **16×32 mm**, 9.6 mm tall. A wrong scale or un-flipped Y is the classic day-one bug.

## Performance bar
Quest must hold **72 FPS minimum** (90 preferred), rendered twice (one per eye). Phase 1's
answer to scale is GPU instancing — deferred, but every Phase 0/0.5 choice should stay friendly to it.

## Target project structure (spec §7)
```
/Assets
  /Meshes        3001_2x4.obj (+ material)
  /Scripts
    Units.cs               // LDU<->Unity conversion (§4)
    ConnectionPoint.cs     // position, gender, axis, occupied flag
    BrickDefinition.cs     // per-part connection-point list (2x4 hand-authored)
    ConnectionRegistry.cs  // spatial hash of open connection points
    SnapSolver.cs          // query + compute snap transform + ghost
    BrickGrabbable.cs      // controller grab + release -> commit
    BrickSpawner.cs        // palette / spawn button
  /Scenes
    Phase0_Pipeline.unity
    Phase05_Snap.unity
```

## Licensing note
Brick geometry/connectivity come from **LDraw** (+ LDCad shadow library), CC Attribution v2.0
(CCAL) — free to redistribute *with attribution*. "LEGO" is a trademark: any public release must
avoid implying official affiliation. Not a concern for a personal prototype.

## Working agreement
- Keep code small and readable — this is a concept spike, not production.
- Ask before adding any dependency not already listed in the spec.
- Stop for on-device testing at each Phase checkpoint (see the kickoff doc).

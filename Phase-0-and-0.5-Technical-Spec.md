# Technical Specification — Phase 0 & Phase 0.5

*AR LEGO World on Meta Quest. This document is written to be handed to Claude Code (or any engineer) as the starting brief for implementation.*

Prepared for Liam · June 2026
Companion to: `AR-LEGO-Feasibility-and-Architecture.md`

---

## 0. Purpose & the "definition of done"

Two small, sharply-scoped milestones that together de-risk the whole project before any serious investment.

- **Phase 0 — Pipeline spike.** Prove the full chain works: Unity → Quest → see the real room (passthrough) → detect the table → render *one* LEGO brick sitting on it, anchored in place so you can walk around it.
- **Phase 0.5 — Connectivity spike.** Prove the core magic: spawn several **2×4 bricks**, pick them up, and snap them to *each other* correctly, in every valid position, stably, at 72+ FPS.

**The acceptance test for the project's promise:** put the headset on a non-engineer — Liam's son — hand him bricks, and have him intuitively snap two 2×4s together and stack a small wall, with no instruction. If that feels magical, the concept is proven and Phase 1 (generalizing to many parts) is justified.

Everything below serves those two goals. Anything not needed for them (full part catalog, hinges, hand-tracking, UI polish, persistence beyond a session) is explicitly **out of scope** and deferred to Phase 1+.

---

## 1. Hardware prerequisite (read first)

| Requirement | Needed | Notes |
|---|---|---|
| Headset | **Meta Quest 3, 3S, or Pro** | Required for *color* passthrough and scene understanding (MRUK). **Quest 2 will not work** for this spec — its passthrough is low-res monochrome with no scene/table detection. |
| Mac | Apple Silicon (M1 or newer) or Intel | Unity + Meta tooling both run on macOS, including M-series. |
| USB cable | USB-C, data-capable | For deploying builds from Mac to headset (Air Link/Wi-Fi also works but USB is more reliable for first setup). |
| Room | One real table, decent lighting | You'll run Quest's Space Setup once so MRUK can detect the table surface. |

> **Action for Liam:** confirm the headset model. If it's a Quest 2, the snap-solver work (Phase 0.5) is still valid, but it would have to run in fully-virtual VR rather than passthrough MR, which changes the "bricks on my real table" experience.

---

## 2. Software to download & install

Install in this order. Versions reflect the current (mid-2026) recommended baseline.

### 2.1 Unity (yes, it works on Mac)

1. **Unity Hub** — the launcher/version manager. Download from unity.com. Free **Unity Personal** license is sufficient.
2. **Unity Editor 6 (Unity 6, the 6000.x LTS line)** installed *through* Unity Hub. On Apple Silicon, choose the **Apple Silicon** editor build.
   - Unity 6 pairs with **Meta XR SDK v74+**, which is the current path. (Unity 2022 LTS + Meta XR v73 is the older fallback if ever needed.)
3. When installing the editor, **add these modules** (the Quest runs a modified Android OS, so Android build support is mandatory):
   - **Android Build Support**
   - **OpenJDK**
   - **Android SDK & NDK Tools**

### 2.2 Meta XR SDK (the Quest integration for Unity)

Installed *inside* the Unity project via the **Unity Package Manager (UPM)** — UPM is now the standard delivery method for Meta's SDKs.

- **Meta XR All-in-One SDK** (`com.meta.xr.sdk.all`) — convenient umbrella that pulls in Core, Interaction, etc. (Or install only **Core SDK** + the pieces below to stay lean.)
- **Mixed Reality Utility Kit / MRUK** (`com.meta.xr.mrutilitykit`) — this is what gives you table/surface detection. Add `"com.meta.xr.mrutilitykit": "latest"` to the project's `Packages/manifest.json` dependencies, or install via Package Manager UI.

### 2.3 Meta Quest Developer Hub (MQDH)

A desktop companion app for Mac (supports M-series) used to deploy builds, view logs, cast the headset view, and monitor performance. Download from the Meta developers site. Strongly recommended — it makes the build/deploy/debug loop far smoother than raw `adb`.

### 2.4 LDraw part data (open, free, CC-licensed)

- **LDraw All-In-One-Installer** *or* the parts library directly from **library.ldraw.org**. Released under **Creative Commons Attribution License v2.0 (CCAL)** — free to redistribute with attribution.
- For Phase 0/0.5 you only need **one file**: the 2×4 brick, `3001.dat`.

### 2.5 LDCad shadow library (open, the connectivity metadata)

- From **melkert.net/LDCad** (shadow library section) or the GitHub mirror **RolandMelkert/LDCadShadowLibrary**.
- This holds the `SNAP_CYL` / `SNAP_FGR` / `SNAP_GEN` connection metadata. For Phase 0.5 you'll read the 2×4's stud/anti-stud info from here (or hand-author it — see §6).

---

## 3. Meta developer account & headset setup

All free. No paid developer program is required to develop and sideload your own apps.

1. **Create a Meta developer account** at the Meta Horizon developer dashboard (sign in with the same Meta account your headset uses).
2. **Create an organization** — enter any name, accept the terms. (This is the free "developer team" that ownership of dev-mode apps hangs off.)
3. **Pass verification** — Meta requires *either* enabling **two-factor authentication** *or* adding a payment method. 2FA is the free, no-card route. Do this once.
4. **Update the headset** to the latest firmware.
5. **Enable Developer Mode** via the **Meta Horizon mobile app**: pair the headset → Headset Settings → Developer Mode → toggle on.
6. **Connect the headset to MQDH** (USB) and approve the "Allow USB debugging" prompt inside the headset.
7. **Run Space Setup** on the headset once, in the room with the table, so the scene model (including the table) exists for MRUK to read.

After this, deploying from Unity → headset is a one-click "Build and Run."

---

## 4. Units & coordinate systems (critical — get this right once)

LDraw and Unity use different conventions. Bake the conversion into one utility and never think about it again.

| | LDraw | Unity |
|---|---|---|
| Unit | **LDU** (LDraw Unit) | metres |
| Scale | 1 LDU = **0.4 mm** | 1 unit = 1 m |
| Up axis | **−Y is up** (Y points down) | **+Y is up** |
| Handedness | right-handed | left-handed |

**Key LEGO dimensions (memorize these):**

- **Stud-to-stud pitch (horizontal): 20 LDU = 8 mm = 0.008 m.** This is the grid everything snaps to.
- **Brick height: 24 LDU = 9.6 mm.** (A plate is 8 LDU; a brick = 3 plates.)
- A **2×4 brick** footprint = 2 × 4 studs = 40 LDU × 80 LDU = 16 mm × 32 mm, height 24 LDU = 9.6 mm.

**Conversion rule (LDraw → Unity):** multiply positions by `0.0004` (LDU→m) and flip the Y axis (negate Y) to correct the up-direction. Verify visually that imported geometry is upright and correctly scaled before doing anything else — a wrong scale or flipped Y is the classic first-day bug.

---

## 5. Phase 0 — Pipeline spike

### 5.1 Scope
Render exactly one 2×4 brick, anchored on the detected real-world table, viewed through color passthrough.

### 5.2 Mesh approach (deliberately the easy path)
Do **not** write a general LDraw parser yet. For Phase 0/0.5, **pre-convert the single `3001.dat` into a standard mesh** (OBJ/FBX/glTF) offline and import that into Unity. This isolates the spike from import-pipeline complexity (which is a Phase 1 problem). Apply a simple lit material and the standard LEGO "bright red" color.
- *Optional aggressive simplification:* studs carry a lot of tiny geometry; a decimated mesh is fine for the spike.

### 5.3 Tasks
1. New Unity 6 project (URP, mobile/VR template). Set platform to **Android**, configure for Quest (Meta XR project setup tool / building blocks).
2. Install Meta XR All-in-One SDK + MRUK via UPM.
3. Enable **passthrough** (Meta XR "Passthrough" building block / `OVRPassthroughLayer`). Scene background = transparent so the real room shows.
4. Add the **MRUK** rig; on start, load the scene model and query for **table** surfaces (MRUK has a "find largest surface / table" helper).
5. Place the imported 2×4 mesh on the table surface, sitting on top of the plane, scaled correctly (§4).
6. Create a **spatial anchor** at the brick's location so it stays world-locked when you walk around.
7. Build & Run to the headset via MQDH.

### 5.4 Acceptance criteria
- [ ] Headset shows the real room in color passthrough.
- [ ] App correctly identifies the table and the brick rests on its surface (not floating/clipping).
- [ ] Brick is at correct real-world scale (a 2×4 looks like a real 2×4 ≈ 16×32 mm).
- [ ] Walking around the table, the brick stays anchored in place.
- [ ] Holds 72+ FPS (trivially, with one brick).

---

## 6. Phase 0.5 — Connectivity spike

This is the heart of the project: a working snap solver, proven on one part.

### 6.1 Scope
- Spawn multiple **2×4 bricks** (e.g. a small palette or a spawn button).
- Pick one up (controller grab), move it near another, see a **ghost preview** of where it will snap, release to **commit** the connection.
- Snapping must be correct for **all valid 2×4-to-2×4 matings** and stable when stacking several.

### 6.2 The 2×4 connection model
A 2×4 brick connects to other bricks **only vertically**, stud-to-anti-stud (there are no side connectors on a plain brick). Define, in brick-local coordinates:

- **8 top studs (gender = Male)** on a 2×4 grid at 20 LDU pitch, on the top face (+Y in Unity after conversion).
- **Anti-studs / tubes (gender = Female)** on the bottom face, on the same 2×4 grid, that accept studs from a brick below.

You can read these positions from the LDCad `SNAP_CYL` metadata for `3001`, **or** simply hand-author them — for a single known part, the 8 grid positions are trivial to write down (x ∈ {±10 LDU}, z ∈ {−30, −10, +10, +30 LDU}, relative to brick center). Hand-authoring is recommended for the spike; it keeps the focus on the *solver*, not the *parser*.

### 6.3 The snap algorithm
1. **Connection-point registry.** Every placed brick contributes its open connection points (top studs = male/up, bottom anti-studs = female/down) to a **spatial hash** keyed by quantized world position (cell size ≈ one stud pitch).
2. **While holding** a brick, take its candidate *down-facing female* points and query the hash for nearby *up-facing male* studs within a **snap radius** (e.g. ~0.5–1 stud pitch) and small angular tolerance.
3. **Compute the snap transform.** If a compatible pair is found, snap the held brick so its stud grid registers with the target's grid: quantize translation to the **20 LDU lattice** and rotation to the **nearest 90° about vertical**. (2×4-to-2×4 is valid at 0/90/180/270° and at any integer-stud overlap of ≥1 stud.)
4. **Ghost preview.** Render a translucent ghost at the snapped pose so the user sees the result before releasing.
5. **Commit on release.** Lock the brick at the snapped pose; mark the now-occupied connection points as consumed; add the new brick's open points to the registry.
6. **No physics on placed bricks** (snap/constraint model). Optional: light physics only on the in-hand brick for feel.

### 6.4 Interaction
- **Controller grab** (Meta XR Interaction SDK ray-grab or direct grab). Controllers first — precise and reliable.
- Hand-tracking is **out of scope** for 0.5 (it's a Phase 1+ "wow" upgrade).

### 6.5 Tasks
1. Build on the Phase 0 project.
2. Add a brick **spawner** (button or palette) producing 2×4 bricks on/near the table.
3. Implement the **ConnectionPoint** data structure and the **spatial-hash registry**.
4. Implement **grab → query → snap → ghost → commit** loop.
5. Enumerate & verify all valid 2×4 matings (see acceptance).
6. Stress: build a small wall (~10–20 bricks); watch FPS and snap stability.

### 6.6 Acceptance criteria
- [ ] Can spawn and grab 2×4 bricks with a controller in passthrough.
- [ ] Snapping works for **all valid matings**: centered stack, offset stacks (1/2/3-stud overlaps), and 90°/180° rotations — not just the obvious centered case.
- [ ] Ghost preview shows the snap target before release; release commits cleanly.
- [ ] Stacking 10–20 bricks stays stable (no drift, no double-occupied studs, no jitter).
- [ ] Holds **72+ FPS** throughout.
- [ ] **The son test:** a first-time user, given no instructions, intuitively snaps two bricks together and stacks a few. *If this delights, the concept is proven.*

---

## 7. Suggested project structure
```
/Assets
  /Meshes        3001_2x4.obj (+ material)
  /Scripts
    Units.cs               // LDU<->Unity conversion helpers (§4)
    ConnectionPoint.cs     // position, gender, axis, occupied flag
    BrickDefinition.cs     // per-part connection-point list (2x4 hand-authored)
    ConnectionRegistry.cs  // spatial hash of open connection points
    SnapSolver.cs          // query + compute snap transform + ghost
    BrickGrabbable.cs      // controller grab + release -> commit
    BrickSpawner.cs        // palette / spawn button
  /Scenes
    Phase0_Pipeline.unity
    Phase05_Snap.unity
  /MRUK, /MetaXR ...        // SDK content
```

---

## 8. Explicitly out of scope (deferred to Phase 1+)
General LDraw importer · the full ~16.5k part catalog · GPU-instanced rendering at scale · hinges/articulated parts (`SNAP_FGR`) · side/odd connections (`SNAP_GEN`) · hand-tracking · model save/load & cross-session persistence · multiplayer · any UI beyond a spawn affordance.

---

## 9. Decisions to confirm before coding
1. **Headset model** — Quest 3 / 3S / Pro confirmed? (Blocks the MR/passthrough assumption.)
2. **Engine** — Unity 6 confirmed (vs. Unreal)? Spec assumes Unity.
3. **Connection data for the 2×4** — hand-author the 8 studs (recommended for speed) vs. parse from LDCad `SNAP_CYL`? Spec assumes hand-author for the spike.
4. **Mesh** — pre-convert `3001.dat` to OBJ offline (recommended) vs. write a minimal LDraw loader now? Spec assumes pre-convert.

---

### Sources
- Unity on Mac / Quest: [Enhanced Mac Support for Unity + Quest](https://developers.meta.com/horizon/blog/mac-support-unity-meta-quest-horizon-developer/), [Hardware & software requirements](https://developers.meta.com/horizon/documentation/unity/unity-development-requirements/), [Unity macOS requirements](https://docs.unity3d.com/6000.1/Documentation/Manual/macos-requirements-and-compatibility.html)
- Meta XR SDK / MRUK: [Meta XR SDKs overview](https://developers.meta.com/horizon/documentation/unity/unity-sdks-overview/), [MRUK getting started](https://developers.meta.com/horizon/documentation/unity/unity-mr-utility-kit-gs/), [Meta XR All-in-One SDK](https://developers.meta.com/horizon/downloads/package/meta-xr-sdk-all-in-one-upm/)
- Developer mode / account: [Enable developer mode on headset](https://developers.meta.com/horizon/documentation/android-apps/enable-developer-mode/), [Device setup](https://developers.meta.com/horizon/documentation/native/android/mobile-device-setup/), [MQDH getting started](https://developers.meta.com/horizon/documentation/native/android/ts-mqdh-getting-started/)
- LDraw / LDCad data: [LDraw Legal Info (CCAL)](https://www.ldraw.org/legal-info), [LDraw Library](https://library.ldraw.org/), [Part Snapping Language Extension](https://wiki.ldraw.org/wiki/Part_Snapping_Language_Extension), [LDCad Shadow Library](https://www.melkert.net/LDCad/tech/shadowLib)

# studar

**An augmented-reality brick builder for Meta Quest 3.** See your real room through
passthrough, place compatible building bricks on a real table, snap them together with a
controller, and physically walk around what you build.

The name is `stud` (the bumps on a brick) + **AR** (augmented reality) → **studar**.

> ⚠️ **Not affiliated with the LEGO Group.** LEGO® is a trademark of the LEGO Group.
> This is an independent, fan-made project and is **not** created, sponsored, endorsed by,
> or affiliated with the LEGO Group in any way. Any reference to "LEGO" here is descriptive
> only — to indicate compatibility with genuine LEGO® bricks. See [NOTICE](NOTICE).

---

## Status

**Environment set up; Quest configuration in progress.** The dev toolchain and headset are
ready, and the Unity project now lives in this repo. Done so far:

- Unity 6.4 (Apple Silicon) with the Android build toolchain, Meta Quest Developer Hub, and
  Rosetta 2 installed.
- Meta developer account + organisation; Developer Mode on; Quest 3 authorised over `adb`.
- Unity 6 project created at the repo root, with the **Meta XR All-in-One SDK + MRUK**
  installed and `Assets/Scripts/Units.cs` (the LDU↔Unity conversion, spec §4) in place.

**Next:** switch the build target to Android, apply the Meta Project Setup Tool fixes, then
build the Phase 0 spike (passthrough → table detection → one anchored brick).

## What it is

A headset-grade building engine sitting on top of two things that already exist and are free:

- **Open brick data** — part geometry from the [LDraw](https://www.ldraw.org) library and
  connection ("clutch") metadata from the [LDCad shadow library](https://www.melkert.net/LDCad).
- **Meta Quest mixed reality** — passthrough video of your real room plus table/surface
  detection via the Mixed Reality Utility Kit (MRUK).

The novel part — and what these early phases de-risk — is the bit in the middle: rendering
many bricks at 72+ FPS and a real-time snap solver that clicks them together correctly.

## Documents (source of truth)

- [AR-LEGO-Feasibility-and-Architecture.md](AR-LEGO-Feasibility-and-Architecture.md) — feasibility, architecture, full roadmap.
- [Phase-0-and-0.5-Technical-Spec.md](Phase-0-and-0.5-Technical-Spec.md) — implementation brief for the first two milestones.
- [Claude-Code-Kickoff.md](Claude-Code-Kickoff.md) — kickoff prompt and on-device test checkpoints.
- [CLAUDE.md](CLAUDE.md) — working context for AI assistants in this repo.

## Roadmap (early phases)

- **Phase 0** — pipeline spike: one brick, on the real table, anchored, through passthrough.
- **Phase 0.5** — connectivity spike: spawn bricks, grab with a controller, snap to every valid mating, stable at 72 FPS.
- **Phase 1+** — generalise to many parts, GPU-instanced rendering, hand-tracking, hinges, save/load. (See the architecture doc.)

## Tech stack

Unity 6 (URP) · Meta XR All-in-One SDK + MRUK · Meta Quest 3 · macOS (Apple Silicon) ·
controller input · snap/constraint brick model (no physics on placed bricks).

## Development setup

Building and deploying to the headset requires (see the spec §2–3 for the full walkthrough):

- **Unity 6** (6000.x, Apple Silicon) installed via Unity Hub, with the **Android Build
  Support**, **OpenJDK**, and **Android SDK & NDK Tools** modules. On Apple Silicon, Unity
  also requires **Rosetta 2** (`softwareupdate --install-rosetta`).
- **Meta Quest Developer Hub (MQDH)** for deploy/log/cast.
- A **Meta developer account** with an organisation, **Developer Mode** enabled on the Quest 3,
  and the headset authorised for `adb` over USB.
- The Meta XR SDK and MRUK are installed *inside* the project via the Unity Package Manager
  (`com.meta.xr.sdk.all`) — no manual download needed; opening the project resolves them.

Brick source data (LDraw library + LDCad shadow library) is staged locally under
`third_party/` and is **git-ignored** — it is not committed. The single 2×4 part used by the
early phases is `3001.dat`. See [NOTICE](NOTICE) for attribution (LDraw is CC&nbsp;BY&nbsp;4.0,
the shadow library CC&nbsp;BY-SA&nbsp;4.0).

## License

Original source code: [MIT](LICENSE). Third-party data, dependencies, and trademarks are
covered by their own terms — see [NOTICE](NOTICE).

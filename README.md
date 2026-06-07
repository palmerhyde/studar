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

**Pre-development / evaluation.** No engine tooling installed and no Unity project created
yet. This repository currently holds the planning documents and project scaffolding. The
first real step is environment setup, then a Phase 0 spike.

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

## License

Original source code: [MIT](LICENSE). Third-party data, dependencies, and trademarks are
covered by their own terms — see [NOTICE](NOTICE).

# Claude Code Kickoff — AR LEGO (Phase 0 → 0.5)

Paste the prompt below into Claude Code from inside the new AR LEGO project folder once Unity 6 (+ Android modules), the Meta XR + MRUK packages, the Meta dev account/dev-mode, and MQDH are installed (see `Phase-0-and-0.5-Technical-Spec.md`, §2–3). Target hardware: **Quest 3, confirmed.**

---

## The prompt to give Claude Code

> I'm building an augmented-reality LEGO builder for Meta Quest 3 in Unity 6 on a Mac (Apple Silicon). Read `Phase-0-and-0.5-Technical-Spec.md` and `AR-LEGO-Feasibility-and-Architecture.md` in this repo — they are the source of truth for scope. Build strictly to Phase 0 and Phase 0.5; treat everything in the "out of scope" section as off-limits for now.
>
> Confirmed decisions: Unity 6 + Meta XR All-in-One SDK + MRUK; controllers (not hands); pre-convert the single `3001.dat` 2×4 brick to a mesh rather than writing an LDraw parser; hand-author the 2×4's 8 stud / anti-stud connection points rather than parsing LDCad metadata. Snap/constraint model — no physics on placed bricks.
>
> Work in this order and stop for me to test on-device at each checkpoint:
>
> 1. **Project setup.** Create/verify the Unity 6 project for Quest: Android platform, URP, Meta XR project-setup fixes applied, Meta XR + MRUK packages present. Implement `Units.cs` first (LDU↔Unity: ×0.0004, flip Y; constants for 20 LDU stud pitch and 24 LDU brick height) with a tiny test that asserts a 2×4 is 16×32 mm at 9.6 mm tall.
> 2. **Phase 0 — pipeline.** Enable color passthrough; use MRUK to find the table; place the imported 2×4 on the table at correct scale; anchor it so it stays put when I walk around. Stop — I'll deploy via MQDH and verify the Phase 0 acceptance checklist.
> 3. **Phase 0.5 — connectivity.** Implement `ConnectionPoint`, `BrickDefinition` (2×4 hand-authored), `ConnectionRegistry` (spatial hash), `SnapSolver` (query → snap-to-20-LDU-lattice + nearest-90° → ghost preview → commit), `BrickGrabbable` (controller grab/release), and `BrickSpawner`. Snapping must cover ALL valid 2×4-to-2×4 matings: centered, 1/2/3-stud offsets, and 0/90/180/270° rotations — not just the centered stack.
> 4. **Verify.** Write whatever edit-mode tests are feasible for the solver math (lattice quantization, mate enumeration, no double-occupied studs). Then I stress-test on-device by stacking ~10–20 bricks and checking it holds 72 FPS.
>
> Keep code small and readable — this is a spike to prove the concept, not production. Match the file structure in the spec's §7. Ask me before adding any dependency not already listed.

---

## What you (Liam) do at each checkpoint
- After step 2: Build & Run from Unity (or deploy the APK via MQDH), put the headset on, walk around the table — does the brick stay put at real-world scale?
- After step 3: the **son test** — hand him the headset with no instructions and see if he snaps bricks together.

## If something stalls
- Brick floating/clipping the table → MRUK surface normal or anchor offset; check `Units` Y-flip.
- Wrong size → the ×0.0004 scale or a double-applied unit conversion.
- Snaps only in the centered position → the mate enumerator isn't iterating stud offsets/rotations (spec §6.3).
- FPS dips with ~20 bricks → expected ceiling signal; note it, it informs the Phase 1 instancing work.

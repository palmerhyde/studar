# AR LEGO World on Meta Quest — Feasibility & Architecture

*A first-pass technical report. Goal: model LEGO bricks digitally, see your real room through the headset, place bricks on a real table, build models, and physically walk around them.*

Prepared for Liam · June 2026

---

## 1. The one-paragraph verdict

This is feasible with today's off-the-shelf technology, and most of the hard parts are already solved by open projects you can build on rather than invent. The LEGO part geometry and the brick-to-brick connection rules — exactly the "hinges, colors, shapes, how parts connect" data that inspired you — already exist as free, redistributable open data (the **LDraw** library plus the **LDCad shadow library**). The "see your room and put things on a real table" half is a standard, well-documented capability of the Meta Quest 3 (passthrough + scene understanding). The genuinely novel work is the bit in the middle: a real-time, headset-friendly building engine that takes that part data, renders thousands of bricks at 72+ FPS, and lets you snap them together with your hands in mixed reality. That is a substantial but bounded engineering project, not a research moonshot.

A useful mental model: **BrickLink Studio, but the canvas is your living room and the camera is your head.** Studio (also called StudIO; its native files use the `.io` extension, which is likely where the "BrickIO" name comes from) is itself just the open LDraw data with a connectivity layer and a renderer on top. You would be rebuilding that same stack against a different rendering and input target.

---

## 2. How Studio / StudIO actually works (and why that's good news)

It helps to see what you're really admiring when you admire Studio, because it de-mystifies the project.

Studio is not a monolithic piece of magic. It is three layers stacked on each other:

**Layer 1 — Part geometry (open).** Every LEGO part's 3D shape comes from the **LDraw** standard, a fan-built open library that has been maintained since 1995. As of early 2026 it contains roughly **16,500+ unique part shapes**, each stored as a human-readable `.dat` text file describing the part out of triangles, quads, lines, and references to sub-parts. Crucially, the official library is released under the **Creative Commons Attribution License v2.0 (CCAL)** — you are explicitly free to copy, redistribute, and use these parts in your own software, provided you give attribution. This is the single most important fact in this report: *the part catalog you want is already free and legally usable.*

**Layer 2 — Connectivity (open, separate).** Plain LDraw `.dat` files describe what a part *looks* like but not how it *connects*. The connection rules live in a separate companion project, the **LDCad shadow library** — a set of "patch" files that get overlaid onto the LDraw parts at load time and add the snapping metadata. This is the data that gives a digital brick its "clutch power." The metadata is expressed as a small set of meta-commands:

- `SNAP_CYL` — cylindrical connections (studs, anti-studs, pins, axle holes). The workhorse; describes a stud or hole as a cylinder with a *gender* (Male/Female), a radius, and a position/orientation. This is how a stud knows it fits into the underside of the brick above it.
- `SNAP_FGR` — "fingers," the interlocking comb-like shapes used by **hinges**, click-hinges, and other articulated parts. Exactly the hinge behavior you mentioned.
- `SNAP_GEN` — "generic," for oddly shaped one-off connections (electric plugs, window glass, etc.) that only mate with a named counterpart.
- Plus grid metas that describe the regular stud array on top of a brick (e.g. a 2×4 grid) instead of listing 8 individual studs.

Connections match by *gender and type*: a male cylinder of a given radius snaps to a female cylinder of the same radius; fingers only test against other fingers in the same group. That simple rule set is what produces the feeling of bricks "wanting" to click together.

**Layer 3 — Renderer + UI + connectivity solver (proprietary to each app).** Studio, LeoCAD, Mecabricks, Bricksmith, and LDCad itself are all different *Layer 3* implementations sitting on the same Layer 1 + Layer 2 foundation. Studio added its own polished renderer, collision data, and a part-designer tool for authoring connectivity, and wraps the result in the `.io` file format. **This top layer is the part you would be rewriting for VR.** You are not reverse-engineering Studio; you are writing a new sibling to it.

The strategic conclusion: **build on LDraw + LDCad directly, not on Studio's `.io` files.** You get the same data Studio gets, with a clear open license, in a documented text format, with no dependence on a closed app. (Studio's connectivity additions are inside its own app and not separately licensed for reuse — another reason to go to the open source.)

---

## 3. The Meta Quest mixed-reality half

The "augmented world you can walk around in" is the most mature part of the whole stack, because Meta has productized it.

**Passthrough.** The Quest 3 (and Pro) render a real-time color video view of your actual room through the headset cameras. Your virtual bricks are then composited on top. This is a standard API — you are not building computer vision from scratch.

**Scene understanding / table detection.** Meta's **Mixed Reality Utility Kit (MRUK)** sits on top of the raw scene system and hands you semantic objects: floor, walls, ceiling, *tables*, couches, and other furniture, as tracked planes and volumes. There is literally a built-in helper for "find the largest table" and "find the closest surface." So "put the bricks on a real table" becomes: ask MRUK for table surfaces, and use one as your build plate. Practically, the user does a one-time room scan (Quest's built-in Space Setup), after which your app gets the table geometry for free.

**Anchors.** *Spatial anchors* pin your model to a fixed real-world location so that when you walk around the table, the model stays put and you orbit it — exactly the "walk around what I built" experience. Anchors can also persist across sessions, so a model left on your table is still there tomorrow.

**Hands and controllers.** You get both hand-tracking and controller input. Controllers are more precise for fine placement (good for a v1); hands are more magical and more in keeping with "picking up a brick," but pinch-precision at small scales is a real UX challenge. Sensible plan: ship controllers first, add hands later.

The honest caveat on this half: passthrough is good but not perfect — there's some visual softness and latency, and small real objects can occlude awkwardly. None of this blocks the project; it just sets expectations about photorealism.

---

## 4. The hard middle: a brick engine that runs on a headset

This is where the real engineering lives, and where your project differs most from desktop Studio. A desktop app rendering a 5,000-brick model can lean on a powerful GPU and 60 FPS. A Quest must hold **72 FPS minimum** (90 preferred) on a mobile chip, rendering everything *twice* (once per eye), or the experience induces discomfort. Three sub-problems:

**4.1 Rendering thousands of near-identical bricks.** This is, fortunately, the *best possible case* for a GPU technique called **instancing**. A LEGO model is the same handful of part shapes repeated hundreds of times in different positions and colors. Instead of issuing one draw call per brick (which would blow the budget — mobile VR wants to stay roughly in the low hundreds of draw calls per frame), you issue *one* draw call per unique part shape and hand the GPU a list of positions and colors. A model made of 2,000 bricks but only 30 distinct part types can potentially render in ~30 draw calls. This single technique is what makes large models viable on the headset. Supporting tactics: a shared texture atlas for the ~50–60 standard LEGO colors, aggressive level-of-detail (LDraw parts have lots of tiny geometric detail on studs that's invisible from across the room), and merging static/finished sub-assemblies into baked meshes.

**4.2 The connectivity solver in real time.** When the user moves a brick near another, the app must, every frame, find nearby compatible connection points (a male stud near a female anti-stud of matching radius) and snap. With thousands of potential connection points you can't check every pair — you need a spatial index (a uniform grid or hash, which is natural because LEGO connections fall on a regular ~8mm/20-LDU lattice). The LDCad metadata gives you the connection points; you supply the fast lookup and the snap logic. Hinges (the `SNAP_FGR` fingers) add rotational degrees of freedom and are meaningfully harder than plain stud-stacking — reasonable to defer past v1.

**4.3 Should bricks use physics?** Two philosophies. **Constraint/snap-based** (like Studio): bricks don't fall; they snap to a logical structure and stay. Simpler, stable, matches how CAD users think, far cheaper on the CPU. **Physics-based** (like the LEGO VR games): bricks have weight and can be knocked over. More magical, much harder to keep stable and performant with thousands of parts. Recommendation: **snap-based for the core builder**, with optional physics only on the single brick currently in your hand. This mirrors what Studio does and what keeps frame rate safe.

---

## 5. Recommended technology stack

| Concern | Recommendation | Why |
|---|---|---|
| Part geometry | **LDraw official library** (`.dat`, CCAL-licensed) | Free, redistributable, ~16.5k parts, documented text format |
| Connectivity / hinges | **LDCad shadow library** meta-commands | The open source of "clutch power" and hinge data |
| Engine | **Unity 6 + Meta XR SDK** (or Unreal 5) | Meta's first-class MR support, MRUK, huge sample base; Unity is the well-trodden path for Quest |
| MR features | **MRUK** (tables/planes), passthrough API, spatial anchors | Productized scene understanding; "find largest table" out of the box |
| Input | Controllers first → hand-tracking later | Controllers are precise enough for v1; hands are the v2 wow factor |
| Brick rendering | GPU instancing + color atlas + LOD | The key to holding 72 FPS with large models |
| Brick logic | Custom snap solver over a spatial hash | The genuinely novel code you'd own |

A note on engine choice: Unity has the deeper Quest ecosystem, more samples, and AR Foundation/Meta XR maturity, so it's the lower-risk default. Unreal is viable and arguably prettier but a rockier road on mobile VR. Either way, **you are not starting from a blank page** — Meta ships passthrough and MR sample projects you can fork.

---

## 6. Key risks & unknowns, ranked

1. **Performance ceiling on large models (highest).** Instancing makes this tractable, but there is a real model-size limit on a mobile GPU. *Mitigation:* prove the instanced renderer with a 2,000-brick model early; that test alone tells you most of what you need to know about scope.
2. **LDraw → engine import pipeline.** Converting 16.5k `.dat` files (with their sub-part references and non-standard winding/geometry quirks) into clean, instanceable engine meshes is fiddly. *Mitigation:* don't import all 16.5k up front — start with the ~50 most common bricks; existing open LDraw importers exist to learn from.
3. **Connectivity correctness, especially hinges.** Stud-on-stud is straightforward; articulated parts are not. *Mitigation:* scope v1 to rigid stud connections only.
4. **Hand-tracking precision at brick scale.** Pinching a 1×1 plate in mid-air is hard. *Mitigation:* controllers first; consider a "scale up the workspace" mode.
5. **Passthrough visual quality / occlusion.** Sets expectations rather than blocking. *Mitigation:* design around it (bricks float slightly above the detected table plane).
6. **Trademark/IP.** LDraw data is CCAL and fine to use; "LEGO" is a trademark, so any public release must avoid implying official affiliation and follow LEGO's fair-play naming guidance. *Mitigation:* relevant only at distribution time, not for a personal prototype.

---

## 7. Suggested phased approach

This isn't a committed roadmap — it's the lowest-risk ordering to learn the most, fastest. Each phase ends with a question answered.

**Phase 0 — De-risk the renderer (days, not weeks).** In Unity, get a single LDraw brick importing and rendering in passthrough on the Quest, anchored to a detected table via MRUK. *Answers: does the basic MR + LDraw pipeline work end to end?*

**Phase 0.5 — De-risk connectivity with one known part.** Take a single common part — the **2×4 brick** — and prove the core magic: spawn several copies on the real table and snap them to *each other*, stud-to-anti-stud, with the right alignment and offset. This deliberately isolates the snap solver from the import pipeline. You hand-define (or import just this one part's) connection points from the LDCad metadata, then build the spatial lookup and snap logic against that single, well-understood geometry. Picking up a 2×4 and feeling it click onto another 2×4 — correctly, every time, in any of its valid positions — is the whole project in miniature. *Answers: does the connectivity model actually work before I invest in generalizing it?*

**Phase 1 — Generalize & scale.** Now that snapping works for one part, extend it to ~50 common parts via the real LDraw import pipeline, add instanced rendering, and place/stack a mix of part types with a controller. Push to ~1,000–2,000 bricks and watch the frame rate. *Answers: does the snap model hold up across different part shapes, and what's the realistic model-size budget?*

**Phase 2 — Build experience.** A brick/color palette UI, delete/move/undo, save & reload a model via persistent anchors, and the "walk around it" orbiting test. *Answers: is it actually pleasant to build in?*

**Phase 3 — Reach features.** Hand-tracking, hinges/articulated parts, the full part catalog, importing existing `.io`/`.ldr` models, sharing. *Answers: how close to "Studio in AR" can it get?*

---

## 8. Bottom line

The two halves you're most excited about — *the rich brick data* and *walking around your build in your real room* — are the two halves that are already solved and freely available. The brick data is open (LDraw + LDCad, CC-licensed, the exact source Studio itself draws from). The room-scanning and table-detection is a productized Meta Quest feature (passthrough + MRUK). Your real invention, and the thing worth prototyping first, is the headset-grade building engine in the middle: instanced rendering plus a real-time snap solver. Build a Phase 0 spike against LDraw + Unity + MRUK and you'll very quickly know how big the real project is.

---

### Sources

- LDraw library, license & format: [LDraw.org Legal Info](https://www.ldraw.org/legal-info), [Parts Library Specs](https://www.ldraw.org/article/512.html), [LDraw on Wikipedia](https://en.wikipedia.org/wiki/LDraw), [LDraw Library](https://library.ldraw.org/)
- Connectivity / snapping metadata: [Part Snapping Language Extension (LDraw Wiki)](https://wiki.ldraw.org/wiki/Part_Snapping_Language_Extension), [LDCad Shadow Library (melkert.net)](https://www.melkert.net/LDCad/tech/shadowLib), [LDCadShadowLibrary (GitHub)](https://github.com/RolandMelkert/LDCadShadowLibrary)
- Studio / StudIO & the `.io` format: [BrickLink Studio (Wikipedia)](https://en.wikipedia.org/wiki/BrickLink_Studio), [Studio import formats](https://studiohelp.bricklink.com/hc/en-us/articles/6502277722647-Import-formats), [Parts packs for Studio (philohome)](https://www.philohome.com/studio/packs.htm)
- Quest mixed reality: [Passthrough API overview](https://developers.meta.com/horizon/documentation/unity/unity-passthrough/), [Mixed Reality Utility Kit](https://blog.learnxr.io/xr-development/mixed-reality-utility-kit-for-unity), [Get started with Quest 3 + Unity](https://unity.com/blog/engine-platform/get-started-developing-for-quest-3-with-unity)
- Performance / rendering: [Optimizing Draw Calls in Unity for Mobile VR](https://pinkcrow.net/development/optimizing-draw-calls-in-unity-for-mobile-vr/), [Meta testing & performance docs](https://developers.meta.com/horizon/documentation/unity/unity-perf/)
- Prior art (VR LEGO): [LEGO Bricktales on Meta Quest](https://www.meta.com/experiences/lego-bricktales/6521909757843713/), [LEGO BrickHeadz Builder VR (Schell Games)](https://schellgames.com/portfolio/lego-brickheadz-builder-vr)

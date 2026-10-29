# Lisztomnezia — environment, lighting, and atmosphere implementation prompt

You are working in `D:\GamedevRepo\Lisztomnezia`. Perform a substantial, cohesive visual polish pass on the existing exploration environment. Act as an environment artist, lighting artist, and technical artist. Deliver working, saved Unity content and verify it visually. Start with a short plan, then implement in stages; do not stop at suggestions.

## Creative direction

Make this illustrated woodland feel damp, decaying, uncanny, and quietly alive. Preserve its hand-drawn sprites, ink-like outlines, rabbits, strange vegetation, crystals, and ruined pillars. Aim for an unsettling storybook forest with tactile grime and a few beautiful, disturbing glints. Keep the existing artistic identity coherent across ground, vegetation, fog, lighting, and particles.

Give the player a readable route through layered darkness. Compose areas of relative visual quiet against clusters of rotten detail. Use wet earth, fungal growth, dirty pooled moisture, floating spores, occasional insects, and restrained crystal highlights. Let the environment become more oppressive near encounters. Every effect should have a location, a material reason, or a narrative purpose.

## Verified project context

The following was inspected on 2026-10-29. Recheck the working tree and live scene before changing anything:

- Unity `6000.3.11f1`; URP package `17.3.0`. The GraphicsSettings pipeline reference resolves to `Assets/SmnStyleHardline/Core/Runtime/Graphics/URPSettings/SSH_RendererAsset.asset`. Check QualitySettings and the live pipeline too. `Assets/_Project/Presets/Rendering/GlobalURP.asset` is a global settings asset, not the active pipeline asset.
- Main project scenes are `Assets/_Project/Scenes/{Bootstrap,Logo,Game,Osu}.unity`. Exploration lives in `Game`; `Osu` is an additive rhythm encounter. Use Bootstrap for an end-to-end playthrough. Preserve scene order and loading contracts.
- The environment uses many SpriteRenderers and `Assets/_Project/Scripts/Common/SpriteBillboard.cs`. It turns sprites toward Camera.main within 25 units. Preserve the billboard presentation and account for camera-facing normals when designing lighting, surface effects, and shadows.
- Existing illustrations are in `Assets/_Project/Sprites/environment`: trees and crowns, bushes, grasses, logs, stumps, fences, broken pillars, stones, flowers, six mushroom images, and four crystal images. Reuse this visual vocabulary.
- `Assets/_Project/Materials/Sprite-Lit.mat` references Sprite Shaders Ultimate's `3D Lit URP SSU` shader. Serialized values include metallic 1, smoothness 0.2, transparent surface, and ZWrite off. Treat these as investigation points, not proof that changing one slider will improve the scene. Identify which objects actually share it before editing.
- `Terrain-Lit.mat` references SSU's `Standard SSU` shader. Inspect actual ground renderers and material assignments before choosing the ground solution.
- A recent, untracked `Assets/_Project/Materials/AtmosphericPath.shader` and matching material already exist. The shader provides world-space soil/moss noise, worn tracks, darker damp patches, and a small main-light highlight. It uses the transparent queue, disables depth writes, includes a minimum illumination floor, and does not explicitly accumulate additional lights. Check these choices in the actual scene before adding competing ground overlays or point lights.
- `Assets/_Project/Presets/Rendering/GameProfile.asset` already contains Bloom, ColorAdjustments, WhiteBalance, Vignette, DepthOfField, FilmGrain, ChromaticAberration, LensDistortion, and ShadowsMidtonesHighlights. Serialized values include exposure -0.65, saturation -42, bloom intensity 0.45, grain intensity 0.32, and vignette intensity 0.42. Verify camera masks, active volumes, and runtime overrides before tuning. Keep `HypnoProfile.asset` and menu presentation independently appropriate.
- Game scene YAML contains a directional light at intensity 2 and two fog objects named `Terrain_Fog_FX_v2_Tip_1` and `_2`. Built-in RenderSettings fog is serialized off; particle fog is a separate system.
- `Assets/FogPlayerAvoid/_Code/Scripts/VFX/ParticleSystem_FogAvoidPlayer.cs` updates fog shader properties through MaterialPropertyBlock. Encounters redirect its viewer with `NewView` and restore it with `ReturnView`. Preserve that behavior.
- `Assets/_Project/Scripts/Game/Effects/OutlineMover.cs` and `NarrativeOutlineDirector.cs` already control narrative outline color and motion. Coordinate with them; inspect current call sites rather than assuming every available preset is used.
- Existing recent effects include `RabbitAbsorption.cs`, `RabbitAbsorption.shader`, and `RabbitBurn.shader`. Preserve their appearance and timing, along with diary/organ sketch work.
- Imported resources include `Assets/Hovl Studio/Background VFX/Prefabs` (Clouds, Sparkles, Blood, Side fumes, and others), `Assets/Vefects/Stylized VFX`, and `Assets/FogPlayerAvoid`. These are candidate components to adapt, not effects already proven suitable for this scene. Inspect textures, shaders, scale, and cost before reuse.
- The inspected pipeline asset enables HDR, depth and opaque textures, additional lights, and shadows. It also specifies a 300-unit shadow distance. Measure actual cost and effect before changing global settings or adding renderer features.

A camera render inspected during this audit showed a dark illustrated forest corridor, brown ground, broken pillars, bare trees, rabbits, mushrooms, and large grainy hands in the foreground. The ground appeared relatively even, the canopy merged into black, and depth separation was limited. These are observations from one camera-only image, not a full gameplay review or an overlay-UI capture. Inspect multiple positions in motion before deciding what to fix.

## Work sequence

### 1. Establish the baseline

Read AGENTS instructions, current source, relevant README sections, and `git status`/diffs. The repository already has substantial uncommitted work; preserve it. README contains older descriptions superseded by newer notes and source.

Inspect the running Editor through Unity CLI, explicitly targeting this project because another Unity project may be open. Preserve unsaved scene work and the user's current Editor/play state. Take baseline screenshots from repeatable player viewpoints: entry, a typical corridor, a rabbit interaction, the diary view, and the rhythm encounter approach. Record camera positions and basic performance measurements under comparable conditions.

Determine what really renders: cameras, volume masks, materials, shader passes, outline features, fog, lighting response, transparency order, ground geometry, and any runtime-created effects. Identify the highest-impact weaknesses before adding content.

### 2. Establish depth and lighting

Build a readable hierarchy of foreground silhouettes, a lit playable middle distance, and a background that dissolves gradually into atmospheric color. Keep important illustrated contours visible. Use a restrained cold ambient palette with selected warmer or sickly accents where the scene supports them.

Give encounters and landmarks deliberate pools of light or value contrast. Make mushrooms and crystals contribute small, localized accents. Verify that the chosen sprite and ground shaders respond to those lights. Use shaped emissive masks, compatible materials, or subtle supporting geometry when appropriate. Keep shadows compatible with flat sprites; reject obvious rectangular shadows and lighting that exposes the cards.

Tune exposure, bloom, grading, grain, and depth of field together in actual gameplay. Maintain legibility of rabbits, hands, diary marks, dialogue, and rhythm targets. Preserve darkness with recoverable detail and clear focal points.

### 3. Make the ground and objects tactile

Develop the existing ground treatment with irregular damp patches, softened path edges, moss, dirt accumulation, leaf litter, and localized dark stains. Vary size and density in authored clusters. Integrate sprite bases with subtle contact darkening or compatible ground detail so plants and pillars feel anchored.

Create selective wet highlights on puddles, slime, stones, and suitable crystal facets. Use masked highlights and roughness variation rather than making the shared vegetation material uniformly metallic. Ground grime belongs on the ground: ensure any decal technique actually works with the active renderer, shader, and transparent sprites; use mesh patches or sprite details where more reliable. Avoid z-fighting, visible overlay edges, and repeating procedural patterns.

### 4. Add atmosphere with a clear distribution

Layer low drifting fog into hollows and between depth planes while preserving the existing player/encounter clearance. Add sparse spores around fungi, small insect clusters near rot, rare drifting debris, and occasional glints near crystals or wet surfaces. Use different speeds, sizes, lifetimes, and spatial concentrations.

Adapt existing particle textures and prefabs where suitable. Remove demonstration framing and tune their color, blending, density, and scale to the forest. Keep screen coverage restrained and the air readable during movement. Check soft intersections, sorting, clipping, and cumulative transparent overdraw. Limit particles and lights by distance or visibility where useful.

### 5. Add localized rot and narrative progression

Compose a few memorable dirty areas: a fungal stump, a damp pillar base, a polluted depression, or a patch of unhealthy growth. Make each fit the existing illustrations. Build unease through material, placement, and subtle movement; decide where organic stains or blood actually support the scene.

Differentiate ordinary traversal, rabbit/diary encounters, and the approach to the rhythm challenge. Where current gameplay exposes suitable transitions, coordinate small changes in fog, local light, particle motion, or emission with the existing outline direction. Restore the calm state reliably after each encounter. Use existing timings and hooks; do not introduce a new progression system for visual polish.

## Implementation constraints

- Keep changes focused and reviewable. A broad visual result can be built from small scene, material, prefab, and script changes.
- Prefer existing dependencies and art. Ask before new dependencies, purchases, or large refactors. Do not migrate render pipelines or upgrade packages as part of this pass.
- Use the connected Editor for scene/asset authoring, with proper Undo/dirty/save handling. Avoid manual serialized-scene surgery when Editor APIs are available.
- Create project-owned variants before altering imported vendor assets or widely shared materials. Keep new assets under a coherent `_Project` location and retain Unity metadata.
- Save durable content outside Play Mode. Do not leave the entire result as temporary runtime objects.
- Preserve colliders, traversal, triggers, localization, dialogue, diary drawing, rabbit absorption/burn, FMOD, rhythm timing, menu layers, camera transitions, and pause/resume behavior.
- Use scaled time for gameplay atmosphere unless a specific existing presentation requires otherwise. Confirm gameplay VFX freeze and resume correctly while the live pause menu continues its intended animation.
- Avoid per-frame material instantiation, needless allocations, and uncontrolled particle/light counts. Do not claim a frame-rate target without hardware, resolution, and measurement evidence.

## Verification and completion

Work through the pass visually: implement a representative section, compare it from the same cameras, refine it, then extend the established treatment across the route. Look for improvement in composition, depth, material contrast, atmosphere, and environmental storytelling.

Check compilation and relevant existing tests if scripts change. Play from Bootstrap through exploration, rabbit interaction, diary, and the rhythm encounter where feasible. Test repeated encounters and pause/resume for restored fog, lighting, particles, and outlines. Inspect camera motion for sprite sorting, shadow artifacts, particle popping, clipping, and ground seams.

Compare before/after screenshots at matching positions and comparable performance samples. Investigate regressions with the Profiler/Frame Debugger where available. Verify scene and asset saves, inspect the final diff, and leave unrelated work untouched. State what was actually tested and what remains unverified.

Deliver the implemented changes, a concise plain-English explanation, before/after evidence, and measured performance findings. The finished forest should feel visibly richer, wetter, dirtier, more layered, and more unsettling while retaining its distinctive illustrated identity and playable clarity.

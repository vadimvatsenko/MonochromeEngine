# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

Monochrome Engine — a 2D game/rendering engine built from scratch in pure C# that renders to the Windows console as a grid of characters (`char[,]`). No game engine framework (Unity/MonoGame/etc.) is used; the "renderer" is a `Console.Write` of a composited character buffer. Sprites are authored as PNG files and converted to ASCII bitmasks at load time (see `SpritesLoaderSystem.LoadCharMask2D`) rather than drawn as pixels — a sprite pixel becomes two `█` (or space) characters side by side to approximate a square aspect ratio in the console.

There is no `.sln` — `MonochromeEngine.csproj` (net10.0, `OutputType=Exe`, `Nullable` enabled) is the only project, and it's a Windows-only console app (uses `user32.dll` P/Invoke for input and `winmm.dll` for audio).

## Build & run

```
dotnet build
```

Running matters more than usual here: `SpritesLoaderSystem` and `ColoredSpritesLoader` locate art via a **hardcoded relative path** `Path.Combine("..", "..", "..", "Sprites")`. This only resolves correctly when the process's working directory is the build output folder (`bin/Debug/net10.0/`), three levels below the project root where `Sprites/` lives. `dotnet run` sets the working directory to the *project* directory by default, which breaks sprite loading — so run the built executable directly from `bin/Debug/net10.0/MonochromeEngine.exe`, or `dotnet run` from inside that output directory, not `dotnet run` from the project root.

There are no test projects and no lint config in this repo.

The app runs as an infinite blocking game loop (`Update.RunUpdate`, fixed timestep, target 60 FPS) that owns the console — don't expect it to exit on its own.

## Architecture

**Game loop → SceneManager → IScene.** `Program.cs` wires up the singletons (`Input`, `SpritesLoaderSystem`, `Map`, `MonoRenderer`, `LayerManager`, `SceneManager`) and pushes a `GameplayScene`, then hands control to `Update.RunUpdate()`, which each frame calls `SceneManager.Update(dt)` then `SceneManager.Draw()`. `SceneManager` is a stack of `IScene` (`PushScene`/`PopScene`/`ChangeScene`); only the top scene updates/draws. A scene (`GameplayScene`) owns its own `List<IUpdatable>` and `List<IRenderable>` of entities and drives them manually — there's no global entity registry.

**Layers, not z-order.** Rendering is layer-based rather than sorted-by-depth: `LayerManager` pre-allocates one `char[,]` buffer per `RenderLayerType` (Background, Ground, Items, Enemies, Player, UI). Entities are constructed with a reference to *one specific layer's buffer* and draw only into it. Every frame, `SceneManager.Draw()` clears all layers except Background, scene objects draw into their assigned layer, then `MonoRenderer.Compose()` flattens the ordered list of layers into one frame (later layers' non-space chars win) and `Render()` writes it to the console in one buffered `Console.Write`.

**Entities: `GameObject` base + composition, not inheritance-heavy.** `Engine/BaseObjects/GameObject.cs` is the base for creatures/blocks (`Hero`, `Bat`, `Skeleton`, `GroundBlock`). It wires up a `BoxCollider2D`, a `CollisionSystem`, and a `BaseAnimator` bound to the entity's name (animations are looked up by matching the entity's `name` string against `SpritesLoaderSystem`'s sprite map — the name passed to the constructor must match a folder under `Sprites/`). Subclasses implement `IUpdatable`/`IRenderable` themselves; combat-capable entities implement `ITarget` (`TakeDamage`). There's no shared base for AI/physics beyond what each subclass hand-rolls (see `Skeleton` for patrol/attack/ledge-detection logic, `Hero` for coyote-time + jump-buffer platforming).

**Collision** is AABB-only (`BoxCollider2D` inside `Engine/Collider/`). `CollisionSystem.ResolveCollisions` resolves X and Y movement against `Map.Blocks` (all `GroundBlock`s) as two separate passes per axis, sets `IsGrounded`, and zeroes velocity on impact — used for both platforming physics and (via `CombatCollider` + `CheckCombatIntersection`) melee hit detection (e.g. `Skeleton` vs `Map.Hero`).

**Sprite loading convention.** `SpritesLoaderSystem` walks `Sprites/<Category>/<OwnerName>/<AnimName>/<Variant>/*.png` (e.g. `Sprites/Enemies/Skeleton/Attack/Base/*.png`), builds one `Sprite` per `OwnerName` keyed case-insensitively, and concatenates `AnimName + Variant` (e.g. `"Attack" + "Base"` → `"AttackBase"`) as the animation key. Frame files within a variant folder are ordered by filename, so animation frame order depends on file naming/sorting. `BaseAnimator` looks up fixed animation names off that map (`IdleBase`, `MoveBase`, `AttackBase`, `DeadBase`→`DeathBase`, `JumpBase`, `FallBase`, `HitBase`, `BowBase`) — adding a new state requires both a matching folder structure under `Sprites/` and a corresponding property/lookup in `BaseAnimator`.

**Two rendering paths exist side by side.** `MonoRenderer`/`IRenderer`/`char[,]` is the one actually wired up in `Program.cs` and used everywhere (`GameObject`, `LayerManager`, `SceneManager` are all hardcoded to `MonoRenderer`/`char[,]`, not the `IRenderer` interface). `ColoredMonoRenderer`/`ConsolePixel[,]` (per-cell `ConsoleColor`) and `ColoredSpritesLoader`/`ColoredSprite` are a parallel, currently-unused implementation for color support — don't assume they're on the active path.

**Levels** are authored as `int[,]` tile-ID grids in `Engine/Levels/LevelModel.cs` (e.g. `Map11`); `GameplayScene.Initialize()` iterates the grid and spawns a `GroundBlock` per non-zero cell, using `tileId - 1` as the static sprite frame index into the block's `IdleBase` animation frames.

**Audio** (`Engine/Audio/RetroSoundEngine.cs` + `WinMMAudio.cs`) is a from-scratch NES-style 5-channel chiptune synthesizer (pulse/triangle/noise waveform generation, raw PCM playback via `winmm.dll`), with hardcoded note-sequence "scores" for background tracks — not currently hooked into the scene/game loop.

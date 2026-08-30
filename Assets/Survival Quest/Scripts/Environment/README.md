# Procedural Environment

`ProceduralEnvironmentGenerator` provides a safe baseline environment dressing system using Unity primitives.

## Setup

1. In Unity, create an empty GameObject named `EnvironmentGenerator`.
2. Add `SurvivalQuest.Environment.ProceduralEnvironmentGenerator`.
3. Assign optional materials for foliage, water, and rocks.
4. Use the component context menu and choose **Generate Environment**.
5. Tune `worldSize`, tree/bush/rock counts, mountain settings, and river/waterfall settings.

The generated objects are grouped under `__ProceduralEnvironment` and can be cleared using **Clear Generated Environment**.

## Art direction

The generator intentionally avoids hard dependencies on external asset packs. Replace the generated primitive objects with authored low-poly/high-quality prefabs later while retaining the same high-level grouping and placement logic.

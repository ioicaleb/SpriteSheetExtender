# SpriteSheetExtender
### A C# / .NET Game Modding Utility for Stardew Valley (SMAPI)

An open-source game modification and asset tool built with **C#** and the **SMAPI** (Stardew Valley Modding API) ecosystem. This utility extends the limit of sprite indices for player characters.

## Key Architectural Features
* **Asset Lifecycle Hooks:** Intercepts and overrides the native XNB asset pipeline using SMAPI’s asset loading protocols to smoothly swap sprites on the fly.
* **OOP Framework:** Utilizes robust C# object-oriented design patterns to handle custom asset tracking and state management cleanly.
* **Optimized Data Pipeline:** Efficiently maps, scales, and parses user-defined configurations to maintain consistent frame pacing and prevent memory leaks.

## Built With
* **Language:** C# / .NET
* **Framework:** SMAPI (Stardew Valley Modding API)
* **IDE:** Visual Studio

## Repository Structure
* `/src` — Core C# source logic executing asset loading hooks.
* `/assets` — Internal structural files and configuration mappings.

## How It Works (Code Insight)
The tool subscribes to SMAPI's core asset loading events, verifying if incoming game request paths match targeting indices for the player's sprite configurations, safely injecting custom multi-layer sprites sheets without breaking native state architecture.

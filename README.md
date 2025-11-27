# Card Matching Game Prototype

![Unity](https://img.shields.io/badge/Engine-Unity%202021%20LTS-black?logo=unity)
![Platform](https://img.shields.io/badge/Platforms-Windows%20%7C%20Android-blue)
![C#](https://img.shields.io/badge/Language-C%23-green)

A lightweight, fully functional **Card Matching Game** prototype built from scratch using **Unity 2021 LTS**, with clean architecture, animations, save/load persistence, dynamic grid layout, sound effects, and combo-based scoring.

---

## Features

### Gameplay

- Smooth **card flip animation** and **match effect**
- Fully supports **continuous flipping** (no waiting for animations)
- Multiple grid layouts supported:
  - `2x2`, `2x3`, `4x4`, `5x6`, etc.
- Dynamic auto-scaling cards — fits all device screens

---

## Save & Load System

Automatic save every **5 seconds**.

### Saved Data:

- Timer
- Score
- Combo
- Turn counter
- Shuffled card order
- Matched/unmatched card status

### Auto-Prevent Load When:

- Timer has expired
- All cards were matched (completed game)
- Save is invalid or incomplete

---

## Scoring System

- +10 base score per match
- **Combo multiplier** increases score for consecutive matches
- Turn counter included

---

## Sound Effects

Includes required SFX:

- Flip
- Match
- Mismatch
- Win
- Game Over

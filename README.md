# fire-training-unity

A VR fire safety training simulation built in Unity 6. Made this to get my hands on Unity and C#, messing with physics, particle systems, XR toolkits, and building actual interactive gameplay from scratch.

## What it is

You're placed inside a school classroom. A fire breaks out on the teacher's desk. Your job is to walk out to the hallway, grab the fire extinguisher, come back, and put it out before it regenerates.

It runs without a VR headset, uses keyboard and mouse to simulate movement and interaction.

## Controls

| Key | Action |
|-----|--------|
| W A S D | Move |
| Mouse | Look around |
| E | Pick up / drop fire extinguisher |
| G (hold) | Spray foam |
| Escape | Unlock cursor |

Click the game view first to capture the mouse, then you're good to go.

## How to run it

1. Open Unity Hub and add the project folder
2. Open the scene at `Assets/Scenes/VRFireTraining.unity`
3. Hit Play

Unity version: **6.5 (6000.5.6f1)**

## What's in it

- Classroom environment with desks, chairs, whiteboard, and a hallway
- Multi-layer fire particle system (flames, embers, rising smoke) with flickering point light
- Fire health system, the fire regenerates if you stop spraying
- Extinguisher pickup with foam particles and raycast-based damage
- Tutorial HUD that walks you through each phase
- Smooth WASD + mouse movement using Unity's New Input System
- XR Interaction Toolkit setup so it can be extended to actual VR hardware later

## Project structure

```
Assets/
  Scenes/         VRFireTraining.unity
  Scripts/        All gameplay C# scripts
  Materials/      PBR materials
  Textures/       Procedurally generated particle textures
```

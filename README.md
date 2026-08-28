# Scary Islands

Scary Islands is a room-scale VR survival-horror game built with Unity and OpenXR. The first playable island, **Widow's Shore**, asks the player to recover and ring the Salt Bell before the black tide consumes the island while a sound-hunting creature called the Mourner stalks the fog.

## Prototype loop

1. Arrive by skiff and take the lantern.
2. Follow three audio beacons through the fog.
3. Collect the chapel key and Salt Bell.
4. Ring the bell at the drowned chapel.
5. Return to the skiff before the tide timer expires.

## Unity setup

- Unity 6 LTS / URP
- OpenXR + XR Interaction Toolkit
- Targets: Meta Quest (Android) and PC VR
- Comfort defaults: snap turn, vignette, teleport, seated/standing calibration

Open the project in Unity, allow packages to resolve, then run **Scary Islands > Build Prototype Scene**. This creates a playable greybox scene using primitives and the included runtime scripts.

## Cloudflare backend

`Backend/worker` is a small Worker + D1 API for anonymous player profiles and leaderboard submissions. It is intentionally optional: the prototype remains fully playable offline.

## Design

The editable experience boards live in [Figma](https://www.figma.com/design/PehL1aTcjaLH9YrAskwXJI).

## Planned maps

Widow's Shore, Abyssal Sea, Dead Orbit, Whisper Caves, Red Waste, Blood Canopy, Shattered Sky, and Slime Lands. Runtime metadata is stored in `Assets/ScaryIslands/Resources/Biomes.json` so map selection and unlock requirements remain data-driven.

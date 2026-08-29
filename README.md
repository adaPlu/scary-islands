# Scary Islands

Scary Islands is a room-scale VR survival-horror game built with Unity and OpenXR. The first playable island, **Widow's Shore**, asks the player to recover and ring the Salt Bell before the black tide consumes the island while a sound-hunting creature called the Mourner stalks the fog.

## Prototype loop

1. Arrive by skiff and take the lantern.
2. Follow three audio beacons through the fog.
3. Collect the chapel key and Salt Bell.
4. Ring the bell at the drowned chapel.
5. Return to the skiff before the tide timer expires.
6. Earn **Dots** and spend them on companion pets between runs.

## Unity setup

- Unity 6 LTS / URP
- OpenXR + XR Interaction Toolkit
- Targets: Meta Quest (Android) and PC VR
- Comfort defaults: snap turn, vignette, teleport, seated/standing calibration
- Body model: floating torso and tracked arms, with no rendered legs
- Wings: every player has one wing attached to each tracked arm
- Ground locomotion: pull the tracked hands backward to move
- Flight: flap both arms downward to take off, climb, and accelerate forward
- Gliding: spread both arms apart in the air to reduce gravity and glide farther
- Currency: **Dots**, stored persistently with a 100-Dot starter balance
- Run reward: successful escapes award 25 Dots
- Pet Shop: buy and equip persistent companion pets using Dots

Open the project in Unity, allow packages to resolve, then run **Scary Islands > Build Prototype Scene**. This creates the playable greybox and places a Pet Shop terminal near the starting area.

## Pet Shop

The initial catalog contains Fog Moth, Lantern Crab, Grave Crow, Mire Slime, Storm Bat, and Little Leviathan. Purchases and the equipped pet persist locally. Equipped pets follow the player using lightweight prototype geometry until final pet art is added.

## Cloudflare backend

`Backend/worker` is a small Worker + D1 API for anonymous player profiles and leaderboard submissions. It is intentionally optional: the prototype remains fully playable offline. Dots and pet ownership currently use local persistence.

## Design

The editable experience boards live in [Figma](https://www.figma.com/design/PehL1aTcjaLH9YrAskwXJI).

## Planned maps

Widow's Shore, Abyssal Sea, Dead Orbit, Whisper Caves, Red Waste, Blood Canopy, Shattered Sky, and Slime Lands. Runtime metadata is stored in `Assets/ScaryIslands/Resources/Biomes.json` so map selection and unlock requirements remain data-driven.

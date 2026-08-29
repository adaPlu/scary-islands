# Scary Islands

Scary Islands is a room-scale VR survival-horror game built with Unity and OpenXR. The first playable island, **Widow's Shore**, asks the player to recover and ring the Salt Bell before the black tide consumes the island while monsters stalk the fog.

## Prototype loop

1. Arrive with a **free starter gun** attached to the tracked right hand.
2. Shoot invincible monsters to build a Dot streak, follow the audio beacons, and recover the chapel key and Salt Bell.
3. Flap the arm-mounted wings to fly and glide around the island.
4. Ring the Salt Bell at the drowned chapel.
5. Return to the skiff before the tide timer expires.
6. Spend earned **Dots** on 10-Dot pet eggs.

## Combat and Dots

- Every player starts with a free automatic starter gun.
- Bind `StarterGun.BeginFire` and `StarterGun.EndFire` to the XR trigger.
- Monsters use `MonsterHealth` and are **invincible by default**.
- Gunfire still registers valid monster hits, but monster health does not decrease and monsters cannot die.
- A continuous monster-hit streak earns increasing Dots:
  - first full second hitting a monster: **+1 Dot**
  - second consecutive second: **+2 Dots**
  - third consecutive second: **+3 Dots**
  - and so on
- Missing/stopping long enough to break the hit streak resets the next reward to +1.
- Successful escapes still award 25 Dots.
- Dot balance persists locally.

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

Open the project in Unity, allow packages to resolve, then run **Scary Islands > Build Prototype Scene**. The greybox includes the Pet Shop and three invincible shootable monster targets.

## Pet Shop

Every pet is purchased as a **10-Dot egg**. The current eggs hatch Fog Moth, Lantern Crab, Grave Crow, Mire Slime, Storm Bat, or Little Leviathan. Purchases and equipped pets persist locally, and equipped pets follow the player using prototype geometry until final models are added.

## Cloudflare backend

`Backend/worker` is a small Worker + D1 API for anonymous player profiles and leaderboard submissions. It is intentionally optional: the prototype remains fully playable offline. Dots and pet ownership currently use local persistence.

## Design

The editable experience boards live in [Figma](https://www.figma.com/design/PehL1aTcjaLH9YrAskwXJI).

## Planned maps

Widow's Shore, Abyssal Sea, Dead Orbit, Whisper Caves, Red Waste, Blood Canopy, Shattered Sky, and Slime Lands. Runtime metadata is stored in `Assets/ScaryIslands/Resources/Biomes.json` so map selection and unlock requirements remain data-driven.

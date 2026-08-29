# Scary Islands

Scary Islands is a room-scale VR survival-horror game built with Unity and OpenXR. The first playable island, **Widow's Shore**, asks players to recover and ring the Salt Bell before the black tide consumes the island while monsters stalk the fog.

## Multiplayer

Scary Islands now includes a **2-4 player co-op prototype** using Unity Netcode for GameObjects 2.7.0 and Unity Transport 2.6.0.

- One player chooses **HOST**.
- Other players enter the host's IP address and choose **JOIN**.
- Default UDP port: **7777**.
- Host listens on all local interfaces.
- Player head, both tracked hands, torso, arm-mounted wings, starter-gun visual, and equipped pet are synchronized.
- Monster position and health are host-authoritative and synchronized to clients.
- Gun damage from clients is sent to the host before the shared monster state is updated.
- The host owns monster AI simulation so clients see the same monsters.
- The multiplayer session does not require network player prefabs; it uses NGO custom snapshot messages around the existing XR rig.
- This first pass is **direct IP/LAN**. Internet hosting may require UDP 7777 port forwarding/NAT configuration until Relay is added.

Unity officially lists Netcode for GameObjects 2.7.0 and Unity Transport 2.6.0 as released packages for Unity 6000.0.

The runtime multiplayer overlay provides Host/Join/Disconnect controls for desktop/editor testing. The prototype scene also includes a world-space Multiplayer Terminal whose public methods are ready to bind to XR interactables.

## Prototype loop

1. Host or join a multiplayer session, or remain offline.
2. Arrive with a **free starter gun** attached to the tracked right hand.
3. Shoot monsters, follow the audio beacons, and recover the chapel key and Salt Bell.
4. Flap the arm-mounted wings to fly and glide around the island.
5. Ring the Salt Bell at the drowned chapel.
6. Return to the skiff before the tide timer expires.
7. Spend earned **Dots** on 10-Dot pet eggs.

## Combat and Dots

- Every player starts with a free automatic starter gun.
- Bind `StarterGun.BeginFire` and `StarterGun.EndFire` to the XR trigger.
- Monsters use `MonsterHealth` and can be killed by gunfire.
- In multiplayer, monster damage is host-authoritative.
- A continuous monster-hit streak earns increasing Dots:
  - first full second hitting a monster: **+1 Dot**
  - second consecutive second: **+2 Dots**
  - third consecutive second: **+3 Dots**
  - and so on
- Missing/stopping long enough to break the hit streak resets the next reward to +1.
- Successful escapes still award 25 Dots.
- Dot balance currently persists locally per player.

## Unity setup

- Unity 6 LTS / URP
- OpenXR + XR Interaction Toolkit
- Netcode for GameObjects + Unity Transport
- Targets: Meta Quest (Android) and PC VR
- Comfort defaults: snap turn, vignette, teleport, seated/standing calibration
- Body model: floating torso and tracked arms, with no rendered legs
- Wings: every player has one wing attached to each tracked arm
- Ground locomotion: pull the tracked hands backward to move
- Flight: flap both arms downward to take off, climb, and accelerate forward
- Gliding: spread both arms apart in the air to reduce gravity and glide farther
- Currency: **Dots**, stored persistently with a 100-Dot starter balance

Open the project in Unity and allow packages to resolve. Then run **Scary Islands > Build Prototype Scene**. The greybox includes the Multiplayer Terminal, Pet Shop, and three shootable monster targets.

## Pet Shop

Every pet is purchased as a **10-Dot egg**. The current eggs hatch Fog Moth, Lantern Crab, Grave Crow, Mire Slime, Storm Bat, or Little Leviathan. Purchases and equipped pets persist locally. Equipped pets are also represented on remote multiplayer avatars.

## Cloudflare backend

`Backend/worker` is a small Worker + D1 API for anonymous player profiles and leaderboard submissions. It remains independent of real-time game traffic; Unity Transport carries multiplayer state. The prototype remains playable offline.

## Design

The editable experience boards live in [Figma](https://www.figma.com/design/PehL1aTcjaLH9YrAskwXJI).

## Planned maps

Widow's Shore, Abyssal Sea, Dead Orbit, Whisper Caves, Red Waste, Blood Canopy, Shattered Sky, and Slime Lands. Runtime metadata is stored in `Assets/ScaryIslands/Resources/Biomes.json` so map selection and unlock requirements remain data-driven.

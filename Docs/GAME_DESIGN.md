# Scary Islands — vertical slice

**Fantasy:** You are a winged wreck-diver crossing a cursed archipelago where each island manifests a different maritime legend. The game supports solo play and a 2-4 player co-op prototype.

**Core tension:** Light reveals navigation clues but attracts monsters. Gunfire can clear immediate threats but is loud and exposes the players' position. The tide remains a visible, physical deadline.

**Multiplayer model:** one player hosts and up to three others join over direct IP/LAN using Unity Netcode for GameObjects and Unity Transport. The existing XR rig remains local; custom snapshots synchronize head, tracked hands, body, wings, gun visual, and equipped pet. The host is authoritative for monster AI, monster position, and monster health. Client gun damage requests are validated/applied by the host and replicated back to all clients. The initial transport is UDP direct-IP on port 7777; Relay/NAT traversal is a later production step.

**Widow's Shore:** 12–18 minute run; drowned village, blackpine trail, sea cave, chapel, skiff. Three randomized clue locations support replayability. The Salt Bell banishes the fog briefly and enrages surviving threats.

**VR interaction rules:** players have a floating upper body and visible arms but no legs or feet. Every player has a wing attached to each tracked arm and a free starter gun attached to the tracked right hand. On the ground, pulling both hands backward drives locomotion. In the air, flapping both arms downward creates lift and forward thrust; spreading the arms creates a lower-gravity glide. The gun trigger maps to automatic fire. Remote players reproduce head/hand pose and wing motion from network snapshots.

**Combat:** monsters use a shared health model and can be killed by the starter gun. In multiplayer the host owns monster simulation and health. A sustained valid hit streak awards local Dots once per completed second. The reward escalates by one each consecutive second: +1, +2, +3, +4, etc. Breaking the hit streak resets the next second to +1 Dot.

**Economy:** the game uses one soft currency called **Dots**. New players start with 100 Dots, successful escapes award 25 Dots, and sustained monster hits award escalating Dots. Dots and pet ownership currently persist locally per player.

**Pet Shop:** every pet is sold as a **10-Dot egg**. Buying the selected egg immediately unlocks/hatches that pet. The first eggs are Fog Moth, Lantern Crab, Grave Crow, Mire Slime, Storm Bat, and Little Leviathan. Equipped pet identity is included in player snapshots so other players see a matching companion.

**Milestone 1 definition of done:** complete loop in headset, 2-4 player host/join test, synchronized remote VR pose, shared host-authoritative monsters, killable monsters, free starter gun, escalating per-second hit rewards, tide fail state, escape success state, functional arm-wing flight/gliding, persistent Dots wallet, 10-Dot pet eggs, working pet purchase/equip flow, comfort menu, stable 72 Hz Quest performance in the greybox.

## Archipelago roadmap

| Map | Horror identity | Relic | Signature mechanic |
|---|---|---|---|
| Abyssal Sea | Lightless ocean trench | Drowned Compass | Diving bell, oxygen and cable traversal |
| Dead Orbit | Derelict station above a black planet | Black Star Core | Zero gravity and magnetic boots |
| Whisper Caves | Living cave network | Echo Skull | Echo-location, climbing and crawling |
| Red Waste | Haunted desert of glass ruins | Glass Scarab | Sandstorms, mirages and wind sled |
| Blood Canopy | Predatory rainforest | Heartseed Idol | Machete paths, vines and river raft |
| Shattered Sky | Floating islands inside a storm | Storm Anchor | Wing flight, gliding, grappling hook and shifting land |
| Slime Lands | Bioluminescent corrosive swamp | Royal Slime Crown | Bounce surfaces and dissolving bridges |

Each map preserves the shared structure—enter, interpret clues, recover a relic, trigger an escalation, and escape—but changes locomotion, sensory pressure, and enemy behavior.

# MarsTS
 This is a practise project dedicated to exploring traditional RTS mechanics & systems, inspired by games like Company of Heroes, Command & Conquer and Starcraft. It's intended to explore the desolate Martian surface with precise control of units and data oriented patterns to help organize the structure. It was spurred on by the idea of using delegates to switch commands and change the mouse's behavior which turned out to be a successful and quite fun exercise. This has now expanded to an indefinitely in progress playspace for myself to explore these systems.

![Screenshot of a scout car fighting two tanks](/FolioImages/UnitCombat.png)

![Screenshot of the selection pane with a Longtank already selected and commands displayed in the bottom right](/FolioImages/SelectionPane_1.png)

 ## Key Features:

 ### Data Oriented Unit Registry
 I created a Data Oriented registry to store references to units which uses GameObject names as keys in registry trees, with the registry giving each a unique instanceID for its type. I designed the system to be expanded on for all Entities when I add buildings to workaround the slow speed of Unity's GetComponent function. By passing a singleton instance of the registry the root transform's name of any object, we can quickly and easily get the Unit class from any GameObject.

 ![Screenshot of the Unit Regisrty code](/FolioImages/UnitRegistry_Code.png)

 ### Observer Pattern UI
 I learnt about the Observer pattern when creating my Minecraft mod, Warpstone. There the pattern is strictly needed to assist in separating client & server code, while not so necessary in Unity with the help of Network Objects I wanted to employ the pattern as practise for a modular and simple design of the UI. This in turn allowed for a huge simplification of the UI and is able to be driven largely by Prefabs and a global Event Bus. I designed the Bus for custom events to be fired on both a local GameObject scope and a global Scene scope, with the UI able to subscribe to either to read needed information.

 ![Screenshot of the EventBus code](/FolioImages/EventAgents.png)
 ![Screenshot of the Player's singleton selection code](/FolioImages/PlayerCommand_Code.png)

 ### Physics Based Unit Movement
 I want the unit behaviors to feel more alive and interactive, I envisioned the scout car jumping off dunes in the martian desert, so I opted for physics based movement where each unit implements it themselves. Decoupling movement from the top-most Unit class has allowed a lot of flexibility, creating distinct behaviors between the Scout Car & Tank. The car will immediately pick up speed and drift while it reaches its destination, attempting to maintain speed above all else, while the tank will prioritize accuracy, accelerating only when pointing roughly towards its target. I'm hoping to expand on this with more varied PhysicsMaterials to play with friction, creating different masses for each unit and most importantly, vary the terrain to allow cars to jump off of dunes.

 ![Screenshot of the Tank's physics-based movement code](/FolioImages/TankMovementImplementation.png)

 ### Multi-Threaded Pathfinding
 The pathfinding is multi-threaded, allowing me to be quite liberal in how I use it. Currently each Unit will update its path every 0.5 seconds using Unity's coroutines to request the path from a Singleton which will handle the multi-threading, keeping it strictly seperated from the main render thread. The pathfinding is my own implementation using Sebastian Lague's A* pathfinding method. I'm hoping to explore both thread pooling and Unity's NavMesh in the future and decide wether a custom A* implementation or the NavMesh would work best for this project.

 ![Screenshot of the Pathfinding's Multi-Threading code](/FolioImages/PathFinding_MultiThreading.png)
 
 ## Progress:
 - [ ] RTS Camera Controls
    - [x] WASD Movement
    - [ ] Zooming
    - [ ] Rotation
    - [ ] Smoothing
 - [x] Unit Selection & Teams
    - [x] Unit Relationships
    - [x] Exclusive Team Selection
    - [x] Selection UI
        - [x] Multiple Unit Types
        - [x] Command Window
 - [ ] Unit Combat
    - [x] Projectile Implementation
    - [x] Hitscan Implementation
    - [] Abilities
 - [x] Unit Movement
    - [x] Physics-Based
    - [x] Car Implementation
        - [ ] Jumps
    - [x] Tank Implementation
 - [ ] UI
   - [x] Selection UI
   - [x] Command Panel
   - [ ] Unit Health
   - [ ] Waypoint Renderer
   - [ ] Unit Icons
 - [ ] Building
    - [ ] Constructor Unit
    - [ ] Resource Silo
    - [ ] Factory
 - [ ] Resource Gathering
    - [ ] Harvester Unit
    - [ ] Mineral Deposits
    - [ ] Oil Deposits
    - [ ] Pumpjack Building
 - [ ] Unit Production
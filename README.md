# MarsTS
 This is a practise project dedicated to exploring traditional RTS mechanics & systems, inspired by games like Company of Heroes, Command & Conquer and Starcraft. It's intended to explore the desolate Martian surface with precise control of units and data oriented patterns to help organize the structure. It was spurred on by the idea of using delegates to switch commands and change the mouse's behavior which turned out to be a successful and quite fun exercise. This has now expanded to an indefinitely in progress playspace for myself to explore these systems.

![Screenshot of a scout car fighting two tanks](/FolioImages/UnitCombat.png)

![Screenshot of the selection pane with a Longtank already selected and commands displayed in the bottom right](/FolioImages/SelectionPane_1.png)

 ## Key Features:

 ### Data Oriented Unit Entity Cache
 I created a Data Oriented Entity Cache to store references to all units & buildings, using GameObject names as keys in a central dictionary, with the cache asigning a unique instance ID to each Entity. It used to be exclusively for Units and has expanded to use a Entity component which will store references of every component implementing the ITaggable interface. Any component implementing this interface can be referenced from the Entity Cache without the need for any GetComponent calls outside of the game initialization, making getting any component using this interface lightning fast and easy to use. Both Buildings & Units, two completely separate objects, use this system to great effect.

 ![Screenshot of the Entity code, showcasing how it stores a reference for every component using ITaggable](/FolioImages/Entity_Code.png)
 ![Screenshot of a use case for the Entity system, showing how easy it is to get a component of an Entity](/FolioImages/Entity_Use_Case.png)

 ### Extensive Observer Pattern Implementation
 I learnt about the Observer pattern when creating my Minecraft mod, Warpstone. There I learnt how useful it can be to expand on game logic on a modular fashion, and I've employed it here to practise. It started out as used entirely for the UI and has since evolved to helping implement all major systems, from unit Selection logic, Entity initialization, unit combat and even unit production. It's all driven by a global Event Bus & local Event Agents, with the ability to use either to subscribe to the other and focus your listeners on the exact events you're interested in. It's a simple system that's allowed for what would otherwise be quite complex code to be streamlined and decoupled.

 ![Screenshot of the EventAgent code](/FolioImages/EventAgent_Code.png)
 ![Screenshot of a use case for the event agents & bus](/FolioImages/EventBus_Use_Case.png)

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
    - [ ] Abilities
 - [x] Unit Movement
    - [x] Physics-Based
    - [x] Car Implementation
        - [ ] Jumps
    - [x] Tank Implementation
 - [ ] UI
    - [x] Selection UI
    - [x] Command Panel
    - [x] Unit Bars
       - [x] Health Bars
       - [x] Building Construction Bars
       - [x] Unit production Bars
    - [ ] Waypoint Renderer
    - [ ] Unit Icons
    - [ ] Production Queue
 - [x] Building
    - [x] Constructor Unit
    - [ ] Resource Silo
    - [x] Factory
 - [ ] Resource Gathering
    - [ ] Harvester Unit
    - [ ] Mineral Deposits
    - [ ] Oil Deposits
    - [ ] Pumpjack Building
 - [ ] Unit Production
    - [x] Constructor Production
    - [x] Production Time
    - [ ] Production Costs
# MarsTS
 This is a practise project dedicated to exploring traditional RTS mechanics & systems, inspired by games like Company of Heroes, Command & Conquer and Starcraft. It's intended to explore the desolate Martian surface with precise control of units and data oriented patterns to help organize the structure. It was spurred on by the idea of using delegates to switch commands and change the mouse's behavior which turned out to be a successful and quite fun exercise. This has now expanded to an indefinitely in progress playspace for myself to explore these systems.

 The game now features unit combat, building construction, unit production, resource gathering, building upgrades, unit abilities and fog of war, having shaped into almost all the essential bones for an RTS game.

 ![Gif of a tank assault climbing a ramp and engaging a small base with artillery and a constructor](https://i.imgur.com/yC9xHgz.gif)
 ![Screenshot of a Mobile Artillery & Roughneck squad working together to destroy a tank without being detected](/FolioImages/ArtilleryAttackingExample.png)

 ## Key Features:

 ### Varied Unit Combat
 Unit Combat has been expanded upon even further with the implementation of Fog of War, now introducing units designed around the system to enhance tactical decision making. Roughnecks and their stealth add a new dimension to combat, capable of sneaking behind enemy lines if they keep their distance, able to co-ordinate with a Mobile Artillery deployed far away to use its massive attack range. Scout cars and flat tanks remain unchanged, making both far more powerful. With the need for artillery to deploy, scout cars have an easier time hunting them down and getting the jump on them, making tanks more valuable with their ability to quickly dispatch individual cars. The composition of an army now has major implications for the outcome of a battle, when combined with the repairing capabilities of a constructor a lot of strategic and tactical depth is easily created.

 ![Closeup screenshot of a large group of scout cars & tanks overwhelming enemy flat tanks](/FolioImages/resourcing/unit_combat_3.png)

 ### Construction & Production
 There are currently 4 different buildings, each filling a specific role; the Scrapyard, Factory, Pumpjack & Makeshift Pumpjack. The Scrapyards serve as your headquarters and can produce civilian units and the fast scout car, intended to be replaced by the Factory which can produce powerful military units, such as a Tank and Mobile Artillery, but costing far more oil than Scrapyard productions. The Pumpjack and its Makeshift variant are meant for harvesting this oil, the Makeshift variant however is constructed by the sneaky Roughneck infantry squad and can be upgraded with camo so its not easily visible from far away, facilitating hidden oil harvesting operations at the cost of harvest rate. All buildings support these upgrades and even research.

 ![Screenshot of a constructor unit building a Pumpjack over an oil deposit](/FolioImages/resourcing/building_construction.png)
 ![Screenshot of a constructor unit selected with a tooltip open for the Pumpjack construction](/FolioImages/resourcing/MakeShiftPumpjack.png)

 ### Resource Gathering
 Resource Harvesting comes currently in two varieties, with plenty of room to expand and play with for more unique levels. There's Resource Units, inspired by the Homeworld franchise it represents all the general materials needed to construct things, scrap, metal, rock, rubber, wire, etc. The second main resource is Oil, less plentiful than RUs it requires more specialized equipment to extract, needing a Pumpjack built on oil deposits before tanker trucks can collect it and deposit it in a player's base. Resource gathering requires transportation of resources before they enter the player's resource banks, requiring planning of supply lines and protecting them so resourcing operations don't get disrupted. Resources can even be stolen from players, with the Roughnecks infantry able to siphon oil out of enemy Pumpjacks without being revealed in stealth, allowing for subversive but effective attacks on a targets economy.

 ![Screenshot of a tanker unit siphoning oil out of a pumpjack, with its headquarters it'll deposit into in the background](/FolioImages/resourcing/resource_gathering_2.png)

 ### Fog of War
 Fog of War has been implemented with a data oriented vision system. All units can see within a range around them, pseudo-raycasts are fired from the edges inwards to determine if any obstacles block the vision, creating a dynamic vision system that utilizes varied height in terrain. Units cannot see over cliffs, but can see and fire upon other units down them. This is utilized for two new units designed for the system; the Roughnecks and Mobile Artillery. The Roughnecks are capable of stealth, reducing the range with which they are visible, so they can creep on enemy units and get sight for the Mobile Artillery, which when deployed has a massive attack range and high damage, but a small range of vision. The vision is multi-threaded for both calculation and texture rendering and has been tested with over a thousand units simultaneously with little impact to performance.

 ![Screenshot of a Roughneck Squad sneaking right next to a tank, remaining undetected](https://i.imgur.com/gtRyiZG.gif)
 ![Gif of vision switching between the main player and enemy AI, showing the Roughneck Squad being revealed and hidden](https://i.imgur.com/BoHYJ2w.gif)

 ## Featured Systems:

 ### Data Oriented Unit Entity Cache
 I created a Data Oriented Entity Cache to store references to all units & buildings, using GameObject names as keys in a central dictionary, with the cache asigning a unique instance ID to each Entity. It used to be exclusively for Units and has expanded to use a Entity component which will store references of every component implementing the ITaggable interface. Any component implementing this interface can be referenced from the Entity Cache without the need for any GetComponent calls outside of the game initialization, making getting any component using this interface lightning fast and easy to use. Both Buildings & Units, two completely separate objects, use this system to great effect.

 ![Screenshot of the Entity code, showcasing how it stores a reference for every component using ITaggable](/FolioImages/Entity_Code.png)
 ![Screenshot of a use case for the Entity system, showing how easy it is to get a component of an Entity](/FolioImages/Entity_Use_Case.png)

 ### Extensive Observer Pattern Implementation
 I learnt about the Observer pattern when creating my Minecraft mod, Warpstone. There I learnt how useful it can be to expand on game logic on a modular fashion, and I've employed it here to practise. It started out as used entirely for the UI and has since evolved to helping implement all major systems, from unit Selection logic, Entity initialization, unit combat, and even unit production. It's all driven by a global Event Bus & local Event Agents, with the ability to use either to subscribe to the other and focus your listeners on the exact events you're interested in. It's a simple system that's allowed for what would otherwise be quite complex code to be streamlined and decoupled.

 ![Screenshot of the EventAgent code](/FolioImages/EventAgent_Code.png)
 ![Screenshot of a use case for the event agents & bus](/FolioImages/EventBus_Use_Case.png)

 ### Physics Based Unit Movement
 I want the unit behaviors to feel more alive and interactive, I envisioned the scout car jumping off dunes in the martian desert, so I opted for physics based movement where each unit implements it themselves. Decoupling movement from the top-most Unit class has allowed a lot of flexibility, creating distinct behaviors between the Scout Car & Tank. The car will immediately pick up speed and drift while it reaches its destination, attempting to maintain speed above all else, while the tank will prioritize accuracy, accelerating only when pointing roughly towards its target. I'm hoping to expand on this with more varied PhysicsMaterials to play with friction, creating different masses for each unit and most importantly, vary the terrain to allow cars to jump off of dunes.

 ![Screenshot of the Tank's physics-based movement code](/FolioImages/TankMovementImplementation.png)

 ### Multi-Threaded Pathfinding
 The pathfinding's multi-threading has been vastly improved, attaining greater performance and further allowing me to be quite liberal in how I use it. Currently each Unit will update its path every 0.5 seconds using Unity's coroutines to request the path from a Singleton which will handle the multi-threading, keeping it strictly seperated from the main render thread. The pathfinding is my own implementation using a modified version of Sebastian Lague's A* pathfinding method to improve performance and reliability of the paths found. I'm hoping to explore both thread pooling and Unity's NavMesh in the future and decide wether a custom A* implementation or the NavMesh would work best for this project.

 ![Screenshot of the Pathfinding's Multi-Threading code](/FolioImages/PathFinding_MultiThreading.png)

 ### Multi-Threaded Vision
 The Vision system is also multi-threaded, which brought a revamping of the pathfinding's multi-threading to improve with what I learnt making this system. Vision works by overlaying a grid of integers, each integer is used as a bit mask containing info on which players can see it or have visited it. Every unit that has a vision component attached will be tracked by the game vision, and 4 times a second the vision system will calculate every cell within range of each vision component and pseudo-raycast to determine if any terrain blocks line of sight to that cell. All of the cells are then assigned the bits representing players that can see them. This is all calculated in a constantly running secondary thread while a tertiary thread collects all the data and converts it into a texture every fixed update, allowing for interpolation between each frames texture to ease out the vision changes.

 ![Screenshot of the Vision System's calculation algorithm](/FolioImages/VisionCalculation_Code.png)
 ![Screenshot of the Vision Renderer's multi-threaded algorithm](/FolioImages/VisionRenderer_Code.png)
 
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
    - [x] Infantry Squads
 - [x] Unit Combat
    - [x] Projectile Implementation
    - [x] Hitscan Implementation
    - [x] Abilities
       - [x] Active Effects
       - [x] Worked Commands
    - [x] Repairing
 - [x] Unit Movement
    - [x] Physics-Based
    - [x] Car Implementation
        - [ ] Jumps
    - [x] Tank Implementation
    - [x] Infantry Implementation
 - [ ] UI
    - [x] Selection UI
    - [x] Command Panel
       - [x] Command Tooltips
          - [x] Command Costs
       - [x] Activity
       - [x] Cooldown
       - [x] Usability
    - [x] Unit Bars
       - [x] Health Bars
       - [x] Building Construction Bars
       - [x] Unit production Bars
       - [x] Resources Stored Bars
    - [x] Rendered Unit Icons
    - [x] Expanded Unit Info
       - [x] Production Queue
       - [x] Health Info
       - [x] Resource Deposit
       - [x] Resources Stored
       - [ ] Current Command Info
    - [x] Player Resources
    - [ ] Waypoint Renderer
    - [ ] Minimap
 - [x] Building
    - [x] Constructor Unit
    - [x] Factory
    - [x] Scrapyard
    - [x] Pumpjack
    - [x] Building Upgrades
    - [ ] Building Large Units
 - [x] Economy
    - [x] Harvester Unit
    - [x] Mineral Deposits
    - [x] Oil Deposits
    - [x] Pumpjack Building
    - [x] Production & Construction Costs
 - [x] Unit Production
    - [x] Constructor Production
    - [x] Production Time
    - [x] Production Costs
    - [ ] Unit Upgrades
 - [X] Fog of War
    - [x] Raycasted Vision
       - [x] Blocked by Terrain
    - [x] Multi-Threading
       - [x] Vision Calculation
       - [x] Vision Rendering
    - [x] Easing
    - [x] Switching Player's Vision
    - [x] Stealth
    - [ ] Gaussian Blur
 - [ ] Networking
 - [ ] Enemy AI

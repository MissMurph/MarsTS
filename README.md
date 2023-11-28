# MarsTS
 This is a practise project dedicated to exploring traditional RTS mechanics & systems, inspired by games like Company of Heroes, Command & Conquer and Starcraft. It's intended to explore the desolate Martian surface with precise control of units and data oriented patterns to help organize the structure. It was spurred on by the idea of using delegates to switch commands and change the mouse's behavior which turned out to be a successful and quite fun exercise. This has now expanded to an indefinitely in progress playspace for myself to explore these systems.

 The game now features unit combat, building construction, unit production and even resource gathering, having shaped into almost all the essential bones for an RTS game.

 ![Screenshot of a scout car, long tank & a flat tank cornering an enemy flat tank](/FolioImages/resourcing/unit_combat_2.png)

 ## Key Features:

 ### Varied Unit Combat
 Unit Combat now features a variety of units with different strengths and weaknesses, creating varied gameplay and tactical decision making in how you utilise each unit. Currently there are 4 nain units used for combat; the Scout Car, Flat Tank, Long Tank & Constructor. The scout car is fast, able to dodge projectiles from tanks and fire quickly, making it an excellent counter to the slower, long range artillery focused long tank, which is able to snipe slow moving flat tanks from afar. Flat tanks however are fast, deadly and precise, able to easily dispatch scout cars when in a group. All of these units can be supported by constructors to repair and sustain them in the battle, however constructors are incredibly slow, requiring encounters to push ahead and fall back when repairs are needed.

 ![Closeup screenshot of a large group of scout cars & tanks overwhelming enemy flat tanks](/FolioImages/resourcing/unit_combat_3.png)

 ### Building Construction
 Constructors primary use however is (unsurprisingly) constructing buildings. Currently there are 3 different buildings that can be built; the Scrapyard, Factory & Pumpjack. Scrapyards serve as your headquarters, you start with one with two turrets to defend it, and are incredibly expensive to build, but are able to support an entire army themelves. Factories are more specialized, with no defenses, limited durability and high cost, they're a high priority target but are able to produce units far quicker than the Scrapyard, and for precious Oil, are able to produce tanks.

 ![Screenshot of a constructor unit building a Pumpjack over an oil deposit](/FolioImages/resourcing/building_construction.png)
 ![Screenshot of a constructor unit selected with a tooltip open for the Pumpjack construction](/FolioImages/resourcing/command_tooltip.png)

 ### Resource Gathering
 Resource Harvesting comes currently in two varieties, with plenty of room to expand and play with for more unique levels. There's Resource Units, inspired by the Homeworld franchise it represents all the general materials needed to construct things, scrap, metal, rock, rubber, wire, etc. The second main resource is Oil, less plentiful than RUs it requires more specialized equipment to extract, needing a Pumpjack built on oil deposits before tanker trucks can collect it and deposit it in a player's base. Resource gathering requires transportation of resources before they enter the player's resource banks, requiring planning of supply lines and protecting them so resourcing operations don't get disrupted.

 ![Screenshot of a tanker unit siphoning oil out of a pumpjac, with its headquarters it'll deposit into in the background](/FolioImages/resourcing/resource_gathering_2.png)

 ### Unit Production
 Units can be produced at the two largest buildings, the Factory & the Scrapyard. Scrapyards are host to a whole suite of blueprints for civilian units, such as the Constructor, Harvester & Tanker units, and is able to produce scout cars, but is quite slow to produce all of them; you want it constantly producing to maximize your resource utilization. Factories can produce things far quicker, at twice the speed of Scrapyards, and have access to more specialized manufacturing equipment allowing them to produce both long and flat tanks.

 ![Screenshot of the scrapyard selected with an ongoing production and additional orders queued up, demonstrating the UI](/FolioImages/resourcing/production_queue.png)

 ![Screenshot of two harvesters moving back and forth between a Scrapyard & a minerals deposit, one of them is currently harvesting while the other is depositing into the scrapyard](/FolioImages/resourcing/resource_gathering.png)

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
    - [x] Repairing
 - [x] Unit Movement
    - [x] Physics-Based
    - [x] Car Implementation
        - [ ] Jumps
    - [x] Tank Implementation
 - [ ] UI
    - [x] Selection UI
    - [x] Command Panel
       - [x] Command Tooltips
    - [x] Unit Bars
       - [x] Health Bars
       - [x] Building Construction Bars
       - [x] Unit production Bars
       - [x] Resources Stored Bars
    - [ ] Waypoint Renderer
    - [x] Rendered Unit Icons
    - [x] Expanded Unit Info
       - [x] Production Queue
       - [x] Health Info
       - [x] Resource Deposit
       - [ ] Resources Stored
       - [ ] Current Command Info
    - [x] Player Resources
    - [ ] Minimap
 - [x] Building
    - [x] Constructor Unit
    - [x] Factory
    - [x] Scrapyard
    - [ ] Building Large Units
 - [x] Resource Gathering
    - [x] Harvester Unit
    - [x] Mineral Deposits
    - [x] Oil Deposits
    - [x] Pumpjack Building
 - [x] Unit Production
    - [x] Constructor Production
    - [x] Production Time
    - [x] Production Costs
    - [ ] Unit Upgrades
 - [ ] Fog of War
 - [ ] Networking
 - [ ] Enemy AI
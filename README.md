Ubisoft Technical Test - Egg Collector Simulation
This project is a multiplayer egg collector simulation developed as a technical test. It focuses on simulating a Client-Server architecture, implementing custom Pathfinding algorithms, and handling Network Interpolation within the Unity environment.

Architecture Overview
The core objective of this project is to completely decouple the Server logic from the Client presentation layer, facilitating a potential future migration to a real backend server.

1. Server Simulation Layer (_Scripts/Simulation)
This acts as the "brain" of the game. All game logic runs independently of the Unity API (no MonoBehaviour is used for core logic).

GameSimulation.cs: Acts as the Server. This class manages the entire GameState, including the positions of players, bots, and collectibles.

Tick System: The server operates on discrete Tick intervals, independent of the Client's frame rate.

Snapshot System: At every update tick, the Server packages the entire game state into EntitySnapshot objects and sends them to the Client. This mechanism simulates network packet transmission.

2. Client Presentation Layer (_Scripts/Client & _Scripts/Presentation)
This is the view layer, responsible solely for receiving data and rendering the state.

Interpolation: The Client receives snapshots from the Server (simulating random latency between 0.1s and 0.5s) and uses interpolation algorithms to render smooth movement between states.

View Separation: Visual logic (BotVisual, PlayerVisual) is completely separated from data logic.
<img width="1741" height="1575" alt="image" src="https://github.com/user-attachments/assets/92d2f309-e28e-4232-9969-ead076826c63" />
<img width="1746" height="1466" alt="image" src="https://github.com/user-attachments/assets/8356a6fc-436d-45ee-92a1-9cc287ebb839" />


Algorithms & Optimization
Core algorithms were built from scratch without relying on existing Unity libraries (such as NavMesh), adhering to the test requirements.

1. Custom A* Pathfinding (AStar.cs)
A manually implemented pathfinding system for Bots:

Grid-based: The map is divided into a grid of Nodes.

Heuristic: Uses the Manhattan distance formula (Mathf.Abs(dx) + Mathf.Abs(dy)), optimized for 4-direction movement.

Priority Queue: Utilizes a custom Priority Queue to optimize the selection of the best next Node, significantly improving performance over standard List traversal.

Dynamic Obstacles: Bots can recognize static obstacles in the grid and recalculate paths accordingly.

2. Procedural Map Generation (CellularAutomataGenerator.cs)
The map is procedurally generated each session to ensure variety:

Cellular Automata: Uses an algorithm similar to Conway's Game of Life to create organic, cave-like structures.

Smoothing: Applies smoothing iterations to remove noise and create distinct wall/floor areas.

Connectivity Check: Ensures the map is fully connected and accessible using a flood-fill based validator (ConnectedMapGenerator), preventing isolated areas.

3. Chunk Loading System (ChunkLoadingSystem.cs)
To optimize performance on large maps with thousands of obstacles, a distance-based object culling system is implemented:

Spatial Partitioning: The map is divided into 16x16 chunks (MapChunk.cs). Obstacles are assigned to specific chunks upon generation.

Hysteresis Loading: To prevent object flickering at the edge of the view, a dual-threshold system is used:

Load: Chunks activate when the player enters a 32-unit radius.

Unload: Chunks deactivate only when the player exceeds a 48-unit radius.

Performance Ticking: The chunk update loop runs every 0.5 seconds rather than every frame to minimize CPU overhead.

Benefit: This drastically reduces the number of active GameObjects and Physics checks, keeping FPS stable even with massive maps.

4. Network Interpolation (InterpolationUtils.cs)
To handle the random server update intervals (Lag simulation):

Linear Interpolation (Lerp): Calculates the position between the two most recent snapshots based on renderTime.

Latency Handling: Automatically adjusts to variable time deltas to prevent jitter or teleportation artifacts on the Client side.

Design Patterns
The project utilizes several standard design patterns to ensure code cleanliness and scalability:

Service Locator Pattern (ServiceLocator.cs):

Provides a global access point for core services (like IGameSimulation, IEventBus) without the need for excessive dependency injection through constructors.

Observer/Event Bus Pattern (EventBus.cs):

Facilitates loose coupling between systems. For example, when an egg is collected, an event is fired; the UI and Audio systems listen for this event without needing direct references to the gameplay logic.

Object Pooling (GameObjectPool.cs):

Optimizes performance by reusing frequently spawned objects like Eggs and Visual Effects, minimizing Instantiate/Destroy calls and Garbage Collection spikes.

Factory Pattern:

Used for the initialization of Entities (Player, Bot, Collectible) via their respective Managers (AgentManager, CollectibleManager).

Known Issues & Future Improvements
Due to the time constraints of this test, some aspects remain basic and are candidates for future improvement:

1. User Interface (UI)
Current State: The UI (Menu, HUD, Scoreboard) is functional but lacks visual polish and advanced UX features.

Solution: Implement DOTween for UI animations (bouncing, fading) and redesign the assets to match a consistent Isometric Flat Design style.

2. Client-Side Prediction
Current State: The local player currently waits for Server confirmation (simulated), which can result in a feeling of input delay if the simulated latency is high.

Solution: Implement Client-Side Prediction. The client should move the player immediately upon input, then perform Reconciliation (checking against the server state and correcting if necessary) when the packet arrives. This would provide a "Zero Latency" feel.

3. Bot AI Logic
Current State: Bots simply find the shortest path to the nearest available egg.

Solution: Upgrade to Utility AI or Behavior Trees. Bots could be programmed to intercept other players or prioritize eggs that are closer to them relative to competitors, rather than just the absolute closest egg.
4. Visuals & Assets (Art Polish)
Current State: The game currently utilizes standard Unity primitives (Cubes, Capsules) and basic materials (programmer art) to focus on the technical implementation of the simulation.

Solution:

3D Models: Replace placeholders with Low Poly or Voxel style character models and environmental props to establish a cohesive art style.

Animations: Implement an Animator Controller with specific states for Idle, Run, Dash, and Interact to make character movement feel more organic and responsive.

VFX: Add Particle Systems for key gameplay events (e.g., confetti explosion upon egg collection, trail renderers for dashing, and dissolve effects for spawning) to improve "Game Juice".

Audio: Integrate a complete Sound Manager with spatial sound effects (SFX) and background music (BGM) to enhance immersion.
Development Environment
Unity Version: 2022.3.61f1

Language: C#

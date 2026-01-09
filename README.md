*Rasmus Eriksson*
# A course repo for learning video game AI techniques with C# and Unity

## Project folder structure

```
Assets/
├── Common/                                    # Projects, All the different projects
│   │
│   ├── lab1_PatrollingGuard/                  # Finite State Machine demo
│   │   ├── Lab1                               # Scene, Lab scene
│   │   └── Scripts/
│   │       ├── PlayerController.cs            # Script, Simple player controller
│   │       ├── SimpleCameraFollow.cs          # Script, Simple camera follow
│   │       └── EnemyPathfinding.cs            # Script, Simple chasing enemy
│   │
│   ├── Lab2_AStar/                            # Grid-based A* demo
│   │   ├── AStartLabScene                     # Scene, Lab scene
│   │   ├── Tile                               # Prefab, Prefab for tiles
│   │   ├── Material/                           # Folder, Folder with more materials
│   │   └── Scripts/
│   │       ├── GridManager.cs                 # Script, Generates the grid
│   │       ├── Node.cs                        # Script, Node Class for storing Node data
│   │       └── Pathfinder.cs                  # Script, Pathfinder script using A*
│   │
│   ├── Lab3_SteeringSwarm/                    # Steering behaviors demo
│   │   ├── SteeingPlayground                  # Scene, Lab scene
│   │   ├── Agent                              # Prefab, Prefab for NPC
│   │   └── Scripts/
│   │       ├── AI
│   │       │   └── SterringAgent.cs           # Script, broid algorithm
│   │       └── Spawnmanager.cs                # Script, Spawns in agents
│   │
│   ├── Lab4_BehaviorTrees/                    # Steering behaviors demo
│   │   ├── BehaviorTreeScene                  # Scene, Lab scene
│   │   ├── GuardAI                            # BehaviourTreeGrapgh, Behavoiur tree graph
│   │   └── Scripts/
│   │       └── GuardSensor.cs                 # Script, The Guards sensor for line of sight, distance to enemy
│   │
│   ├── Lab5_GOAP/                             # Steering behaviors demo
│   │   ├── GOAP                               # Scene, Lab scene
│   │   └── Scripts/
│   │         ├── Actions/
│   │         │   ├── MoveToTargetAction.cs    # Script, Move to Target based on action
│   │         │   ├── MoveToWeapon.cs          # Script, Move to weapon based on action
│   │         │   ├── PatrolAction.cs          # Script, Patrol action for partolling
│   │         │   ├── PickUpWeapon.cs          # Script, Pick up action
│   │         │   └── TagTargetAction.cs       # Script, Tag Target Action for taging/Attacking
│   │         ├── GoapActionBase.cs            # Script, Generic Action Base for GOAP
│   │         ├── GoapAgent.cs                 # Script, Base Goap agent
│   │         ├── GoapCore.cs                  # Script, Goap Core
│   │         ├── GoapSensor.cs                # Script, Basic sensor script for AI
│   │         └── GoapDebugHUD.cs              # Script, Debug hud for goap AI
│   │
│   └── Material/                              # Folder, Common materials
```

## Overview

This repository serves as a practical resource for learning various AI techniques used in video game development, specifically utilizing C# and Unity. The project is structured to facilitate hands-on learning through demos and tutorials.

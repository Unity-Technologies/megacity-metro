![Megacity-Metro](Readme/header.jpg)

## Contents and Quick Links
- [Megacity-Metro Overview](#megacity-metro-overview)
- [Megacity-Metro Prerequisites](Documentation/prerequisites.md)
- [Multiplayer Setup](Documentation/multiplayer-setup.md)
- [Add Unity Gaming Services (UGS)](Documentation/ugs.md)
- [Index of Resources in this Project](Documentation/script-index.md)
- [Assemblies](Documentation/assemblies.md)
- [Netcode Atrributes](Documentation/attributes.md)
- [Gameplay Controls](#gameplay-controls)
  - [Mouse and Keyboard](#mouse-and-keyboard)
- [Troubleshooting](#troubleshooting)
  - [Bugs](#bugs)
- [Disclaimer](#disclaimer)
- [License](#license)


## Megacity-Metro Overview

Megacity-Metro is an action-packed, shooter game based on the original Megacity sample. It leverages the power of Netcode for Entities for an immersive, multiplayer experience that can support 128+ players simultaneously. The latest DOTS packages and Unity Gaming Services (UGS) enhances the Megacity-Metro user experience. Megacity-Metro showcases how to create engaging and immersive multiplayer experiences with a suite of netcode and multiplayer tools, tech, and services. 

Some important points of this demo are:
- Large-scale streaming and rendering with the Entity Component System (ECS for Unity)
- 128+ players per game session
- Server-authoritative gameplay with feature prediction, interpolation, and lag compensation using Netcode for Entities
- Unity Gaming Services (UGS) integration for Game Server Hosting, Matchmaking, and Vivox voice chat
- Universal Render Pipeline (URP)
- Cross-platform support for Windows, Mac and Android

## Gameplay Controls

### Mouse and Keyboard

| Input        | Action       |
|--------------|--------------|
| Mouse Movement / Arrow Keys | Steering |
| Left Click / Space | Shoot |
| W/S | Thrust / Reverse |
| A/D | Steering |
| E/Q | Roll |
| Tab | LeaderBoard |
| V | Toggle Vivox |
| P | Netcode Panel Stats |
| ESC| in game menu |

## Contents and Quick Links

- [Important Note Before You Begin](#important-note-before-you-begin)
- [Clone the Project](#clone-the-project)
- [Quick Start](#quick-start) 

# Getting Started

## Important Note Before You Begin

The Megacity-Metro sample is large, so the **first time** cloning and playing the sample may take more time than expected. Subsequent plays should load much quicker because of caching.

First time clone and load time estimates:
- Cloning the Megacity-Metro repo: Up to 20 min
- Opening the project with library build: Up to 20 min
- Building the project for each platform for the first time: Up to 50 min. 
Navigate to `Project Settings > Other Settings > Configuration > C++ Compiler Configuration`and set `Debug` if you need to compile faster than optimal.

To get the Megacity-Metro sample, you can clone the project using Git. 
**Please note that direct download of the project is not currently supported for this repository**.

## Clone the Project

Before you can clone the project, you must install Git Large File Support (LFS). Megacity-Metro uses Git LFS to handle all large assets required locally. 
Refer to [Git LFS installation options](https://github.com/git-lfs/git-lfs/wiki/Installation) for instructions on Windows and Mac. 

## Quick Start

After you clone the project, follow these steps to start playing:
1. Install a compatible Unity Editor version; we highly recommend using the version specified in `ProjectSettings/ProjectVersion.txt`. During installation, make sure to include Windows/Mac Build Support (IL2CPP), Windows/Mac Dedicated Server Build Support, Android/iOS Build Support (if you plan to build for mobile devices), and Linux Dedicated Server Build Support.
2. To add the project to the **Unity Hub**, click the **Add** button and select the root folder of the cloned project.
3. Open the **Menu** scene located in `Assets/Scenes/Menu`. 
4. Click the **Play** button to start.
5. Start the Single Player mode. Multiplayer requires more setup which will be explained next.  

## Disclaimer

This repository does not accept pull requests, GitHub review requests, or any other GitHub-hosted issue management requests.

## License

Megacity Metro is licensed under the Unity Companion License. See [LICENCE](LICENCE.md) for more legal information.

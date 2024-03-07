![Megacity-Metro](Readme/header.jpg)

## Contents and Quick Links
- [Megacity-Metro Overview](#megacity-metro-overview)
- [Megacity-Metro Prerequisites](Documentation/prerequisites.md)
- [Get Started](Documentation/getting-started.md)
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


## Troubleshooting

### Bugs

Report bugs in Megacity Multiplayer using GitHub [issues](https://github.com/Unity-Technologies/Megacity-Metro/issues). If the bugs are related to the Entities packages, use the Entities GitHub issues.

## Disclaimer

This repository does not accept pull requests, GitHub review requests, or any other GitHub-hosted issue management requests.

## License

Megacity Metro is licensed under the Unity Companion License. See [LICENCE](LICENCE.md) for more legal information.

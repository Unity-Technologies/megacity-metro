## Contents and Quick Links

- [Index of Resources in this Project](#index-of-resources-in-this-project)
  - [Gameplay](#gameplay)
  - [Audio](#audio)
  - [Connectivity](#connectivity)
  - [Services (Vivox, Matchmaker, etc.)](#services-vivox-matchmaker-etc)
  - [UI](#ui)
  - [Tools and Utilities](#tools-and-utilities)

## Index of Resources in this Project

### Gameplay
**Client:**
- [Client in Game](../Assets/Scripts/Gameplay/Client/Netcode/ClientInGame.cs)
- [Vehicle Input System](../Assets/Scripts/Gameplay/Client/Player/PlayerVehicleInputSystem.cs)
- [Setup PlayerInfo System](../Assets/Scripts/Gameplay/Client/Player/SetupPlayerInfoSystem.cs)
- [Bound System](../Assets/Scripts/Gameplay/Client/Level/BoundsSystem.cs)
- [Update Player Rank System](../Assets/Scripts/Gameplay/Client/Player/UpdatePlayerRankSystem.cs)

**Mix:**
- [Player Vehicle Jobs](../Assets/Scripts/Gameplay/Mix/Player/Jobs/PlayerVehicleJobs.cs)
- [Shooting System](../Assets/Scripts/Gameplay/Mix/Shooting/ShootingSystem.cs)
- [Laser Job](../Assets/Scripts/Gameplay/Mix/Shooting/Jobs/LaserJob.cs)
- [Laser Visual Job](../Assets/Scripts/Gameplay/Mix/Shooting/Jobs/LaserVisualJob.cs)

**Server**
- [Server in Game](../Assets/Scripts/Gameplay/Server/Netcode/ServerInGame.cs)
- [Shooting Server System](../Assets/Scripts/Gameplay/Server/Shooting/ShootingServerSystem.cs)
- [Damage Job](../Assets/Scripts/Gameplay/Server/Shooting/Jobs/DamageJob.cs)

### Audio
- [AudioMaster](../Assets/Scripts/Gameplay/Client/Audio/MonoBehaviours/AudioMaster.cs)

### Connectivity
- [NetcodeBootstrap](../Assets/Scripts/Gameplay/Mix/Netcode/NetcodeBootstrap.cs)

### Services (Vivox, Matchmaker, etc.)
- [VivoxManager](../Assets/Scripts/Gameplay/Client/Vivox/MonoBehaviours/VivoxManager.cs)
- [ClientMatchmaker](../Assets/Scripts/UGS/Client/ClientMatchmaker.cs)
- [GameServerManager](../Assets/Scripts/UGS/Server/GameServerManager.cs)

### UI
- [HUD](../Assets/Scripts/Gameplay/Client/UI/MonoBehaviours/HUD/HUD.cs)
- [MainMenu](../Assets/Scripts/Gameplay/Client/UI/MonoBehaviours/MainMenu/MainMenu.cs)
- [UIGameSettings](../Assets/Scripts/Gameplay/Client/UI/MonoBehaviours/Settings/UIGameSettings.cs)

### Tools and Utilities
- [NetcodePanelStats](../Assets/Scripts/Utils/NetcodeExtensions/UI/NetcodePanelStats.cs)

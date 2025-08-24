# IKT VR Game

An immersive virtual reality dungeon crawler built with Unity and SteamVR, featuring procedural level generation, progressive difficulty scaling, and intense melee combat mechanics.

## 🎮 Game Overview

IKT VR Game is a roguelike dungeon crawler designed specifically for virtual reality platforms. Players navigate through procedurally generated levels filled with increasingly challenging enemies, collect loot, and face epic boss battles in a low-poly, atmospheric environment.

### Key Features

- **⚔️ VR Melee Combat**: Intuitive sword fighting with katana weapons  
- **📈 Progressive Difficulty**: Enemies become more challenging as players advance deeper
- **👹 Atmospheric Design**: Glitch textures and jump scares create an immersive horror atmosphere
- **🎪 Boss Battle**: Epic encounter with unique mechanics including flickering and teleportation
- **🚶 VR Movement**: Full VR locomotion and interaction systems using XR Interaction Toolkit

## 🛠️ Technical Specifications

- **Engine**: Unity 6000.0.38f1
- **VR Platform**: SteamVR with XR Interaction Toolkit 3.0.8
- **Target Platforms**: PC VR (SteamVR compatible headsets)
- **Rendering**: Low-poly 3D graphics optimized for VR performance
- **Audio**: 3D spatial audio support

### System Requirements

- **VR Headset**: SteamVR compatible device (HTC Vive, Oculus Rift, Valve Index, etc.)
- **OS**: Windows 10/11
- **Unity Version**: 2022.3 LTS or newer
- **Graphics**: DirectX 11 compatible GPU
- **RAM**: 8GB minimum, 16GB recommended

## 🚀 Installation & Setup

### For Developers

1. **Clone the Repository**
   ```bash
   git clone https://github.com/christoph1j2/IKT_VR-game.git
   cd IKT_VR-game
   ```

2. **Unity Setup**
   - Open Unity Hub
   - Click "Open" and select the project directory
   - Ensure Unity 6000.0.38f1 or compatible version is installed
   - Allow Unity to import all assets (this may take several minutes)

3. **SteamVR Configuration**
   - Install SteamVR through Steam
   - Connect and set up your VR headset
   - Open SteamVR Input window in Unity (Window → SteamVR Input)
   - Click "Yes" to copy example JSONs, then "Save and Generate"

4. **Build & Run**
   - Open the MainScene in Assets/Scenes/
   - Press Play in Unity Editor for testing, or build for standalone VR deployment

## 🏗️ Project Architecture

### Core Systems

- **Enemy Management**: Spawn system with health and melee damage components
- **Player Systems**: VR movement, health management, and weapon interaction
- **Level Generation**: Procedural room and encounter generation
- **UI Systems**: VR-optimized health display and menu controls
- **Boss Mechanics**: Specialized AI behaviors including teleportation and visual effects

### Key Scripts

- `EnemySpawnManager.cs` - Handles dynamic enemy spawning
- `VRPlayerMovement.cs` - VR locomotion and interaction
- `BossController.cs` - Boss AI and special abilities
- `WeaponDamage.cs` - Melee combat damage calculation
- `HealthUI.cs` - VR health visualization
- `DoorController.cs` - Level progression mechanics

## 🎨 Assets & Content

- **3D Models**: Katana weapons, environment pieces, character models
- **Audio**: Spatial audio effects and atmospheric sounds
- **Shaders**: Custom low-poly and glitch effect shaders
- **Textures**: Sky boxes and environmental textures from SkySeries

## 🔧 Development Status

This project demonstrates modern VR game development practices including:
- Modular component-based architecture
- VR-specific UI/UX design patterns
- Performance optimization for VR rendering
- Integration of multiple Unity packages and VR SDKs

## 📸 Screenshots

*Screenshots and gameplay videos coming soon*

## 🤝 Contributing

This project was developed as part of an educational initiative. While not actively seeking contributions, developers interested in VR game development are welcome to explore the codebase and implementation patterns.

## 📄 License

This project is educational and uses various Unity packages and third-party assets. Please refer to individual asset licenses for usage rights.

---

**Note**: This is a Unity VR project requiring appropriate VR hardware and software setup. Ensure all dependencies are properly installed before attempting to run the project.

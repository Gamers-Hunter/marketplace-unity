# Changelog
---

# 1.0.0
- Developed and tested with Unity 2022.3.4f1 (LTS)
- Support for portrait and landscape Mobile apps (iOS and Android)
- Hunter Editor tools
  - Context menus
  - Easy market button creation and configuration
  - Documentation and developer portal links
  - Automatic settings asset creation
- Marketplace
  - Full button support and customization
  - Automatic and manual HunterCoin animations
  - Canvas detection/creation
  - Full Setup and Marketplace webviews (iOS, Android, and MacOS player)
    - Missing Windows player support
    - SDK bridge
  - Full server based security
- Analytic events support
- Session start, stop and checkpoint support
- Online and offline request functionality
- Internal
  - Transition animations
    - Missing alpha transitions on webviews
    - Initialization and sync cache
    - SDK activity logging
  - Unity Dependencies:
    - [Newtonsoft Json 13.0.2](https://docs.unity3d.com/Packages/com.unity.nuget.newtonsoft-json@3.1/manual/index.html)
  - Built in plugins:
    - [Gree WebView](https://github.com/gree/unity-webview)

# 1.1.0
- Developed and tested with Unity 2022.3.4f1
  - Fixes andorid burst errors
- Updated MarketButton component
  - Added Coin size, time and spread options
  - Added optional coin sprite
- Updated transition to dynamic target
  - Added scaledown transition to coins
- Marketplace
  - Added partner checkout support
  - Removed setup texture

# CrowdedSherrif

Very small [BepInEx](https://github.com/BepInEx/BepInEx/) plugin adding Sheriff role in the game
Also this codebase *isn't perfect* because it was written for less than 24 hours

# How to Install
1. [Download](https://builds.bepis.io/projects/bepinex_be) latest BepInEx x86 Il2Cpp Release and extract it in the game folder
2. Get latest mod version from [releases]() (soon)
3. Put it in `Among Us/BepInEx/plugins`

# How to use
1. Everyone in the lobby *must* have this mod
2. Build [server](https://github.com/Galster2010/Impostor/) or [get latest release](https://github.com/Galster2010/Impostor/releases/latest) and set it up
3. Connect to server via [CustomServersClient](https://github.com/andruzzzhka/CustomServersClient/) or [these instructions](https://impostor.github.io/Impostor/)
4. Enjoy the game

### How to disable
- If you want to disable only this mod - put its `.dll` somewhere else
- If you want to disable BepInEx at all but save it for future - rename `winhttp.dll` in your game root directory

# Building
- Create directory `libs` in project root and put all required libraries (from BepInEx) there
- Build

# TODO
 - [x] Make sheriff's target die too (customizable)
 - [ ] Make ^ look better
 - [ ] Improve codebase
 - [ ] Add support for other roles ??

# Special thanks
- [@XtraCube](https://github.com/XtraCube) helping with custom menu options
- [@przebor](https://github.com/przebor) common helping
- [@dimaguy](https://github.com/dimaguy) helping with server
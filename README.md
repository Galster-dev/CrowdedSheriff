# Warning!
This mod is deprecated. It supports 2020.12.9s only and not going to be updated. Watch [CrowdedMods](https://github.com/CrowdedMods) for other cool mods

# CrowdedSherrif
Very small [BepInEx](https://github.com/BepInEx/BepInEx/) plugin adding Sheriff role in the game\
Also this codebase *isn't perfect* because it was written for less than 24 hours

# How to Install
## Method 1
1. [Download](https://builds.bepis.io/projects/bepinex_be) latest BepInEx x86 Il2Cpp Release and extract it in the game folder
2. Get latest `.dll` from [releases](https://github.com/Galster2010/CrowdedSheriff/releases)
3. Put it in `Among Us/BepInEx/plugins` 
4. (Optional) Remove any other role mods from there, since this mod doesn't support cross-roles
## Method 2
1. Get latest `.zip` from [releases](https://github.com/Galster2010/CrowdedSheriff/releases)
2. Extract it in game root directory

# How to use
1. Everyone in the lobby *must* have this mod
2. ~~Your server has to be patched (as for now) to allow non-impostors to kill. To do this build [this server](https://github.com/Galster2010/Impostor/) or [get latest release](https://github.com/Galster2010/Impostor/releases/latest) and set it up~~\
   Now you can play on any server, even on official (which i do not support)
3. (if required) Connect to server via [CustomServersClient](https://github.com/andruzzzhka/CustomServersClient/) or [these instructions](https://impostor.github.io/Impostor/)
4. Enjoy the game

### How to disable
- If you want to disable only this mod - put its `.dll` somewhere else
- If you want to disable BepInEx at all but save it for future - rename `winhttp.dll` in your game root directory

# FAQ
- Q: How do i see if mod is loaded correctly?
- A: Among Us version should become orange in main menu
<br><br>
- Q: "Sheriff count" is always 0
- A: The host doesn't have this mod or just don't wanna play with sheriff lul
<br><br>
- Q: My screen turned black/my kill button disappeared/anything else
- A: This is probably a bug, please open an [issue]() and provide more info
<br><br>
- Q: Why if we have "Sheriff's target dies" on it shows i killed myself when sheriff kills me?
- A: It is a temporary solution to not stack bodies in one spot, because snapping to target's position is hard-coded in Among Us

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
- [@Woodi-dev](https://github.com/Woodi-dev) idea how to pass anticheat without patching the server *(also thanks for nickname suffix)*
- [@dimaguy](https://github.com/dimaguy) helping with server

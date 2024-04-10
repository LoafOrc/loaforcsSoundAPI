## v1.0.0!!!
It's taken a bit of time but it's here.

Added:
- Conditions, you're now able to make your sounds react better to what's happening in game.
- `"update_every_frame"`. A new option that lets you swap out a sound as its playing, it will also respect the amount of time that has elapsed.

Changed:
- Changed to a thread pool based multithreading system.
 - Intro Skipper mods should work better.
 - Crashes should also be fixed.
 - Genuinely wild speedup. LethalResonance's non-startup sounds took 12 seconds to load on v0.x. It now takes 2 (for me at least, your numbers will vary a lot)

Fixed:
- Added more null checks, should be more compatible with LittleCompany.

### v1.0.2
- Fixed an issue where AudioSourceReplaceHandler would try to unregister itself multiple times.

### v1.0.3
- Maybe registering stuff would help.

### v1.0.4
- Temporary debug logs for conditions

### v1.0.5
- More null checks

### v1.0.6
- :skull:

<details>
<summary>Old Versions</summary>

## v0.1
- Added basic condition system
- Added support for soundpacks to have custom configs
- Fixed an issue where the voice chat could have been broken (seems to work now).

## v0.1.1
- Fixed an issue where the ship thrusters were silent
- More processing done on matching strings: `Flame (3):ThrusterCloseAudio (1):ShipThrusterClose` to `Flame:ThrusterCloseAudio:ShipThrusterClose`

## v0.1.2
- Fixed an issue where if two sound replacers replaced a sound *and* the one that loaded first was disabled due to a config, the second enabled one couldn't play

## v0.1.3
- Finally fixed the changelog not being placed correctly on the mod page
- Put a quick warning message for advanced company users.

## v0.0
- Inital release

### v0.0.2
- Fixed an issue where debug logs were happening twice
- Fixed an issue where `Ship3dSFX` wasn't being picked up correctly

### v0.0.3
- Fixed an issue where soundpacks wouldn't load if they were unpacked by a mod manager

</details>
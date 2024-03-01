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
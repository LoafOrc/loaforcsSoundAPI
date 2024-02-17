# loaforcsSoundAPI
A soundtool made to give Sound-Pack creators ultimate control, and users the ultimate experience.

> [!IMPORTANT]
> Absolutely not meant as a replacement for [LCSoundTool](https://thunderstore.io/c/lethal-company/p/no00ob/LCSoundTool/) and [CustomSounds](https://thunderstore.io/c/lethal-company/p/Clementinise/CustomSounds/). This sound replacement API is *much, much* more complex than either of those and is meant for advanced users.

## Ultimate Experience
loaforcsSoundAPI makes use of Multithreading, splitting sound loading into two steps. With very little effort [LethalResonance](https://thunderstore.io/c/lethal-company/p/LethalResonance/LETHALRESONANCE/) was decreased from 4 seconds of time during startup to just under 250 ms.

## Ultimate Control
loaforcsSoundAPI affords a Sound-Pack creator much more control over how their sounds are played and how they organise. 
The main benefit of loaforcsSoundAPI's custom sound loading system is being able to customise the internal folder structure of your mod much more finely. With CustomSounds it required a folder structure of `YourMod/CustomSounds/YourMod/<all_sounds>`, you could only go a single subfolder deep and each sound file had to be named *exactly* what it was ingame. loaforcsSoundAPI removes this limitation by defining the mappings via `.json` files so you can do the following `YourMod/sounds/<go_hogwild_doesnt_matter>` and the path will just resolve.
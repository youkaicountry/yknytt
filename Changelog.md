All notable changes to YKnytt will be documented here.

## [0.3.0] - 2021-01-22

This is a huge release featuring many exciting new changes. Lots of new objects, Knytt Story Plus support,
a level downloader, improved physics, bugfixes, and optimizations.

## Added
- Shift quantization
- Bullet System
- Many new objects
- Volume settings
- More built-in official worlds
- Knytt Stories Plus support
- Golden Creatures (KS+)
- Multiple message signs (KS+)
- Level map (KS+)
- Raw Audio cache
- Triggers (KS+)
- Flag(All) (KS+)
- Invulnerable debug mode
- Artifacts (KS+)
- Autoplay (KS+)
- Level Downloader
- Welcome Screen (KS+)
- Delayed Shift (KS+)

## Fixed
- Invisible barrier was not blocking movement properly
- Juni now spawns perfectly at ground level
- Juni's double jump counter was not resetting occasionally
- C# project and solution names had incorrect case
- Fix info panel showing 2 blue keys, and not the purple key
- CallDeferred on several methods to prevent crashes

## Changed
- X Button on window and back button on mobile now close the main menu
- Stationary animated objects now dervice from a base class
- Optimized initial tileset loading
- World selection screen layout
- Better slope traversal
- UI skin for sliders
- Tweak Umbrella physics
- Tweak Water hit boxes
- Optimize makeTileset
- Imprtove Juni movement physics
- Improve jumping logic
- Knytt Stories Plus icons in info panel
- Touchscreen controls improved
- Knytt Stories Plus controls column added to input config screen
- YKnyttLib is now a submodule
- World slot button coloration

## Contributors
- up-left
- youkaicountry
- akien-mga

## [0.2.0] - 2020-10-20

This release features many new object implementations, touchscreen support, and various fixes and optimizations.
It also adds built-in versions of the official worlds Sky Flower and Gustav's Daughter

## Added
- Invisible barrier
- Fly Spike enemy
- Shift object
- Warp object
- No Wall object
- Ghost Marker
- Proximity Blocks
- Spring
- Updraft
- Cat
- "Sky Flower" and "Gustav's Daughter" worlds now built-in
- Walk / Climb particle spawning
- Cloud object
- Yellow Dog
- Red Follow Ball Enemy
- Buzzing Flying Monsters
- Touchscreen controls [Contribution by: up-left]
- Trap Ceiling
- Various decorations
- Elementals [Contribution by: up-left]
- Circle Bird [Contribution by: up-left]
- Add star

## Fixed
- Rain drops now spawn down a bit (They were colliding with the tile above)
- Enemy detector now works properly when the hologram is deployed
- Minor jumping issues fixed
- "The Machine" and "Tutorial" worlds were not properly built-in

## Changed
- More accurate distance checks
- Clean up UI scenes
- Fix some issues with area paging
- Major object framework refactor
- Refactor raindrop spawning to use object pools

## [0.1.0] - 2020-09-20

## Added

- Main Menu
- Settings Menu
- Key configuration
- Load .bin worlds
- Create physics objects from tilesets
- Asset caching
- Implement all powers
- Implement all enemies in tutorial
- Implement all objects in tutorial
- Smooth scrolling area transitions
- Pause menu

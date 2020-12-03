All notable changes to YKnytt will be documented here.

## [Unreleased]

## Added
- Shift quantization
- Sun ray
- Stationary Knytts [Contribution by: up-left]
- Fluff [Contribution by: up-left]
- Rock [Contribution by: up-left]
- Bubbles
- Fish
- Scene particle system
- Leaves
- Water Monster
- Spike Traps [Contribution by: up-left]
- Drop Traps [Contribution by: up-left]
- Lasers [Contribution by: up-left]
- Cannons [Contribution by: up-left]
- Bullet System [Contribution by: up-left]
- Password / Locks [Contribution by: up-left]
- Object Selector [Contribution by: up-left]
- Invisible Death Zones
- Invisible Slopes
- Volume settings

## Fixed
- Invisible barrier was not blocking movement properly
- Juni now spawns perfectly at ground level
- Juni's double jump counter was not resetting occasionally
- C# project and solution names had incorrect case [Contribution by: akien-mga]
- Fix info panel showing 2 blue keys, and not the purple key

## Changed
- X Button on window and back button on mobile now close the main menu
- Stationary animated objects now dervice from a base class [Contribution by: up-left]
- Optimized initial tileset loading [Contribution by: up-left]
- World selection screen layout
- Better slope traversal

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

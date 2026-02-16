# Console Commands Reference (Temporary - Delete After Use)

## System Architecture

- **Console.cs** — Main UI console (CanvasLayer), input handling, message display
- **ConsoleCommands.cs** — Command definitions and implementations via builder pattern
- **ConsoleExecutionEnvironment.cs** — Execution context with access to command parser and console instance

### Registration Pattern
```csharp
cs.AddCommand(new CommandDeclaration(
    name: string,
    description: string,
    detailed_description: string or null,
    hidden: bool,
    instantiation: CommandInstantiation delegate,
    params CommandArg[] args  // FloatArg, StringArg, IntArg, BoolArg — each can be optional
));
```

Each command is a nested partial class implementing `ICommand` with `string Execute(object environment)`.
Environment is cast to `ConsoleExecutionEnvironment` to access `Console`, `Game`, `Juni`, etc.

---

## All 25 Commands

### 1. speed
- **Params:** `value` (float, optional)
- **Range:** 0.1–5.0
- **Does:** View/set game speed multiplier via `GDKnyttDataStore.CurrentSpeed`
- `speed` → prints current; `speed 2` → 2x fast-forward

### 2. help
- **Params:** `name` (string, optional)
- **Does:** Show help for a command or general help

### 3. list
- **Params:** none
- **Does:** Lists all registered commands with descriptions

### 4. save
- **Params:** `subcmd` (string, optional: "print", "copy", "paste", "backup")
- **Does:**
  - `save` — save at current position
  - `save print` — print save file contents to console
  - `save copy` — copy save to clipboard
  - `save paste` — load save from clipboard
  - `save backup` — zip all saves to `~/Documents/yknytt-saves.zip`

### 5. pass
- **Params:** `subcmd` (string, optional: "copy", "paste")
- **Does:** Compressed password save system
  - `pass` — print password
  - `pass copy` — copy to clipboard
  - `pass paste` — load from clipboard
- Uses `KnyttUtil.CompressString()`/`DecompressString()`

### 6. set
- **Params:** `power` (string), `value` (string: "on"/"off"/"1"/"0")
- **Valid powers:** run, climb, doublejump, highjump, eye, enemydetector, umbrella, hologram, redkey, yellowkey, bluekey, purplekey, map
- **Valid flags:** flag0–flag9
- **Special bulk:** `set powers 1100000000000` (13-char binary), `set flags 0000000000` (10-char binary)
- **Does:** Modifies `Juni.Powers`, calls `game.sendCheat()`

### 7. mon
- **Params:** `subcmd` (string, optional: "flash", "flags", "off")
- **Does:** Monitor Juni's position/flags
  - `mon` — continuous position display
  - `mon flash` — only show on value change
  - `mon flags` — also show flags
  - `mon off` — disable

### 8. idclip
- **Params:** `on` (bool, optional)
- **Does:** No-clip mode. Sets `Juni.DebugFlyMode`, free movement with arrows.
- Toggles if no param given.

### 9. iddqd
- **Params:** `on` (bool, optional)
- **Does:** Invulnerability. Sets `Juni.Immune`.
- Toggles if no param given.

### 10. map
- **Params:** `on` (bool, optional)
- **Does:** Force-enable map display. Sets `game.UI.ForceMap` and grants map power.
- Toggles if no param given.

### 11. shift
- **Params:** `xMap` (int), `yMap` (int), `xPos` (int, opt), `yPos` (int, opt)
- **Does:** Move Juni RELATIVE to current position.
- Uses `KnyttShift` with `AbsoluteTarget = false`
- `shift 0 1` → one area down; `shift 1 0 50 50` → one area right, position at (50,50)

### 12. goto
- **Params:** `xMap` (int), `yMap` (int), `xPos` (int, opt), `yPos` (int, opt)
- **Does:** Move Juni to ABSOLUTE map position.
- Uses `KnyttShift` with `AbsoluteTarget = true`
- `goto 1001 1000` → teleport to area (1001, 1000)

### 13. world
- **Params:** `world` (string), `intro` (bool, opt)
- **Does:** Load a world from `res://knytt/worlds/` or `<DataDirectory>/Worlds/`
- `world myworld` or `world myworld true`

### 14. col (debug only)
- **Params:** none
- **Hidden:** yes (only in debug builds)
- **Does:** Toggle collision shape visualization via `Tree.DebugCollisionsHint`

### 15. trail
- **Params:** `size` (int, opt), `frames` (int, opt)
- **Does:** Toggle debug movement trails. Controls `game.Trails.On`, `TrailCount`, `TrailFrames`.
- `trail` → toggle; `trail 20 30` → set size/frames then toggle

### 16. death
- **Params:** none
- **Does:** Toggle death markers via `game.Deaths.On`

### 17. reboot
- **Params:** none
- **Does:** Reload world from latest save. Calls `refreshWorld()` then `startGame(false)`.

### 18. youtube
- **Params:** none
- **Does:** Opens browser to YouTube search for the current world's playthrough.
- URL: `https://www.youtube.com/results?search_query=Knytt+Stories+<world_name>`

### 19. settings
- **Params:** `section` (string, opt), `name` (string, opt), `value` (string, opt)
- **Does:** View/modify settings.ini
  - `settings` → print entire file
  - `settings copy` → copy to clipboard
  - `settings Graphics Resolution 1280x720` → set specific value

### 20. hell
- **Params:** none
- **Does:** Teleport to hardest place (most deaths). Shows stats: total time, total deaths, hardest location, death count there.

### 21. exit
- **Params:** none
- **Does:** Hide console. Calls `console.toggleConsole()`.

### 22. quit (hidden alias)
- **Params:** none
- **Does:** Same as exit.

---

## Console UI Details

- **Hotkey:** `debug_console` input action
- **History:** Up/Down arrows scroll through command history
- **Pre-populated history:** exit, list, reboot, mon, trail, shift 0 0 2 0, set highjump on, save print, hell, speed 0.1, speed 1, speed 0.5, iddqd, idclip, map, mon flags, save paste, save copy, youtube, save
- **History buffer:** 256 lines
- **Slide speed:** 5.0 (export)
- **Mobile:** Virtual keyboard button and history button on touch
- **Output:** RichTextLabel with BBCode color support

## Console.cs Key Properties/Exports
- `HistoryLength` (int, default 256)
- `SlideSpeed` (float, default 5.0)
- `IsOpen` (bool) — whether console is currently visible

## Key Methods
- `toggleConsole()` — show/hide
- `addMessage(string text)` — add output line
- `addFormattedMessage(string bbcode)` — add colored output

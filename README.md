## YKnytt

![image](screenshots/cover.png)

An open source implementation of Knytt Stories in C# using Godot Engine

### Screenshots

![image](screenshots/screen6.png)

![image](screenshots/screen5.png)

![image](screenshots/screen3.png)

![image](screenshots/screen4.png)

![image](screenshots/screen1.png)

### Building

Requires Godot 3.5.2.stable.mono and .NET Framework 4.8 Developer Pack for Windows (or mono-devel for Linux)

Also has Godot 4 branch in development: this one requires Godot 4.0.2.stable.mono and .NET SDK 6 or 7

#### Export settings

To properly export, `knytt/worlds/*` must be included in the export filters

To build a proper Android APK, activate the "Internet" permission.

### Playing

There must be a worlds directory in the project root (or next to the binary if you have exported).

Place packed (.knytt.bin) or unpacked worlds in that directory.

You can also download additional levels from the in-game level downloader (if our server is running)
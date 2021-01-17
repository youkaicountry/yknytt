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

Requires Godot 3.2.3.stable.mono

Make sure to fetch / update submodules:

`git submodule update --init --recursive`

#### Export settings

To properly export, `*.raw, knytt/worlds/*` must be included in the export filters

To build a proper Android APK, activate the "Internet" permission.

### Playing

There must be a worlds directory in the project root (or next to the binary if you have exported).

Place packed (.knytt.bin) or unpacked worlds in that directory.

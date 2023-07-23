## YKnytt

![image](screenshots/cover.png)

An open source implementation of Knytt Stories in C# using Godot Engine

[Download YKnytt v0.5 beta for Android](https://github.com/youkaicountry/yknytt/releases/download/0.5.1/YKnytt_v0.5.1.apk)

[Download YKnytt v0.5 beta for Windows](https://github.com/youkaicountry/yknytt/releases/download/0.5.1/YKnytt_v0.5.1_win.zip)

[Download YKnytt v0.5 beta for Linux/X11](https://github.com/youkaicountry/yknytt/releases/download/0.5.1/YKnytt_v0.5.1_linux.zip)

YKnytt for Web: coming soon

### Screenshots

![image](screenshots/screen6.png)

![image](screenshots/screen5.png)

![image](screenshots/screen3.png)

![image](screenshots/screen4.png)

![image](screenshots/screen7.png)

### Reporting a broken level

Feel free to report any bug you found in levels on the issues page. It would be great if you provide us a brief description and attach a save file.

<details>
<summary>Where to find save files</summary>

On Windows: `Users/[User]/AppData/Roaming/Godot/app_userdata/YKnytt/Saves/`

On Linux: `~/.local/share/godot/app_userdata/YKnytt/Saves/`

On mobiles: open the console by pressing [down] + [info] + [jump] simultaneously, and type `save copy`. You will get a save file in your clipboard. In a similar way you can paste a file from the clipboard by `save paste`.
</details>

### Building

Requires Godot 3.6.beta2.mono and .NET Framework 4.8 Developer Pack for Windows (or mono-devel for Linux)

#### Export settings

To properly export, `knytt/worlds/*` must be included in the export filters

To build a proper Android APK, activate the "Internet" permission.

### Playing

You can download additional levels from the built-in level downloader (if our server is running).

Or you can place packed (.knytt.bin) or unpacked levels in `worlds` directory.

That directory must be next to the binary file (or in the project root).

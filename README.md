## YKnytt

![image](screenshots/cover.png)

An open source implementation of Knytt Stories in C# using Godot Engine

[Download YKnytt v0.6 beta for Android](https://github.com/youkaicountry/yknytt/releases/download/0.6.0/YKnytt_v0.6.apk)

[Download YKnytt v0.6 beta for Windows](https://github.com/youkaicountry/yknytt/releases/download/0.6.0/YKnytt_v0.6_win.zip)

[Download YKnytt v0.6 beta for Linux](https://github.com/youkaicountry/yknytt/releases/download/0.6.0/YKnytt_v0.6_linux.zip)

YKnytt for Web: coming soon

### Screenshots

![image](screenshots/screen6.png)

![image](screenshots/screen5.png)

![image](screenshots/screen3.png)

![image](screenshots/screen4.png)

![image](screenshots/screen7.png)

### Reporting a broken level

If you find a bug in a level, just press "mark as broken" on a level info screen. Your latest save will be sent to the server. There is no need to report it in issues, unless it requires some explanation.

### Building

Requires Godot 3.6.beta2.mono and .NET Framework 4.8 Developer Pack for Windows (or mono-devel for Linux)

#### Export settings

To properly export, `knytt/worlds/*` must be included in the export filters

To build a proper Android APK, activate the "Internet" permission.

### Playing

You can download additional levels from the built-in level downloader (if our server is running).

Or you can place packed (.knytt.bin) or unpacked levels in `worlds` directory.

That directory must be next to the binary file (or in the project root).

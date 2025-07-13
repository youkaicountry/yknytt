## YKnytt

![image](screenshots/cover.png)

An open source implementation of Knytt Stories in C# using Godot Engine

[Download YKnytt v0.6.8 beta for Android](https://github.com/youkaicountry/yknytt/releases/download/0.6.8/YKnytt_v0.6.8.apk)

[Download YKnytt v0.6.8 beta for Windows](https://github.com/youkaicountry/yknytt/releases/download/0.6.8/YKnytt_v0.6.8_win.zip)

[Download YKnytt v0.6.8 beta for Linux](https://github.com/youkaicountry/yknytt/releases/download/0.6.8/YKnytt_v0.6.8_linux.zip)

[Web version: play YKnytt v0.6.5 on itch.io](https://youkaicountry.itch.io/yknytt)

### Screenshots

![image](screenshots/screen6.png)

![image](screenshots/screen5.png)

![image](screenshots/screen3.png)

![image](screenshots/screen4.png)

![image](screenshots/screen7.png)

![image](screenshots/screen9.png)

![image](screenshots/screen8.png)

![image](screenshots/screen11.png)

### Reporting a broken level

If you find a bug in a level, just press "complain" on a level info screen. Your latest save will be sent to the server. There is no need to report it in issues, unless it requires some explanation. Results may appear [here](https://github.com/youkaicountry/yknytt/issues/200).

### Working directory

/Users/{user}/AppData/Roaming/YKnytt/ for Windows, ~/.local/share/YKnytt/ for Linux

### Building

Requires Godot 3.6.1.stable.mono and .NET Framework 4.8 Developer Pack for Windows (or mono-devel for Linux)

#### Export settings

To properly export, `knytt/worlds/*` must be included in the export filters

To build a proper Android APK, activate the "Internet" permission.

#!/bin/bash
# Abridged version of instructions in https://docs.google.com/document/d/1gP1deupAZ0Gui_Rh0Bv3-efmi-TDItu04pMQYQuZNKc/edit?tab=t.v4s87snt5pdq

# 1
#~/apps/pck-explorer/GodotPCKExplorer.Console -e YKnytt-old.pck unpacked skip
#cp unpacked/.mono/assemblies/Release/GodotSharp.dll unpacked/.mono/assemblies/Release/GodotSharp.pdb .

# 2
# Then create Linux export preset with release template = godot.frt.opt.mono, architecture = arm64, filters to export = knytt/worlds/*,override_arm64.cfg

# 3
rm -r unpacked
~/apps/pck-explorer/GodotPCKExplorer.Console -e ../YKnytt_v0.7.1_linux_arm64/YKnytt.pck unpacked skip
cp GodotSharp.dll GodotSharp.pdb unpacked/.mono/assemblies/Release
~/apps/pck-explorer/GodotPCKExplorer.Console -p unpacked YKnytt.pck 1.3.6.1

# 4 Copy pck to old directory on console
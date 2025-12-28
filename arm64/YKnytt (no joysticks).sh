#!/bin/bash

# Use this if some gamepad-to-keyboard software is running
BASEDIR=$(dirname "$0")/yknytt
LD_LIBRARY_PATH=$BASEDIR:$LD_LIBRARY_PATH $BASEDIR/godot.frt.opt.mono.nojoy --main-pack $BASEDIR/YKnytt.pck

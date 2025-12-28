#!/bin/bash

BASEDIR=$(dirname "$0")/yknytt
LD_LIBRARY_PATH=$BASEDIR:$LD_LIBRARY_PATH $BASEDIR/godot.frt.opt.mono --main-pack $BASEDIR/YKnytt.pck

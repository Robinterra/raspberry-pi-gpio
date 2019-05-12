#! /bin/bash
mkdir bin/
mkdir log/
./work.sh --make --target library --out "bin/raspberry-pi-gpio.dll" --log log/build_log.txt --src src/ $@
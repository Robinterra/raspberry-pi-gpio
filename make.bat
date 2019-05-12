if not exist "bin\" mkdir bin
if not exist "log\" mkdir log
work.bat --make --target:library --version "--out:bin\raspberry-pi-gpio.dll" "--log:log\build_log.txt" "--src:src\" %*
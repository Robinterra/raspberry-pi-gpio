if not exist "bin\" mkdir bin
if not exist "log\" mkdir log
work.bat --make --target:library --version "--out:bin\Shortcuts.dll" "--test:Testing.ProgramTest" "--testpath:test\stern.cs" "--log:log\build_log.txt" "--src:src\" %*
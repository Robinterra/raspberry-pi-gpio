#! /bin/bash

./work.sh --make --target library --out "bin/grimoire.dll" --log log/build_log.txt --src src/cs/Debug/ --test "Versalitic.Testing.ProgramTest" --inc "src/cs/IO/\*.cs" --testpath "test/cs/\*.cs" $@
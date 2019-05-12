@echo off
if defined DEBUG_LOGFILE echo on

REM ----------------------------------------------------------------------------------------------
REM @author     :   Robin D'Andrea
REM @date       :   06.11.2017
REM @syntax     :   Rufe --help auf um die Kommandozeilenparameter Aufzurufen
REM @see        :   http://wiki.versalitic.int/index.php/C-Sharp_make.bat
REM ----------------------------------------------------------------------------------------------

setlocal

echo %0 %*

REM ----------------------------------------------------------------------------------------------

REM /**
REM  * Self Variablen
REM  */

REM - Laufwerksbuchstaben
set SELF_DRIVE=%~d0

REM - Eigener Dateiname
set SELF_NAME=%~nx0

REM - Eigner Pfad
set SELF_PATH=%~p0
set SELF_PATH=%SELF_PATH:~0,-1%
set SELF_PATH=%SELF_DRIVE%%SELF_PATH%

REM - Eigenen Verzeichnisnamen
for %%i in ("%SELF_PATH%") do set SELF_DIRNAME=%%~ni

REM ----------------------------------------------------------------------------------------------

REM /**
REM  * Init Variablen
REM  */
set CSC_EXE=%windir%\Microsoft.NET\Framework64\v4.0.30319\csc.exe
set DOXYGEN_EXE=
set DEFAULT_RESGEN_EXE=%ProgramFiles(x86)%\Microsoft SDKs\Windows\v7.0A\bin\resgen.exe
set RESGEN_EXE=resgen.exe
set GIT_EXE=%HOMEDRIVE%\App\Git\bin\git.exe
set JUST_HELP=
set JUST_DEBUG=
set JUST_MAKE=
set JUST_VERSION=
set THE_TEST=
set THE_TESTPATH=
set THE_MAIN=
set THE_LOGFILE=
set THE_OUT=
set THE_SRC=
set THE_DOC=
set THE_TARGET=
set THE_DLL=
set THE_INC=
set THE_COMMENT=
set RC_gitExeNotFound=1000
set RC_gitCommitFailed=2000
set RC_doxygenExeNotFound=3000
set RC_doxygenFailed=4000
set RC_cscExeNotFound=5000
set RC_testFailed=6000
set RC_ResgenExeNotFound=7000
set RC=0

REM ----------------------------------------------------------------------------------------------

REM /**
REM  * Kommandozeilen Parameters einlesen
REM  */

set ARGV=%*
for %%i in (%ARGV%) do call:parseCommandlineArgument %%i

REM ----------------------------------------------------------------------------------------------

REM /**
REM  * Main
REM  */

    REM - Der Aufruf der Hilfe, danach wird das Programm beendet
    if defined JUST_HELP call:hilfe
    if defined JUST_HELP goto:ausstieg

    REM - RESGEN
    call:locateExe RESGEN_EXE "%RESGEN_EXE%" "%DEFAULT_RESGEN_EXE%"
    set RC=%ERRORLEVEL%
    if defined JUST_VERSION if %RC% neq 0 call:abort "Die resgen.exe wurde nicht gefunden" %RC_ResgenExeNotFound%&exit /b %ERRORLEVEL%

    REM - Doxygen
    if not defined DOXYGEN_EXE call:locateDoxygenExe DOXYGEN_EXE
    set doxyerror=0
    if not defined DOXYGEN_EXE set doxyerror=1
    if defined THE_DOC set /a doxyerror=%doxyerror%*2
    if %doxyerror% equ 2 call:abort "Die Doxygen.exe wurde nicht gefunden" %RC_doxygenExeNotFound%&exit /b %ERRORLEVEL%
    if defined THE_DOC call:doxygen "%THE_DOC%"
    set RC=%ERRORLEVEL%
    if %RC% neq 0 call:abort "Doxygen ist fehlgeschlagen" %RC_doxygenFailed%&exit /b %ERRORLEVEL%

    REM - Pruefen ob Git.exe gefunden werden kann und wenn commit ausgeuehrt werden soll und die git.exe nicht gefunden werden kann, dann beende das Programm
    if not defined GIT_EXE call:locateGitExe GIT_EXE
    set giterror=0
    if not defined GIT_EXE set giterror=1
    if defined THE_COMMENT set /a giterror=%giterror%*2
    if %giterror% equ 2 call:abort "Die Git.exe konnte nicht gefunden werden" %RC_gitExeNotFound%&exit /b %ERRORLEVEL%
    if defined THE_COMMENT call:commit "%THE_COMMENT%"
    set RC=%ERRORLEVEL%
    if %RC% neq 0 call:abort "Der Commit war fehlerhaft" %RC_gitCommitFailed%&exit /b %ERRORLEVEL%

    REM - Die csc.exe finden
    if not defined THE_LOGFILE set THE_LOGFILE=build_log.txt
    set cscerror=0
    if not exist "%CSC_EXE%" set cscerror=1
    if defined JUST_MAKE set /a cscerror=%cscerror%*2
    if %cscerror% equ 2 call:abort "Die csc.exe konnte nicht gefunden werden" %RC_cscExeNotFound%&exit /b %ERRORLEVEL%

    REM - Der Test vorgang
    if defined THE_TEST call:test "%THE_OUT%" "%THE_SRC%" "%THE_DLL%" "%THE_LOGFILE%" "%THE_INC%" "%JUST_DEBUG%" "%THE_TEST%" "%THE_TESTPATH%"
    set RC=%ERRORLEVEL%
    if %RC% neq 0 call:showBuildLog "%THE_LOGFILE%_test.txt"&call:abort "Der Test ist fehlgeschlagen" %RC_testFailed%&exit /b %ERRORLEVEL%

    REM - Erstelle eine Ressourcedatei in der die Versionsnummer enthalten ist
    if defined JUST_VERSION call:AddVersion

    REM - Der Make vorgang
    if defined JUST_MAKE call:make "%THE_OUT%" "%THE_SRC%" "%THE_DLL%" "%THE_LOGFILE%" "%THE_INC%" "%JUST_DEBUG%" "%THE_MAIN%" "%THE_TARGET%" "%JUST_VERSION%"
    set RC=%ERRORLEVEL%
    if %RC% neq 0 call:showBuildLog "%THE_LOGFILE%"

goto:ausstieg

REM ----------------------------------------------------------------------------------------------

:parseCommandlineArgument
REM /**
REM  * Auswertung, die Kommandozeilen Parameter
REM  */
    set arg=%~1

    if "%arg%" equ "--help" (set JUST_HELP=true&goto:eof)

    if "%arg%" equ "--make" (set JUST_MAKE=true&goto:eof)

    if "%arg%" equ "--debug" (set JUST_DEBUG=true&goto:eof)

    if "%arg%" equ "--version" (set JUST_VERSION=true&goto:eof)

    set testarg=%arg:--csc:=%
    if "%testarg%" neq "%arg%" (set CSC_EXE=%testarg%&goto:eof)

    set testarg=%arg:--cscversion:=%
    if "%testarg%" neq "%arg%" (set CSC_EXE=%windir%\Microsoft.Net\Framework64\%testarg%\csc.exe&goto:eof)

    set testarg=%arg:--log:=%
    if "%testarg%" neq "%arg%" (set THE_LOGFILE=%testarg%&goto:eof)

    set testarg=%arg:--out:=%
    if "%testarg%" neq "%arg%" (set THE_OUT=%testarg%&goto:eof)

    set testarg=%arg:--src:=%
    if "%testarg%" neq "%arg%" (set THE_SRC=%testarg%&goto:eof)

    set testarg=%arg:--test:=%
    if "%testarg%" neq "%arg%" (set THE_TEST=%testarg%&goto:eof)

    set testarg=%arg:--dll:=%
    if "%testarg%" neq "%arg%" (set THE_DLL=%testarg%&goto:eof)

    set testarg=%arg:--commit:=%
    if "%testarg%" neq "%arg%" (set THE_COMMENT=%testarg%&goto:eof)

    set testarg=%arg:--git:=%
    if "%testarg%" neq "%arg%" (set GIT_EXE=%testarg%&goto:eof)

    set testarg=%arg:--main:=%
    if "%testarg%" neq "%arg%" (set THE_MAIN=%testarg%&goto:eof)

    set testarg=%arg:--doxy:=%
    if "%testarg%" neq "%arg%" (set DOXYGEN_EXE=%testarg%&goto:eof)

    set testarg=%arg:--testpath:=%
    if "%testarg%" neq "%arg%" (set THE_TESTPATH=%testarg%&goto:eof)

    set testarg=%arg:--doc:=%
    if "%testarg%" neq "%arg%" (set THE_DOC=%testarg%&goto:eof)

    set testarg=%arg:--target:=%
    if "%testarg%" neq "%arg%" (set THE_TARGET=%testarg%&goto:eof)

    set testarg=%arg:--inc:=%
    if "%testarg%" neq "%arg%" (set THE_INC=%testarg%&goto:eof)

    set arg=
    set testarg=
goto:eof

REM ----------------------------------------------------------------------------------------------

:hilfe
REM /**
REM  * Gibt die Hilfe in der Console aus
REM  */
setlocal

    echo.%SELF_NAME% "--cscversion:v4.0.30319" "--inc:inc\stern.cs" "--doxy:C:\Windows\doxygen.exe" "--git:C:\git\git.exe" "--commit:Bugfixes: foobar" --make "--doc:pro\doxyconfig" "--lib:/r:lib\mysql.dll /r:lib\pgsql.dll" "--out:bin\Pr0gram.exe" "--src:src\" "--log:log\log.txt"
    echo.
    echo. "--csc:^<filepath^>"        Der Pfad zur csc.exe
    echo. "--cscversion:^<version^>"  Die Version von csc. Kann verwendet werden, wenn es sich um eine Windows Standard installation handelt und .netFramework unter C:\Windows\Microsoft.NET\Framework64\[version] liegt
    echo. "--git:^<filepath^>"        Der Pfad zur git.exe
    echo. "--doxy:^<filepath^>"       Der Pfad zur doxygen.exe
    echo. --help                    Gibt die Hilfe aus
    echo. --make                    Startet den make vorgang
    echo. "--doc:^<filepath^>"        Den Pfad zur Config von doxygen und fuehrt doxygen aus
    echo. "--lib:^<dlls^>"            Eine Auflistung aller dlls
    echo. "--out:^<filepath^>"        Wie soll die Ausgabeexe heissen und wo soll sie liegen
    echo. "--src:^<path^>"            Wo der Srccode liegt
    echo. "--log:^<filepath^>"        Wo hin die build_log gespeichert werden
    echo. "--test:^<namespace.class^>"       Der namespace.class wo die Main Funktion fürs Testen liegt. Gibt das Programm nicht 0 wieder, wird das Compilieren nicht durchgeführt.
    echo. "--testpath:^<filepath^>"          Der Pfad wo die Dateien fürs Testen liegen
    echo. "--main:^<namespace.class^>"       Die Main Funktion für das Hauptprogramm
    echo. "--inc:^<paths^>"           Die Pfade wo weiterer quellcode liegt stern = ^*, bitte stern schreiben
    echo. "--target:^<target^>"       exe       Ausführbare Konsolendatei erstellen (Standard)
    echo.                             winexe    Ausführbare Windows-Datei erstellen
    echo.                             library   Bibliothek erstellen
    echo. --debug                    ^-define beim compiler mit LOGLEVEL_DEBUG
    echo. "--commit:^<comment^>"      Ein Commit durchfuehren (ausgeuehrt vor make aber nach doxygen) (git add .)^&(git commit -m "^<comment^>")
    echo.

    pause

endlocal&goto:eof

REM ----------------------------------------------------------------------------------------------

:showBuildLog
REM /**
REM  * Gibt den BuildLog_file auf dem Bildschirm aus
REM  */
setlocal
    set logfile=%~1
    if not defined DEBUG_LOGFILE cls

    REM Es wird kein "type %logfile%" verwendet, da ich die Zeilennummern haben möchte
    findstr /N /V /C:"praise the sun" "%logfile%"
    pause

endlocal&exit /b 0

REM ----------------------------------------------------------------------------------------------

:AddVersion
REM /**
REM  * Erstellt eine Ressourcedatei für die Versionsnummer
REM  */
setlocal

    set resourcefile=RESOURCES.txt
    if exist "%resourcefile%" (del "%resourcefile%")

    for /f "tokens=*" %%a in ('"%GIT_EXE%" tag -l v* 2^>NUL') do set version=%%a
    for /f "tokens=*" %%a in ('"%GIT_EXE%" rev-parse HEAD 2^>NUL') do set number=%%a
    echo.number=%number%>"%resourcefile%"
    echo.version=%version%>>"%resourcefile%"

    "%RESGEN_EXE%" "%resourcefile%"

endlocal&exit /b 0

REM ----------------------------------------------------------------------------------------------

:locateExe
REM /**
REM  * Findet eine exe
REM  * @param[out] result (string) Pfad zur exe
REM  * @param[in] anwendung (string) Der Name der Anwendung
REM  * @param[in] ersatzPath (string) Falls die Anwendung nicht auf direkt gefunden werden konnte, ein ersatz pfad in dem geschaut wird
REM  */
setlocal
    set result=
    set pathExe=
    set anwendung=%~2
    set ersatzPath=%~3

    for /f "tokens=*" %%a in ('where %anwendung% 2^>NUL') do set pathExe=%%a
    if not defined pathExe set pathExe=%ersatzPath%
    if exist "%pathExe%" set result=%pathExe%

    if not defined result exit /b 1
    if not exist "%result%" exit /b 1

endlocal&set %~1=%result%&exit /b 0

REM ----------------------------------------------------------------------------------------------

:locateDoxygenExe
REM /**
REM  * Findet Doxygen.exe
REM  */
setlocal
    set result=
    set pathGitExe=

    for /f "tokens=*" %%a in ('where doxygen.exe 2^>NUL') do set pathGitExe=%%a
    if not defined pathGitExe set pathGitExe=%DOXYGEN_EXE%
    if exist "%pathGitExe%" set result=%pathGitExe%

endlocal&set %~1=%result%&exit /b 0

REM ----------------------------------------------------------------------------------------------

:locateGitExe
REM /**
REM  * Findet Git.exe
REM  */
setlocal
    set result=
    set pathGitExe=

    for /f "tokens=*" %%a in ('where git.exe 2^>NUL') do set pathGitExe=%%a
    if not defined pathGitExe set pathGitExe=%GIT_EXE%
    if exist "%pathGitExe%" set result=%pathGitExe%

endlocal&set %~1=%result%&exit /b 0

REM ----------------------------------------------------------------------------------------------

:test
REM /**
REM  * Führt einen Module/Integrationstest durch
REM  */
setlocal
    set out=%~1
    set src=%~2
    set dll=%~3
    set logfile=%~4
    set include=%~5
    set debug=%~6
    set test=%~7
    set testpath=%~8


    if defined testpath set include=%include% %testpath%
    set logfile=%logfile%_test.txt
    set out=%out%_test.exe
    set rc=

    call:make "%out%" "%src%" "%dll%" "%logfile%" "%include%" "%debug%" "%test%"
    set RC=%ERRORLEVEL%
    if %RC% neq 0 exit /b %RC%

    "%out%">>"%logfile%"
    set RC=%ERRORLEVEL%

endlocal&exit /b %rc%

REM ----------------------------------------------------------------------------------------------

:doxygen
REM /**
REM  * Fuehrt doxygen aus
REM  * @todo ueberarbeiten wird so nicht funktionieren und wurde noch nicht getestet
REM  */
setlocal
    set doxyconfig=%~1

    "%DOXYGEN_EXE%" "%doxyconfig%"
    set localrc=%ERRORLEVEL%

endlocal&exit /b %localrc%

REM ----------------------------------------------------------------------------------------------

:commit
REM /**
REM  * Fuehrt ein git add . und ein git commit aus
REM  */
setlocal
    set comment=%~1

    "%GIT_EXE%" add .
    "%GIT_EXE%" commit -m "%comment%"
    set localrc=%ERRORLEVEL%

endlocal&exit /b %localrc%

REM ----------------------------------------------------------------------------------------------

:make
REM /**
REM  * Fuehrt die csc.exe aus
REM  */
setlocal
    set out=%~1
    set src=%~2
    set dll=%~3
    set logfile=%~4
    set include=%~5
    set debug=%~6
    set mein=%~7
    set target=%~8
    set ressource=%~9

    if defined out set out=%out:"=%
    if defined debug set debug="/define:LOGLEVEL_DEBUG"
    if defined src set src=%src:"=%
    if defined src set src="%src%*.cs"
    if defined dll set dll=%dll:"=%
    if defined logfile set logfile=%logfile:"=%
    if defined include set include=%include:"=%
    if defined include set include=%include:stern=*%
    if defined include set include=%include:+= %
    if defined mein set mein=/main:"%mein%"
    if defined target set target=/target:%target%
    if defined ressource set ressource=/resource:RESOURCES.resources

    "%CSC_EXE%" /out:"%out%" /nologo /define:WINDOWS %ressource% %target% %mein% %debug% %dll% %src% %include%>"%logfile%"
    set localrc=%ERRORLEVEL%

endlocal&exit /b %localrc%

REM ----------------------------------------------------------------------------------------------

:ausstieg
REM /**
REM  * Der Aussteigspunkt der Anwendung
REM  */

echo off
echo.
echo. RC:%RC%
echo. Work ended at: %TIME%
echo.
echo. ************************************ [ Work wurde beendet ] ************************************
echo.


exit /b %RC%

REM ----------------------------------------------------------------------------------------------

:abort
REM /**
REM  * Der Aussteigspunkt der Anwendung bei einem Fehler
REM  */
set errormessage=%~1
set abortrc=%~2

echo off
echo. Message: %errormessage%
echo. RC: %abortrc%
echo. Work ended at: %TIME%
echo.
echo. ************************************ [ Work wurde FEHLERHAFT beendet ] ************************************
echo.

exit /b %abortrc%

REM ----------------------------------------------------------------------------------------------

REM -- [EOF] --
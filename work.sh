#! /bin/bash

# ----------------------------------------------------------------------------------------------
# @author     :   Robin D'Andrea
# @date       :   13.01.2018
# @syntax     :   Rufe --help auf um die Kommandozeilenparameter Aufzurufen
# @see        :   http://wiki.versalitic.int/index.php/C-Sharp_make.sh
# ----------------------------------------------------------------------------------------------

echo $0 $@

# ----------------------------------------------------------------------------------------------

# /**
#  * Self Variablen
#  */
SELF_NAME=$0

# ----------------------------------------------------------------------------------------------

# /**
#  * Init Variablen
#  */
CSC_EXE=mcs
DOXYGEN_EXE=doxygen
GIT_EXE=
JUST_HELP=
JUST_MAKE=
JUST_DEBUG=
THE_LOGFILE=
THE_OUT=
THE_TEST=
THE_TESTPATH=
THE_MAIN=
THE_TARGET=
THE_SRC=
THE_DOC=
THE_DLL=
THE_INC=
THE_COMMENT=
RC_gitExeNotFound=1000
RC_gitCommitFailed=2000
RC_doxygenExeNotFound=3000
RC_doxygenFailed=4000
RC_cscExeNotFound=5000
RC_testFailed=6000
RC=0

# ----------------------------------------------------------------------------------------------

# /**
#  * Auswertung, die Kommandozeilen Parameter
#  *
#  * @see https://stackoverflow.com/questions/192249/how-do-i-parse-command-line-arguments-in-bash
#  */
parseCommandlineArgument (  )
{
    while [[ $# -gt 0 ]]
    do
    key="$1"

    case $key in
        -h|--help)
            JUST_HELP=true
            shift # past argument
        ;;
        -m|--make)
            JUST_MAKE=true
            shift # past argument
        ;;
        --debug)
            JUST_DEBUG=true
            shift # past argument
        ;;
        -i|--inc)
            THE_INC="$2"
            shift # past argument
            shift # past value
        ;;
        -l|--log)
            THE_LOGFILE="$2"
            shift # past argument
            shift # past value
        ;;
        -o|--out)
            THE_OUT="$2"
            shift # past argument
            shift # past value
        ;;
        -s|--src)
            THE_SRC="$2"
            shift # past argument
            shift # past value
        ;;
        -d|--doc)
            THE_DOC="$2"
            shift # past argument
            shift # past value
        ;;
        --main)
            THE_MAIN="$2"
            shift # past argument
            shift # past value
        ;;
        --testpath)
            THE_TESTPATH="$2"
            shift # past argument
            shift # past value
        ;;
        --test)
            THE_TEST="$2"
            shift # past argument
            shift # past value
        ;;
        --target)
            THE_TARGET="$2"
            shift # past argument
            shift # past value
        ;;
        *)    # unknown option
            POSITIONAL+=("$1") # save it in an array for later
            shift # past argument
        ;;
    esac
    done
}

# ----------------------------------------------------------------------------------------------

# /**
#  * Gibt den BuildLog_file auf dem Bildschirm aus
#  */
showBuildLog (  )
{
    logfile=$1
    echo
    echo "----------------LogFile----------------"
    cat $logfile
    echo
}

# ----------------------------------------------------------------------------------------------

# /**
#  * Fuehrt die csc.exe aus
#  */
make (  )
{
    out=$1
    src=$2
    dll=$3
    logfile=$4
    include=$5
    debug=$6
    mein=$7
    target=$8

    if [ "$debug" == true ]
        then
        debug="-define:LOGLEVEL_DEBUG"
    else
        debug=""
    fi

    if [ "$mein" != "" ]
        then
        mein=-main:"$mein"
    fi

    if [ "$target" != "" ]
        then
        target=-target:$target
    fi

    if [ "$logfile" == "" ]
        then
        echo
        "$CSC_EXE" -out:"$out" $mein $target -define:LINUX $debug $dll "$src*.cs" $include
        echo
    else
        "$CSC_EXE" -out:"$out" $mein $target -define:LINUX $debug $dll "$src*.cs" $include>"$logfile" 2>&1
        showBuildLog $logfile
    fi
}

# ----------------------------------------------------------------------------------------------

# /**
#  * Führt einen Module/Integrationstest durch
#  */
testf (  )
{
    out=$1
    src=$2
    dll=$3
    logfile=$4
    include=$5
    debug=$6
    test=$7
    testpath=$8

    out=$out.exe

    make "$out" "$src" "$dll" "$logfile" "$testpath $include" "$debug" "$test"

    "$out">"$logfile" 2>&1
}

# ----------------------------------------------------------------------------------------------

# /**
#  * Gibt die Hilfe in der Console aus
#  */
hilfe (  )
{
    echo
    echo "----------------HILFE----------------"
    echo "$SELF_NAME --make --out bin/bubble.exe --log log/build_log.txt --src src/ --doc pro/doxygenconfig"
    echo
    echo "--csc ^<filepath^>        Der Pfad zur csc.exe"
    echo "--help                    Gibt die Hilfe aus"
    echo "--make                    Startet den make vorgang"
    echo "--doc ^<filepath^>        Den Pfad zur Config von doxygen und fuehrt doxygen aus"
    echo "--lib ^<dlls^>            Eine Auflistung aller dlls"
    echo "--out ^<filepath^>        Wie soll die Ausgabeexe heissen und wo soll sie liegen"
    echo "--src ^<path^>            Wo der Srccode liegt"
    echo "--log ^<filepath^>        Wo hin die build_log gespeichert werden"
    echo "--test ^<namespace.class^>       Der namespace.class wo die Main Funktion fürs Testen liegt. Gibt das Programm nicht 0 wieder, wird das Compilieren nicht durchgeführt."
    echo "--testpath ^<filepath^>          Der Pfad wo die Dateien fürs Testen liegen"
    echo "--main ^<namespace.class^>       Die Main Funktion für das Hauptprogramm"
    echo "--debug                   -define beim compiler mit LOGLEVEL_DEBUG"
    echo "--target ^<target^>         exe       Ausführbare Konsolendatei erstellen (Standard)"
    echo "                            winexe    Ausführbare Windows-Datei erstellen"
    echo "                            library   Bibliothek erstellen"
    echo "--inc ^<paths^>           Die Pfade wo weiterer quellcode liegt stern = ^*, bitte stern schreiben"
    echo
}

# ----------------------------------------------------------------------------------------------

# /**
#  * Fuehrt doxygen aus
#  */
run_doxygen (  )
{
    doc=$1

    "$DOXYGEN_EXE" "$doc">/dev/null
    (cd html && tar c .) | (cd doc/html && tar xf -)
    rm -rf html
}

# ----------------------------------------------------------------------------------------------

# /**
#  * Main
#  */
main (  )
{
    if [ "$JUST_HELP" == true ]
        then
            hilfe
    fi
    if [ "$THE_DOC" != "" ]
        then
            run_doxygen $THE_DOC
    fi
    if [ "$THE_TEST" != "" ]
        then
            testf "$THE_OUT" "$THE_SRC" "$THE_DLL" "$THE_LOGFILE" "$THE_INC" "$JUST_DEBUG" "$THE_TEST" "$THE_TESTPATH"
    fi
    if [ "$JUST_MAKE" == true ]
        then
            make "$THE_OUT" "$THE_SRC" "$THE_DLL" "$THE_LOGFILE" "$THE_INC" "$JUST_DEBUG" "$THE_MAIN" "$THE_TARGET"
    fi
}

# ----------------------------------------------------------------------------------------------

parseCommandlineArgument $@
main

# ----------------------------------------------------------------------------------------------
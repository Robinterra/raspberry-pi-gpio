using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;

namespace RaspberryPi
{

    // =================================================================

    /**
     * Um die GPIO eines Raspberry Pi anzusteuern (Write)
     * @author Robin D'Andrea
     * @Date 2019.05.12
     */
    public class WriteGPIO : GPIO
    {
        #if (LOGLEVEL_DEBUG)
            public const string KLASSE = "WriteGPIO";
        #endif

        // -------------------------------------------------------------

        #region ctor

        // -------------------------------------------------------------

        /**
         * Konstuktor dieser Klasse
         *
         * @param[in] _pin (Pin) Der Pin der diese Instance zugewiesen wird
         */
        public WriteGPIO ( Pin _pin )
            : base ( _pin, PinSetup.Output )
        {
            #if (LOGLEVEL_DEBUG)
                string methodeName = string.Format ( "new {0} ( Pin _pin ) - Konstruktor", KLASSE );
                Logging.Trace ( methodeName );
            #endif
        }

        // -------------------------------------------------------------

        /**
         * Der Dekonstruktor zu dieser Klasse
         */
        ~WriteGPIO (  )
        {
            #if (LOGLEVEL_DEBUG)
                Logging.Trace ( KLASSE + " - Dekonstruktor" );
            #endif

            // -------------------------------

            this.Dispose (  );
        }

        // -------------------------------------------------------------

        #endregion ctor

        // -------------------------------------------------------------

        #region methods

        // -------------------------------------------------------------

        /**
         * Um einen Wert für den GPIO zu setzen
         *
         * @param[in] _value (bool) true = gpio high; false = gpio low
         *
         * @return (bool) Wenn true zurück gegeben wird gab es keine probleme. Bei false konnte kein wert gesetzt werden
         */
        public new bool Write ( ValueState _value )
        {
            #if (LOGLEVEL_DEBUG)
                string methodeName = KLASSE + ".Write ( bool _value )";
                Logging.Trace ( methodeName );
                Logging.Debug ( methodeName, "_value", _value );
            #endif

            // -------------------------------

            return base.Write ( _value );
        }

        // -------------------------------------------------------------

        #endregion methods

        // -------------------------------------------------------------
    }

    // =================================================================

    /**
     * Um die GPIO eines Raspberry Pi anzusteuern (Read)
     * @author Robin D'Andrea
     * @Date 2019.05.12
     */
    public class ReadGPIO : GPIO
    {
        #if (LOGLEVEL_DEBUG)
            public const string KLASSE = "ReadGPIO";
        #endif

        // -------------------------------------------------------------

        #region ctor

        // -------------------------------------------------------------

        /**
         * Konstuktor dieser Klasse
         *
         * @param[in] _pin (Pin) Der Pin der diese Instance zugewiesen wird
         */
        public ReadGPIO ( Pin _pin )
            : base ( _pin, PinSetup.Input )
        {
            #if (LOGLEVEL_DEBUG)
                sstring methodeName = string.Format ( "new {0} ( Pin _pin ) - Konstruktor", KLASSE );
                Logging.Trace ( methodeName );
                Logging.Debug ( methodeName, "_pin", _pin );
            #endif
        }

        // -------------------------------------------------------------

        /**
         * Der Dekonstruktor zu dieser Klasse
         */
        ~ReadGPIO (  )
        {
            #if (LOGLEVEL_DEBUG)
                Logging.Trace ( KLASSE + " - Dekonstruktor" );
            #endif

            // -------------------------------

            this.Dispose (  );
        }

        // -------------------------------------------------------------

        #endregion ctor

        // -------------------------------------------------------------

        #region methods

        // -------------------------------------------------------------

        /**
         * Gibt den Wert des GPIO wieder
         *
         * @return (bool) true = gpio high; false = gpio low oder Wert konnte nicht abgerufen werden
         */
        public new ValueState Read (  )
        {
            #if (LOGLEVEL_DEBUG)
                sstring methodeName = KLASSE + ".Read (  )";
                Logging.Trace ( methodeName );
            #endif

            // -------------------------------

            return base.Read (  );
        }

        // -------------------------------------------------------------

        #endregion methods

        // -------------------------------------------------------------
    }

    // =================================================================

    /** [FACADE]
     * Um die GPIO eines Raspberry Pi anzusteuern
     * @author Robin D'Andrea
     * @Date 2019.05.12
     */
    public class GPIO : IDisposable
    {
        #if (LOGLEVEL_DEBUG)
            public const string KLASSE = "GPIO";
        #endif

        // -------------------------------------------------------------

        #region vars

        // -------------------------------------------------------------

        /**
         * Der Pin dieser instance
         */
        private GPIOPin pin;

        /**
         * Die Anzahl der hinzugefügten events
         */
        private int eventsAnzahl = 0;

        /**
         * Die Listen Klasse, welche den Gpio überwacht um so den event auszulösen
         */
        private ListenOnGpio listenGpio;

        // -------------------------------------------------------------

        #endregion vars

        // -------------------------------------------------------------

        #region delegate

        // -------------------------------------------------------------

        /**
         * Die Delagete Funktion für das Event
         *
         * @param[in] Der Gpio in dem sich die Value geändert hat.
         * @param[in] Die alte Value des Gpio
         * @param[in] Die neue Value des Gpio
         *
         * @return (bool) Akutell nicht in Verwendung
         */
        public delegate bool ValueChangeFunktion ( GPIO _currentGpio, ValueState _oldValue, ValueState _newValue );

        // -------------------------------------------------------------

        #endregion delegate

        // -------------------------------------------------------------

        #region get/set

        // -------------------------------------------------------------

        /**
         * Die Listen Klasse, welche den Gpio überwacht um so den event auszulösen
         */
        private ListenOnGpio ListenGpio
        {
            get
            {
                if ( this.listenGpio != null ) return this.listenGpio;

                this.listenGpio = new ListenOnGpio ( this );

                return this.listenGpio;
            }
        }

        // -------------------------------------------------------------

        /**
         * Gibt den ausgewählten Pin wieder
         */
        public Pin Pin
        {
            get
            {
                return this.pin.Pin;
            }
        }

        // -------------------------------------------------------------

        /**
         * Dieser Event wird ausgelöst wenn sich die Value ändert.
         * Startet auch einen Listener der den GPIO überwacht ob sich der Wert geändert hat.
         */
        public event ValueChangeFunktion ValueChanged
        {
            add
            {
                this.eventsAnzahl++;

                this.ListenGpio.ValueChanged += value;
            }
            remove
            {
                this.eventsAnzahl--;

                this.ListenGpio.ValueChanged -= value;
            }
        }

        // -------------------------------------------------------------

        /**
         * Gibt den Setup des Pins wieder
         */
        public PinSetup Setup
        {
            get
            {
                return this.pin.Setup;
            }
        }

        // -------------------------------------------------------------

        /**
         * Die Value des Gpio.
         *
         * @return (bool) true = ValueState.HIGH; false = ValueState.LOW || ValueState.UNKNOWN
         */
        public bool Value
        {
            get
            {
                return this.Read (  ) == ValueState.HIGH;
            }
            set
            {
                this.Write ( value ? ValueState.HIGH : ValueState.LOW );
            }
        }

        // -------------------------------------------------------------

        #endregion get/set

        // -------------------------------------------------------------

        #region ctor

        // -------------------------------------------------------------

        /**
         * Konstuktor dieser Klasse
         *
         * @param[in] _pin (Pin) Der Pin der diese Instance zugewiesen wird
         * @param[in] _setup (PinSetup) Der Setup mit dem dieser Pin Initialisiert wurde
         */
        protected GPIO ( Pin _pin, PinSetup _setup )
        {
            this.pin = GPIOController.Instance.SetupOrChangeGPIOPin ( _pin, _setup );
        }

        // -------------------------------------------------------------

        /**
         * Der Dekonstruktor zu dieser Klasse
         */
        ~GPIO (  )
        {
            this.Dispose (  );
        }

        // -------------------------------------------------------------

        #endregion ctor

        // -------------------------------------------------------------

        #region methods

        // -------------------------------------------------------------

        /**
         * Gibt den Pin wieder frei
         */
        public void Dispose (  )
        {
            this.ListenGpio.Dispose (  );

            this.listenGpio = null;

            this.pin = null;

        }

        // -------------------------------------------------------------

        /**
         * Gibt den Wert des GPIO wieder
         *
         * @return (bool) true = gpio high; false = gpio low oder Wert konnte nicht abgerufen werden
         */
        protected ValueState Read (  )
        {
            if ( this.pin == null ) return ValueState.UNKNOWN;

            ValueState valueState = this.pin.Read (  );

            return valueState;
        }

        // -------------------------------------------------------------

        /**
         * Um einen Wert für den GPIO zu setzen
         *
         * @param[in] _value (bool) true = gpio high; false = gpio low
         *
         * @return (bool) Wenn true zurück gegeben wird gab es keine probleme. Bei false konnte kein wert gesetzt werden
         */
        protected bool Write ( ValueState _value )
        {
            if ( this.pin == null ) return false;

            return this.pin.Write ( _value );
        }

        // -------------------------------------------------------------

        #endregion methods

        // -------------------------------------------------------------

    }

    // =================================================================

    /** [SINGLETON]
     * @author Robin D'Andrea
     * @Date 2019.05.12
     */
    class GPIOController : IDisposable
    {
        #if (LOGLEVEL_DEBUG)
            public const string KLASSE = "GPIOController";
        #endif

        // -------------------------------------------------------------

        #region const

        // -------------------------------------------------------------

        /** [CONST]
         * Gibt den Path für die gpios wieder
         */
        public const string GPIOPATH = "/sys/class/gpio/";

        /** [CONST]
         * Gibt den Path für die gpios value wieder
         */
        public const string GPIOPATH_VALUE = GPIOController.GPIOPATH + "gpio{0}/value";

        /** [CONST]
         * Gibt den Path für den Unexport wieder
         */
        public const string GPIOPATH_UNEXPORT = GPIOController.GPIOPATH + "unexport";

        /** [CONST]
         * Gibt den Path für den export wieder
         */
        public const string GPIOPATH_EXPORT = GPIOController.GPIOPATH + "export";

        /** [CONST]
         * Gibt den Path für die gpio direktion wieder
         */
        public const string GPIOPATH_DIRECTION = GPIOController.GPIOPATH + "gpio{0}/direction";

        // -------------------------------------------------------------

        #endregion const

        // -------------------------------------------------------------

        #region vars

        // -------------------------------------------------------------

        /** [STATIC]
         * Die instance für die Klasse
         */
        private static GPIOController instance;

        /**
         * Liste mit allen aktiven Pins
         */
        private List<GPIOPin> gpioPins;

        /**
         * 
         */
        private List<ListenOnGpio> listenOnGpios;

        
        /**
         * Der Thread der für das Überwachen der Gpios zuständig ist
         */
        private Thread threadListenGpios;

        /**
         * gibt an ob das auslesen am laufen ist
         */
        private bool isRunning;

        /**
         * 
         */
        private bool listeVonListenOnGpiosIsUse;

        /**
         * 
         */
        private bool isDisposed;

        // -------------------------------------------------------------

        #endregion vars

        // -------------------------------------------------------------

        #region get/set

        // -------------------------------------------------------------

        /**
         * Gibt an ob er bereits überwacht
         */
        public bool OnListen
        {
            get;
            private set;
        }

        // -------------------------------------------------------------

        /**
         * Liste mit allen aktiven Pins
         */
        private List<GPIOPin> GPIOPins
        {
            get
            {
                if ( this.gpioPins != null ) return this.gpioPins;

                this.gpioPins = new List<RaspberryPi.GPIOPin> (  );

                return this.gpioPins;
            }
        }

        // -------------------------------------------------------------

        /**
         * 
         */
        public List<ListenOnGpio> ListenOnGpios
        {
            get
            {
                if ( this.listenOnGpios != null ) return this.listenOnGpios;

                this.listenOnGpios = new List<ListenOnGpio> (  );

                return this.listenOnGpios;
            }
        }

        // -------------------------------------------------------------

        /** [STATIC]
         * Die Instance dieser Klasse
         */
        public static GPIOController Instance
        {
            get
            {
                if ( GPIOController.instance != null ) return GPIOController.instance;

                GPIOController.instance = new RaspberryPi.GPIOController (  );

                return GPIOController.instance;
            }
        }

        // -------------------------------------------------------------

        public GPIOPin this[Pin _pin]
        {
            get
            {
                return this.GetGPIOPin ( _pin );
            }
        }

        // -------------------------------------------------------------

        #endregion get/set

        // -------------------------------------------------------------

        #region ctor

        // -------------------------------------------------------------

        /**
         * Konstuktor dieser Klasse
         */
        private GPIOController (  )
        {

        }

        // -------------------------------------------------------------

        /**
         * Der Dekonstruktor zu dieser Klasse
         */
        ~GPIOController (  )
        {
            this.Dispose (  );
        }

        // -------------------------------------------------------------

        #endregion ctor

        // -------------------------------------------------------------

        #region methods

        // -------------------------------------------------------------

        public bool RemoveListen ( ListenOnGpio listenOnGpio )
        {
            if ( listenOnGpio == null ) return false;

            if ( this.isDisposed ) return false;

            while ( this.listeVonListenOnGpiosIsUse ) {  }

            this.listeVonListenOnGpiosIsUse = true;

            if ( !this.ListenOnGpios.Contains ( listenOnGpio ) ) return this.listeVonListenOnGpiosIsUse = false;

            this.ListenOnGpios.Remove ( listenOnGpio );

            bool isok = this.ListenOnGpios.Count == 0;

            this.listeVonListenOnGpiosIsUse = false;

            if ( isok ) this.StopListen (  );

            return true;
        }

        // -------------------------------------------------------------

        public bool AddListen ( ListenOnGpio listenOnGpio )
        {
            if ( listenOnGpio == null ) return false;

            if ( this.isDisposed ) return false;

            while ( this.listeVonListenOnGpiosIsUse ) {  }

            this.listeVonListenOnGpiosIsUse = true;

            if ( this.ListenOnGpios.Contains ( listenOnGpio ) ) return this.listeVonListenOnGpiosIsUse = false;

            this.ListenOnGpios.Add ( listenOnGpio );

            bool isok = this.ListenOnGpios.Count == 1;

            this.listeVonListenOnGpiosIsUse = false;

            if ( isok ) this.StartListen (  );

            return true;
        }

        // -------------------------------------------------------------

        /**
         * Startet das überwachen des Gpio
         */
        public bool StartListen (  )
        {
            if ( this.OnListen ) return false;

            this.OnListen = true;

            this.threadListenGpios = new Thread ( this.Listen );

            this.threadListenGpios.Start (  );

            return true;
        }

        // -------------------------------------------------------------

        /**
         * Diese übernimmt das überwachen des Gpio
         */
        private void Listen (  )
        {
            this.isRunning = true;

            while ( this.isRunning )
            {
                Thread.Sleep ( 1 );

                while ( this.listeVonListenOnGpiosIsUse ) {  }

                this.listeVonListenOnGpiosIsUse = true;

                foreach ( ListenOnGpio listenGpio in this.ListenOnGpios )
                {
                    listenGpio.Listen (  );
                }

                this.listeVonListenOnGpiosIsUse = false;
            }

            this.OnListen = false;
        }

        // -------------------------------------------------------------

        /**
         * Beendet das Überwachen des Gpio
         */
        public bool StopListen (  )
        {
            this.isRunning = false;

            while ( this.OnListen )
            {

            }

            return true;
        }

        // -------------------------------------------------------------

        /**
         * * Gibt alle Pins wieder frei
         */
        public void Dispose (  )
        {
            this.isDisposed = true;

            foreach ( GPIOPin pin in this.GPIOPins )
            {
                if ( pin == null ) continue;

                pin.Dispose (  );
            }

            this.StopListen (  );

            foreach ( ListenOnGpio listenGpio in this.ListenOnGpios )
            {
                listenGpio.Dispose (  );
            }

            this.ListenOnGpios.Clear (  );

            this.listenOnGpios = null;

            this.GPIOPins.Clear (  );

            this.gpioPins = null;
        }

        // -------------------------------------------------------------

        /**
         * Gibt die Instance für einen Pin wieder
         *
         * @param[in] _pin (Pin) Den Pin für den die Instance abgerufen wird
         *
         * @return (GPIOPin) Die Instance für den abgerufen Pin. Konnte kein Pin abgerufen werden wird null wieder gegeben
         */
        public GPIOPin GetGPIOPin ( Pin _pin )
        {
            foreach ( GPIOPin pin in this.GPIOPins )
            {
                if ( pin == null ) continue;

                if ( pin.Pin == _pin ) return pin;
            }

            return null;
        }

        // -------------------------------------------------------------

        /**
         * Gibt die Instance für einen Pin wieder
         *
         * @param[in] _pin (Pin) Den Pin für den die Instance abgerufen wird
         * @param[in] _setup (PinSetup) Der Setup für diesen Pin
         *
         * @return (GPIOPin) Die Instance für den abgerufen Pin.
         */
        public GPIOPin SetupOrChangeGPIOPin ( Pin _pin, PinSetup _setup )
        {
            GPIOPin result = this[_pin];

            if ( result == null )
            {
                result = new GPIOPin ( _pin, _setup );

                this.GPIOPins.Add ( result );
            }
            else
            {
                result.ChangeSetup ( _setup );
            }

            return result;
        }

        // -------------------------------------------------------------

        #endregion methods

        // -------------------------------------------------------------

    }

    // =================================================================

    /**
     * @author Robin D'Andrea
     * @Date 2019.05.12
     */
    class GPIOPin : IDisposable
    {
        #if (LOGLEVEL_DEBUG)
            public const string KLASSE = "GPIOPin";
        #endif

        // -------------------------------------------------------------

        #region vars

        // -------------------------------------------------------------

        /**
         * Die Setup Konfiguration des Pins
         */
        private PinSetup setup;

        /**
         * Der Pin um den es geht
         */
        private Pin pin;

        /**
         * Der Pfad für diesen Pin um die Value zu setzen oder abzurufen
         */
        private string valuePath;

        // -------------------------------------------------------------

        #endregion vars

        // -------------------------------------------------------------

        #region get/set

        // -------------------------------------------------------------

        /**
         * Die Setup Konfiguration des Pins
         */
        public PinSetup Setup
        {
            get
            {
                #if (LOGLEVEL_DEBUG)
                    string methodeName = KLASSE + ".Setup GET";
                    Logging.Trace ( methodeName );
                    Logging.Debug ( methodeName, "setup", this.setup );
                #endif

                return this.setup;
            }
        }

        // -------------------------------------------------------------

        /**
         * Der Pin um den es sich handelt
         */
        public Pin Pin
        {
            get
            {
                #if (LOGLEVEL_DEBUG)
                    string methodeName = KLASSE + ".Pin GET";
                    Logging.Trace ( methodeName );
                    Logging.Debug ( methodeName, "pin", this.pin );
                #endif

                return this.pin;
            }
        }

        // -------------------------------------------------------------

        /**
         * Der Pfad für diesen Pin um die Value zu setzen oder abzurufen
         */
        private string ValuePath
        {
            get
            {
                if ( !string.IsNullOrEmpty ( this.valuePath ) ) return this.valuePath;

                this.valuePath = string.Format ( GPIOController.GPIOPATH_VALUE, (int)this.Pin );

                return this.valuePath;
            }
        }

        // -------------------------------------------------------------

        #endregion get/set

        // -------------------------------------------------------------

        #region ctor

        // -------------------------------------------------------------

        /**
         * Konstuktor dieser Klasse
         *
         * @param[in] _pin (Pin) Der Pin der diese Instance zugewiesen wird
         * @param[in] _setup (PinSetup) Das Setup mit dem dieser Pin Initialisiert wurde
         */
        public GPIOPin ( Pin _pin, PinSetup _setup )
        {
            this.pin = _pin;
            this.setup = _setup;

            this.Export (  );
        }

        // -------------------------------------------------------------

        /**
         * Der Dekonstruktor zu dieser Klasse
         */
        ~GPIOPin (  )
        {
            this.Dispose (  );
        }

        // -------------------------------------------------------------

        #endregion ctor

        // -------------------------------------------------------------

        #region methods

        // -------------------------------------------------------------

        /**
         * Um einen Wert für den GPIO zu setzen
         *
         * @param[in] _value (bool) true = gpio high; false = gpio low
         *
         * @return (bool) Wenn true zurück gegeben wird gab es keine probleme. Bei false konnte kein wert gesetzt werden
         */
        public bool Write ( ValueState _out )
        {
            if ( this.setup != PinSetup.Output ) return false;
            if ( !File.Exists ( this.ValuePath ) ) return false;
            if ( _out == ValueState.UNKNOWN ) return false;

            string value = _out == ValueState.LOW ? "0" : "1";

            try
            {
                File.WriteAllText ( this.ValuePath, value );
            }
            catch
            {
                this.setup = PinSetup.None;

                return false;
            }

            return true;
        }

        // -------------------------------------------------------------

        /**
         * 
         */
        public bool AnalogWrite ( byte _strength, int _duration )
        {
            if ( this.setup != PinSetup.Output ) return false;
            if ( !File.Exists ( this.ValuePath ) ) return false;
            if ( _strength == 0 )
            {
                this.Write ( ValueState.LOW );
                Thread.Sleep ( _duration );
                return true;
            }

            int signalDuration = (_strength * 1000) / 255;
            int currentDuration = 0;
            ValueState highOrLow = ValueState.HIGH;

            while ( currentDuration < _duration )
            {
                highOrLow = ( currentDuration / signalDuration ) % 2 == 0 ? ValueState.HIGH : ValueState.LOW;

                this.Write ( highOrLow );

                Thread.Sleep ( signalDuration );

                if ( _duration != -1 ) continue;

                if ( highOrLow == ValueState.HIGH ) continue;

                currentDuration = 0;
            }

            return true;
        }

        // -------------------------------------------------------------

        /**
         * Gibt den Wert des GPIO wieder
         *
         * @return (ValueState) Gibt die Value des Pins wieder
         */
        public ValueState Read (  )
        {
            if ( this.setup == PinSetup.None ) return ValueState.UNKNOWN;
            if ( !File.Exists ( this.ValuePath ) ) return ValueState.UNKNOWN;

            string value;

            try
            {
                value = File.ReadAllText ( this.ValuePath );
            }
            catch { return ValueState.UNKNOWN; }

            if (value.Length < 1) return ValueState.UNKNOWN;

            return value[0] == '0' ? ValueState.LOW : ValueState.HIGH;
        }

        // -------------------------------------------------------------

        /**
         * Gibt den Pin wieder frei
         */
        public void Dispose (  )
        {
            this.Unexport (  );
        }

        // -------------------------------------------------------------

        /**
         * Ändert das Setup für diesen Pin
         *
         * @param[in] _setup (PinSetup) Das neue Setup für diesen Pin
         *
         * @return (bool) true = setup wurde geändert; false = setup konnte nicht geändert werden
         */
        public bool ChangeSetup ( PinSetup _setup )
        {
            if ( _setup == this.setup ) return true;

            this.setup = _setup;

            return this.SetDirection (  );
        }

        // -------------------------------------------------------------

        /**
         * Exportiert einen Pin um ihn zu verwenden
         *
         * @return (bool) true = export erfolgreich; false = fehlgeschlagen
         */
        private bool Export (  )
        {
            if ( !File.Exists ( this.ValuePath ) )
            {
                try
                {
                    File.WriteAllText ( GPIOController.GPIOPATH_EXPORT, ((int)this.Pin).ToString (  ) );
                }
                catch { return false; }
            }

            bool isok = this.SetDirection (  );

            if ( !isok ) this.setup = PinSetup.None;

            return isok;
        }

        // -------------------------------------------------------------

        /**
         * Setzt die Direction für einen Pin
         *
         * @return (bool) true = direction konnte gesetzt werden; false = direction konnte nicht gesetzt werden
         */
        private bool SetDirection (  )
        {
            if ( this.setup == PinSetup.None ) return false;

            string path = string.Format ( GPIOController.GPIOPATH_DIRECTION, (int)this.Pin );

            string direction = this.Setup == PinSetup.Input ? "in" : "out";

            try
            {
                File.WriteAllText( path, direction );
            }
            catch { return false; }

            return true;
        }

        // -------------------------------------------------------------

        /**
         * Unexportiert einen Pin um ihn wieder frei zugeben
         *
         * @return (bool) true = unexport efolgreich; false = unexport fehlgeschlagen
         */
        private bool Unexport (  )
        {
            if ( this.setup == PinSetup.None ) return true;

            this.setup = PinSetup.None;

            if ( !File.Exists ( this.ValuePath ) ) return true;

            try
            {
                File.WriteAllText ( GPIOController.GPIOPATH_UNEXPORT, ((int)this.Pin).ToString (  ) );
            }
            catch { return false; }

            return true;
        }

        // -------------------------------------------------------------

        #endregion methods

        // -------------------------------------------------------------

    }

    // =================================================================

    /**
     * Die Überwachung eines Gpio
     */
    class ListenOnGpio : IDisposable
    {
        #if (LOGLEVEL_DEBUG)
            public const string KLASSE = "ListenOnGpio";
        #endif

        // -------------------------------------------------------------

        #region vars

        // -------------------------------------------------------------

        /**
         * Die zuletzt erfasste Value
         */
        private ValueState currentValue;

        /**
         *
         */
        private List<GPIO.ValueChangeFunktion> valueChangedDelegates;

        /**
         * Die Anzahl der hinzugefügten events
         */
        private int eventsAnzahl = 0;

        // -------------------------------------------------------------

        #endregion vars

        // -------------------------------------------------------------

        #region events

        // -------------------------------------------------------------

        /**
         * Der Event wird ausgeführt wenn die Value sich ändert
         */
        private event GPIO.ValueChangeFunktion valueChanged;

        // -------------------------------------------------------------

        #endregion events

        // -------------------------------------------------------------

        #region get/set

        // -------------------------------------------------------------

        private List<GPIO.ValueChangeFunktion> ValueChangedDelegates
        {
            get
            {
                if ( this.valueChangedDelegates != null ) return this.valueChangedDelegates;

                this.valueChangedDelegates = new List<GPIO.ValueChangeFunktion> (  );

                return this.valueChangedDelegates;
            }
        }

        // -------------------------------------------------------------

        /**
         * Der Event wird ausgeführt wenn die Value sich ändert
         */
        public event GPIO.ValueChangeFunktion ValueChanged
        {
            add
            {
                this.eventsAnzahl++;

                this.valueChanged += value;
                this.ValueChangedDelegates.Add ( value );

                if ( this.eventsAnzahl == 1 ) GPIOController.Instance.AddListen ( this );
            }
            remove
            {
                this.valueChanged -= value;
                this.ValueChangedDelegates.Remove ( value );

                if ( this.eventsAnzahl == 0 ) GPIOController.Instance.RemoveListen ( this );
            }
        }

        // -------------------------------------------------------------

        /**
         * Der Gpio welcher überwacht wird
         */
        public GPIO ListenGpio
        {
            get;
            private set;
        }

        // -------------------------------------------------------------

        /**
         * Die zuletzt erfasste Value
         */
        public ValueState CurrentValue
        {
            get
            {
                return this.currentValue;
            }
            set
            {
                if (this.currentValue == value) return;

                if (this.valueChanged != null) this.valueChanged ( this.ListenGpio, this.currentValue, value );

                this.currentValue = value;
            }
        }

        // -------------------------------------------------------------

        #endregion get/set

        // -------------------------------------------------------------

        #region ctor

        // -------------------------------------------------------------

        /**
         * Konstruktor dieser Klasse
         *
         * @param[in] listenGpio (GPIO) Der zu überwachende Gpio
         */
        public ListenOnGpio ( GPIO listenGpio )
        {
            //this.currentValue = ValueState.UNKNOWN;

            this.ListenGpio = listenGpio;

            this.currentValue = this.ListenGpio.Value ? ValueState.HIGH : ValueState.LOW;
        }

        // -------------------------------------------------------------

        /**
         * Der dekonstruktor dieser Klasse
         */
        ~ListenOnGpio (  )
        {
            this.Dispose (  );
        }

        // -------------------------------------------------------------

        #endregion ctor

        // -------------------------------------------------------------

        #region methods

        // -------------------------------------------------------------

        /**
         * Diese übernimmt das überwachen des Gpio
         */
        public void Listen (  )
        {
            this.CurrentValue = this.ListenGpio.Value ? ValueState.HIGH : ValueState.LOW;
        }

        // -------------------------------------------------------------

        /**
         * Disposed dieses Objekt
         */
        public void Dispose (  )
        {
            GPIOController.Instance.RemoveListen ( this );

            foreach ( GPIO.ValueChangeFunktion valueChangedFunktion in this.ValueChangedDelegates )
            {
                this.valueChanged -= valueChangedFunktion;
            }

            this.ValueChangedDelegates.Clear (  );

            this.valueChangedDelegates = null;

            this.ListenGpio = null;
        }

        // -------------------------------------------------------------

        #endregion methods

        // -------------------------------------------------------------

    }

    // =================================================================

    /**
     * @author Robin D'Andrea
     * @Date 2019.05.12
     */
    public enum PinSetup
    {
        None = 0,
        Input = 1,
        Output = 2
    }

    // =================================================================

    public enum ValueState
    {
        UNKNOWN = -1,
        LOW = 0,
        HIGH = 1
    }

    // =================================================================

    /**
     * Pin des Raspberry Pi
     * @author Robin D'Andrea
     * @Date 2019.05.12
     */
    public enum Pin
    {
        gpio2 = 2,
        gpio3 = 3,
        gpio4 = 4,
        gpio5 = 5,
        gpio6 = 6,
        gpio7 = 7,
        gpio8 = 8,
        gpio9 = 9,
        gpio10 = 10,
        gpio11 = 11,
        gpio12 = 12,
        gpio13 = 13,
        gpio14 = 14,
        gpio15 = 15,
        gpio16 = 16,
        gpio17 = 17,
        gpio18 = 18,
        gpio19 = 19,
        gpio20 = 20,
        gpio21 = 21,
        gpio22 = 22,
        gpio23 = 23,
        gpio24 = 24,
        gpio25 = 25,
        gpio26 = 26,
        gpio27 = 27,
    }

    // =================================================================

}

// -- [EOF] --
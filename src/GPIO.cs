using System;
using System.IO;
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
        public new bool Write ( bool _value )
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
        public new bool Read (  )
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

        // -------------------------------------------------------------

        #endregion vars

        // -------------------------------------------------------------

        #region get/set

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
            this.pin.Dispose (  );

            this.pin = null;
        }

        // -------------------------------------------------------------

        /**
         * Gibt den Wert des GPIO wieder
         *
         * @return (bool) true = gpio high; false = gpio low oder Wert konnte nicht abgerufen werden
         */
        protected bool Read (  )
        {
            return this.pin.Read (  );
        }

        // -------------------------------------------------------------

        /**
         * Um einen Wert für den GPIO zu setzen
         *
         * @param[in] _value (bool) true = gpio high; false = gpio low
         *
         * @return (bool) Wenn true zurück gegeben wird gab es keine probleme. Bei false konnte kein wert gesetzt werden
         */
        protected bool Write ( bool _value )
        {
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
        public const string GPIOPATH_VALUE = GPIOController.GPIOPATH + "{0}/value";

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
        public const string GPIOPATH_DIRECTION = GPIOController.GPIOPATH + "{0}/direction";

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

        // -------------------------------------------------------------

        #endregion vars

        // -------------------------------------------------------------

        #region get/set

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

        /**
         * * Gibt alle Pins wieder frei
         */
        public void Dispose (  )
        {
            foreach ( GPIOPin pin in this.GPIOPins )
            {
                if ( pin == null ) continue;

                pin.Dispose (  );
            }

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
            GPIOPin result = this.GetGPIOPin ( _pin );

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
        public bool Write ( bool _out )
        {
            if ( this.setup != PinSetup.Output ) return false;

            string value = _out ? "0" : "1";

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
         * Gibt den Wert des GPIO wieder
         *
         * @return (bool) true = gpio high; false = gpio low oder Wert konnte nicht abgerufen werden
         */
        public bool Read (  )
        {
            if ( this.setup == PinSetup.None ) return false;
            if ( !File.Exists ( this.ValuePath ) ) return false;

            string value;

            try
            {
                value = File.ReadAllText ( this.ValuePath );
            }
            catch { return false; }

            return value == "1";
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

            bool isok = this.Unexport (  );

            if ( !isok ) return false;

            this.setup = _setup;

            return this.Export (  );
        }

        // -------------------------------------------------------------

        /**
         * Exportiert einen Pin um ihn zu verwenden
         *
         * @return (bool) true = export erfolgreich; false = fehlgeschlagen
         */
        private bool Export (  )
        {
            if ( this.setup != PinSetup.None ) return false;

            try
            {
                File.WriteAllText ( GPIOController.GPIOPATH_EXPORT, ((int)this.Pin).ToString (  ) );
            }
            catch { return false; }

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

    /**
     * Pin des Raspberry Pi
     * @author Robin D'Andrea
     * @Date 2019.05.12
     */
    public enum Pin
    {
        gpio0 = 0,
        gpio1 = 1,
        gpio4 = 4,
        gpio7 = 7,
        gpio8 = 8,
        gpio9 = 9,
        gpio10 = 10,
        gpio11 = 11,
        gpio14 = 14,
        gpio15 = 15,
        gpio17 = 17,
        gpio18 = 18,
        gpio20 = 20,
        gpio21 = 21,
        gpio22 = 22,
        gpio23 = 23,
        gpio24 = 24,
        gpio25 = 25,
        gpio26 = 26,
        gpio27 = 27
    }

    // =================================================================

}

// -- [EOF] --
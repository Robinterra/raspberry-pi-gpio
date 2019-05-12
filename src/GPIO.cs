using System;
using System.IO;
using System.Collections.Generic;

namespace RaspberryPi
{

    // =================================================================

    /**
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

        // -------------------------------------------------------------

    }

    // =================================================================

    /** [SINGLETON]
     * 
     */
    private class GPIOController : IDisposable
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

        // -------------------------------------------------------------

        #endregion vars

        // -------------------------------------------------------------

        #region get/set

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

        private GPIOController (  )
        {

        }

        // -------------------------------------------------------------

        ~GPIOController (  )
        {

        }

        // -------------------------------------------------------------

        #endregion ctor

        // -------------------------------------------------------------

    }

    // =================================================================

    private class GPIOPin : IDisposable
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
         * 
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

        private string ValuePath
        {
            get
            {
                if ( !string.IsNullOrEmpty (  ) ) return this.valuePath;

                this.valuePath = string.Format ( GPIOController.GPIOPATH_VALUE, (int)this.Pin );

                return this.valuePath;
            }
        }

        // -------------------------------------------------------------

        #endregion get/set

        // -------------------------------------------------------------

        #region ctor

        // -------------------------------------------------------------

        public GPIOPin ( Pin pin, PinSetup setup )
        {
            this.pin = pin;
            this.setup = setup;

            this.Export (  );
        }

        // -------------------------------------------------------------

        ~GPIOPin (  )
        {
            this.Dispose (  );
        }

        // -------------------------------------------------------------

        #endregion ctor

        // -------------------------------------------------------------

        #region methods

        // -------------------------------------------------------------

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

        public void Dispose (  )
        {
            this.Unexport (  );
        }

        // -------------------------------------------------------------

        public bool ChangeSetup ( PinSetup setup )
        {
            if ( setup == this.setup ) return true;

            bool isok = this.Unexport (  );

            if ( !isok ) return false;

            this.setup = setup;

            return this.Export (  );
        }

        // -------------------------------------------------------------

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

    public enum PinSetup
    {
        None = 0,
        Input = 1,
        Output = 2
    }

    // =================================================================

    /**
     * PIN des Raspberry Pi
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
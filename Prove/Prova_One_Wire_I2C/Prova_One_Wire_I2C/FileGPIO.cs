using System;
using System.Collections.Generic; //required for List<>
using System.IO;


namespace GPIO//Avoid naming class like namespace, you can end up with calls like "new FileGPIO.FileGPIO()";
{
    // programma preso da: 
    // http://www.raspberrypi.org/forums/viewtopic.php?p=88063

    public class FileGPIO // meglio farla statica, tanto non serve istanziarla! 
    {
        /// <summary>
        /// GPIO connector on the Pi (P1) (as found next to the yellow RCA video socket on the Rpi circuit board)
        /// </summary>
        /// <remarks>
        /// P1-01 = top left,    P1-02 = top right
        ///P1-25 = bottom left, P1-26 = bottom right
        ///pi connector P1 pin     = GPIOnum = slice of pi v1.0 board label
        ///                  P1-07 = GPIO4   = GP7
        ///                  P1-11 = GPIO17  = GP0
        ///                  P1-12 = GPIO18  = GP1
        ///                  P1-13 = GPIO21  = GP2
        ///                  P1-15 = GPIO22  = GP3
        ///                  P1-16 = GPIO23  = GP4
        ///                  P1-18 = GPIO24  = GP5
        ///                  P1-22 = GPIO25  = GP6
        ///So to turn on Pin7 on the GPIO connector, pass in enumGPIOPIN.gpio4 as the pin parameter
        /// </remarks>
        public enum PinEnum
        {
            gpio0 = 0, gpio1 = 1, gpio4 = 4, gpio7 = 7, gpio8 = 8, gpio9 = 9, gpio10 = 10, gpio11 = 11,
            gpio14 = 14, gpio15 = 15, gpio17 = 17, gpio18 = 18, gpio21 = 21, gpio22 = 22, gpio23 = 23, gpio24 = 24, gpio25 = 25
        };

        public enum DirectionEnum { IN, OUT };

        private const string GPIO_PATH = "/sys/class/gpio/";

        /// <summary>
        /// contains list of pins exported with an OUT direction
        /// </summary>
        readonly List<PinEnum> _outExported = new List<PinEnum>();

        /// <summary>
        /// contains list of pins exported with an IN direction
        /// </summary>
        readonly List<PinEnum> _inExported = new List<PinEnum>();
        
        /// <summary>
        /// this gets called automatically when we try and output to, or input from, a pin
        /// </summary>
        /// <param name="pin"></param>
        /// <param name="direction"></param>
        private void SetupPin(PinEnum pin, DirectionEnum direction)
        {
            //unexport if it we're using it already
            if (_outExported.Contains(pin) || _inExported.Contains(pin)) UnexportPin(pin);

            //export
            File.WriteAllText(GPIO_PATH + "export", GetPinNumber(pin));

            Debug.WriteLine("exporting pin " + pin + " as " + direction);

            // set i/o direction
            File.WriteAllText(GPIO_PATH + pin + "/direction", direction.ToString().ToLower());

            //record the fact that we've setup that pin
            if (direction == DirectionEnum.OUT)
                _outExported.Add(pin);
            else
                _inExported.Add(pin);
        }

        //no need to setup pin this is done for you
        public void OutputPin(PinEnum pin, bool value)
        {
            //if we havent used the pin before,  or if we used it as an input before, set it up
            if (!_outExported.Contains(pin) || _inExported.Contains(pin)) SetupPin(pin, DirectionEnum.OUT);

            string writeValue = "0";
            if (value) writeValue = "1";
            File.WriteAllText(GetFilenameValue(pin), writeValue);

            Debug.WriteLine("output to pin " + pin + ", value was " + value);
        }

        //no need to setup pin this is done for you
        public bool InputPin(PinEnum pin)
        {
            bool returnValue = false;

            //if we havent used the pin before, or if we used it as an output before, set it up
            if (!_inExported.Contains(pin) || _outExported.Contains(pin)) SetupPin(pin, DirectionEnum.IN);

            string filename = GetFilenameValue(pin);
            if (File.Exists(filename))
            {
                string readValue = File.ReadAllText(filename);
                if (readValue.Length > 0 && readValue[0] == '1') returnValue = true;
            }
            else
                throw new Exception(string.Format("Cannot read from {0}. File does not exist", pin));

            Debug.WriteLine("input from pin " + pin + ", value was " + returnValue);

            return returnValue;
        }

        /// <summary>
        /// File names cache
        /// </summary>
        /// <remarks>
        /// Dictionary key is int, because enum dictionaries are slow in frameworks prior to 4.0
        /// </remarks>
        readonly Dictionary<int,string>_filenames4Values = new Dictionary<int, string>();

        private string GetFilenameValue(PinEnum pin)
        {
            string result;
            if (_filenames4Values.TryGetValue((int)pin, out result))
                return result;
            result = GPIO_PATH + pin + "/value";
            _filenames4Values.Add((int)pin,result);
            return result;
        }

        /// <summary>
        /// if for any reason you want to unexport a particular pin use this, otherwise just call CleanUpAllPins when you're done
        /// </summary>
        /// <param name="pin"></param>
        public void UnexportPin(PinEnum pin)
        {
            bool found = false;
            if (_outExported.Contains(pin))
            {
                found = true;
                _outExported.Remove(pin);
            }
            if (_inExported.Contains(pin))
            {
                found = true;
                _inExported.Remove(pin);
            }

            if (found)
            {
                File.WriteAllText(GPIO_PATH + "unexport", GetPinNumber(pin));
                Debug.WriteLine("unexporting  pin " + pin);
            }
        }

        public void CleanUpAllPins()
        {
            for (int p = _outExported.Count - 1; p >= 0; p--) UnexportPin(_outExported[p]); //unexport in reverse order
            for (int p = _inExported.Count - 1; p >= 0; p--) UnexportPin(_inExported[p]);
        }

        private static string GetPinNumber(PinEnum pin)
        {
            return ((int)pin).ToString(); //e.g. returns 17 for enum value of gpio17
        }

    }

}
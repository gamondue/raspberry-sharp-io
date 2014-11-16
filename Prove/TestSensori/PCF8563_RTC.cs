using System;
using System.Diagnostics;

namespace GOR.ITT.Cesena
{
    public class PCF8563_RTC
    {
        // programma ispirato a: 
        // http://bradsrpi.blogspot.it/2014/07/c-mono-code-to-read-tmp102-i2c.html

        // comando da lanciare (mettere TUTTA la path, a partire da /)
        private string program = "/usr/sbin/i2cget";
        //private string program = "/bin/cat"; // per test

        // argomento del comando (ad RRR verrà sostitutito il numero del registro)
        // -y 1 = canale I2C n. 1, 0x51 = indirizzo del device, 2 = n.ro registro (secondi), b = letto a Byte
        private string programArguments = "-y 1 0x51 RRR b"; // sostituiremo RRR con il registro da usare effettivamente
        //private string programArguments = " /sys/bus/w1/devices/28-0000062196f0/w1_slave"; // per test

        private Process p;

        public PCF8563_RTC()
        {
            p = new Process();
            // Don't raise event when process exits
            p.EnableRaisingEvents = false;
            // We're using an executable not document, so UseShellExecute false
            p.StartInfo.UseShellExecute = false;
            // Redirect StandardError
            p.StartInfo.RedirectStandardError = true;
            // Redirect StandardOutput so we can capture it
            p.StartInfo.RedirectStandardOutput = true;

            // i2cgetExe has full path to executable
            p.StartInfo.FileName = program;
        }

        public string Lettura(int register)
        {
            // comando con il registro passato come paramentro
            string i2cgetCmdArgs = programArguments.Replace("RRR", register.ToString("0"));
            //Console.WriteLine(i2cgetCmdArgs); 

            // Pass arguments as a single string
            p.StartInfo.Arguments = i2cgetCmdArgs;
            // Now run i2cget & wait for it to finish
            p.Start();
            p.WaitForExit();
            string data = p.StandardOutput.ReadToEnd();

            p.Close();
            p.Dispose();

            //Console.WriteLine(data);
            return (data);
        }

        public int Seconds()
        {
            string d = Lettura(2);
            //Console.WriteLine("{0}  {1}", d.Substring(2, 1), d.Substring(3, 1));

            // da BCD a decimale
            // nibble più significativo
            int s = Int32.Parse(d.Substring(2, 1),
                System.Globalization.NumberStyles.AllowHexSpecifier);
            // mette a zero il bit più significativo di questo nibble 
            s &= 0x7;
            s *= 10;
            // nibble meno significativo
            s += Int32.Parse(d.Substring(3, 1),
                System.Globalization.NumberStyles.AllowHexSpecifier);
            return s;
        }

        public int Month()
        {
            string d = Lettura(7);

            // da BCD a decimale
            // nibble più significativo
            int s = Int32.Parse(d.Substring(2, 1),
                System.Globalization.NumberStyles.AllowHexSpecifier);
            // il primo bit è il riporto dell'anno, lo cancelliamo comunque
            // per l'anno sono usati solo i bit da 0 a 4. Cancelliamo tutti gli altri
            // con un mascheramento: 
            s &= 0x1;
            s *= 10;
            // nibble meno significativo
            s += Int32.Parse(d.Substring(3, 1),
                System.Globalization.NumberStyles.AllowHexSpecifier);
            return s;
        }
    }
}

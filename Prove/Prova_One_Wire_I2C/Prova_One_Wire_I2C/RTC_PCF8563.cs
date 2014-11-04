using System;
using System.Diagnostics;
using System.Threading;

public class RTC_PCF8563
{
    // programma ispirato a: 
    // http://bradsrpi.blogspot.it/2014/07/c-mono-code-to-read-tmp102-i2c.html

    private string i2cgetExe = "/usr/sbin/i2cget";
    // -y 1 = canale I2C n. 1, 0x51 = indirizzo del device, 2 = n.ro registro (secondi), b = letto a Byte
    private string comandoGenericoI2cget = "-y 1 0x51 RRR b"; // sostituiremo RRR con il registro da usare effettivamente
    private string hexString = "";
    private Process p;

    public RTC_PCF8563()
    {
        p = new Process();
    }

    public int Seconds()
    {
        string d = readRawDateData(2);
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
        string d = readRawDateData(7);

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

    private string readRawDateData(int register)
    {
        // comando con il registro passato come paramentro
        string i2cgetCmdArgs = comandoGenericoI2cget.Replace("RRR", register.ToString("0"));
        //Console.WriteLine(i2cgetCmdArgs); 
        // Don't raise event when process exits
        p.EnableRaisingEvents = false;
        // We're using an executable not document, so UseShellExecute false
        p.StartInfo.UseShellExecute = false;
        // Redirect StandardError
        p.StartInfo.RedirectStandardError = true;
        // Redirect StandardOutput so we can capture it
        p.StartInfo.RedirectStandardOutput = true;
        // i2cgetExe has full path to executable
        // Need full path because UseShellExecute is false

        p.StartInfo.FileName = i2cgetExe;
        // Pass arguments as a single string
        p.StartInfo.Arguments = i2cgetCmdArgs;
        // Now run i2cget & wait for it to finish
        p.Start();
        p.WaitForExit();

        string data = p.StandardOutput.ReadToEnd();
        return (data);
    }

    public static void Main(string[] args)
    {
        RTC_PCF8563 t = new RTC_PCF8563();
        while (true)
        {
            //Console.WriteLine("{0}  {1}", t.readRawDateData(), t.Seconds()); 
            Console.WriteLine("{0} s; {1} mese", t.Seconds(), t.Month()); 
            Thread.Sleep(500); 
        }
    }
}
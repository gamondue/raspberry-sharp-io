//#define MAP_SHARED   0x01      /* Share changes */
//#define MAP_PRIVATE   0x02      /* Changes are private */
//#define MAP_TYPE   0x0f      /* Mask for type of mapping */
//#define MAP_FIXED   0x10      /* Interpret addr exactly */
//#define MAP_ANONYMOUS   0x20      /* don't use a file */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace ConsoleApplication9
{
//////    internal class Program
//////    {
//////        private static void Main()
//////        {
//////            int g, rep;

//////            // Set up gpi pointer for direct register access
            
//////            // Switch GPIO 7..11 to output mode

//////            /************************************************************************\
//////             * You are about to change the GPIO settings of your computer.          *
//////             * Mess this up and it will stop working!                               *
//////             * It might be a good idea to 'sync' before running this program        *
//////             * so at least you still have your code changes written to the SD-card! *
//////            \************************************************************************/

//////            // Set GPIO pins 7-11 to output
//////            for (g = 7; g <= 11; g++)
//////            {
//////                Console.WriteLine("Setting up pi {0}",g);
//////                IO.INP_GPIO((uint)g); // must use INP_GPIO before we can use OUT_GPIO
//////                IO.OUT_GPIO((uint)g);
//////            }

//////            for (rep = 0; rep < 10; rep++)
//////            {
//////                Console.WriteLine("Cycle {0} of 10",rep);
//////                for (g = 7; g <= 11; g++)
//////                {
//////                    Console.WriteLine("Setting up pin {0}",g);
//////                    IO.GPIO_SET = (uint)(1 << g);
//////                    Thread.Sleep(333);
//////                }
//////                for (g = 7; g <= 11; g++)
//////                {
//////                    Console.WriteLine("Resetting pin {0}",g);
//////                    IO.GPIO_CLR = (uint)(1 << g);
//////                    Thread.Sleep(333);
//////                }
//////            }

//////            //dont try it
//////            /*Console.WriteLine("Testing max frequency");
//////            var sw = new Stopwatch();
//////            sw.Start();
//////            int cyclesCount = 100000;
//////            for(int i=0;i<cyclesCount;i++)
//////            {
//////                IO.GPIO_SET = (uint)(0x100);
//////                IO.GPIO_CLR = (uint)(0x100);
//////            }
//////            sw.Stop();
//////            Console.WriteLine("Emitted {0} cycles in {1}. Frequency {2}KHz",cyclesCount,sw.Elapsed,cyclesCount/sw.ElapsedMilliseconds);*/
//////        }
//////    }

//////    public static unsafe class IO
//////    {
//////        private const uint BCM2708_PERI_BASE = 0x20000000;
//////        private const uint GPIO_BASE = BCM2708_PERI_BASE + 0x200000; /* GPIO controller */

//////        private const uint PAGE_SIZE = (4 * 1024);
//////        private const uint BLOCK_SIZE = (4 * 1024);

//////        private static int mem_fd;
//////        private static byte* gpio_mem;
//////        private static byte* gpio_map;
//////        private static byte* spi0_mem;
//////        private static byte* spi0_map;

//////        // I/O access
//////        private static volatile uint* gpio;
//////        private static readonly void* NULL = (void*)0;

//////        //TODO: Find constants for libc open
//////        private const int O_RDWR = 02;
//////        private const int O_SYNC = 010000;

//////        //TODO: Find constants for mmap
//////        private const int PROT_READ = 0x1 /* page can be read */;
//////        private const int PROT_WRITE = 0x2 /* page can be written */;
//////        /*
         
//////#define PROT_EXEC   0x4      // page can be executed 
//////#define PROT_NONE   0x0      // page can not be accessed 
//////         */
//////        private static int MAP_SHARED = 0x01      /* Share changes */;
//////        private static int MAP_FIXED = 0x10      /* Interpret addr exactly */;
//////        /*
//////#define MAP_PRIVATE   0x02      // Changes are private
//////#define MAP_TYPE   0x0f      // Mask for type of mapping
//////#define MAP_FIXED   0x10      // Interpret addr exactly
//////#define MAP_ANONYMOUS   0x20      // don't use a file
//////         */

//////        // GPIO setup macros. Always use INP_GPIO(x) before using OUT_GPIO(x) or SET_GPIO_ALT(x,y)
//////        public static void INP_GPIO(uint g)
//////        {
//////            *(gpio + ((g) / 10)) &= ~(7u << (int)(((g) % 10) * 3));
//////        }

//////        public static void OUT_GPIO(uint g)
//////        {
//////            *(gpio + ((g) / 10)) |= 1u << (int)(((g) % 10) * 3);
//////        }

//////        private static void SET_GPIO_ALT(uint g, uint a)
//////        {
//////            *(gpio + (((g) / 10))) |= (((a) <= 3 ? (a) + 4 : (a) == 4 ? 3u : 2) << (int)(((g) % 10) * 3));
//////        }

//////        /// <summary>
//////        /// sets   bits which are 1 ignores bits which are 0
//////        /// </summary>
//////        public static uint GPIO_SET
//////        {
//////            set { *(gpio + 7) = value; }
//////        }

//////        /// <summary>
//////        /// clears bits which are 1 ignores bits which are 0
//////        /// </summary>
//////        public static uint GPIO_CLR
//////        {
//////            set { *(gpio + 10) = value; }
//////        }

//////        /// <summary>
//////        /// Get data 
//////        /// </summary>
//////        public static uint GPIO_IN0 { get { return *(gpio + 13); } }

//////        static IO()
//////        {
//////            setup_io();
//////        }

//////        /// <summary>
//////        /// Set up a memory regions to access GPIO
//////        /// </summary>
//////        private static void setup_io()
//////        {

//////            /* open /dev/mem */
//////            if ((mem_fd = open("/dev/mem", O_RDWR | O_SYNC)) < 0)
//////            {
//////                Console.WriteLine("can't open /dev/mem");
//////                throw new Exception();
//////            }

//////            /* mmap GPIO */

//////            // Allocate MAP block
//////            if ((gpio_mem = malloc(BLOCK_SIZE + (PAGE_SIZE - 1))) == NULL)
//////            {
//////                Console.WriteLine("allocation error");
//////                throw new Exception();
//////            }

//////            // Make sure pointer is on 4K boundary
//////            if ((ulong)gpio_mem % PAGE_SIZE != 0)
//////                gpio_mem += PAGE_SIZE - ((ulong)gpio_mem % PAGE_SIZE);

//////            // Now map it
//////            gpio_map = (byte*)mmap(
//////                gpio_mem,
//////                BLOCK_SIZE,
//////                PROT_READ | PROT_WRITE,
//////                MAP_SHARED | MAP_FIXED,
//////                mem_fd,
//////                GPIO_BASE
//////                                   );

//////            if ((long)gpio_map < 0)
//////            {
//////                Console.WriteLine("mmap error {0}", (int)gpio_map);
//////                throw new Exception();
//////            }

//////            // Always use volatile pointer!
//////            gpio = (uint*)gpio_map;
//////        }

//////        // setup_io
//////        [DllImport("libc.so.6")]
//////        static extern void* mmap(void* addr, uint length, int prot, int flags,
//////                  int fd, uint offset);

//////        [DllImport("libc.so.6")]
//////        static extern int open(string file, int mode /*, int permissions */);

//////        [DllImport("libc.so.6")]
//////        static extern byte* malloc(uint size);
//////    }
}
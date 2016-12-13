using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarCoinApi.Common
{
    public static class ExeptionExtensions
    {
        public static void PrintToConsole(this Exception e)
        {
            if (e is AggregateException)
            {
                foreach (var err in (e as AggregateException).InnerExceptions)
                {
                    
                    var ie = err;
                    while (ie != null)
                    {
                        Console.WriteLine(ie.Message);
                        Console.WriteLine();
                        Console.WriteLine("Stack trace:");
                        Console.WriteLine(ie.StackTrace);

                        ie = ie.InnerException;
                    }
                }
            }
            else
            {
                var ie = e;
                while (ie != null)
                {
                    Console.WriteLine(ie.Message);
                    Console.WriteLine();
                    Console.WriteLine("Stack trace:");
                    Console.WriteLine(ie.StackTrace);

                    ie = ie.InnerException;
                }
            }
        }
    }
}

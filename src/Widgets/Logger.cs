using System.Diagnostics;

namespace WidgetsForUniGetUI
{
    internal static class Logger
    {
        public static void Log(string s)
        {
            Console.WriteLine(s);
            Debug.WriteLine(s);
        }

        public static void Log(Exception e)
        {
            Console.WriteLine(e.ToString());
            Debug.WriteLine(e.ToString());
        }
        public static void Log(int i)
        {
            Console.WriteLine(i.ToString());
            Debug.WriteLine(i.ToString());
        }
    }
}

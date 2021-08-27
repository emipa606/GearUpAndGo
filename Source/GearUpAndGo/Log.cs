using System.Diagnostics;

namespace GearUpAndGo
{
    internal static class Log
    {
        [Conditional("DEBUG")]
        public static void Message(string x)
        {
            Verse.Log.Message(x);
        }
    }
}
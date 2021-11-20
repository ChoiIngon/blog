using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamnet.Util
{
    public static class Debug
    {
        public static string __FILE__()
        {
            string file = StackTraceUtility.ExtractStackTrace();
            file = file.Substring(file.IndexOf("\n") + 1);
            file = file.Substring(0, file.IndexOf("\n"));
            return file;
        }

        public static string __FUNC__()
        {
            string func = StackTraceUtility.ExtractStackTrace();
            func = func.Substring(func.IndexOf("\n") + 1);
            func = func.Substring(0, func.IndexOf("("));
            return func;
        }
    }
}
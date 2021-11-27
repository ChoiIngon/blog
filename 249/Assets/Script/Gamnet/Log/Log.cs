using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Gamnet
{
    class Log
    {
        public enum LogLevel
        {
            DEV,
            INF,
            WRN,
            ERR,
            __Max__
        }

        public enum LogType
        {
            StdErr = 0x00000001,
            File = 0x00000010,
            SysLog = 0x00000100,
        }

        private static Log instance = new Log();

        private int[] levels = new int[(int)LogLevel.__Max__];
        private File file = null;
        public static void Init(string logDir = "log", string prefix = "log", int maxFileSize = 5)
        {
#if !UNITY_EDITOR
            Application.SetStackTraceLogType(UnityEngine.LogType.Log, StackTraceLogType.None);
            Application.SetStackTraceLogType(UnityEngine.LogType.Error, StackTraceLogType.None);
#endif
            instance.SetLevelOuput(LogLevel.DEV, (int)(LogType.StdErr | LogType.File));
            instance.SetLevelOuput(LogLevel.INF, (int)(LogType.StdErr | LogType.File));
            instance.SetLevelOuput(LogLevel.WRN, (int)(LogType.StdErr | LogType.File));
            instance.SetLevelOuput(LogLevel.ERR, (int)(LogType.StdErr | LogType.File));

            DirectoryInfo di = new DirectoryInfo(logDir);
            if (false == di.Exists)
            {
                di.Create();
            }
            instance.file = new File(logDir, prefix, maxFileSize);
        }

        public static void Write(LogLevel level, string str)
        {
            string logStr = $"[{DateTime.Now.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss")}] {level.ToString()} {str}";
            if (0 != (instance.levels[(int)level] & (int)LogType.StdErr))
            {
                switch (level)
                {
                    case LogLevel.DEV :
                        Debug.Log(logStr);
                        break;
                    case LogLevel.INF:
                        Debug.Log(logStr);
                        break;
                    case LogLevel.WRN:
                        Debug.LogWarning(logStr);
                        break;
                    case LogLevel.ERR:
                        Debug.LogError(logStr);
                        break;
                }
            }

            if (0 != (instance.levels[(int)level] & (int)LogType.File))
            {
                StreamWriter st = instance.file.Open();
                st.WriteLine(logStr);
                st.Flush();
            }

            if (0 != (instance.levels[(int)level] & (int)LogType.SysLog))
            {
            }
        }

        void SetLevelOuput(LogLevel level, int flag)
        {
            this.levels[(int)level] = flag;
        }

        class File
        {
            private DateTime now = DateTime.Now;
            private readonly string path;
            private readonly string prefix;
            private readonly int maxFileSize;

            private string fileName;
            private FileInfo fileInfo;
            private StreamWriter streamWriter;
            public File(string path, string prefix, int maxFileSize)
            {
                this.path = path;
                this.prefix = prefix;
                this.maxFileSize = maxFileSize * 1024 * 1024;
            }

            public StreamWriter Open()
            {
                if (null == streamWriter)
                {
                    fileName = path + "/" + prefix + "_" + now.ToString("yyyyMMdd") + ".txt";
                    fileInfo = new FileInfo(fileName);
                    streamWriter = System.IO.File.AppendText(fileName);
                }

                if (maxFileSize < fileInfo.Length)
                {
                    fileName = path + "/" + prefix + "_" + now.ToString("yyyyMMdd") + "_" + now.ToString("HHmmss") + ".txt";
                    fileInfo = new FileInfo(fileName);
                    streamWriter = System.IO.File.AppendText(fileName);
                }

                if (now.Year != DateTime.Now.Year || now.DayOfYear != DateTime.Now.DayOfYear)
                {
                    streamWriter.Close();
                    now = DateTime.Now;
                    fileName = path + "/" + prefix + "_" + now.ToString("yyyyMMdd") + ".txt";
                    fileInfo = new FileInfo(fileName);
                    streamWriter = System.IO.File.AppendText(fileName);
                }

                return streamWriter;
            }
        }
    }
}

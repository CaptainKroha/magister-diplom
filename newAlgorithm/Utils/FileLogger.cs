using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace magisterDiplom.Utils
{
    public class FileLogger : ILogger
    {

        private readonly StreamWriter _writer;
        private readonly object _lock = new object();

        public FileLogger(string filename)
        {
            _writer = new StreamWriter(filename, append: true, Encoding.UTF8)
            {
                AutoFlush = true
            };
        }

        void ILogger.Debug(string message) => Log("DEBUG", message);
        void ILogger.Info(string message) => Log("INFO", message);
        void ILogger.Warning(string message) => Log("WARNING", message);
        void ILogger.Error(string message) => Log("ERROR", message);

        void ILogger.Print(string message)
        {
            lock (_lock)
            {
                _writer.WriteLine(message);
            }
        }

        void ILogger.Print(string message,int[] array)
        {
            string logline = message + " [";
            for(int i = 0; i < array.Length; i++)
            {
                logline += array[i].ToString() + " ";
            }
            logline += "]";
            lock (_lock)
            {
                _writer.WriteLine(logline);   
            }
        }

        void ILogger.Print(string message, List<List<int>> matrix)
        {
            string logline = message + "\n[";
            for (int i = 0; i < matrix.Count; i++)
            {
                for(int j = 0; j < matrix[i].Count; j++)
                {
                    logline += matrix[i].ToString() + " ";
                }
                if(i < matrix.Count - 1) logline += "\n";
            }
            logline += "]";
            lock (_lock)
            {
                _writer.WriteLine(logline);
            }
        }

        private void Log(string level, string message)
        {
            string logLine = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}";
            lock (_lock)
            {
                _writer.WriteLine(logLine);
            }
        }

        public void Dispose()
        {
            _writer?.Dispose();
        }
    }
}

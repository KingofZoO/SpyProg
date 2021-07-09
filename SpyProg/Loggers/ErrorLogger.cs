using System;
using System.IO;

namespace SpyProg.Loggers {
    class ErrorLogger {
        private string errorLogFile;

        public ErrorLogger() {
            errorLogFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "errorLogFile.txt");
        }

        public void Log(Exception exception) {
            using (StreamWriter sw = File.AppendText(errorLogFile)) {
                sw.WriteLine($"{DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss")} - {exception.StackTrace}");
            }
        }
    }
}

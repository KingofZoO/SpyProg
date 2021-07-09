using System;
using System.Collections.Generic;
using System.IO;

namespace SpyProg.Loggers {
    //logger for tracking time, spending in Config's processes
    class SessionSpyLogger {
        private string sessionSpyFile;

        public void NewSession() {
            sessionSpyFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"sessionSpyFile({DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss")}).txt");
        }

        public void Log(Dictionary<string,string> processDictionary) {
            using (StreamWriter sw = new StreamWriter(sessionSpyFile, false)) {
                foreach (var p in processDictionary) {
                    sw.WriteLine($"{p.Key} - {p.Value}");
                }
            }
        }
    }
}

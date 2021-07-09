using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using IWshRuntimeLibrary;
using SpyProg.Loggers;

namespace SpyProg {
    public partial class Form1 : Form {
        //current active user's window
        private string currentWindow = "";
        //previous active user's window
        private string previousWindow = "";
        //flag for spying time restriction
        private bool isNewSession = true;

        private readonly Config config;

        //dictionary contains Config's processes(key) and time, spending in it(value)
        private Dictionary<string, string> processDictionary = new Dictionary<string, string>();
        //time of user's activity in tracking process
        private DateTime actTime = DateTime.MinValue;

        private SessionSpyLogger sessionLogger;
        private DaySpyLogger dayLogger;
        private ErrorLogger errorLogger;

        public Form1() {
            InitializeComponent();
            this.Load += HideForm;
            CreateShortcut();

            config = new Config();
            config.SerializeConfig();

            sessionLogger = new SessionSpyLogger();
            dayLogger = new DaySpyLogger();
            errorLogger = new ErrorLogger();
        }

        // hide application from user
        private void HideForm(object sender, EventArgs e) {
            Form form = (Form)sender;
            form.ShowInTaskbar = false;
            form.Location = new System.Drawing.Point(-10000, -10000);
        }

        private void spyTimer_Tick(object sender, EventArgs e) {
            try {
                if (IsSpyingTime) {
                    if (isNewSession) {
                        NewSpyingSession();
                    }

                    currentWindow = WinApi.GetActiveWindowName();
                    //if active window is change - log
                    if (currentWindow != previousWindow && !string.IsNullOrEmpty(currentWindow)) {
                        dayLogger.Log(currentWindow);
                    }

                    //check if windows must be tracked (contains in Config's processes)
                    string currentProcess = ProcessNameComparison(currentWindow);
                    string previousProcess = ProcessNameComparison(previousWindow);

                    //if tracking processes change - log
                    if (currentWindow != previousWindow && previousProcess != null) {
                        SaveTrackingProcessInfo(previousProcess);
                    }

                    //if user activate new tracking process - reset actTime
                    if (currentWindow != previousWindow && currentProcess != null) {
                        actTime = DateTime.Now;
                    }
                    previousWindow = currentWindow;

                    return;
                }

                if (!isNewSession) {
                    SpyingSessionIsOver();
                }
            }
            catch(Exception ex) {
                errorLogger.Log(ex);
            }
        }

        //spying time is restricted in Config file
        private bool IsSpyingTime => DateTime.Now >= config.ActStartTime && DateTime.Now < config.ActEndTime;

        //if window name contains name of tracking process - return window name, otherwise null
        private string ProcessNameComparison(string procName) => config.Processes.FirstOrDefault(p => procName.ToLower().Contains(p.ToLower()));

        private void NewSpyingSession() {
            isNewSession = false;
            sessionLogger.NewSession();
            processDictionary = new Dictionary<string, string>();
        }

        private void SpyingSessionIsOver() {
            string currentProcess = ProcessNameComparison(currentWindow);
            if (currentProcess != null)
                SaveTrackingProcessInfo(currentProcess);

            if (currentWindow != previousWindow) {
                dayLogger.Log(currentWindow);
            }

            currentWindow = "";
            previousWindow = "";
            isNewSession = true;
        }

        private void SaveTrackingProcessInfo(string processName) {
            TimeSpan timeSpan = DateTime.Now - actTime;

            //tracking time that user spent in process
            if (processDictionary.ContainsKey(processName)) {
                processDictionary[processName] = (DateTime.Parse(processDictionary[processName]) + timeSpan).ToString("HH:mm:ss");
            }
            else
                processDictionary.Add(processName, (new DateTime(timeSpan.Ticks)).ToString("HH:mm:ss"));

            sessionLogger.Log(processDictionary);
        }

        //create autorun shortcut
        private void CreateShortcut() {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "SpyProg.lnk");
            if (!System.IO.File.Exists(path)) {
                WshShell wsh = new WshShell();
                IWshShortcut shortcut = wsh.CreateShortcut(path);
                shortcut.TargetPath = System.Reflection.Assembly.GetEntryAssembly().Location;
                shortcut.Save();
            }
        }
    }
}

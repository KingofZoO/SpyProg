using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using IWshRuntimeLibrary;

namespace SpyProg {
    public partial class Form1 : Form {
        private string currWindow = "";
        private string prevWindow = "";
        private bool lastSaveFlag = false;

        private string spyFilePath;

        private readonly Config config;

        private Dictionary<string, string> procDict = new Dictionary<string, string>();
        private DateTime actTime = DateTime.MinValue;

        public Form1() {
            InitializeComponent();
            this.Load += new System.EventHandler(this.OnFormLoad);
            CreateShortcut();

            spyFilePath = NewFilePath;

            config = new Config();
            config.SerializeConfig();
        }

        private string NewFilePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"spyFile({DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss")}).txt");

        // hide application from user
        private void OnFormLoad(object sender, EventArgs e) {
            Form form = (Form)sender;
            form.ShowInTaskbar = false;
            form.Location = new System.Drawing.Point(-10000, -10000);
        }

        private void spyTimer_Tick(object sender, EventArgs e) {
            try {
                if (TimeRestrict) {
                    if (!lastSaveFlag) {
                        lastSaveFlag = true;
                        spyFilePath = NewFilePath;
                        procDict = new Dictionary<string, string>();
                    }

                    currWindow = WinApi.GetActiveWindowName();
                    string currProc = ProcRestrict(currWindow);
                    string prevProc = ProcRestrict(prevWindow);

                    if (currWindow != prevWindow && prevProc != null) {
                        SaveSpyInfo(prevProc);
                    }

                    if (currWindow != prevWindow && currProc != null) {
                        actTime = DateTime.Now;
                    }
                    prevWindow = currWindow;

                    return;
                }

                if (lastSaveFlag) {
                    string currProc = ProcRestrict(currWindow);
                    if (currProc != null)
                        SaveSpyInfo(currProc);

                    currWindow = "";
                    prevWindow = "";
                    spyFilePath = "";
                    lastSaveFlag = false;
                }
            }
            catch { }
        }

        private bool TimeRestrict => DateTime.Now >= config.ActStartTime && DateTime.Now < config.ActEndTime;
        private string ProcRestrict(string procName) => config.Processes.FirstOrDefault(p => procName.ToLower().Contains(p.ToLower()));

        private void SaveSpyInfo(string procName) {
            TimeSpan timeSpan = DateTime.Now - actTime;

            if (procDict.ContainsKey(procName)) {
                procDict[procName] = (DateTime.Parse(procDict[procName]) + timeSpan).ToString("HH:mm:ss");
            }
            else
                procDict.Add(procName, (new DateTime(timeSpan.Ticks)).ToString("HH:mm:ss"));

            using (StreamWriter sw = new StreamWriter(spyFilePath, false)) {
                foreach (var p in procDict) {
                    sw.WriteLine($"{p.Key} - {p.Value}");
                }
            }
        }

        // autorun shortcut
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

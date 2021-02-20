using System;
using System.IO;
using System.Xml.Serialization;

namespace SpyProg {
    public class Config {
        public string StartTime { get; set; } = "09:00:00";
        public string EndTime { get; set; } = "18:00:00";

        // processes to track
        public string[] Processes { get; set; } = new string[] {
            "Chrome",
            "Edge",
            "Opera",
            "Mozilla",
            "Word",
            "Excel"
        };

        public DateTime ActStartTime => DateTime.Parse(StartTime);
        public DateTime ActEndTime => DateTime.Parse(EndTime);

        private XmlSerializer xml = new XmlSerializer(typeof(Config));
        private readonly string configPath;

        public Config() {
            configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.xml");
        }

        public void SerializeConfig() {
            if (File.Exists(configPath)) {
                using (FileStream fs = new FileStream(configPath, FileMode.Open)) {
                    var c = (Config)xml.Deserialize(fs);
                    StartTime = c.StartTime;
                    EndTime = c.EndTime;
                    Processes = c.Processes;
                }
            }
            else {
                using (FileStream fs = new FileStream(configPath, FileMode.Create)) {
                    xml.Serialize(fs, this);
                }
            }
        }
    }
}

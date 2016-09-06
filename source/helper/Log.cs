using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;

namespace MyDocGenerator.source.helper
{
    class Log
    {
        private static volatile Log logInstance = null;
        private static object sync = new Object();

        private Log() { 
            
        }

        public static Log getInstance { 
            
            get{
                lock (sync) {
                    if (logInstance == null)
                    {
                        logInstance = new Log();
                    }
                    return logInstance;
                }
            } 
        }

        public void writeToLog(string logData){
            try
            {
                Directory.CreateDirectory(@"./logs");

                string date = System.DateTime.Today.ToString("MM-dd-yyyy", CultureInfo.CreateSpecificCulture("en-US"));
                string path = "./logs/" + date + ".txt";
                using(StreamWriter writer = new StreamWriter(path,true)){
                    writer.WriteLine(System.DateTime.Now.ToString("MM-dd-yyyy hh:mm:ss: ", CultureInfo.CreateSpecificCulture("en-US")) + logData);
                    writer.Flush();
                }
            }
            catch (Exception e) { 
            
            }
        }
    }
}

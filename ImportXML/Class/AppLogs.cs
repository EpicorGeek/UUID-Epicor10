using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Threading.Tasks;
using System.IO;

namespace ImportXML.Class
{
    class AppLogs
    {
        public string todayLogPath = String.Empty;

        public void CreateLog()
        {
            // Creación de carpeta de logs
            string root = ConfigurationManager.AppSettings["rootFolder"];
            if (!System.IO.File.Exists(root))
                System.IO.Directory.CreateDirectory(root);

            // Creación de la carpeta del día
            string today = DateTime.Now.ToString("yyyy-MM-dd_HHmmss") + ".txt";
            string todayLog = System.IO.Path.Combine(root,today);
            if (!System.IO.File.Exists(todayLog))
            {
                System.IO.StreamWriter file = new System.IO.StreamWriter(todayLog);
                file.Close();

                todayLogPath = todayLog;

                using (System.IO.StreamWriter sw = System.IO.File.AppendText(todayLog))
                {
                    sw.WriteLine("Comienza el proceso de sincronización de datos en Epicor");
                }
            }
            else
            {
                using (System.IO.StreamWriter sw = System.IO.File.AppendText(todayLog))
                {
                    sw.WriteLine("Comienza el proceso de sincronización de datos en Epicor...");
                }
            }
        }

        public void writeContentToFile(string text)
        {
            using (StreamWriter sw = File.AppendText(todayLogPath))
                sw.WriteLine(text);
        }
    }
}

using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Text;
using System.Data;
using System.Configuration;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ImportXML.Class
{
    class Orchestrator
    {
        public string collector = String.Empty;
        public List<string> fileNames = new List<string>();
        public List<string> filesPath = new List<string>();

        public void delProcessedFiles()
        {
            DirectoryInfo processed = new DirectoryInfo(ConfigurationManager.AppSettings["destinationFolder"]);
            Console.WriteLine("Elminando archivos procesados.");
            string[] files = Directory.GetFiles(ConfigurationManager.AppSettings["destinationFolder"], "*.XML");

            if (files.Length > 0)
            {
                foreach (var file in processed.GetFiles("*.XML"))
                {
                    file.Delete();
                }
            }
        }

        public void obtainXMLFiles()
        {
            int ind = 0;
            int nFacturas = Convert.ToInt32(ConfigurationManager.AppSettings["invcToProcess"]);
            DirectoryInfo carpeta = new DirectoryInfo(ConfigurationManager.AppSettings["sourceXMLFolder"]);
            Console.WriteLine("En esta ejecución se procesarán los siguientes archivos...");
            string[] files = Directory.GetFiles(ConfigurationManager.AppSettings["sourceXMLFolder"], "*.XML");
            if (files.Length == 0)
            {
                Console.WriteLine("No se encontraron archivos que leer, termina la ejecución");
                //collector = "No se encontraron archivos que leer, termina la ejecución";
            }
            else
            {
                foreach (var archivo in carpeta.GetFiles("*.XML"))
                {
                    if (ind < nFacturas)
                    {
                        Console.WriteLine(archivo.Name);
                        fileNames.Add(archivo.Name);
                        filesPath.Add(archivo.FullName);
                        collector += archivo.Name + "\n";
                    }
                    else
                        break;
                    ind++;
                }
            }
        }

        public string companyToConnect(string rfc)
        {
            collector = String.Empty;

            if (rfc.Equals("DLO120720KT5"))
                collector = "DLMAC";
            if (rfc.Equals("IES081127F24"))
                collector = "TT";
            if (rfc.Equals("CRE100317IM3"))
                collector = "CC";
            if (rfc.Equals("SIS031011SE2"))
                collector = "SIS";
            if (rfc.Equals("GFC861222CZ9"))
                collector = "GFC";
            if (rfc.Equals("GIC031010AP2"))
                collector = "MAC";
            if (rfc.Equals("PSC0501261S2"))
                collector = "PSCZ";

            return collector;
        }
    }
}

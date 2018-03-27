using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Threading.Tasks;
using Epicor10;

namespace ImportXML
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Class.AppLogs log = new Class.AppLogs();
                Class.Orchestrator core = new Class.Orchestrator();
                string envioCorrectos = ConfigurationManager.AppSettings["destinationFolder"];
                int factura = 0;
                int indexFileNameList = 0;
                String fecha = String.Empty;
                string UUID = String.Empty;
                string SATCertificate = String.Empty;
                string rfc = String.Empty;
                log.CreateLog();
                core.delProcessedFiles(); // Borrado de archivos ya procesados
                Console.WriteLine("Inicia el proceso de sincronización en Epicor...");
            
                //Obtención de archivos XML
                core.obtainXMLFiles();
                if (core.collector.Equals(""))
                    log.writeContentToFile("No se encontraron archivos que leer, termina la ejecución");
                else
                    log.writeContentToFile(core.collector);

                // Iniciando conexión a Epicor
                Epicor10.EpiAdapters epicor = new EpiAdapters("vordmaker", "maker2016");

                //Obtención de datos fiscales por cada XML
                foreach (String item in core.filesPath)
                {
                    Console.WriteLine("\n");
                    Console.WriteLine("Obteniendo datos del archivo " + item);
                    log.writeContentToFile("\n");
                    log.writeContentToFile("Obteniendo datos del archivo " + item);

                    System.Xml.XmlTextReader reader = new System.Xml.XmlTextReader(item);

                    while (reader.Read())
                    {
                        if (reader.NodeType == System.Xml.XmlNodeType.Element)
                        {
                            if (reader.Name.Equals("cfdi:Emisor"))
                            {
                                if (reader.HasAttributes)
                                {
                                    Console.WriteLine("_cfdi:Emisor_");
                                    Console.WriteLine("Compañía: " + reader.GetAttribute("Nombre"));
                                    Console.WriteLine("RFC: " + reader.GetAttribute("Rfc"));
                                    rfc = reader.GetAttribute("Rfc");
                                    log.writeContentToFile("Compañía: " + reader.GetAttribute("Nombre"));
                                    log.writeContentToFile("RFC: " + reader.GetAttribute("Rfc"));
                                }
                            }

                            if (reader.Name.Equals("cfdi:Addenda"))
                            {
                                reader.ReadToFollowing("fa:AddendaComercial");
                                if (!reader.HasAttributes)
                                {
                                    reader.ReadToFollowing("fa:Empresa");
                                    factura = Convert.ToInt32(reader.GetAttribute("NumeroInterno"));
                                    Console.WriteLine("_cfdi:Empresa_");
                                    Console.WriteLine("Número Legal: " + reader.GetAttribute("NumeroLegal"));
                                    Console.WriteLine("Número Interno: " + reader.GetAttribute("NumeroInterno"));
                                    Console.WriteLine("Fecha Contable: " + reader.GetAttribute("FechaContable"));
                                    Console.WriteLine("Fecha Emision: " + reader.GetAttribute("FechaEmision"));

                                    log.writeContentToFile("Número Legal: " + reader.GetAttribute("NumeroLegal"));
                                    log.writeContentToFile("Número Interno: " + reader.GetAttribute("NumeroInterno"));
                                    log.writeContentToFile("Fecha Contable: " + reader.GetAttribute("FechaContable"));
                                    log.writeContentToFile("Fecha Emision: " + reader.GetAttribute("FechaEmision"));
                                }
                            }

                            if (reader.Name.Equals("cfdi:Complemento"))
                            {
                                reader.ReadToFollowing("tfd:TimbreFiscalDigital");
                                Console.WriteLine("_cfdi:Complemento_");
                                Console.WriteLine("UUID: " + reader.GetAttribute("UUID"));
                                UUID = reader.GetAttribute("UUID");
                                fecha = reader.GetAttribute("FechaTimbrado");
                                Console.WriteLine("Fecha: " + reader.GetAttribute("FechaTimbrado"));
                                Console.WriteLine("SAT: " + reader.GetAttribute("noCertificadoSAT")); // Versión 3.2
                                SATCertificate = reader.GetAttribute("noCertificadoSAT");
                                log.writeContentToFile("UUID: " + reader.GetAttribute("UUID"));
                                log.writeContentToFile("Fecha: " + reader.GetAttribute("FechaTimbrado"));
                                log.writeContentToFile("SAT: " + reader.GetAttribute("noCertificadoSAT"));
                            }
                            /*
                            if (reader.Name.Equals("cfdi:Comprobante"))
                            {
                                if (reader.HasAttributes)
                                {
                                    Console.WriteLine("_cfdi:Comprobante_");
                                    Console.WriteLine("Version: " + reader.GetAttribute("version"));
                                    Console.WriteLine("TipoComprobante: " + reader.GetAttribute("tipoDeComprobante"));
                                }
                            }
                            */

                            /*
                            if (reader.Name.Equals("cfdi:Receptor"))
                            {
                                if (reader.HasAttributes)
                                {
                                    Console.WriteLine("_cfdi:Receptor_");
                                    Console.WriteLine("RFC: " + reader.GetAttribute("rfc"));
                                }
                            }
                            */
                        }
                    }

                    //Actualizacion Epicor
                    Console.WriteLine("Actualizando UUID de la factura...");
                    string empresa = core.companyToConnect(rfc);
                    Console.WriteLine("Conectando a ... " + empresa);
                
                    epicor.setCompany(empresa);
                    if (epicor.EventCollector.Equals(""))
                    {
                        epicor.UpdateInvcHeader(factura, UUID, SATCertificate, fecha);
                        if (epicor.EventCollector.Equals(""))
                        {
                            Console.WriteLine("Actualización de datos completa.");
                            log.writeContentToFile("Actualización de datos completa.");

                            reader.Close();
                            string destination = System.IO.Path.Combine(envioCorrectos, core.fileNames[indexFileNameList]);
                            System.IO.File.Copy(item, destination);
                            System.IO.File.Delete(item);
                        }
                        else
                        {
                            Console.WriteLine("Ocurrió un error al actualizar la información \n" + epicor.EventCollector);
                            log.writeContentToFile("Ocurrió un error al actualizar la información \n" + epicor.EventCollector);
                        }
                    }
                    else
                    {
                        Console.WriteLine(epicor.EventCollector);
                        log.writeContentToFile(epicor.EventCollector);
                    }
                
                    Console.WriteLine("\n");
                    log.writeContentToFile("\n");
                    indexFileNameList++;
                }
                log.writeContentToFile("Hemos terminado !!!");
            }
            catch(Exception rmp)
            {
                Console.WriteLine(rmp.StackTrace);
            }
            Console.WriteLine("Hemos terminado !!!");
        }
    }
}

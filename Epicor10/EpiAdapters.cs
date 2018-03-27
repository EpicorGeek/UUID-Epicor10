using System;
using System.Data;
using System.Configuration;
using System.ServiceModel.Channels;
using Epicor.ServiceModel.StandardBindings;
using Ice.Proxy.BO;
using Ice.Lib;
using Erp.Proxy.BO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Configuration;
using Ice.BO;
using Erp.BO;
using Ice.Core;

namespace Epicor10
{
    public class EpiAdapters
    {
        Credenciales cred;
        Session epiSession;
        public string EventCollector = String.Empty;
        protected string fileSys;
        protected string getCompany;

        public EpiAdapters(string environment, string company, string user, string pass)
        {
            cred = new Credenciales();
            fileSys = String.Format(ConfigurationManager.AppSettings["epiEnvironment"].ToString(), environment);
            getCompany = company;
            cred.username = user;
            cred.password = pass;
        }

        public EpiAdapters(string user, string pass, string company)
        {
            cred = new Credenciales();
            fileSys = String.Format(ConfigurationManager.AppSettings["epiEnvironment"].ToString(), "Epicor10");
            cred.username = user;
            cred.password = pass;
            epiSession = new Session(cred.username, cred.password, Session.LicenseType.Default, fileSys);
            getCompany = company;
        }

        public EpiAdapters(string user, string pass)
        {
            cred = new Credenciales();
            fileSys = String.Format(ConfigurationManager.AppSettings["epiEnvironment"], "Epicor10");
            cred.username = user;
            cred.password = pass;
        }

        public EpiAdapters(string company)
        {
            getCompany = company;
            fileSys = ConfigurationManager.AppSettings["epiEnvironment"].ToString();
            cred.username = "vordmaker";
            cred.password = "maker2016";
        }

        public void setCompany(string currentCompany)
        {
            try
            {
                EventCollector = String.Empty;
                string appServerUrl = string.Empty;
                EnvironmentInformation.ConfigurationFileName = fileSys;
                appServerUrl = AppSettingsHandler.AppServerUrl;
                CustomBinding wcfBinding = NetTcp.UsernameWindowsChannel();
                Uri CustSvcUri = new Uri(String.Format("{0}/Ice/BO/{1}.svc", appServerUrl, "UserFile"));

                using (Ice.Proxy.BO.UserFileImpl US = new Ice.Proxy.BO.UserFileImpl(wcfBinding, CustSvcUri))
                {
                    US.ClientCredentials.UserName.UserName = cred.username;
                    US.ClientCredentials.UserName.Password = cred.password;
                    US.SaveSettings(cred.username, true, currentCompany, true, false, true, true, true, true, true, true, true,
                                               false, false, -2, 0, 1456, 886, 2, "MAINMENU", "", "", 0, -1, 0, "", false);
                    US.Close();
                    US.Dispose();
                }
            }
            catch (System.UnauthorizedAccessException loginError)
            {
                EventCollector = loginError.Message;
            }
            catch (Ice.Common.BusinessObjectException businessException)
            {
                EventCollector = businessException.Message;
            }
            catch (Exception error)
            {
                EventCollector = error.Message;
            }
        }

        public void NewPatchFldRow()
        {
            try
            {
                string appServerUrl = string.Empty;
                EventCollector = String.Empty;

                EnvironmentInformation.ConfigurationFileName = fileSys;
                appServerUrl = AppSettingsHandler.AppServerUrl;
                CustomBinding wcfBinding = new CustomBinding();
                wcfBinding = NetTcp.UsernameWindowsChannel();
                Uri CustSvcUri = new Uri(string.Format("{0}/Ice/BO/{1}.svc", appServerUrl, "PatchFld"));
                using (PatchFldImpl OB = new PatchFldImpl(wcfBinding, CustSvcUri))
                {
                    OB.ClientCredentials.UserName.UserName = cred.username;
                    OB.ClientCredentials.UserName.Password = cred.password;
                    PatchFldDataSet dataRow = new PatchFldDataSet();
                    OB.GetNewPatchFld(dataRow, "DLMAC", "InvcHead", "MXFiscalFolio");
                }
            }
            catch (Ice.Common.BusinessObjectException epiException)
            {
                Console.WriteLine(epiException.Message);
            }
            catch (Exception sysException)
            {
                Console.WriteLine(sysException.Message);
            }
        }

        public void UpdateInvcHeader(int invoiceNum, string UUID, string SATCertificate,string fecha)
        {
            try
            {
                string appServerUrl = string.Empty;
                EventCollector = String.Empty;

                EnvironmentInformation.ConfigurationFileName = fileSys;
                appServerUrl = AppSettingsHandler.AppServerUrl;
                CustomBinding wcfBinding = new CustomBinding();
                wcfBinding = NetTcp.UsernameWindowsChannel();
                Uri CustSvcUri = new Uri(string.Format("{0}/Erp/BO/{1}.svc", appServerUrl, "ARInvoice"));
                using (ARInvoiceImpl OB = new ARInvoiceImpl(wcfBinding, CustSvcUri))
                {
                    OB.ClientCredentials.UserName.UserName = cred.username;
                    OB.ClientCredentials.UserName.Password = cred.password;
                    ARInvoiceDataSet InvcData = OB.GetByID(invoiceNum);
                    InvcData.Tables["InvcHead"].Rows[0]["MXFiscalFolio"] = UUID;
                    InvcData.Tables["InvcHead"].Rows[0]["MXCertifiedTimeStamp"] = fecha;
                    InvcData.Tables["InvcHead"].Rows[0]["MXSATCertificateSN"] = "00001000000406725461";
                    OB.Update(InvcData);
                }
            }
            catch (Ice.Common.BusinessObjectException epiException)
            {
                EventCollector = epiException.Message + "\n" + epiException.StackTrace;
            }
            catch (Exception sysException)
            {
                EventCollector = sysException.Message + "\n" + sysException.StackTrace;
            }
            /*
            finally
            {
                epiSession.Dispose();
                epiSession = null;
            }
            */
        }
    }
}

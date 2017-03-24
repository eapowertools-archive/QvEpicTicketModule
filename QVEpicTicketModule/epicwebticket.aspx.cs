using System;
using System.Web;
using System.IO;
using System.Web.Security;
using System.Security.Cryptography;
using System.Net;
using System.Xml.Linq;
using System.Configuration;
using System.Text;

namespace QVEpicTicketModule
{
    public partial class epicwebticket : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //Step 1 - Is there a token query string parameter
            string token = null;
            string EpicUser = string.Empty;
            string decryptedData;

            if (Request.QueryString["token"] != null)
            {
                token = Server.UrlDecode(Request.QueryString["token"].ToString());
                decryptedData = DecryptString(token);
                EpicUser = decryptedData;
            }

            //Step 2 - Extract document name if it exists for direct document connectivity to QlikView.
            string document = null;
            if (Request.QueryString["iDocID"] != null)
            {
                document = Request.QueryString["iDocID"].ToString();
            }

            //Step 3 - Check for the existence of an Accesspoint session cookie to avoid ticket process.
            HttpCookie cookie = Request.Cookies.Get("AccessPointSession");

            auth(cookie, ConfigurationManager.AppSettings["QlikViewServerHostname"], EpicUser, document);

        }

        // This function is going to take the username and groups and return a web ticket from QV
        // User and groups relate to the user you want to request a ticket for
        private string getTicket(string user)
        {
            try
            {
                string QlikViewServerURL = "http://10.8.114.11/qvajaxzfc/getwebticket.aspx";
                string webTicketXml = string.Format("<Global method=\"GetWebTicket\"><UserId>{0}</UserId></Global>", user);

                HttpWebRequest client = (HttpWebRequest)WebRequest.Create(new Uri(QlikViewServerURL));
                client.Method = "POST";
                client.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                using (StreamWriter sw = new StreamWriter(client.GetRequestStream()))
                    sw.WriteLine(webTicketXml);
                StreamReader sr = new StreamReader(client.GetResponse().GetResponseStream());
                string result = sr.ReadToEnd();

                XDocument doc = XDocument.Parse(result);
                return doc.Root.Element("_retval_").Value;
            }
            catch(Exception e)
            {
                Console.WriteLine("Exprienced error: {0}", e.Message);
                throw;
            }

        }

        private void auth(HttpCookie cookie, string QlikViewHostname, string EpicUser, string document)
        {
            try
            {
                bool qlikSession = hasSession(cookie);

                if (qlikSession && !string.IsNullOrEmpty(document))
                {
                    string destUrl = Server.UrlEncode("/qvajaxzfc/opendoc.htm?document=" + document);
                    Response.Redirect("../qvajaxzfc/authenticate.aspx?keep=&type=html&try=" + destUrl + "&back=");
                   
                }
                else if (qlikSession && string.IsNullOrEmpty(document))
                {
                    //launch access point
                    string destUrl = Server.UrlEncode("/qlikview");
                    Response.Redirect("../qvajaxzfc/authenticate.aspx?keep=&type=html&try=" + destUrl + "&back=");
                  
                }
                else
                {
                    if (!string.IsNullOrEmpty(EpicUser))
                    {
                        //first time connecting to QlikView
                        string ticky = getTicket(EpicUser);
                        Response.Redirect("http://" + QlikViewHostname + "/qvajaxzfc/authenticate.aspx?type=html&try=/qlikview&back=/LoginPage.htm&webticket=" + ticky);
                      
                    }
                    else
                    {
                        if (!qlikSession)
                        {
                            Response.Write("No userId parameter supplied.  Error!");
                          
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("Experienced Error: {0}", e.Message);
                throw;
            }
        }

        private bool hasSession(HttpCookie cookie)
        {
            if (cookie == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private string DecryptString(string encryptedString)
        {
            var appSettings = ConfigurationManager.AppSettings;
            byte[] sharedSecret = Encoding.ASCII.GetBytes(appSettings["sharedSecret"]);
            byte[] iv = Encoding.ASCII.GetBytes(appSettings["iv"]);
            string decrypted;
            byte[] encryptedBytes = Convert.FromBase64String(encryptedString);

            decrypted = DecryptStringFromBytes_Aes(encryptedBytes, sharedSecret, iv);
            return decrypted;
        }


        static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            try
            {
                // Check arguments.
                if (cipherText == null || cipherText.Length <= 0)
                    throw new ArgumentNullException("cipherText");
                if (Key == null || Key.Length <= 0)
                    throw new ArgumentNullException("Key");
                if (IV == null || IV.Length <= 0)
                    throw new ArgumentNullException("IV");

                // Declare the string used to hold
                // the decrypted text.
                string plaintext = null;

                // Create an AesManaged object
                // with the specified key and IV.
                using (AesManaged aesAlg = new AesManaged())
                {
                    aesAlg.Key = Key;
                    aesAlg.IV = IV;

                    // Create a decrytor to perform the stream transform.
                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                    // Create the streams used for decryption.
                    using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {

                                // Read the decrypted bytes from the decrypting stream
                                // and place them in a string.
                                plaintext = srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
                return plaintext;
            }
            catch(Exception e)
            {
                Console.WriteLine("Experienced Error: {0}", e.Message);
                throw;
            }
        }
    }
}
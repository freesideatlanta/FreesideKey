using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.ComponentModel;
using System.Configuration;

using System.Net;
using System.Net.Sockets;


using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;


using System.Diagnostics;


namespace FreesideKeyService
{
    static class SSLKeyManager
    {


        private const String KEY_FILE = "FreesideKeySystem.pfx";
        private const String KEY_PASS = "AECNBEHNRDPXP2R4KQ76YN5C";
        private static readonly byte[] SERIAL_BYTES = new byte[] { 0xcd, 0xef, 0xfd, 0xbd };
        private const String KEY_FN = "FS Key Server SSL Cert (v0.1)";

        //private const int port = FSKeyCommon.Properties.Settings.Default.serverPort;
        public static bool SetupSSLCert()
        {
            //Get Certificate From Embedded Resources
            Assembly execAssembly = Assembly.GetExecutingAssembly();

            //Check For Existing Cert:
            X509Store store;


            X509Certificate2 rootCert = null;

            store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadWrite);

            bool rootCertFound = false;
            foreach (X509Certificate2 c in store.Certificates)
            {
                if (c.FriendlyName == KEY_FN)
                    if (Convert.ToDateTime(c.GetExpirationDateString()) < DateTime.Now.AddYears(1))  //Cert About to Expire REmove It.
                        store.Remove(c);
                    else if (rootCertFound) //Clear Duplicate Root Certs
                        store.Remove(c);
                    else
                    {
                        rootCert = c;
                        rootCertFound = true;
                    }
            }
            store.Close();

            //Grab Personal Cert
            X509Certificate2 serviceCert = null;
            store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadWrite);

            foreach (X509Certificate2 c in store.Certificates)
            {
                if (c.FriendlyName == KEY_FN)
                    if (Convert.ToDateTime(c.GetExpirationDateString()) < DateTime.Now.AddYears(1))  //Cert About to Expire. Remove it.
                        store.Remove(c);
                    else
                        if (rootCert == null)  //No Root Cert Found. Delete All Personal Certs
                        store.Remove(c);
                    else if (rootCert.PublicKey != c.PublicKey)  //Doesn't Match our root cert
                    {
                        serviceCert = c;
                        break;
                    }
            }
            store.Close();

            //Delete Root Cert if No Matching Service Cert
            if ((rootCert != null) && (serviceCert == null))
            {
                store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadWrite);
                store.Remove(rootCert);
                store.Close();
                rootCert = null;
            }


            //Create New Certs if Needed
            if (rootCert == null)
            {

                String DNSname = System.Net.Dns.GetHostName();

                var rsa = RSA.Create(2048); // generate asymmetric key pair
                var req = new CertificateRequest("CN=Freeside Technology Spaces LLC", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

                req.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.DataEncipherment, false));
                req.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection { new Oid("1.3.6.1.5.5.7.3.1") }, false));

                //Build SAN
                SubjectAlternativeNameBuilder SANBuilder = new SubjectAlternativeNameBuilder();
                SANBuilder.AddDnsName(DNSname);
                req.CertificateExtensions.Add(SANBuilder.Build());
                //Create Cert
                rootCert = req.CreateSelfSigned(new DateTimeOffset(DateTime.UtcNow).AddDays(-5), new DateTimeOffset(DateTime.UtcNow).AddYears(20));
                rootCert.FriendlyName = "FS Key Server SSL Cert (v0.1)";


                byte[] rootCertBytes = rootCert.Export(X509ContentType.Pfx);
                serviceCert = new X509Certificate2(rootCertBytes, "", X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.MachineKeySet);



                //Add To Root if Not There Already

                store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadWrite);
                store.Add(rootCert);
                store.Close();

                store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadWrite);
                store.Add(serviceCert);
                store.Close();

            }




            GuidAttribute g = (GuidAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), true)[0];

            byte[] sslHash = Enumerable.Range(0, serviceCert.Thumbprint.Length)
                    .Where(x => x % 2 == 0)
                    .Select(x => Convert.ToByte(serviceCert.Thumbprint.Substring(x, 2), 16))
                    .ToArray();

            //Bind SSL Cert To Port
            try
            {
                Microsoft.Web.Administration.NativeMethods.DeleteCertificateBinding(new IPEndPoint(IPAddress.Any, Properties.Settings.Default.serverPort ));
            }
            catch { }

            Microsoft.Web.Administration.NativeMethods.BindCertificate(new IPEndPoint(IPAddress.Any,Properties.Settings.Default.serverPort ), sslHash, "MY", new Guid(g.Value));


            //REsult 0 = Success, 183 = Already Exists


            return true;
        }




    }


}

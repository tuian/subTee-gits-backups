using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Security.Cryptography.X509Certificates;
 
using CERTENROLLLib;
 
public class Program
{
    public static void Main(string[] args)
    {
        IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, 8081);
        TcpListener listener = new TcpListener(endpoint);
        TcpClient client = new TcpClient();
 
        //Setup CA Certificate;
        X509Store CAstore = new X509Store(StoreName.My, StoreLocation.LocalMachine);
        CAstore.Open(OpenFlags.ReadOnly);
        X509Certificate2Collection certList = CAstore.Certificates.Find(X509FindType.FindBySubjectName, "__Interceptor_Trusted_Root" , false);
        if (certList.Count > 0)
        {
            Console.WriteLine(certList[0].Thumbprint);
        }
        else
        {
            Console.WriteLine("Installing Trusted Root");
            X509Certificate2 x509 = CreateCertificate("__Interceptor_Trusted_Root", true);
            CAstore.Close();
            Console.WriteLine("Ready");
        }
 
         
 
        listener.Start();
 
        while (true)
        {
 
            client = listener.AcceptTcpClient();
            if (client != null)
            {
 
                NetworkStream nwStream = client.GetStream();
                byte[] buffer = new byte[client.ReceiveBufferSize];
 
                int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);
 
                string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine("Received : \n" + dataReceived);
                string requestString = Encoding.UTF8.GetString(buffer);
                if (requestString.StartsWith("CONNECT"))
                {
                    //Client is requesting SSL, Promote the Stream;
                    // Get Domain Requested
                    string[] requestArray = requestString.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                    string[] DomainParse = requestArray[0].Split(new string[] { " ", ":" }, StringSplitOptions.None);
                    Console.WriteLine("*** SSL REQUEST TO {0} ***" , DomainParse[1]);
                    //Spoof Success Response
                    byte[] connectSpoof = Encoding.ASCII.GetBytes("HTTP/1.1 200 Connection Established\r\nTimeStamp: " + DateTime.Now.ToString() + "\r\n\r\n");
                    nwStream.Write(connectSpoof, 0, connectSpoof.Length);
                    nwStream.Flush();
 
                    SslStream sslStream = new SslStream(nwStream, false);
                    //Check if certificate already exists
                    CAstore.Open(OpenFlags.ReadOnly);
                    X509Certificate2Collection tempCertCheck = CAstore.Certificates.Find(X509FindType.FindBySubjectName, DomainParse[1], false);
                    X509Certificate2 tempCert;
                    if (tempCertCheck.Count > 0)
                    {
                        tempCert = tempCertCheck[0];
                    }
                    else
                    {
                        tempCert = CreateCertificate(DomainParse[1], false);
                    }
                    sslStream.AuthenticateAsServer(tempCert, false, System.Security.Authentication.SslProtocols.Tls12, false);
 
                    byte[] responseBytes = Encoding.UTF8.GetBytes("<html><H1>Yup!</H1></html>");
                    sslStream.Write(responseBytes, 0, responseBytes.Length);
 
                }
                else
                {
                    byte[] responseBytes = Encoding.UTF8.GetBytes("<html><H1>Yup!</H1></html>");
                    nwStream.Write(responseBytes, 0, responseBytes.Length);
                }
                 
                //client.Close();
                //listener.Stop();
                //Console.ReadLine();
            }
 
        }
 
 
    }
    public static X509Certificate2 CreateCertificate(string certSubject, bool isCA)
    {
        string CAsubject = certSubject;
        CX500DistinguishedName dn = new CX500DistinguishedName();
 
        dn.Encode("CN=" + CAsubject, X500NameFlags.XCN_CERT_NAME_STR_NONE);
 
        string strRfc822Name = certSubject;
 
        CAlternativeName objRfc822Name = new CAlternativeName();
        CAlternativeNames objAlternativeNames = new CAlternativeNames();
        CX509ExtensionAlternativeNames objExtensionAlternativeNames = new CX509ExtensionAlternativeNames(); 
         
         // Set Alternative RFC822 Name 
        objRfc822Name.InitializeFromString(AlternativeNameType.XCN_CERT_ALT_NAME_DNS_NAME, strRfc822Name);
 
        // Set Alternative Names 
        objAlternativeNames.Add(objRfc822Name);
        objExtensionAlternativeNames.InitializeEncode(objAlternativeNames);
        //objPkcs10.X509Extensions.Add((CX509Extension)objExtensionAlternativeNames);
 
 
 
 
 
 
 
        //Issuer Property for cleanup
        string issuer = "__Interceptor_Trusted_Root";
        CX500DistinguishedName issuerdn = new CX500DistinguishedName();
         
        issuerdn.Encode("CN=" + issuer, X500NameFlags.XCN_CERT_NAME_STR_NONE);
        // Create a new Private Key
 
        CX509PrivateKey key = new CX509PrivateKey();
        key.ProviderName = "Microsoft Enhanced RSA and AES Cryptographic Provider"; //"Microsoft Enhanced Cryptographic Provider v1.0"
                                                                                    // Set CAcert to 1 to be used for Signature
        if (isCA)
        {
            key.KeySpec = X509KeySpec.XCN_AT_SIGNATURE;
        }
        else
        {
            key.KeySpec = X509KeySpec.XCN_AT_KEYEXCHANGE;
        }
        key.Length = 2048;
        key.MachineContext = true;
        key.Create();
 
        // Create Attributes
        //var serverauthoid = new X509Enrollment.CObjectId();
        CObjectId serverauthoid = new CObjectId();
        serverauthoid.InitializeFromValue("1.3.6.1.5.5.7.3.1");
        CObjectIds ekuoids = new CObjectIds();
        ekuoids.Add(serverauthoid);
        CX509ExtensionEnhancedKeyUsage ekuext = new CX509ExtensionEnhancedKeyUsage();
        ekuext.InitializeEncode(ekuoids);
 
        CX509CertificateRequestCertificate cert = new CX509CertificateRequestCertificate();
        cert.InitializeFromPrivateKey(X509CertificateEnrollmentContext.ContextMachine, key, "");
        cert.Subject = dn;
        cert.Issuer = issuerdn;
        cert.NotBefore = (DateTime.Now).AddDays(-1);//Backup One day to Avoid Timing Issues
        cert.NotAfter = cert.NotBefore.AddDays(90); //Arbitrary... Change to persist longer...
                                                     //Use Sha256
        CObjectId hashAlgorithmObject = new CObjectId();
        hashAlgorithmObject.InitializeFromAlgorithmName(ObjectIdGroupId.XCN_CRYPT_HASH_ALG_OID_GROUP_ID, 0, 0, "SHA256");
        cert.HashAlgorithm = hashAlgorithmObject;
         
        cert.X509Extensions.Add((CX509Extension) ekuext);
        cert.X509Extensions.Add((CX509Extension)objExtensionAlternativeNames);
        //https://blogs.msdn.microsoft.com/alejacma/2011/11/07/how-to-add-subject-alternative-name-to-your-certificate-requests-c/
        if (isCA)
        {
            CX509ExtensionBasicConstraints basicConst = new CX509ExtensionBasicConstraints();
            basicConst.InitializeEncode(true, 1);
            cert.X509Extensions.Add((CX509Extension)basicConst);
        }
        else
        {
            var store = new X509Store(StoreName.My ,StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            X509Certificate2Collection signer = store.Certificates.Find(X509FindType.FindBySubjectName, "__Interceptor_Trusted_Root", false);
 
            CSignerCertificate signerCertificate = new CSignerCertificate();
            signerCertificate.Initialize(true, 0, EncodingType.XCN_CRYPT_STRING_HEX, signer[0].Thumbprint);
            cert.SignerCertificate = signerCertificate;
        }
        cert.Encode();
 
        CX509Enrollment enrollment = new CX509Enrollment();
        enrollment.InitializeFromRequest(cert);
        string certdata = enrollment.CreateRequest(0);
        enrollment.InstallResponse(InstallResponseRestrictionFlags.AllowUntrustedCertificate, certdata, 0, "");
         
        if (isCA)
        {
 
            //Install CA Root Certificate
            X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            X509Certificate2Collection certList = store.Certificates.Find(X509FindType.FindBySubjectName, "__Interceptor_Trusted_Root", false);
            store.Close();
 
            X509Store rootStore = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
            rootStore.Open(OpenFlags.ReadWrite);
            X509Certificate2Collection rootcertList = rootStore.Certificates.Find(X509FindType.FindBySubjectName, "__Interceptor_Trusted_Root", false);
            rootStore.Add(certList[0]);
            rootStore.Close();
            return certList[0];
        }
        else
        {
            //Return Per Domain Cert
            X509Store xstore = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            xstore.Open(OpenFlags.ReadOnly);
            X509Certificate2Collection certList = xstore.Certificates.Find(X509FindType.FindBySubjectName, certSubject, false);
            xstore.Close();
            return certList[0];
        }
 
    }
}
 
//Add InstallUtil Invocation Class
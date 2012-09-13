using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;

namespace OAuthShared
{  
    /// <summary>
    /// Configure the certs - done here as we run into "disposed" issues when stored in web context as the app is async.
    /// </summary>
    public class AuthenticationConfiguration : IDisposable
    {
        RSACryptoServiceProvider SigningKey { get; set; }
        RSACryptoServiceProvider EncryptionKey { get; set; }

        public RSACryptoServiceProvider CreateAuthorizationServerSigningServiceProvider()
        {
            var cert = new X509Certificate2(System.Configuration.ConfigurationManager.AppSettings["AbsolutePathToCertificate"]);
            SigningKey = (RSACryptoServiceProvider)cert.PublicKey.Key;
            return SigningKey;
        }

        public RSACryptoServiceProvider CreateResourceServerEncryptionServiceProvider()
        {
            var cert = new X509Certificate2(System.Configuration.ConfigurationManager.AppSettings["AbsolutePathToPfx"], System.Configuration.ConfigurationManager.AppSettings["CertificatePassword"]);
            EncryptionKey = (RSACryptoServiceProvider)cert.PrivateKey;
            return EncryptionKey;
        }

        public void Dispose()
        {
            if (SigningKey != null) SigningKey.Dispose();
            if (EncryptionKey != null) EncryptionKey.Dispose();
        }
    }
}

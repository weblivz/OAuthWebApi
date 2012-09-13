using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;

namespace OAuthShared
{
    public class AuthenticationConfig
    {        
        public static void ConfigureGlobal(HttpConfiguration globalConfig)
        {
            globalConfig.MessageHandlers.Add(new AuthenticationHandler(CreateConfiguration()));
            globalConfig.MessageHandlers.Add(new CorsHandler());
        }

        private static AuthenticationConfiguration CreateConfiguration()
        {
            var cfg = new AuthenticationConfiguration();

            return cfg;
        }
    }

    public class AuthenticationConfiguration
    {
        internal static readonly RSAParameters ResourceServerEncryptionPrivateKey;
        public static readonly RSAParameters AuthorizationServerSigningPublicKey;

        /// <summary>
        /// I copy the details from the real certificates and dispose of them quickly.
        /// Was an assumption that there were somehow being held onto and so affecting the response but don't believe this is the case.
        /// </summary>
        static AuthenticationConfiguration()
        {
            var cert = new X509Certificate2(System.Configuration.ConfigurationManager.AppSettings["AbsolutePathToCertificate"]);
            var pfx = new X509Certificate2(System.Configuration.ConfigurationManager.AppSettings["AbsolutePathToPfx"], System.Configuration.ConfigurationManager.AppSettings["CertificatePassword"], X509KeyStorageFlags.Exportable);

            using (var key = (RSACryptoServiceProvider)cert.PublicKey.Key)
            {
                AuthorizationServerSigningPublicKey = key.ExportParameters(false);
            }

            using (var key = (RSACryptoServiceProvider)pfx.PrivateKey)
            {
                ResourceServerEncryptionPrivateKey = key.ExportParameters(true);
            }
        }

        public RSACryptoServiceProvider CreateAuthorizationServerSigningServiceProvider()
        {
            var authorizationServerSigningServiceProvider = new RSACryptoServiceProvider();
            authorizationServerSigningServiceProvider.ImportParameters(AuthorizationServerSigningPublicKey);
            return authorizationServerSigningServiceProvider;
        }

        public RSACryptoServiceProvider CreateResourceServerEncryptionServiceProvider()
        {
            var resourceServerEncryptionServiceProvider = new RSACryptoServiceProvider();
            resourceServerEncryptionServiceProvider.ImportParameters(ResourceServerEncryptionPrivateKey);
            return resourceServerEncryptionServiceProvider;
        }
    }
}
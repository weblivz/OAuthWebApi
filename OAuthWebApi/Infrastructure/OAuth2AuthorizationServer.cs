using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using DotNetOpenAuth.Messaging.Bindings;
using DotNetOpenAuth.OAuth2;
using DotNetOpenAuth.OAuth2.ChannelElements;
using DotNetOpenAuth.OAuth2.Messages;
using System.Security.Cryptography.X509Certificates;

namespace OAuthWebApi
{
    internal class OAuth2AuthorizationServer : IAuthorizationServerHost, IDisposable {

        X509Certificate2 SigningCertificate;
        X509Certificate2 EncryptionCertificate;
        RSACryptoServiceProvider PublicEncryptionKey;
        RSACryptoServiceProvider PrivateSigningKey;

        internal OAuth2AuthorizationServer(X509Certificate2 SigningCertificate, X509Certificate2 EncryptionCertificate) 
        {
            this.SigningCertificate = SigningCertificate;
            this.EncryptionCertificate = EncryptionCertificate;
        }


        public void Dispose()
        {
            if (PublicEncryptionKey != null) PublicEncryptionKey.Dispose();
            if (PrivateSigningKey != null) PrivateSigningKey.Dispose();
        }

		#region Implementation of IAuthorizationServerHost

		public ICryptoKeyStore CryptoKeyStore {
			get { return MvcApplication.KeyNonceStore; }
		}

		public INonceStore NonceStore {
			get { return MvcApplication.KeyNonceStore; }
		}

		public AccessTokenResult CreateAccessToken(IAccessTokenRequest accessTokenRequestMessage) {
			var accessToken = new AuthorizationServerAccessToken();

			// Just for the sake of the sample, we use a short-lived token.  This can be useful to mitigate the security risks
			// of access tokens that are used over standard HTTP.
			// But this is just the lifetime of the access token.  The client can still renew it using their refresh token until
			// the authorization itself expires.
			accessToken.Lifetime = TimeSpan.FromMinutes(20);

			// Also take into account the remaining life of the authorization and artificially shorten the access token's lifetime
			// to account for that if necessary.
			//// TODO: code here

            PublicEncryptionKey = (RSACryptoServiceProvider)EncryptionCertificate.PublicKey.Key;
            accessToken.ResourceServerEncryptionKey = PublicEncryptionKey;
            PrivateSigningKey = (RSACryptoServiceProvider)SigningCertificate.PrivateKey;
            accessToken.AccessTokenSigningKey = PrivateSigningKey;

			return new AccessTokenResult(accessToken);
		}

		public IClientDescription GetClient(string clientIdentifier) {

            var consumerRow = MvcApplication.DataContext.Clients.SingleOrDefault(
                consumerCandidate => consumerCandidate.ClientIdentifier == clientIdentifier);
            if (consumerRow == null)
            {
                throw new ArgumentOutOfRangeException("clientIdentifier");
            }

            return consumerRow;
		}

		public bool IsAuthorizationValid(IAuthorizationDescription authorization) {
			return this.IsAuthorizationValid(authorization.Scope, authorization.ClientIdentifier, authorization.UtcIssued, authorization.User);
		}

		public bool TryAuthorizeResourceOwnerCredentialGrant(string userName, string password, IAccessTokenRequest accessRequest, out string canonicalUserName) {

            var user = MvcApplication.DataContext.Users.SingleOrDefault(
                u => u.Username == userName && u.Password == password);

            canonicalUserName = userName;

            if (user == null)
            {
                return false;
            }

            #region add an authorization for this client

            // The authorization we file in our database lasts until the user explicitly revokes it.
            // You can cause the authorization to expire by setting the ExpirationDateUTC
            // property in the below created ClientAuthorization.
            var client = MvcApplication.DataContext.Clients.First(c => c.ClientIdentifier == accessRequest.ClientIdentifier);
            client.ClientAuthorizations.Add(
                new ClientAuthorization
                {
                    Scope = OAuthUtilities.JoinScopes(accessRequest.Scope),
                    User = MvcApplication.DataContext.Users.FirstOrDefault(u => u.Username == userName),
                    CreatedOnUtc = DateTime.UtcNow,
                });
            MvcApplication.DataContext.SaveChanges(); // submit now so that this new row can be retrieved later in this same HTTP request
            
            #endregion

            return true;
		}

		public bool TryAuthorizeClientCredentialsGrant(IAccessTokenRequest accessRequest) {
			throw new NotImplementedException();
		}

		#endregion

		public bool CanBeAutoApproved(EndUserAuthorizationRequest authorizationRequest) {
			if (authorizationRequest == null) {
				throw new ArgumentNullException("authorizationRequest");
			}

			// NEVER issue an auto-approval to a client that would end up getting an access token immediately
			// (without a client secret), as that would allow arbitrary clients to masquarade as an approved client
			// and obtain unauthorized access to user data.
			if (authorizationRequest.ResponseType == EndUserAuthorizationResponseType.AuthorizationCode) {
				// Never issue auto-approval if the client secret is blank, since that too makes it easy to spoof
				// a client's identity and obtain unauthorized access.

                var requestingClient = MvcApplication.DataContext.Clients.First(c => c.ClientIdentifier == authorizationRequest.ClientIdentifier);
                    if (!string.IsNullOrEmpty(requestingClient.ClientSecret))
                    {
                        return this.IsAuthorizationValid(
                            authorizationRequest.Scope,
                            authorizationRequest.ClientIdentifier,
                            DateTime.UtcNow,
                            HttpContext.Current.User.Identity.Name);
                    }
			}

			// Default to not auto-approving.
			return false;
		}
		

		private bool IsAuthorizationValid(HashSet<string> requestedScopes, string clientIdentifier, DateTime issuedUtc, string username) {
			// If db precision exceeds token time precision (which is common), the following query would
			// often disregard a token that is minted immediately after the authorization record is stored in the db.
			// To compensate for this, we'll increase the timestamp on the token's issue date by 1 second.
			issuedUtc += TimeSpan.FromSeconds(1);

            var grantedScopeStrings = from auth in MvcApplication.DataContext.ClientAuthorizations
                                        where
                                            auth.Client.ClientIdentifier == clientIdentifier &&
                                            auth.CreatedOnUtc <= issuedUtc &&
                                            (!auth.ExpirationDateUtc.HasValue || auth.ExpirationDateUtc.Value >= DateTime.UtcNow) &&
                                            auth.User.Username == username
                                        select auth.Scope;

            if (!grantedScopeStrings.Any())
            {
                // No granted authorizations prior to the issuance of this token, so it must have been revoked.
                // Even if later authorizations restore this client's ability to call in, we can't allow
                // access tokens issued before the re-authorization because the revoked authorization should
                // effectively and permanently revoke all access and refresh tokens.
                return false;
            }

            var grantedScopes = new HashSet<string>(OAuthUtilities.ScopeStringComparer);
            foreach (string scope in grantedScopeStrings)
            {
                grantedScopes.UnionWith(OAuthUtilities.SplitScopes(scope));
            }

            return requestedScopes.IsSubsetOf(grantedScopes);
		}
    }
}
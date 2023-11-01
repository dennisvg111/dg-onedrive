using DG.Common.Http.Authorization;
using DG.OneDrive.Serialized;
using System;

namespace DG.OneDrive
{
    public class AccessTokenHeaderProvider : IExpiringAuthorizationProvider
    {
        private readonly Authentication _authentication;

        private string _accessToken;
        private AccessToken _deserializedToken;

        public AccessTokenHeaderProvider(Authentication authentication, string accessToken)
        {
            _authentication = authentication;
            _accessToken = accessToken;
            _deserializedToken = AccessToken.Decrypt(accessToken);
        }

        public bool TryRefreshAuthorization(out ExpiringAuthorization authorization)
        {
            DateTimeOffset expiration;
            if (!_deserializedToken.IsExpired)
            {
                expiration = new DateTimeOffset(_deserializedToken.ExpirationDate);
                authorization = ExpiringAuthorization.With(AuthorizationHeader.ForBearer(_deserializedToken.access_token), expiration);
                return true;
            }

            _accessToken = _authentication.Refresh(_accessToken).Result;
            _deserializedToken = AccessToken.Decrypt(_accessToken);

            expiration = new DateTimeOffset(_deserializedToken.ExpirationDate);
            authorization = ExpiringAuthorization.With(AuthorizationHeader.ForBearer(_deserializedToken.access_token), expiration);

            return true;
        }
    }
}

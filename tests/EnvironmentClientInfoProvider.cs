using System;

namespace DG.OneDrive.Tests
{
    public class EnvironmentClientInfoProvider : IClientInfoProvider
    {
        private static readonly EnvironmentClientInfoProvider _instance = new EnvironmentClientInfoProvider();
        public static EnvironmentClientInfoProvider Instance => _instance;

        private static readonly string _clientId = GetAnyEnvironmentVariable("ONEDRIVE_CLIENT_ID");
        private static readonly string _clientSecret = GetAnyEnvironmentVariable("ONEDRIVE_CLIENT_SECRET");

        public string ClientId => _clientId;
        public string ClientSecret => _clientSecret;
        public static string AccessToken => GetAnyEnvironmentVariable("ONEDRIVE_ACCESS_TOKEN");

        private static string GetAnyEnvironmentVariable(string key)
        {
            var variable = Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Process);
            if (variable != null)
            {
                return variable;
            }

            variable = Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.User);
            if (variable != null)
            {
                return variable;
            }

            return Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Machine);
        }
    }
}

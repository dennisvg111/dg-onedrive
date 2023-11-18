using System;

namespace DG.OneDrive.Tests
{
    public static class EnvironmentAccessTokenProvider
    {
        public const string AccessTokenVariableName = "ONEDRIVE_ACCESS_TOKEN";

        public static string AccessToken => GetAnyEnvironmentVariable(AccessTokenVariableName);

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

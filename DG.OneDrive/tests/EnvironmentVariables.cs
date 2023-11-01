using System;

namespace DG.OneDrive.Tests
{
    public static class EnvironmentVariables
    {
        public static string ClientId => Environment.GetEnvironmentVariable("ONEDRIVE_CLIENT_ID");
        public static string ClientSecret => Environment.GetEnvironmentVariable("ONEDRIVE_CLIENT_SECRET");
        public static string AccessToken => Environment.GetEnvironmentVariable("ONEDRIVE_ACCESS_TOKEN");
    }
}

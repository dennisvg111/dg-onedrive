using System;

namespace DG.OneDrive
{
    /// <summary>
    /// An implementation of <see cref="IClientInfoProvider"/> that retrieves <see cref="ClientId"/> and <see cref="ClientSecret"/> from environment variables.
    /// </summary>
    public class EnvironmentClientInfoProvider : IClientInfoProvider
    {
        /// <summary>
        /// ONEDRIVE_CLIENT_ID
        /// </summary>
        public const string DefaultClientIdVariableName = "ONEDRIVE_CLIENT_ID";

        /// <summary>
        /// ONEDRIVE_CLIENT_SECRET
        /// </summary>
        public const string DefaultClientSecretVariableName = "ONEDRIVE_CLIENT_SECRET";

        private static readonly EnvironmentClientInfoProvider _defaultInstance = new EnvironmentClientInfoProvider(DefaultClientIdVariableName, DefaultClientSecretVariableName);

        /// <summary>
        /// The default instance of <see cref="EnvironmentClientInfoProvider"/>, that uses <inheritdoc cref="DefaultClientIdVariableName"/> and <inheritdoc cref="DefaultClientSecretVariableName"/> as variable names.
        /// </summary>
        public static EnvironmentClientInfoProvider Default => _defaultInstance;

        private readonly string _clientId;
        private readonly string _clientSecret;

        /// <inheritdoc/>
        public string ClientId => _clientId;

        /// <inheritdoc/>
        public string ClientSecret => _clientSecret;

        /// <summary>
        /// Initializes a new instance of <see cref="EnvironmentClientInfoProvider"/> with the given variable names used to retrieve <see cref="ClientId"/> and <see cref="ClientSecret"/>.
        /// </summary>
        /// <param name="clientIdVariableName"></param>
        /// <param name="clientSecretVariableName"></param>
        public EnvironmentClientInfoProvider(string clientIdVariableName, string clientSecretVariableName)
        {
            _clientId = GetVariableFromAnyEnvironment(clientIdVariableName);
            _clientSecret = GetVariableFromAnyEnvironment(clientSecretVariableName);
        }

        private static string GetVariableFromAnyEnvironment(string key)
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

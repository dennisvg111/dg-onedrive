namespace DG.OneDrive
{
    /// <summary>
    /// Defines a way to provide a client id and a client secret to a <see cref="Client"/>.
    /// </summary>
    public interface IClientInfoProvider
    {
        /// <summary>
        /// The client id.
        /// </summary>
        string ClientId { get; }

        /// <summary>
        /// The client secret.
        /// </summary>
        string ClientSecret { get; }
    }
}

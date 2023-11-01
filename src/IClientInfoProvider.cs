namespace DG.OneDrive
{
    public interface IClientInfoProvider
    {
        string ClientId { get; }
        string ClientSecret { get; }
    }
}

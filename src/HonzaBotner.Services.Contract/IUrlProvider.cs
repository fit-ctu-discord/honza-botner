namespace HonzaBotner.Services.Contract
{
    public interface IUrlProvider
    {
        string GetAuthLink(ulong userId);
    }
}

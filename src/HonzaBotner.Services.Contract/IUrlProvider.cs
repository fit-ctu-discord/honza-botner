using HonzaBotner.Services.Contract.Dto;

namespace HonzaBotner.Services.Contract
{
    public interface IUrlProvider
    {
        string GetAuthLink(ulong userId, RolesPool pool);
    }
}

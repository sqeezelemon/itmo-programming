using Reports.App.Dto;

namespace Reports.App.Services;

public interface IAccountService
{
    Task<AccountDto> MakeAccount(string id, string serviceId, string ownerLogin);
    Task<IReadOnlyList<AccountDto>> FindByOwner(string ownerLogin);
    Task<IReadOnlyList<AccountDto>> FindByService(string serviceId);
    Task<AccountDto> FindAccount(string id, string serviceId);
}
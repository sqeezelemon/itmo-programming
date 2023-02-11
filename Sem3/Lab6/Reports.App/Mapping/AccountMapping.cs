using Reports.App.Dto;
using Reports.Models;

namespace Reports.App.Mapping;

public static class AccountMapping
{
    public static AccountDto AsDto(this Account account)
    {
        if (account.Owner is null)
            return new AccountDto(account.Id, account.Name, account.ServiceId, null);
        return new AccountDto(account.Id, account.Name, account.ServiceId, account.Owner.Login);
    }
}
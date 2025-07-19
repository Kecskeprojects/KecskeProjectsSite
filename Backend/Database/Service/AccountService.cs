using Backend.Communication.Incoming;
using Backend.Communication.Internal;
using Backend.Database.Model;
using Backend.Database.Repository;
using Backend.Enums;
using Backend.Tools;

namespace Backend.Database.Service;

public class AccountService(GenericRepository<Account> repository) : GenericService<Account>(repository)
{
    public async Task<DatabaseActionResult<int>> RegisterAsync(RegisterData form)
    {
        string username = form.UserName.Trim();
        bool usernameAlreadyRegistered = await repository.ExistsAsync(a => a.UserName.Contains(username));
        if (usernameAlreadyRegistered)
        {
            return CreateResult(DatabaseActionResultEnum.AlreadyExists, 0);
        }

        Account account = new()
        {
            UserName = form.UserName.Trim()
        };

        account.Password = HashTools.GetHashBytes(account, form.Password);

        int result = await repository.AddAsync(account);

        return CreateResult(DatabaseActionResultEnum.Success, result);
    }

    public async Task<DatabaseActionResult<int>> UpdateLastLoginAsync(int accountId)
    {
        Account? account = await repository.FindByIdAsync(accountId);
        if (account is null)
        {
            return CreateResult(DatabaseActionResultEnum.NotFound, 0);
        }
        account.LastLoginOnUtc = DateTime.UtcNow;
        int result = await repository.UpdateAsync(account);
        return CreateResult(DatabaseActionResultEnum.Success, result);
    }
}

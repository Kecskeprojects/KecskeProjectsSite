using Backend.Communication.Incoming;
using Backend.Communication.Internal;
using Backend.Database.Model;
using Backend.Database.Repository;
using Backend.Enum;
using Microsoft.AspNetCore.Identity;
using System.Text;

namespace Backend.Database.Service;

public class AccountService(GenericRepository<Account> repository) : GenericService<Account>(repository)
{
    public async Task<DatabaseActionResult<int>> RegisterAsync(RegisterData form)
    {
        if (string.IsNullOrEmpty(form.UserName) || string.IsNullOrEmpty(form.Password))
        {
            return CreateResult(DatabaseActionResultEnum.FailureWithSpecialMessage, 0, message: "Username or Password cannot be empty.");
        }

        Account account = new()
        {
            UserName = form.UserName.Trim()
        };

        PasswordHasher<Account> hasher = new();
        account.Password = Encoding.UTF8.GetBytes(hasher.HashPassword(account, form.Password));

        int result = await repository.AddAsync(account);

        return CreateResult(DatabaseActionResultEnum.Success, result);
    }

    public async Task<DatabaseActionResult<int>> UpdateLastLoginAsync(int? accountId)
    {
        if(accountId is null)
        {
            return CreateResult(DatabaseActionResultEnum.NotFound, 0);
        }

        Account? account = await repository.FindByIdAsync(accountId.Value);
        if (account is null)
        {
            return CreateResult(DatabaseActionResultEnum.NotFound, 0);
        }
        account.LastLoginOnUtc = DateTime.UtcNow;
        int result = await repository.UpdateAsync(account);
        return CreateResult(DatabaseActionResultEnum.Success, result);
    }
}

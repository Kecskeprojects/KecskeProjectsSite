using DatabaseORM.Model;
using DatabaseORM.Communication;
using DatabaseORM.Enums;
using DatabaseORM.Repository;
using DatabaseORM.Tools;

namespace DatabaseORM.Service;

public class AccountService(GenericRepository<Account> repository) : GenericService<Account>(repository)
{
    public async Task<DatabaseActionResult<string?>> RegisterAsync(string username, string password)
    {
        username = username.Trim();
        bool usernameAlreadyRegistered = await repository.ExistsAsync(a => a.UserName.Contains(username));
        if (usernameAlreadyRegistered)
        {
            return CreateResult(DatabaseActionResultEnum.AlreadyExists, "Username is taken.");
        }

        Account account = new()
        {
            UserName = username
        };

        account.Password = PasswordHasherTools.GetHashBytes(account, password);

        //Secret key used to reset password
        string uuid = Guid.NewGuid().ToString();
        account.SecretKey = PasswordHasherTools.GetHashBytes(account, uuid);
        account.Password = PasswordHasherTools.GetHashBytes(account, password);

        int result = await repository.AddAsync(account);

        return CreateResult(
            result > 0
                ? DatabaseActionResultEnum.Success
                : DatabaseActionResultEnum.Failure,
            uuid);
    }

    public async Task<DatabaseActionResult<int>> UpdateLastLoginAsync(int accountId)
    {
        Account? account = await repository.FindByIdAsync(accountId);
        if (account is null)
        {
            return CreateResult(DatabaseActionResultEnum.NotFound, 0);
        }
        account.LastLoginOnUtc = DateTime.UtcNow;

        return await SaveChangesAsync();
    }

    public async Task<DatabaseActionResult<string?>> GenerateNewSecretKeyAsync(int accountId)
    {
        Account? account = await repository.FindByIdAsync(accountId);
        if (account is null)
        {
            return CreateResult<string>(DatabaseActionResultEnum.NotFound, null);
        }

        string uuid = Guid.NewGuid().ToString();
        account.SecretKey = PasswordHasherTools.GetHashBytes(account, uuid);
        DatabaseActionResult<int> updateResult = await UpdateAsync(account);

        return updateResult.Status != DatabaseActionResultEnum.Success
            ? CreateResult<string>(DatabaseActionResultEnum.Failure, null)
            : CreateResult(DatabaseActionResultEnum.Success, uuid);
    }

    public async Task<DatabaseActionResult<int>> ResetPasswordAsync(string username, string secretKey, string newPassword)
    {
        username = username.Trim();
        Account? account = await repository.FirstOrDefaultAsync(a => a.UserName.Equals(username));
        if (account is null)
        {
            return CreateResult(DatabaseActionResultEnum.NotFound, 0);
        }

        if (!PasswordHasherTools.VerifyPassword(account, secretKey, account.SecretKey))
        {
            return CreateResult(DatabaseActionResultEnum.DifferingHash, 0);
        }

        account.Password = PasswordHasherTools.GetHashBytes(account, newPassword);
        return await SaveChangesAsync();
    }

    public async Task<DatabaseActionResult<List<AccountResource>?>> GetUsersAsync()
    {
        List<Account> accounts = await repository.GetListAsync();
        return CreateMappedResult<Account, AccountResource>(DatabaseActionResultEnum.Success, accounts);
    }
}

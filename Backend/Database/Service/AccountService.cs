using Backend.Communication.Incoming;
using Backend.Communication.Internal;
using Backend.Communication.Outgoing;
using Backend.Database.Model;
using Backend.Database.Repository;
using Backend.Enums;
using Backend.Tools;

namespace Backend.Database.Service;

public class AccountService(GenericRepository<Account> repository) : GenericService<Account>(repository)
{
    public async Task<DatabaseActionResult<string?>> RegisterAsync(RegisterData form)
    {
        string username = form.UserName.Trim();
        bool usernameAlreadyRegistered = await repository.ExistsAsync(a => a.UserName.Contains(username));
        if (usernameAlreadyRegistered)
        {
            return CreateResult(DatabaseActionResultEnum.AlreadyExists, "Username is taken.");
        }

        Account account = new()
        {
            UserName = form.UserName.Trim()
        };

        account.Password = EncryptionTools.GetHashBytes(account, form.Password);

        //Secret key used to reset password
        string uuid = Guid.NewGuid().ToString();
        account.SecretKey = EncryptionTools.GetHashBytes(account, uuid);
        account.Password = EncryptionTools.GetHashBytes(account, form.Password);

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
        account.SecretKey = EncryptionTools.GetHashBytes(account, uuid);
        DatabaseActionResult<int> updateResult = await UpdateAsync(account);

        return updateResult.Status != DatabaseActionResultEnum.Success
            ? CreateResult<string>(DatabaseActionResultEnum.Failure, null)
            : CreateResult(DatabaseActionResultEnum.Success, uuid);
    }

    public async Task<DatabaseActionResult<int>> ResetPasswordAsync(ResetPasswordData form)
    {
        string username = form.UserName.Trim();
        Account? account = await repository.FirstOrDefaultAsync(a => a.UserName.Equals(username));
        if (account is null)
        {
            return CreateResult(DatabaseActionResultEnum.NotFound, 0);
        }

        if (!EncryptionTools.VerifyPassword(account, form.SecretKey, account.SecretKey))
        {
            return CreateResult(DatabaseActionResultEnum.DifferingHash, 0);
        }

        account.Password = EncryptionTools.GetHashBytes(account, form.Password);
        return await SaveChangesAsync();
    }

    internal async Task<DatabaseActionResult<List<AccountResource>?>> GetUsersAsync()
    {
        List<Account> accounts = await repository.GetListAsync();
        return CreateMappedResult<Account, AccountResource>(DatabaseActionResultEnum.Success, accounts);        
    }
}

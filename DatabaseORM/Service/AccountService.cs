using DatabaseORM.Communication.Exceptions;
using DatabaseORM.Communication.Resource;
using DatabaseORM.Model;
using DatabaseORM.Repository;
using DatabaseORM.Tools;

namespace DatabaseORM.Service;

public class AccountService(GenericRepository<Account> repository) : GenericService<Account>(repository)
{
    /// <summary>
    /// Returns the account with its roles, or null if no account has that name.
    /// </summary>
    public Task<Account?> FindByUserNameAsync(string username)
    {
        username = username.Trim();
        return repository.FirstOrDefaultAsync(a => a.UserName.Equals(username), a => a.Roles);
    }

    /// <summary>
    /// Creates the account and returns its secret key, the only way to reset the password.
    /// </summary>
    /// <exception cref="ConflictException">The username is taken.</exception>
    public async Task<string> RegisterAsync(string username, string password)
    {
        username = username.Trim();
        // The column's collation is case-insensitive, so this also catches names differing only in case
        bool usernameAlreadyRegistered = await repository.ExistsAsync(a => a.UserName == username);
        if (usernameAlreadyRegistered)
        {
            throw new ConflictException("Username already exists.");
        }

        Account account = new()
        {
            UserName = username
        };

        //Secret key used to reset password
        string uuid = Guid.NewGuid().ToString();
        account.SecretKey = PasswordHasherTools.GetHashBytes(account, uuid);
        account.Password = PasswordHasherTools.GetHashBytes(account, password);

        _ = await repository.AddAsync(account);

        return uuid;
    }

    /// <exception cref="NotFoundException">No account has that ID.</exception>
    public async Task UpdateLastLoginAsync(int accountId)
    {
        Account account = await GetByIdAsync(accountId);
        account.LastLoginOnUtc = DateTime.UtcNow;

        _ = await repository.SaveChangesAsync();
    }

    /// <summary>
    /// Replaces the account's secret key and returns the new one.
    /// </summary>
    /// <exception cref="NotFoundException">No account has that ID.</exception>
    public async Task<string> GenerateNewSecretKeyAsync(int accountId)
    {
        Account account = await GetByIdAsync(accountId);

        string uuid = Guid.NewGuid().ToString();
        account.SecretKey = PasswordHasherTools.GetHashBytes(account, uuid);
        _ = await repository.SaveChangesAsync();

        return uuid;
    }

    /// <exception cref="NotFoundException">No account has that name.</exception>
    /// <exception cref="InvalidSecretException">The secret key doesn't match.</exception>
    public async Task ResetPasswordAsync(string username, string secretKey, string newPassword)
    {
        username = username.Trim();
        Account account = await repository.FirstOrDefaultAsync(a => a.UserName.Equals(username))
            ?? throw new NotFoundException("Account with that name doesn't exist.");

        if (!PasswordHasherTools.VerifyPassword(account, secretKey, account.SecretKey))
        {
            throw new InvalidSecretException("Key is incorrect.");
        }

        account.Password = PasswordHasherTools.GetHashBytes(account, newPassword);
        _ = await repository.SaveChangesAsync();
    }

    public async Task<List<AccountResource>> GetUsersAsync()
    {
        List<Account> accounts = await repository.GetListAsync(null, a => a.UserName, true, a => a.Roles);
        return MapList<Account, AccountResource>(accounts);
    }

    private async Task<Account> GetByIdAsync(int accountId)
    {
        return await repository.FindByIdAsync(accountId)
            ?? throw new NotFoundException("Account with that ID doesn't exist.");
    }
}

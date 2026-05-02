using DatabaseORM.Model;
using DatabaseORM.Communication;
using DatabaseORM.Enums;
using DatabaseORM.Repository;

namespace DatabaseORM.Service;

public class PermittedIpAddressService(GenericRepository<PermittedIpAddress> repository) : GenericService<PermittedIpAddress>(repository)
{
    public async Task<DatabaseActionResult<int>> AddAsync(int expirationMinutes, int accountId, string ipAddress)
    {
        PermittedIpAddress? entry = await repository.FirstOrDefaultAsync(entry => entry.IpAddress == ipAddress);

        entry ??= new()
        {
            IpAddress = ipAddress,
            AccountId = accountId,
        };

        entry.ExpiresOnUtc = DateTime.UtcNow.AddMinutes(expirationMinutes);

        return entry.PermittedIpAddressId > 0 ? await UpdateAsync(entry) : await AddAsync(entry);
    }

    public async Task<DatabaseActionResult<List<string>?>> GetExpiredIPAddressesAsync()
    {
        List<PermittedIpAddress> expiredEntries = await repository.GetListAsync(entry => entry.ExpiresOnUtc <= DateTime.UtcNow);
        List<string> expiredIPs = [.. expiredEntries.Select(entry => entry.IpAddress)];

        return CreateResult(DatabaseActionResultEnum.Success, expiredIPs);
    }

    public async Task<DatabaseActionResult<int>> RemoveIPAddressesAsync(List<string> expiredIPs)
    {
        List<PermittedIpAddress> entriesToRemove = await repository.GetListAsync(entry => expiredIPs.Contains(entry.IpAddress));
        return await RemoveAsync(entriesToRemove);
    }
}

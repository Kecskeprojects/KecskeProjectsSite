using DatabaseORM.Model;
using DatabaseORM.Repository;

namespace DatabaseORM.Service;

public class PermittedIpAddressService(GenericRepository<PermittedIpAddress> repository) : GenericService<PermittedIpAddress>(repository)
{
    /// <summary>
    /// Permits the IP address, or extends its expiry if it is already permitted.
    /// </summary>
    public async Task AddAsync(int expirationMinutes, int accountId, string ipAddress)
    {
        PermittedIpAddress? entry = await repository.FirstOrDefaultAsync(entry => entry.IpAddress == ipAddress);

        entry ??= new()
        {
            IpAddress = ipAddress,
            AccountId = accountId,
        };

        entry.ExpiresOnUtc = DateTime.UtcNow.AddMinutes(expirationMinutes);

        _ = entry.PermittedIpAddressId > 0
            ? await repository.UpdateAsync(entry)
            : await repository.AddAsync(entry);
    }

    public async Task<List<string>> GetExpiredIPAddressesAsync()
    {
        List<PermittedIpAddress> expiredEntries = await repository.GetListAsync(entry => entry.ExpiresOnUtc <= DateTime.UtcNow);
        return [.. expiredEntries.Select(entry => entry.IpAddress)];
    }

    public async Task RemoveIPAddressesAsync(List<string> expiredIPs)
    {
        List<PermittedIpAddress> entriesToRemove = await repository.GetListAsync(entry => expiredIPs.Contains(entry.IpAddress));
        _ = await repository.RemoveAsync(entriesToRemove);
    }
}

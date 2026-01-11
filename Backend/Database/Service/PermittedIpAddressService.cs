using Backend.Communication.Internal;
using Backend.Database.Model;
using Backend.Database.Repository;
using Backend.Enums;

namespace Backend.Database.Service;

public class PermittedIpAddressService(GenericRepository<PermittedIpAddress> repository) : GenericService<PermittedIpAddress>(repository)
{
    public Task<DatabaseActionResult<int>> AddAsync(int accountId, string ipAddress)
    {
        PermittedIpAddress newEntry = new()
        {
            IpAddress = ipAddress,
            AccountId = accountId,
            ExpiresOnUtc = DateTime.UtcNow.AddHours(6),
        };

        return AddAsync(newEntry);
    }

    public async Task<DatabaseActionResult<List<string>?>> GetExpiredIPAddressesAsync()
    {
        List<PermittedIpAddress> expiredEntries = await repository.GetListAsync(entry => entry.ExpiresOnUtc <= DateTime.UtcNow);
        List<string> expiredIPs = expiredEntries.Select(entry => entry.IpAddress).ToList();

        return CreateResult(DatabaseActionResultEnum.Success, expiredIPs);
    }

    public async Task<DatabaseActionResult<int>> RemoveIPAddressesAsync(List<string> expiredIPs)
    {
        List<PermittedIpAddress> entriesToRemove = await repository.GetListAsync(entry => expiredIPs.Contains(entry.IpAddress));
        return await RemoveAsync(entriesToRemove);
    }
}

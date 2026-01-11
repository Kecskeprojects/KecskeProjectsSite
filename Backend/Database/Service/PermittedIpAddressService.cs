using Backend.Communication.Internal;
using Backend.Database.Model;
using Backend.Database.Repository;
using Backend.Enums;

namespace Backend.Database.Service;

public class PermittedIpAddressService(IConfiguration configuration, GenericRepository<PermittedIpAddress> repository) : GenericService<PermittedIpAddress>(repository)
{
    public async Task<DatabaseActionResult<int>> AddAsync(int accountId, string ipAddress)
    {
        int expirationMinutes = configuration.GetValue<int>("RDPAccessExpirationMinutes");

        PermittedIpAddress? entry = await repository.FirstOrDefaultAsync(entry => entry.IpAddress == ipAddress);

        entry ??= new()
        {
            IpAddress = ipAddress,
            AccountId = accountId,
        };

        entry.ExpiresOnUtc = DateTime.UtcNow.AddMinutes(expirationMinutes);

        if(entry.PermittedIpAddressId > 0)
        {
            return await UpdateAsync(entry);
        }
        return await AddAsync(entry);
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

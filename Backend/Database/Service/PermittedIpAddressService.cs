using Backend.Communication.Internal;
using Backend.Constants;
using Backend.Database.Model;
using Backend.Database.Repository;
using Backend.Enums;

namespace Backend.Database.Service;

public class PermittedIpAddressService(
    GenericRepository<PermittedIpAddress> repository,
    IConfiguration configuration) : GenericService<PermittedIpAddress>(repository)
{
    public async Task<DatabaseActionResult<int>> AddAsync(int accountId, string ipAddress)
    {
        int expirationMinutes = configuration.GetValue<int>(ConfigurationConstants.RDPAccessExpirationMinutesKey);

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
        List<string> expiredIPs = expiredEntries.Select(entry => entry.IpAddress).ToList();

        return CreateResult(DatabaseActionResultEnum.Success, expiredIPs);
    }

    public async Task<DatabaseActionResult<int>> RemoveIPAddressesAsync(List<string> expiredIPs)
    {
        List<PermittedIpAddress> entriesToRemove = await repository.GetListAsync(entry => expiredIPs.Contains(entry.IpAddress));
        return await RemoveAsync(entriesToRemove);
    }
}

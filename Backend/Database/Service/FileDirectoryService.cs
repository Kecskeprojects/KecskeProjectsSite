using AutoMapper;
using Backend.Communication.Internal;
using Backend.Communication.Outgoing;
using Backend.Database.Model;
using Backend.Database.Repository;

namespace Backend.Database.Service;

public class FileDirectoryService(GenericRepository<FileDirectory> repository, IMapper mapper) : GenericService<FileDirectory>(repository, mapper)
{
    public Task<DatabaseActionResult<bool>> AccountHasAccessToDirectoryAsync(LoggedInAccount loggedInAccount, string categoryDirectory)
    {
        return ExistsAsync(ff => ff.RelativePath == categoryDirectory
                                && (ff.Roles.Any(x => loggedInAccount.Roles.Any(y => y == x.Name))
                                    || ff.Roles.Count == 0));
    }

    public async Task<DatabaseActionResult<List<FileDirectoryResource>?>> GetAccountAccessibleDirectoriesAsync(LoggedInAccount loggedInAccount)
    {
        List<FileDirectory> directories = await repository.GetListAsync(ff => ff.Roles.Any(x => loggedInAccount.Roles.Any(y => y == x.Name))
                                                                    || ff.Roles.Count == 0);

        return CreateMappedResult<List<FileDirectory>, List<FileDirectoryResource>>(Enums.DatabaseActionResultEnum.Success, directories);
    }
}

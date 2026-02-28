using Backend.Communication.Internal;
using Backend.Communication.Outgoing;
using Backend.Database.Model;
using Backend.Database.Repository;
using Backend.Mapping.MappingProfiles;

namespace Backend.Database.Service;

public class FileDirectoryService(GenericRepository<FileDirectory> repository) : GenericService<FileDirectory>(repository)
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

        return CreateMappedResult<SimpleMapper, FileDirectory, FileDirectoryResource>(Enums.DatabaseActionResultEnum.Success, directories);
    }
}

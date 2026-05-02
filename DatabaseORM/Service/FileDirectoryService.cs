using DatabaseORM.Model;
using DatabaseORM.Communication;
using DatabaseORM.Enums;
using DatabaseORM.Repository;

namespace DatabaseORM.Service;

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

        return CreateMappedResult<FileDirectory, FileDirectoryResource>(DatabaseActionResultEnum.Success, directories);
    }
}

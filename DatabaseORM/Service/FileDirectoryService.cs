using DatabaseORM.Communication.Resource;
using DatabaseORM.Model;
using DatabaseORM.Repository;

namespace DatabaseORM.Service;

public class FileDirectoryService(GenericRepository<FileDirectory> repository) : GenericService<FileDirectory>(repository)
{
    public Task<bool> AccountHasAccessToDirectoryAsync(LoggedInAccount loggedInAccount, string categoryDirectory)
    {
        return repository.ExistsAsync(ff => ff.RelativePath == categoryDirectory
                                            && (ff.Roles.Any(x => loggedInAccount.Roles.Contains(x.Name))
                                                || ff.Roles.Count == 0));
    }

    public async Task<List<FileDirectoryResource>> GetAccountAccessibleDirectoriesAsync(LoggedInAccount loggedInAccount)
    {
        List<FileDirectory> directories = await repository.GetListAsync(ff => ff.Roles.Any(x => loggedInAccount.Roles.Contains(x.Name))
                                                                            || ff.Roles.Count == 0);

        return MapList<FileDirectory, FileDirectoryResource>(directories);
    }
}

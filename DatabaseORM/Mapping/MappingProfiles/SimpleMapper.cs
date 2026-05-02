using DatabaseORM.Communication.Resource;
using DatabaseORM.Model;
using Riok.Mapperly.Abstractions;

namespace DatabaseORM.Mapping.MappingProfiles;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class SimpleMapper : MapperUtilities
{
    public partial FileDirectoryResource FileDirectoryToFileDirectoryResource(FileDirectory fileDirectory);
    public partial AccountResource AccountToAccountResource(Account account);
}

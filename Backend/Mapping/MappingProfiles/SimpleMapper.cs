using Backend.Communication.Outgoing;
using Backend.Database.Model;
using Riok.Mapperly.Abstractions;

namespace Backend.Mapping.MappingProfiles;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class SimpleMapper : MapperUtilities
{
    public partial FileDirectoryResource FileDirectoryToFileDirectoryResource(FileDirectory fileDirectory);
    public partial AccountResource AccountToAccountResource(Account account);
}

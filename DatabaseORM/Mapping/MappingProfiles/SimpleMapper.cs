using DatabaseORM.Communication.Resource;
using DatabaseORM.Model;
using Mapster;

namespace DatabaseORM.Mapping.MappingProfiles;

public static class MappingConfiguration
{
    public static void RegisterMappings()
    {
        TypeAdapterConfig<FileDirectory, FileDirectoryResource>.NewConfig().Compile();
        TypeAdapterConfig<Account, AccountResource>.NewConfig().Compile();
    }
}

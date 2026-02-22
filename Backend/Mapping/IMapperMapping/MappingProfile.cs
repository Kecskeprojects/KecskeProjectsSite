using AutoMapper;
using Backend.Communication.Outgoing;
using Backend.Database.Model;

namespace Backend.Mapping.IMapperMapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<FileDirectory, FileDirectoryResource>();
    }
}

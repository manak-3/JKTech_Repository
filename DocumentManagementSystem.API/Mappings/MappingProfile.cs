using AutoMapper;
using DocumentManagementSystem.Core.Dtos.DocumentDtos;
using DocumentManagementSystem.Core.Entities;

namespace DocumentManagementSystem.API.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Document mappings
        CreateMap<Document, DocumentDto>();
        CreateMap<DocumentDto, Document>();
        CreateMap<UploadDocumentDto, Document>();
        CreateMap<UpdateDocumentDto, Document>();
        
        // Metadata mappings
        CreateMap<DocumentMetadata, DocumentMetadataDto>();
        CreateMap<DocumentMetadataDto, DocumentMetadata>();
    }
}
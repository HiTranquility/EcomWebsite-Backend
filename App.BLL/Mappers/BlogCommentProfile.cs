using App.BLL.Dtos.BlogDto.Requests;
using App.BLL.Dtos.BlogDto.Results;
using App.DAL.BlogModels;
using AutoMapper;

namespace App.BLL.Mappers;

public class BlogCommentProfile : Profile
{
    public BlogCommentProfile()
    {
        // INPUT mapping: CreateBlogCommentReq → BlogComment
        CreateMap<CreateBlogCommentReq, BlogComment>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.BlogId, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content.Trim()))
            .ForMember(dest => dest.Parent, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Blog, opt => opt.Ignore())
            .ForMember(dest => dest.InverseParentNavigation, opt => opt.Ignore())
            .ForMember(dest => dest.ParentNavigation, opt => opt.Ignore());

        // INPUT mapping: ReplyBlogCommentReq → BlogComment
        CreateMap<ReplyBlogCommentReq, BlogComment>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.BlogId, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content.Trim()))
            .ForMember(dest => dest.Parent, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Blog, opt => opt.Ignore())
            .ForMember(dest => dest.InverseParentNavigation, opt => opt.Ignore())
            .ForMember(dest => dest.ParentNavigation, opt => opt.Ignore());

        // OUTPUT mapping: BlogComment → BlogCommentRes
        CreateMap<BlogComment, BlogCommentRes>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.FullName, opt => opt.Ignore())
            .ForMember(dest => dest.Email, opt => opt.Ignore())
            .ForMember(dest => dest.AvatarUrl, opt => opt.Ignore())
            .ForMember(dest => dest.Children, opt => opt.MapFrom(src => 
                src.InverseParentNavigation != null 
                    ? src.InverseParentNavigation.Where(c => c.DeletedAt == null).ToList() 
                    : new List<BlogComment>()));
    }
}
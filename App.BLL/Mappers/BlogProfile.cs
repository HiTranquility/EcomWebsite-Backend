using App.BLL.Dtos.BlogDto.Results;
using App.DAL.BlogModels;
using AutoMapper;
namespace App.BLL.Mappers;

public class BlogProfile : Profile
{
    public BlogProfile()
    {
        CreateMap<BlogCategory, BlogListRes.BlogCategoryItemRes>();
        CreateMap<BlogTag, BlogListRes.BlogTagItemRes>();

        CreateMap<Blog, BlogListRes>()
            .ForMember(d => d.AllCategories, o => o.MapFrom(s =>
                s.BlogCategoryJoins.Select(j => j.BlogCategory)))
            .ForMember(d => d.AllTags, o => o.MapFrom(s =>
                s.BlogTagJoins.Select(j => j.BlogTag)))
            .ForMember(d => d.BlogCommentCount, o => o.MapFrom(s => s.CommentCount));
    }
}
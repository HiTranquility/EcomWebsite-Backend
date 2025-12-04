using App.BLL.Dtos.BlogDto.Results;
using App.BLL.Dtos.BlogDto.Shares;
using App.BLL.Dtos.BlogDto.Requests;
using App.DAL.BlogModels;
using AutoMapper;
using System.Security.Cryptography;
using System.Text;
namespace App.BLL.Mappers;

public class BlogProfile : Profile
{
    public BlogProfile()
    {
        //Items
        CreateMap<BlogCategory, CategoryItem>();
        CreateMap<BlogTag, TagItem>();
        CreateMap<BlogVariant, VariantItem>();
        CreateMap<Quote, QuoteItem>();

        //List
        CreateMap<Blog, BlogListRes>()
            .ForMember(d => d.AllCategories, o => o.MapFrom(s =>
                s.BlogCategoryJoins.Select(j => j.BlogCategory)))
            .ForMember(d => d.AllTags, o => o.MapFrom(s =>
                s.BlogTagJoins.Select(j => j.BlogTag)))
            .ForMember(d => d.AllVariants, o => o.MapFrom(s => s.BlogVariants))
            .ForMember(d => d.BlogCommentCount, o => o.MapFrom(s =>
                s.CommentCount ?? s.BlogComments.Count(c => c.DeletedAt == null)));

        //Detail
        CreateMap<Blog, BlogDetailRes>()
            .ForMember(d => d.ImageUrl, o => o.MapFrom(s => s.BlogVariants
                .Where(v => v.Type == "image").Select(v => v.Url).FirstOrDefault()))
            .ForMember(d => d.AllCategories, o => o.MapFrom(s =>
                s.BlogCategoryJoins.Select(j => j.BlogCategory)))
            .ForMember(d => d.AllTags, o => o.MapFrom(s =>
                s.BlogTagJoins.Select(j => j.BlogTag)))
            .ForMember(d => d.AllVariants, o => o.MapFrom(s => s.BlogVariants))
            .ForMember(d => d.Quote, o => o.MapFrom(s => s.Quote));

        //Comments
        CreateMap<BlogComment, BlogCommentRes>()
            .ForMember(d => d.Children, o => o.MapFrom(s => s.InverseParentNavigation
                .Where(r => r.DeletedAt == null)
                .OrderBy(r => r.CreatedAt)));

        //Requests
        CreateMap<CreateBlogCommentReq, BlogComment>()
            .ForMember(d => d.Content, o => o.MapFrom(s => (s.Content ?? string.Empty).Trim()))
            .ForMember(d => d.UserId, o => o.MapFrom((src, _, __, ctx) => ctx.Items.ContainsKey("UserId") ? (int?)ctx.Items["UserId"] : null))
            .ForMember(d => d.BlogId, o => o.MapFrom((src, _, __, ctx) => (int)ctx.Items["BlogId"]))
            .ForMember(d => d.CreatedAt, o => o.MapFrom(_ => DateTime.UtcNow))
            .ForMember(d => d.UpdatedAt, o => o.MapFrom(_ => DateTime.UtcNow));

    
        CreateMap<ReplyBlogCommentReq, BlogComment>()
            .ForMember(d => d.Content, o => o.MapFrom(s => (s.Content ?? string.Empty).Trim()))
            .ForMember(d => d.UserId, o => o.MapFrom((src, _, __, ctx) => ctx.Items.ContainsKey("UserId") ? (int?)ctx.Items["UserId"] : null))
            .ForMember(d => d.BlogId, o => o.MapFrom((src, _, __, ctx) => (int)ctx.Items["BlogId"]))
            .ForMember(d => d.Parent, o => o.MapFrom((src, _, __, ctx) => (int)ctx.Items["ParentId"]))
            .ForMember(d => d.CreatedAt, o => o.MapFrom(_ => DateTime.UtcNow))
            .ForMember(d => d.UpdatedAt, o => o.MapFrom(_ => DateTime.UtcNow));
    }
}
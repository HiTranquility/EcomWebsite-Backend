using System;
using System.Collections.Generic;
using System.Linq;
using App.DAL.BlogModels;
using App.UTIL.Abstractions.DTO.Result;

namespace App.BLL.Dtos.BlogDto.Results;

public class BlogListRes : BaseResult
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? ImageUrl { get; set; }
    public string? Author { get; set; }
    //Categories and Tags Phân tích lại xem nên import từ file khác hay là tạo mới tại đây
    public List<BlogCategoryItemRes>? AllCategories { get; set; }
    public List<BlogTagItemRes>? AllTags { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int? BlogCommentCount { get; set; }
    public sealed class BlogCategoryItemRes
    {
        public int Id { get; init; }
        public string Title { get; init; } = string.Empty;
    }

    public sealed class BlogTagItemRes
    {
        public int Id { get; init; }
        public string Title { get; init; } = string.Empty;
    }
}
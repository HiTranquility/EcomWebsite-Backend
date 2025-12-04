using App.BLL.Dtos.BlogDto.Requests;
using App.BLL.Dtos.BlogDto.Results;
using App.DAL.BlogModels;
using App.DAL.Repositories;
using App.DAL.UserModels;
using App.UTIL.Abstractions.BLL;
using App.UTIL.Abstractions.DTO.Response;
using App.UTIL.Helpers.Cache;
using App.UTIL.Helpers.Cache.Schemas;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace App.BLL.Services;

public class BlogCommentSvc : GenericSvc<BlogCommentRepo, BlogComment>
{
    private readonly BlogRepo _blogRepo;
    private readonly UserRepo _userRepo;
    private readonly ICacheService _cacheService;

    public BlogCommentSvc(
        BlogCommentRepo repo, 
        BlogRepo blogRepo,
        UserRepo userRepo,
        ICacheService cacheService,
        IMapper mapper) : base(repo, mapper)
    {
        _blogRepo = blogRepo;
        _userRepo = userRepo;
        _cacheService = cacheService;
    }

    // ✅ Nhận userId trực tiếp từ controller (đã lấy từ ClaimsPrincipal)
    public async Task<BaseResponse> CreateBlogCommentAsync(
        int blogId,
        int userId,
        CreateBlogCommentReq request,
        CancellationToken ct = default)
    {
        BaseResponse rsp = new BaseResponse();

        if (string.IsNullOrWhiteSpace(request.Content))
        {
            rsp.SetError("INVALID_CONTENT", "Content is required", "Comment content cannot be empty", 400);
            return rsp;
        }

        Blog? blog = await _blogRepo.ReadAsync(blogId, ct);
        if (blog == null || blog.DeletedAt != null)
        {
            rsp.SetError("BLOG_NOT_FOUND", "Blog not found", "The blog you are commenting on does not exist", 404);
            return rsp;
        }

        BlogComment comment = _mapper.Map<BlogComment>(request);
        comment.BlogId = blogId;
        comment.UserId = userId;
        comment.Parent = null;
        comment.CreatedAt = DateTime.UtcNow;
        comment.UpdatedAt = DateTime.UtcNow;
        comment.DeletedAt = null;

            await _repo.CreateAsync(comment, ct);
            
        BlogComment? createdComment = await _repo.All
                .AsNoTracking()
                .Where(c => c.Id == comment.Id)
                .FirstOrDefaultAsync(ct);

        BlogCommentRes commentRes = _mapper.Map<BlogCommentRes>(createdComment);
        if (createdComment?.UserId.HasValue == true)
        {
            User? user = await _userRepo.ReadAsync(createdComment.UserId.Value, ct);
            if (user != null)
            {
                commentRes.FullName = $"{user.FirstName} {user.LastName}".Trim();
                commentRes.Email = user.Email;
                commentRes.AvatarUrl = user.ImageUrl;
            }
        }
            
            rsp.SetData(commentRes, "Comment created successfully", 201);
            return rsp;
        }

    public async Task<BaseResponse> ReplyBlogCommentAsync(
        int blogId,
        int parentCommentId,
        int userId,
        ReplyBlogCommentReq request,
        CancellationToken ct = default)
    {
        BaseResponse rsp = new BaseResponse();

        if (string.IsNullOrWhiteSpace(request.Content))
        {
            rsp.SetError("INVALID_CONTENT", "Content is required", "Reply content cannot be empty", 400);
            return rsp;
        }

        BlogComment reply = _mapper.Map<BlogComment>(request);
        reply.BlogId = blogId;
        reply.UserId = userId;
        reply.Parent = parentCommentId;
        reply.CreatedAt = DateTime.UtcNow;
        reply.UpdatedAt = DateTime.UtcNow;
        reply.DeletedAt = null;

        await _repo.CreateAsync(reply, ct);
        
        BlogComment? createdReply = await _repo.All
            .AsNoTracking()
            .Where(c => c.Id == reply.Id)
            .FirstOrDefaultAsync(ct);

        BlogCommentRes replyRes = _mapper.Map<BlogCommentRes>(createdReply);
        if (createdReply?.UserId.HasValue == true)
        {
            User? user = await _userRepo.ReadAsync(createdReply.UserId.Value, ct);
            if (user != null)
            {
                replyRes.FullName = $"{user.FirstName} {user.LastName}".Trim();
                replyRes.Email = user.Email;
                replyRes.AvatarUrl = user.ImageUrl;
            }
        }

        rsp.SetData(replyRes, "Reply created successfully", 201);
        return rsp;
    }

    public async Task<BaseResponse> GetBlogCommentsAsync(int blogId, CancellationToken ct = default)
        {
        BaseResponse rsp = new BaseResponse();

        List<BlogComment> allComments = await _repo.All
            .AsNoTracking()
            .Where(c => c.BlogId == blogId && c.DeletedAt == null)
            .ToListAsync(ct);

        HashSet<int> userIds = allComments
            .Where(c => c.UserId.HasValue)
            .Select(c => c.UserId!.Value)
            .ToHashSet();

        Dictionary<int, User> userDict = await LoadUsersWithCacheAsync(userIds, ct);

        Dictionary<int, BlogCommentRes> commentResDict = new Dictionary<int, BlogCommentRes>();
        List<BlogCommentRes> parentComments = new List<BlogCommentRes>();

        foreach (BlogComment comment in allComments)
        {
            BlogCommentRes commentRes = _mapper.Map<BlogCommentRes>(comment);
            
            if (comment.UserId.HasValue && userDict.TryGetValue(comment.UserId.Value, out User? user))
            {
                commentRes.FullName = $"{user.FirstName} {user.LastName}".Trim();
                commentRes.Email = user.Email;
                commentRes.AvatarUrl = user.ImageUrl;
            }

            commentResDict[comment.Id] = commentRes;

            if (comment.Parent == null)
            {
                parentComments.Add(commentRes);
            }
        }

        foreach (BlogComment comment in allComments)
        {
            if (comment.Parent.HasValue && commentResDict.TryGetValue(comment.Parent.Value, out BlogCommentRes? parentRes))
            {
                if (parentRes.Children == null)
                {
                    parentRes.Children = new List<BlogCommentRes>();
                }
                parentRes.Children.Add(commentResDict[comment.Id]);
            }
        }

        List<BlogCommentRes> sortedParents = parentComments
            .OrderByDescending(c => c.CreatedAt)
            .ToList();

        rsp.SetData(sortedParents, "Get comments successfully", 200);
            return rsp;
        }

    private async Task<Dictionary<int, User>> LoadUsersWithCacheAsync(HashSet<int> userIds, CancellationToken ct)
    {
        if (userIds.Count == 0)
        {
            return new Dictionary<int, User>();
        }

        Dictionary<int, User> userDict = new Dictionary<int, User>();
        List<int> uncachedUserIds = new List<int>();

        foreach (int userId in userIds)
        {
            string cacheKey = UserCacheConfig.BuildCommentKey(userId);
            User? cachedUser = await _cacheService.GetOrSetAsync<User?>(
                cacheKey,
                async token => await _userRepo.ReadAsync(userId, token),
                UserCacheConfig.CommentTtl,
                UserCacheConfig.CommentPrefix,
                ct);

            if (cachedUser != null)
            {
                userDict[userId] = cachedUser;
            }
            else
            {
                uncachedUserIds.Add(userId);
            }
        }

        if (uncachedUserIds.Count > 0)
        {
            Dictionary<int, User> batchUsers = await _userRepo.GetUsersByIdsAsync(uncachedUserIds, ct);
            
            foreach (KeyValuePair<int, User> kvp in batchUsers)
            {
                userDict[kvp.Key] = kvp.Value;
                
                string cacheKey = UserCacheConfig.BuildCommentKey(kvp.Key);
                await _cacheService.GetOrSetAsync<User>(
                    cacheKey,
                    token => Task.FromResult(kvp.Value),
                    UserCacheConfig.CommentTtl,
                    UserCacheConfig.CommentPrefix,
                    ct);
            }
        }

        return userDict;
    }
}
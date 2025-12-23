using App.DAL.UserModels;
using App.UTIL.Abstractions.DAL;

namespace App.DAL.Interfaces;

public interface IUserTagRepo : IGenericRepo<UserTag>
{
    Task<List<UserTag>> GetProductTagsAsync(int productId, CancellationToken ct = default);
    Task<UserTag?> GetUserProductTagAsync(int userId, int productId, string tagTitle, CancellationToken ct = default);
}


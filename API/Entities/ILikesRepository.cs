using System;
using API.DTOs;
using API.Helpers;

namespace API.Entities;

public interface ILikesRepository
{
    Task<UserLike> GetUserLike(int sourceUserId, int targetUserId);
    Task<PagedList<MemberDto>>GetUserLikes(LikesParams likesParams);
    Task<IEnumerable<int>>GetCurrentUserLikeIds(int currentUserId);
    void DeleteLike(UserLike like);
    void AddLike(UserLike like);
    Task<bool> SaveChange();
    
}

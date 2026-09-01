using System.Collections.Generic;
using System.Threading.Tasks;
using GMK360.Core.Entities;

namespace GMK360.Core.Interfaces
{
    public interface IArticleService
    {
        Task<IEnumerable<Article>> GetAllArticlesAsync(bool includeUnpublished = false);
        Task<Article?> GetArticleByIdAsync(int id);
        Task<Article?> GetArticleBySlugAsync(string slug);
        Task<Article> CreateArticleAsync(Article article);
        Task UpdateArticleAsync(Article article);
        Task DeleteArticleAsync(int id);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GMK360.Core.Entities;
using GMK360.Core.Interfaces;

namespace GMK360.Core.Services
{
    public class ArticleService : IArticleService
    {
        private readonly IRepository<Article> _articleRepository;

        public ArticleService(IRepository<Article> articleRepository)
        {
            _articleRepository = articleRepository;
        }

        public async Task<IEnumerable<Article>> GetAllArticlesAsync(bool includeUnpublished = false)
        {
            var articles = await _articleRepository.GetAllAsync();
            if (!includeUnpublished)
            {
                articles = articles.Where(a => a.IsPublished).ToList();
            }
            return articles.OrderByDescending(a => a.PublishedAt ?? a.CreatedAt);
        }

        public async Task<Article?> GetArticleByIdAsync(int id)
        {
            return await _articleRepository.GetByIdAsync(id);
        }

        public async Task<Article?> GetArticleBySlugAsync(string slug)
        {
            var articles = await _articleRepository.GetAllAsync();
            return articles.FirstOrDefault(a => a.Slug == slug && a.IsPublished);
        }

        public async Task<Article> CreateArticleAsync(Article article)
        {
            if (article.IsPublished && article.PublishedAt == null)
            {
                article.PublishedAt = DateTime.UtcNow;
            }
            
            await _articleRepository.AddAsync(article);
            return article;
        }

        public async Task UpdateArticleAsync(Article article)
        {
            var existing = await _articleRepository.GetByIdAsync(article.Id);
            if (existing != null)
            {
                existing.Title = article.Title;
                existing.Slug = article.Slug;
                existing.HtmlContent = article.HtmlContent;
                existing.CoverImageUrl = article.CoverImageUrl;
                existing.SeoTags = article.SeoTags;
                
                // Yayınlanma durumu değiştiyse tarihi güncelle
                if (article.IsPublished && !existing.IsPublished)
                {
                    existing.PublishedAt = DateTime.UtcNow;
                }
                
                existing.IsPublished = article.IsPublished;
                existing.UpdatedAt = DateTime.UtcNow;

                await _articleRepository.UpdateAsync(existing);
            }
        }

        public async Task DeleteArticleAsync(int id)
        {
            var existing = await _articleRepository.GetByIdAsync(id);
            if (existing != null)
            {
                await _articleRepository.DeleteAsync(existing);
            }
        }
    }
}

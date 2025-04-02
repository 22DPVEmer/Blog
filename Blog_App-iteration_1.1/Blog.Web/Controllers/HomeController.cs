using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Blog.Web.Models;
using Blog.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Linq;
using Blog.Infrastructure.Entities;
using System.Collections.Generic;

namespace Blog.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IArticleService _articleService;

    public class ArticleViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Intro { get; set; }
        public string FeaturedImage { get; set; }
        public string Author { get; set; }
        public int CommentCount { get; set; }
    }

    public HomeController(
        ILogger<HomeController> logger,
        IArticleService articleService)
    {
        _logger = logger;
        _articleService = articleService;
    }

    public async Task<IActionResult> Index()
    {
        var articles = await _articleService.GetLatestArticlesAsync(3);
        
        // Create simplified article view models to avoid circular references
        var articleViewModels = articles.Select(a => new ArticleViewModel
        {
            Id = a.Id,
            Title = a.Title,
            Intro = a.Intro,
            FeaturedImage = a.FeaturedImage,
            Author = a.User?.UserName,
            CommentCount = a.Comments?.Count ?? 0
        }).ToList();
        
        ViewBag.LatestArticles = articleViewModels;
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

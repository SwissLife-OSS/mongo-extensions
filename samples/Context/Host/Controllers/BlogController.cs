using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Models;
using SimpleBlog.Domain;

namespace SimpleBlog.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BlogController : ControllerBase
    {
        private readonly IBlogService _blogService;

        public BlogController(IBlogService blogService)
        {
            _blogService = blogService;
        }

        [HttpPost]
        public async Task PostBlog(BlogPost blogPost, CancellationToken cancellationToken)
        {
            await _blogService.PostBlogAsync(blogPost, cancellationToken);
        }
    }
}

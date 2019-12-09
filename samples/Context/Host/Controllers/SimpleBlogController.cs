using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Models;
using SimpleBlog.Domain;

namespace SimpleBlog.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SimpleBlogController : ControllerBase
    {
        private readonly IBlogService _blogService;
        private readonly IUserService _userService;

        public SimpleBlogController(
            IBlogService blogService, IUserService userService)
        {
            _blogService = blogService;
            _userService = userService;
        }

        [HttpPost]
        [Route("blogs")]
        public async Task PostBlog(
            BlogPost blogPost, CancellationToken cancellationToken)
        {
            await _userService.EnsureUserAsync(blogPost.UserId, cancellationToken);
            await _blogService.PostBlogAsync(blogPost, cancellationToken);
        }

        [HttpGet]
        [Route("blogs")]
        public async Task<ActionResult<IEnumerable<Blog>>> GetBlogs(
            CancellationToken cancellationToken)
        {
            IEnumerable<Blog> result = await _blogService.GetAllBlogsAsync(cancellationToken);

            return Ok(result);
        }

        [HttpGet]
        [Route("tags")]
        public async Task<ActionResult<IEnumerable<Tag>>> GetTags(
            CancellationToken cancellationToken)
        {
            IEnumerable<Tag> result = await _blogService.GetAllTagsAsync(cancellationToken);

            return Ok(result);
        }
    }
}

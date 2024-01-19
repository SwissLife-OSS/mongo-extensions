using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Models;
using SimpleBlog.DataAccess;

namespace SimpleBlog.Domain
{
    public class BlogService : IBlogService
    {
        private readonly IBlogRepository _blogRepository;
        private readonly IUserRepository _userRepository;
        private readonly ITagRepository _tagRepository;

        public BlogService(
            IBlogRepository blogRepository,
            IUserRepository userRepository,
            ITagRepository tagRepository)
        {
            _blogRepository = blogRepository;
            _userRepository = userRepository;
            _tagRepository = tagRepository;
        }

        public async Task PostBlogAsync(BlogPost blogPost, CancellationToken cancellationToken)
        {
            var blog = new Blog()
            {
                Id = Guid.NewGuid(),
                TimeStamp = DateTime.UtcNow,
                UserId = blogPost.UserId,
                Titel = blogPost.Titel,
                Text = blogPost.Text,
                Tags = blogPost.Tags
            };

            await _blogRepository.AddBlogAsync(blog, cancellationToken);
            await _userRepository.AttachBlogToUserAsync(blog.UserId, blog.Id, cancellationToken);
            await _tagRepository.TryAddTagsAsync(blogPost.Tags, cancellationToken);
        }

        public async Task<IEnumerable<Tag>> GetAllTagsAsync(CancellationToken cancellationToken)
        {
            return await _tagRepository.GetTagsAsync(cancellationToken);
        }

        public async Task<IEnumerable<Blog>> GetAllBlogsAsync(CancellationToken cancellationToken)
        {
            return await _blogRepository.GetBlogsAsync(cancellationToken);
        }
    }
}

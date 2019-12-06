using System;
using System.Threading;
using System.Threading.Tasks;
using Models;
using SimpleBlog.DataAccess;

namespace SimpleBlog.Domain
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task RegisterUserAsync(
            User newUser, CancellationToken cancellationToken = default)
        {
            await _userRepository.AddUserAsync(newUser, cancellationToken);
        }
    }
}

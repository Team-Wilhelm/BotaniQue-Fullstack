using Infrastructure.Repositories;
using Shared.Dtos.FromClient;
using Shared.Models.Identity;

namespace Core.Services;

public class UserService(UserRepository userRepository, JwtService jwtService)
{
    public async Task<User> CreateUser(RegisterUserDto registerUserDto)
    {
        return await userRepository.CreateUser(registerUserDto);
    }

    public async Task<string?> Login(LoginDto loginDto)
    {
        var user = await userRepository.Login(loginDto);
        return user == null ? null : jwtService.IssueJwt(user);
    }
}
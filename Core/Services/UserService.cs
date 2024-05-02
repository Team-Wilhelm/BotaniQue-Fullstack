using Infrastructure.Repositories;
using Shared.Dtos.FromClient;
using Shared.Dtos.FromClient.Identity;
using Shared.Exceptions;
using Shared.Models.Identity;

namespace Core.Services;

public class UserService(UserRepository userRepository, JwtService jwtService)
{
    public async Task<User> CreateUser(RegisterUserDto registerUserDto)
    {
        var user = await userRepository.GetUserByEmail(registerUserDto.Email);
        if (user != null) throw new UserAlreadyExistsException();
       
        return await userRepository.CreateUser(registerUserDto);
    }

    public async Task<string?> Login(LoginDto loginDto)
    {
        var user = await userRepository.Login(loginDto);
        return user == null ? null : jwtService.IssueJwt(user);
    }

    public async Task<User?> GetUserByEmail(string email)
    {
        return await userRepository.GetUserByEmail(email);
    }
}
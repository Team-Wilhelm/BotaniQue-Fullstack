using Core.Services.External.BlobStorage;
using Infrastructure.Repositories;
using Shared.Dtos;
using Shared.Dtos.FromClient;
using Shared.Dtos.FromClient.Identity;
using Shared.Exceptions;
using Shared.Models.Identity;

namespace Core.Services;

public class UserService(UserRepository userRepository, JwtService jwtService, IBlobStorageService blobStorageService)
{
    public async Task CreateUser(RegisterUserDto registerUserDto)
    {
        var user = await userRepository.GetUserByEmail(registerUserDto.Email);
        if (user != null) throw new UserAlreadyExistsException();
        
        if (registerUserDto.Base64Image != null)
        {
            registerUserDto.BlobUrl = await blobStorageService.SaveImageToBlobStorage(registerUserDto.Base64Image, registerUserDto.Email);
        }
       
        await userRepository.CreateUser(registerUserDto);
    }

    public async Task<string?> Login(LoginDto loginDto)
    {
        var user = await userRepository.Login(loginDto);
        return user == null ? null : jwtService.IssueJwt(user);
    }

    public async Task<User> GetUserByEmail(string email)
    {
        var user = await userRepository.GetUserByEmail(email);
        if (user == null) throw new NotFoundException();
        return user;
    }

    public async Task<GetUserDto?> UpdateUser(UserDto userDto, string email)
    {
        var userToUpdate = await userRepository.GetUserByEmail(email);
        if (userToUpdate == null) throw new NotFoundException("User not found");
        
        if (userDto.Username != null && !userDto.Username.Equals(string.Empty))
        {
            userToUpdate.UserName = userDto.Username;
        }
        
        var currentBlobUrl = userToUpdate.BlobUrl;
        if (userDto.Base64Image != null)
        {
            userDto.BlobUrl = await blobStorageService.SaveImageToBlobStorage(userDto.Base64Image, email, currentBlobUrl);
        }
        
        return await userRepository.UpdateUser(userToUpdate, userDto.Password);
    }
}
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
            registerUserDto.BlobUrl = await blobStorageService.SaveImageToBlobStorage(registerUserDto.Base64Image, registerUserDto.Email, null);
        }
       
        await userRepository.CreateUser(registerUserDto);
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

    public async Task<GetUserDto?> UpdateUser(UpdateUserDto updateUserDto)
    {
        if (updateUserDto.Base64Image != null)
        {
            updateUserDto.BlobUrl = await blobStorageService.SaveImageToBlobStorage(updateUserDto.Base64Image, updateUserDto.UserEmail, null);
        }
        return await userRepository.UpdateUser(updateUserDto);
    }
}
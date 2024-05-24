using api.Core.Services.External.BlobStorage;
using Infrastructure.Repositories;
using Shared.Dtos.FromClient.Identity;
using Shared.Exceptions;
using Shared.Models.Identity;

namespace api.Core.Services;

public class UserService(UserRepository userRepository, JwtService jwtService, IBlobStorageService blobStorageService)
{
    public async Task CreateUser(RegisterUserDto registerUserDto)
    {
        var user = await userRepository.GetUserByEmail(registerUserDto.Email);
        if (user != null) throw new UserAlreadyExistsException();
        
        if (registerUserDto.Base64Image != null)
        {
            registerUserDto.BlobUrl = await blobStorageService.SaveImageToBlobStorage(registerUserDto.Base64Image, registerUserDto.Email, false);
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
    public async Task DeleteProfileImage(string email)
    {
        var user = await ValidateAndGetUser(email);
        
        if (user.BlobUrl == null) return;
        
        var blobToDelete = blobStorageService.GetBlobUrlFromSasUri(user.BlobUrl);
        await blobStorageService.DeleteImageFromBlobStorage(blobToDelete, false);
        user.BlobUrl = null;
        await userRepository.UpdateUserProfile(user);
    }

    public async Task<string> UpdateUserProfileImage(string email, string base64Image)
    {
        var userToUpdate = await ValidateAndGetUser(email);
        
        var currentBlobUrl = userToUpdate.BlobUrl;
        userToUpdate.BlobUrl = await blobStorageService.SaveImageToBlobStorage(base64Image, email, false, string.IsNullOrEmpty(currentBlobUrl) ? null : currentBlobUrl);
        await userRepository.UpdateUserProfile(userToUpdate);
        var blobUrl = blobStorageService.GenerateSasUri(userToUpdate.BlobUrl, false);
        return blobUrl;
    }
    
    public async Task<string> UpdateUsername(string email, string username)
    {
        var userToUpdate = await ValidateAndGetUser(email);
        userToUpdate.UserName = username;
        await userRepository.UpdateUserProfile(userToUpdate);
        return userToUpdate.UserName;
    }
    
    public async Task UpdatePassword(string email, string password)
    {
        var userToUpdate = await ValidateAndGetUser(email);
        await userRepository.UpdatePassword(userToUpdate, password);
    }
    
    public async Task<string?> GetEmailFromDeviceId(string deviceId)
    {
        var userEmail = await userRepository.GetUserByDeviceId(deviceId);
        if (userEmail == null) throw new NotFoundException("User not found for this device");
        return userEmail;
    }
    
    private async Task<User> ValidateAndGetUser(string email)
    {
        var user = await userRepository.GetUserByEmail(email);
        if (user == null) throw new NotFoundException("User not found");
        return user;
    }
}
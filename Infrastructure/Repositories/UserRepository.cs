using Microsoft.EntityFrameworkCore;
using Shared.Dtos;
using Shared.Dtos.FromClient;
using Shared.Dtos.FromClient.Identity;
using Shared.Models.Identity;

namespace Infrastructure.Repositories;

public class UserRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory)
{
    public async Task<User?> GetUserByEmail(string email)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.Users.FirstOrDefaultAsync(u => u.UserEmail == email);
    }

    public async Task CreateUser(RegisterUserDto registerUserDto)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        // Hash password
        var passwordHashAndSalt = PasswordHasher.HashPassword(registerUserDto.Password);

        var user = new User
        {
            UserEmail = registerUserDto.Email,
            UserName = registerUserDto.Username,
            PasswordHash = passwordHashAndSalt[1],
            PasswordSalt = passwordHashAndSalt[0]
        };
        
        if(registerUserDto.BlobUrl != null)
        {
            user.BlobUrl = registerUserDto.BlobUrl;
        }

        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
    }

    public async Task<User?> Login(LoginDto loginDto)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        var user = await context.Users.FirstOrDefaultAsync(u => u.UserEmail == loginDto.Email);

        if (user == null) return null;
        if (!user.PasswordHash.SequenceEqual(PasswordHasher.HashPassword(loginDto.Password, user.PasswordSalt)))
            return null;

        return user;
    }

    public async Task<GetUserDto?> UpdateUser(User userToUpdate, string? password)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        
        if (password != null && !password.Equals(string.Empty))
        {
            var passwordHashAndSalt = PasswordHasher.HashPassword(password);
            userToUpdate.PasswordHash = passwordHashAndSalt[1];
            userToUpdate.PasswordSalt = passwordHashAndSalt[0];
        }
        
        var getUserDto = new GetUserDto
        {
            UserEmail = userToUpdate.UserEmail,
            Username = userToUpdate.UserName,
            BlobUrl = userToUpdate.BlobUrl
        };
        
        context.Users.Update(userToUpdate);
        await context.SaveChangesAsync();

        return getUserDto;
    }
}
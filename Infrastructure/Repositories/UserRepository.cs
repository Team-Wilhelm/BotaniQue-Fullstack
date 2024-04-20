using Microsoft.EntityFrameworkCore;
using Shared.Dtos.FromClient;
using Shared.Models.Identity;

namespace Infrastructure.Repositories;

public class UserRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory)
{
    public async Task<User?> GetUserByEmail(string email)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.Users.FirstOrDefaultAsync(u => u.UserEmail == email);
    }

    public async Task<User> CreateUser(RegisterUserDto registerUserDto)
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

        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
        return user;
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
}
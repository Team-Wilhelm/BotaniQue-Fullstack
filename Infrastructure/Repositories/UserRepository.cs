using Microsoft.EntityFrameworkCore;
using Shared.Dtos;
using Shared.Models.Identity;

namespace Infrastructure.Repositories;

public class UserRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory)
{
    public async Task<User> GetUserByEmail(string email)
    {
        throw new NotImplementedException();
    }
    
    public async Task<User> CreateUser(RegisterUserDto registerUserDto)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        Console.WriteLine("Creating user");
        
        // Hash password
        var passwordHashAndSalt = PasswordHasher.HashPassword(registerUserDto.Password);
        
        var user = new User
        {
            UserEmail = registerUserDto.Email,
            UserName = registerUserDto.Username,
            PasswordHash = passwordHashAndSalt[0],
            PasswordSalt = passwordHashAndSalt[1],
        };
        
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
        return user;
    }
    
    public async Task<bool> Login(LoginDto loginDto)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        var user = await context.Users.FirstOrDefaultAsync(u => u.UserEmail == loginDto.Email);
        if (user is null)
        {
            return false;
        }

        return user.PasswordHash == PasswordHasher.HashPassword(loginDto.Password, user.PasswordSalt);
    }
}
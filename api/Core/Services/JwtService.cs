using System.Security.Authentication;
using api.Core.Options;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;
using Shared.Models.Identity;

namespace api.Core.Services;

public class JwtService(IOptions<JwtOptions> jwtOptions)
{
    public string IssueJwt(User user)
    {
        try
        {
            IJwtAlgorithm algorithm = new HMACSHA512Algorithm();
            IJsonSerializer serializer = new JsonNetSerializer();
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);
            
            var userClaims = new Dictionary<string, object>
            {
                { "email", user.UserEmail },
                { "name", user.UserName }
            };
            return encoder.Encode(userClaims, jwtOptions.Value.Key);
        }
        catch (Exception e)
        {
            Log.Error(e, "IssueJWT");
            throw new InvalidOperationException("User authentication succeeded, but could not create token");
        }
    }

    private Dictionary<string, string> GetClaimsFromJwt(string jwt)
    {
        try
        {
            IJsonSerializer serializer = new JsonNetSerializer();
            var provider = new UtcDateTimeProvider();
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtValidator validator = new JwtValidator(serializer, provider);
            IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder, new HMACSHA512Algorithm());
            var json = decoder.Decode(jwt, jwtOptions.Value.Key);
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(json)!;
        }
        catch (Exception e)
        {
            Log.Error(e, "ValidateJwtAndReturnClaims");
            throw new AuthenticationException("Authentication failed.");
        }
    }
    
    public string GetEmailFromJwt(string jwt)
    {
        var claims = GetClaimsFromJwt(jwt);
        return claims["email"];
    }
    
    public bool IsJwtTokenValid(string jwt)
    {
        try
        {
            IJsonSerializer serializer = new JsonNetSerializer();
            var provider = new UtcDateTimeProvider();
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtValidator validator = new JwtValidator(serializer, provider);
            IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder, new HMACSHA512Algorithm());
            decoder.Decode(jwt, jwtOptions.Value.Key);
            return true;
        }
        catch (Exception e)
        {
            Log.Error(e, "IsJwtTokenValid");
            return false;
        }
    }
}
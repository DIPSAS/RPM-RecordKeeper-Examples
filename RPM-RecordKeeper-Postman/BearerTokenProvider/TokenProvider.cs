using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.IdentityModel.Tokens;
using static IdentityModel.OidcConstants;

namespace BearerTokenProvider;

public class TokenProvider
{
    /// <param name="stsUrl"></param>
    /// <param name="clientId"></param>
    /// <param name="scopes"></param>
    /// <param name="jwkPrivateKey"></param>
    /// <returns>Bearer token</returns>
    public static async Task<string> GetBearerToken(string stsUrl, string clientId, string scopes, string jwkPrivateKey)
    {
        var c = new HttpClient();
        var disco = await c.GetDiscoveryDocumentAsync(stsUrl);
        if (disco.IsError)
        {
            throw new Exception(disco.Error);
        }

        var response = await c.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = disco.TokenEndpoint,
            ClientId = clientId,
            GrantType = GrantTypes.ClientCredentials,
            Scope = scopes,
            ClientAssertion = new ClientAssertion
            {
                Type = ClientAssertionTypes.JwtBearer,
                Value = BuildClientAssertion(disco, clientId, jwkPrivateKey)
            },
            ClientCredentialStyle = ClientCredentialStyle.PostBody
        });

        if (response.IsError)
        {
            throw new Exception(response.Error);
        }
        return response.AccessToken;
    }

    private static string BuildClientAssertion(DiscoveryDocumentResponse disco, string clientId, string jwkPrivateKey)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtClaimTypes.Subject, clientId),
            new Claim(JwtClaimTypes.IssuedAt, DateTimeOffset.Now.ToUnixTimeSeconds().ToString()),
            new Claim(JwtClaimTypes.JwtId, Guid.NewGuid().ToString("N")),
        };
        var credentials = new JwtSecurityToken(clientId, disco.TokenEndpoint, claims, DateTime.UtcNow, DateTime.UtcNow.AddSeconds(60), GetClientAssertionSigningCredentials(jwkPrivateKey));

        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(credentials);
    }
    private static SigningCredentials GetClientAssertionSigningCredentials(string jwkPrivateKey)
    {
        var securityKey = new JsonWebKey(jwkPrivateKey);
        return new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256);
    }
}
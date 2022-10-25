using IdentityModel;
using IdentityModel.Client;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using static IdentityModel.OidcConstants;

namespace HelseId.Demo.ClientCredentials.Jwk
{
    class Program
    {
        /*
         * Insert 
         * _jwkPrivateKey and _clientId from https://selvbetjening.test.helseid.no
         * _openDipsSubscriptionKey from https://open.dips.no
         */
        private const string _jwkPrivateKey =
            @"Add your JWK private key here";
        private const string _clientId = "Add your client_id here";
        private const string _openDipsSubscriptionKey = "Add your OpenDips subscription key here";

        private const string _scopes = "dips-dho:record-keeper/demo";
        private const string _apiUrl = "https://api.dips.no/rpm-recordkeeper/api/v1/utils/AuthCheck";

        /// <summary>
        /// Simple sample demonstrating client credentials
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static async Task Main(string[] args)
        {
            const string stsUrl = "https://helseid-sts.test.nhn.no";

            try
            {
                var c = new HttpClient();
                var disco = await c.GetDiscoveryDocumentAsync(stsUrl);
                if (disco.IsError)
                {
                    throw new Exception(disco.Error);
                }

                var response = await c.RequestClientCredentialsTokenAsync(new IdentityModel.Client.ClientCredentialsTokenRequest
                {
                    Address = disco.TokenEndpoint,
                    ClientId = _clientId,
                    GrantType = GrantTypes.ClientCredentials,
                    Scope = _scopes,
                    ClientAssertion = new ClientAssertion
                    {
                        Type = ClientAssertionTypes.JwtBearer,
                        Value = BuildClientAssertion(disco, _clientId)
                    },
                    ClientCredentialStyle = ClientCredentialStyle.PostBody
                });

                if (response.IsError)
                {
                    throw new Exception(response.Error);
                }

                await CallApi(response.AccessToken);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Error:");
                Console.Error.WriteLine(e.ToString());
            }
        }

        private static async Task CallApi(string accessToken)
        {
            var c = new HttpClient(new LoggingHandler(new HttpClientHandler()));
            c.SetBearerToken(accessToken);
            c.DefaultRequestHeaders.Add("Dips-Subscription-Key", _openDipsSubscriptionKey);
            await c.GetAsync(_apiUrl);
        }

        private static string BuildClientAssertion(DiscoveryDocumentResponse disco, string clientId)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.Subject, clientId),
                new Claim(JwtClaimTypes.IssuedAt, DateTimeOffset.Now.ToUnixTimeSeconds().ToString()),
                new Claim(JwtClaimTypes.JwtId, Guid.NewGuid().ToString("N")),
            };

            var credentials = new JwtSecurityToken(clientId, disco.TokenEndpoint, claims, DateTime.UtcNow, DateTime.UtcNow.AddSeconds(60), GetClientAssertionSigningCredentials());

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(credentials);
        }
        private static SigningCredentials GetClientAssertionSigningCredentials()
        {
            var securityKey = new JsonWebKey(_jwkPrivateKey);
            return new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256);
        }

        private class LoggingHandler : DelegatingHandler
        {
            public LoggingHandler(HttpMessageHandler innerHandler)
                : base(innerHandler)
            {
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                Console.WriteLine("Request:");
                Console.WriteLine(request.ToString());
                if (request.Content != null)
                {
                    Console.WriteLine();
                    Console.WriteLine("Request content:");
                    Console.WriteLine(await request.Content.ReadAsStringAsync());
                }

                Console.WriteLine();

                var response = await base.SendAsync(request, cancellationToken);

                Console.WriteLine("Response:");
                Console.WriteLine(response.ToString());

                if (response.Content != null)
                {
                    Console.WriteLine();
                    Console.WriteLine("Response content:");
                    Console.WriteLine(await response.Content.ReadAsStringAsync());
                }

                Console.WriteLine();

                return response;
            }
        }
    }
}
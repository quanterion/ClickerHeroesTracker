﻿// <copyright file="MicrosoftAssertionGrantHandlerTests.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace UnitTests.Services.Authentication
{
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Protocols.OpenIdConnect;
    using Microsoft.IdentityModel.Tokens;
    using UnitTests.Mocks;
    using Website.Models.Authentication;
    using Website.Services.Authentication;
    using Xunit;

    public sealed class MicrosoftAssertionGrantHandlerTests
    {
        private const string ClientId = "SomeClientId";
        private const string ExternalUserId = "SomeExternalUserId";
        private const string ExternalUserEmail = "SomeExternalUserEmail";
        private const string ConfigurationEndpoint = "https://login.microsoftonline.com/common/v2.0/.well-known/openid-configuration";

        // Taken from http://self-issued.info/docs/draft-ietf-jose-json-web-key.html#rfc.appendix.A.2
        private static JsonWebKey jsonWebKey = new JsonWebKey
        {
            Kty = "RSA",
            N = "0vx7agoebGcQSuuPiLJXZptN9nndrQmbXEps2aiAFbWhM78LhWx4cbbfAAtVT86zwu1RK7aPFFxuhDR1L6tSoc_BJECPebWKRXjBZCiFV4n3oknjhMstn64tZ_2W-5JsGY4Hc5n9yBXArwl93lqt7_RN5w6Cf0h4QyQ5v-65YGjQR0_FDW2QvzqY368QQMicAtaSqzs8KJZgnYb9c7d0zgdAZHzu6qMQvRL5hajrn1n91CbOpbISD08qNLyrdkt-bFTWhAI4vMQFh6WeZu0fM4lFd2NcRwr3XPksINHaQ-G_xBniIqbw0Ls1jF44-csFCur-kEgU8awapJzKnqDKgw",
            E = "AQAB",
            D = "X4cTteJY_gn4FYPsXB8rdXix5vwsg1FLN5E3EaG6RJoVH-HLLKD9M7dx5oo7GURknchnrRweUkC7hT5fJLM0WbFAKNLWY2vv7B6NqXSzUvxT0_YSfqijwp3RTzlBaCxWp4doFk5N2o8Gy_nHNKroADIkJ46pRUohsXywbReAdYaMwFs9tv8d_cPVY3i07a3t8MN6TNwm0dSawm9v47UiCl3Sk5ZiG7xojPLu4sbg1U2jx4IBTNBznbJSzFHK66jT8bgkuqsk0GjskDJk19Z4qwjwbsnn4j2WBii3RL-Us2lGVkY8fkFzme1z0HbIkfz0Y6mqnOYtqc0X4jfcKoAC8Q",
            P = "83i-7IvMGXoMXCskv73TKr8637FiO7Z27zv8oj6pbWUQyLPQBQxtPVnwD20R-60eTDmD2ujnMt5PoqMrm8RfmNhVWDtjjMmCMjOpSXicFHj7XOuVIYQyqVWlWEh6dN36GVZYk93N8Bc9vY41xy8B9RzzOGVQzXvNEvn7O0nVbfs",
            Q = "3dfOR9cuYq-0S-mkFLzgItgMEfFzB2q3hWehMuG0oCuqnb3vobLyumqjVZQO1dIrdwgTnCdpYzBcOfW5r370AFXjiWft_NGEiovonizhKpo9VVS78TzFgxkIdrecRezsZ-1kYd_s1qDbxtkDEgfAITAG9LUnADun4vIcb6yelxk",
            DP = "G4sPXkc6Ya9y8oJW9_ILj4xuppu0lzi_H7VTkS8xj5SdX3coE0oimYwxIi2emTAue0UOa5dpgFGyBJ4c8tQ2VF402XRugKDTP8akYhFo5tAA77Qe_NmtuYZc3C3m3I24G2GvR5sSDxUyAN2zq8Lfn9EUms6rY3Ob8YeiKkTiBj0",
            DQ = "s9lAH9fggBsoFR8Oac2R_E2gw282rT2kGOAhvIllETE1efrA6huUUvMfBcMpn8lqeW6vzznYY5SSQF7pMdC_agI3nG8Ibp1BUb0JUiraRNqUfLhcQb_d9GF4Dh7e74WbRsobRonujTYN1xCaP6TO61jvWrX-L18txXw494Q_cgk",
            QI = "GyM_p6JrXySiz1toFgKbWV-JdI3jQ4ypu9rbMWx3rQJBfmt0FoYzgUIZEVFEcOqwemRN81zoDAaa-Bk0KWNGDjJHZDdDmFhW3AN7lI-puxk_mHZGJ11rxyR8O55XLSe3SPmRfKwZI6yU24ZxvQKFYItdldUKGzO6Ia6zTKhAVRU",
            Alg = "RS256",
            Kid = "SomeKid",
        };

        [Fact]
        public async Task ValidateAsync_Success()
        {
            var authenticationSettings = new AuthenticationSettings
            {
                Microsoft = new MicrosoftAuthenticationSettings
                {
                    ClientId = ClientId,
                },
            };
            var options = Options.Create(authenticationSettings);

            var configuration = new OpenIdConnectConfiguration();
            configuration.JsonWebKeySet = new JsonWebKeySet();
            configuration.JsonWebKeySet.Keys.Add(jsonWebKey);

            var httpClient = new MockHttpClient();
            httpClient.AddMockResponse(ConfigurationEndpoint, OpenIdConnectConfiguration.Write(configuration));

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = new JwtSecurityToken(
                audience: ClientId,
                claims: new[] { new Claim("sub", ExternalUserId), new Claim("email", ExternalUserEmail) },
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow + TimeSpan.FromHours(1),
                signingCredentials: new SigningCredentials(jsonWebKey, jsonWebKey.Alg));

            var handler = new MicrosoftAssertionGrantHandler(options, httpClient);
            var result = await handler.ValidateAsync(tokenHandler.WriteToken(token));

            Assert.NotNull(result);
            Assert.True(result.IsSuccessful);
            Assert.Equal(ExternalUserId, result.ExternalUserId);
            Assert.Equal(ExternalUserEmail, result.ExternalUserEmail);

            httpClient.VerifyNoOutstandingRequests();
        }

        [Fact]
        public async Task ValidateAsync_WrongAudience()
        {
            var authenticationSettings = new AuthenticationSettings
            {
                Microsoft = new MicrosoftAuthenticationSettings
                {
                    ClientId = ClientId,
                },
            };
            var options = Options.Create(authenticationSettings);

            var configuration = new OpenIdConnectConfiguration();
            configuration.JsonWebKeySet = new JsonWebKeySet();
            configuration.JsonWebKeySet.Keys.Add(jsonWebKey);

            var httpClient = new MockHttpClient();
            httpClient.AddMockResponse(ConfigurationEndpoint, OpenIdConnectConfiguration.Write(configuration));

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = new JwtSecurityToken(
                audience: "SomeOtherClientId",
                claims: new[] { new Claim("sub", ExternalUserId), new Claim("email", ExternalUserEmail) },
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow + TimeSpan.FromHours(1),
                signingCredentials: new SigningCredentials(jsonWebKey, jsonWebKey.Alg));

            var handler = new MicrosoftAssertionGrantHandler(options, httpClient);
            var result = await handler.ValidateAsync(tokenHandler.WriteToken(token));

            Assert.NotNull(result);
            Assert.False(result.IsSuccessful);

            httpClient.VerifyNoOutstandingRequests();
        }

        [Fact]
        public async Task ValidateAsync_InvalidToken()
        {
            var authenticationSettings = new AuthenticationSettings
            {
                Microsoft = new MicrosoftAuthenticationSettings
                {
                    ClientId = ClientId,
                },
            };
            var options = Options.Create(authenticationSettings);

            var configuration = new OpenIdConnectConfiguration();
            configuration.JsonWebKeySet = new JsonWebKeySet();
            configuration.JsonWebKeySet.Keys.Add(jsonWebKey);

            var httpClient = new MockHttpClient();
            httpClient.AddMockResponse(ConfigurationEndpoint, OpenIdConnectConfiguration.Write(configuration));

            var handler = new MicrosoftAssertionGrantHandler(options, httpClient);
            var result = await handler.ValidateAsync("SomeBadAssertion");

            Assert.NotNull(result);
            Assert.False(result.IsSuccessful);

            httpClient.VerifyNoOutstandingRequests();
        }
    }
}

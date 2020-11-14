using Microsoft.IdentityModel.Tokens;
using SOLUNESDIGITAL.FinancialEducation.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SOLUNESDIGITAL.Connector.Token.Mangers
{
    public interface ITokenManger
    {
        string GenerateAccessToken(IEnumerable<Claim> claims);
        FinancialEducation.Core.Entity.RefreshToken GenerateRefreshToken(string ipAddress, double timeExpires);
        Response GetPrincipalFromExpiredToken(string token);
    }
    public class TokenManger : ITokenManger
    {
        private readonly double _minutesExpiratioTime;
        private readonly string _validIssuer;
        private readonly string _validAudience;

        public TokenManger(double minutesExpiratioTime,string validIssuer,string validAudience)
        {
            _minutesExpiratioTime = minutesExpiratioTime;
            _validIssuer = validIssuer;
            _validAudience = validAudience;
        }

        public string GenerateAccessToken(IEnumerable<Claim> claims)
        {
            var key = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("SECRETKEY"));
            var secret = new SymmetricSecurityKey(key);

            var signinCredentials = new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);

            var tokeOptions = new JwtSecurityToken(
                issuer: _validIssuer,
                audience: _validAudience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(_minutesExpiratioTime),
                signingCredentials: signinCredentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);
            return tokenString;
        }

        public FinancialEducation.Core.Entity.RefreshToken GenerateRefreshToken(string ipAddress, double timeExpires)
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);

            return new FinancialEducation.Core.Entity.RefreshToken
            {
                Token = Convert.ToBase64String(randomNumber),
                Expires = DateTime.UtcNow.AddDays(timeExpires),
                CreatedByIp = ipAddress
            };
        }

        public Response GetPrincipalFromExpiredToken(string token)
        {
            Response response = new Response();
            var secretKey = Environment.GetEnvironmentVariable("SECRETKEY");

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _validIssuer,
                ValidAudience = _validAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase)) 
            {
                var validateError = Response.Error("InvalidToken");
                response.Message = validateError.Message.ToString();
                response.State = validateError.State.ToString();
                response.Data = null;

                return response;
            }
            var validated = Response.Success(principal,"TokenCorrect");

            response.Message = validated.Message.ToString();
            response.State = validated.State.ToString();
            response.Data = validated.Data;

            return response;

        }
    }
}

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Entities;
using API.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace API.Services {
    public class TokenService : ITokenService{
        // a type of encryption where only 1 key is usde to both encrypt and decrypt electronic info
        //https requires assymemetric, meanins 2 different keys, one to encrypt and another to decrypt, private and public
        private readonly SymmetricSecurityKey _key; 
        public TokenService(IConfiguration config){
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"]));
        }
        public string CreateTroken(AppUser user){
            //adding the claim
            var claims = new List<Claim>{
                new Claim(JwtRegisteredClaimNames.NameId, user.Username)
            };
            //creating the credentials
            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

            //describing our token is gonna look
            var tokenDescriptor = new SecurityTokenDescriptor{
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(7),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            //Creates the token
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
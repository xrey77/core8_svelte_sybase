using AutoMapper.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using System;
using AdoNetCore.AseClient;
using core8_svelte_sybase.Helpers;
using core8_svelte_sybase.Entities;

namespace core8_svelte_sybase.Services
{
    public interface IJWTTokenServices
    {
        Task<JWTTokens> Authenticate(User users);
    }
    public class JWTServiceManage : IJWTTokenServices
    {
        private readonly IConfiguration _configuration;
        private readonly ISybaseConnectionFactory _connectionFactory;


         IConfiguration config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddEnvironmentVariables()
        .Build();

 
        public JWTServiceManage(IConfiguration configuration, ISybaseConnectionFactory connectionFactory)
        {
            _configuration = configuration;
            _connectionFactory = connectionFactory;            
        }
        public async Task<JWTTokens> Authenticate(User users)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {                         
                string sql = "SELECT * FROM Users WHERE username = @username AND password_hash = @password";
                using (AseCommand command = new AseCommand(sql, (AseConnection)connection))
                {
                    command.Parameters.AddWithValue("@username", users.UserName);
                    command.Parameters.AddWithValue("@password", users.Password_hash);
                    using (AseDataReader reader = (AseDataReader)await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            if (reader.GetString(reader.GetOrdinal("email")) is null) {
                            return null;
                            }
                        }
                    }
                }
            }
 
            var tokenhandler = new JwtSecurityTokenHandler();
            var tkey = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
            var ToeknDescp = new SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, users.UserName)
                }),
                Expires = DateTime.UtcNow.AddMinutes(5),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tkey), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenhandler.CreateToken(ToeknDescp);
 
            return new JWTTokens { Token = tokenhandler.WriteToken(token) };
 
        }
    }    
    
}
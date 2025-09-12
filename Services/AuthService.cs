using System;
using System.IO;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using AdoNetCore.AseClient;
using core8_svelte_sybase.Entities;
using core8_svelte_sybase.Helpers;
using core8_svelte_sybase.Models.dto;

namespace core8_svelte_sybase.Services
{    
    public interface IAuthService {
        Task<User> SignupUser(User userdata, string passwd);
        Task<User> SigninUser(string usrname, string pwd);
    }

    public class AuthService : IAuthService
    {
        private readonly AppSettings _appSettings;
        private readonly ISybaseConnectionFactory _connectionFactory;

         IConfiguration config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddEnvironmentVariables()
        .Build();

        public AuthService(IOptions<AppSettings> appSettings, ISybaseConnectionFactory connectionFactory)
        {
            _appSettings = appSettings.Value;
            _connectionFactory = connectionFactory;            
        }

        public async Task<User> SignupUser(User userdata, string passwd)
        {
            // await using var connection = new AseConnection(config["ConnectionStrings:SybaseConnection"]);
            // await connection.OpenAsync();
            string sqlemail = "SELECT email FROM dbo.users WHERE email = @email";
            using (var connection = _connectionFactory.CreateConnection())
            {
                using (AseCommand findEmail = new AseCommand(sqlemail, (AseConnection)connection))
                {
                    findEmail.Parameters.AddWithValue("@email", userdata.Email);
                    using (AseDataReader reader = (AseDataReader)await findEmail.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            if (reader.GetString(reader.GetOrdinal("email")) is not null) {
                                throw new AppException("Email Address was already taken...");                    
                            } 
                        }
                    }
                }            

                string sqlusername = "SELECT username FROM dbo.users WHERE username = @username";
                using (AseCommand findUser = new AseCommand(sqlusername, (AseConnection)connection))
                {
                    findUser.Parameters.AddWithValue("@username", userdata.UserName);
                    using (AseDataReader reader = (AseDataReader)await findUser.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            if (reader.GetString(reader.GetOrdinal("username")) is not null) {
                            {
                                throw new AppException("Username was already taken...");                  
                            }
                        }
                    }
                }            

                var tokenHandler = new JwtSecurityTokenHandler();
                var xkey = config["Jwt:Key"];
                var key = Encoding.ASCII.GetBytes(xkey);

                // CREATE SECRET KEY FOR USER TOKEN===============
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, userdata.Email)
                    }),
                    // Expires = DateTime.UtcNow.AddDays(7),
                    Expires = DateTime.UtcNow.AddHours(8),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var secret = tokenHandler.CreateToken(tokenDescriptor);
                var secretkey = tokenHandler.WriteToken(secret);

                userdata.Secretkey = secretkey.ToUpper();             
                userdata.Password_hash = BCrypt.Net.BCrypt.HashPassword(passwd);

                try {
                        String sqlAdd = "INSERT INTO dbo.users(firstname, lastname, email, mobile, username, password_hash, roles, isactivated, isblocked, mailtoken, qrcodeurl, profilepic, secretkey, createdat, updatedat) VALUES (@firstname, @lastname, @email, @mobile, @username, @password, @roles, @isactivated, @isblocked, @mailtoken, @qrcodeurl, @profilepic, @secretkey, @createdat, @updatedat)";
                        using (AseCommand command = new AseCommand(sqlAdd, (AseConnection)connection))
                        {
                            command.Parameters.AddWithValue("@firstname", userdata.FirstName);
                            command.Parameters.AddWithValue("@lastname", userdata.LastName);
                            command.Parameters.AddWithValue("@email", userdata.Email);
                            command.Parameters.AddWithValue("@mobile", userdata.Mobile);
                            command.Parameters.AddWithValue("@username", userdata.UserName);
                            command.Parameters.AddWithValue("@password", userdata.Password_hash);
                            command.Parameters.AddWithValue("@roles", userdata.Roles);
                            command.Parameters.AddWithValue("@isactivated", userdata.IsActivated);
                            command.Parameters.AddWithValue("@isblocked", userdata.Isblocked);
                            command.Parameters.AddWithValue("@mailtoken", userdata.Mailtoken);
                            command.Parameters.AddWithValue("@qrcodeurl", "/images/qrcode.png");
                            command.Parameters.AddWithValue("@profilepic", userdata.Profilepic);
                            command.Parameters.AddWithValue("@secretkey", userdata.Secretkey);
                            command.Parameters.AddWithValue("@createdat", userdata.CreatedAt);
                            command.Parameters.AddWithValue("@updatedat", userdata.UpdatedAt);
                            command.ExecuteNonQuery();
                        };       




                        return userdata;      
                } catch (Exception ex) {
                    throw new AppException(ex.Message);
                }
            }
          }
        }

        public async Task<User> SigninUser(string usrname, string pwd)
        {
            // await using var connection = new AseConnection(config["ConnectionStrings:SybaseConnection"]);
            // await connection.OpenAsync();
            var user = new User();
            string sql = "SELECT id, firstname, lastname, email, mobile, username, password_hash, roles, isactivated, isblocked, mailtoken, qrcodeurl, profilepic, secretkey, createdat, updatedat FROM dbo.users WHERE username = @username";
            using (var connection = _connectionFactory.CreateConnection())
            {
                using (AseCommand command = new AseCommand(sql, (AseConnection)connection)) 
                {
                    command.Parameters.AddWithValue("@username", usrname);
                    using (AseDataReader reader = (AseDataReader)await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            if (reader.GetString(reader.GetOrdinal("username")) is not null) {
                                if (!BCrypt.Net.BCrypt.Verify(pwd, reader.GetString(reader.GetOrdinal("password_hash")))) {
                                    throw new AppException("Incorrect Password...");
                                }
                                if (reader.GetInt32(reader.GetOrdinal("isactivated")) == 0) {
                                    throw new AppException("Please activate your account, check your email client inbox and click or tap the Activate button.");
                                }
                            }
                                user.Id = reader.GetInt32(reader.GetOrdinal("id"));
                                user.FirstName = reader.GetString(reader.GetOrdinal("firstname"));
                                user.LastName = reader.GetString(reader.GetOrdinal("lastname"));
                                user.Email = reader.GetString(reader.GetOrdinal("email"));
                                user.Mobile = reader.GetString(reader.GetOrdinal("mobile"));
                                user.UserName = reader.GetString(reader.GetOrdinal("username"));
                                user.Password_hash = "";                            
                                user.Roles = reader.GetString(reader.GetOrdinal("roles"));
                                user.IsActivated = reader.GetInt32(reader.GetOrdinal("isactivated"));
                                user.Isblocked = reader.GetInt32(reader.GetOrdinal("isblocked"));
                                user.Mailtoken = reader.GetInt32(reader.GetOrdinal("mailtoken"));
                                user.Qrcodeurl = reader.GetString(reader.GetOrdinal("qrcodeurl"));
                                user.Profilepic = reader.GetString(reader.GetOrdinal("profilepic"));
                                user.Secretkey = reader.GetString(reader.GetOrdinal("secretkey"));
                                user.CreatedAt = reader.GetDateTime(reader.GetOrdinal("createdat"));
                                user.UpdatedAt = reader.GetDateTime(reader.GetOrdinal("updatedat"));
                        }
                    }
                }
                return user;
            }
        }
    }
}
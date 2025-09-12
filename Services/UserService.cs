using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using AdoNetCore.AseClient;
using core8_svelte_sybase.Entities;
using core8_svelte_sybase.Helpers;

namespace core8_svelte_sybase.Services
{
    public interface IUserService {
        IEnumerable<User> GetAll();
        Task<User> GetById(int id);
        Task<bool> UpdateProfile(User user);
        Task<bool> Delete(int id);
        Task<bool> ActivateMfa(int id, bool opt, string qrcode_url);
        Task<bool> UpdatePicture(int id, string file);
        Task<bool> UpdatePassword(User user, string password = null);
        int EmailToken(int etoken);
        Task<int> SendEmailToken(string email);
        Task<bool> ActivateUser(int id);
        Task<bool> ChangePassword(User userParam);
    }

    public class UserService : IUserService
    {
        private readonly AppSettings _appSettings;
        private readonly ISybaseConnectionFactory _connectionFactory;

         IConfiguration config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddEnvironmentVariables()
        .Build();

        public UserService(IOptions<AppSettings> appSettings, ISybaseConnectionFactory connectionFactory)
        {
            _appSettings = appSettings.Value;
            _connectionFactory = connectionFactory;            
        }

        public async Task<bool> Delete(int id)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {            
                string sql = "DELETE FROM dbo.users WHERE id = @id";
                using (AseCommand deleteCommand = new AseCommand(sql, (AseConnection)connection))
                {
                    deleteCommand.Parameters.AddWithValue("@id", id);
                    using (AseDataReader reader = (AseDataReader)await deleteCommand.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            if (reader.GetInt32(reader.GetOrdinal("id")) == 0) {
                                throw new AppException("User not found");                  
                            }
                        }
                    }
                    return true;
                }
            }
        }

        public IEnumerable<User> GetAll()
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                List<User> records = new List<User>();
                string sqlQuery = "SELECT * FROM dbo.users";
                using (AseCommand command = new AseCommand(sqlQuery, (AseConnection)connection))            
                {
                    using (AseDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            User record = new User
                            {
                                Id = int.Parse(reader["id"].ToString()),
                                FirstName = reader["firstname"].ToString(),
                                LastName = reader["lastname"].ToString(),
                                Email = reader["email"].ToString(),
                                Mobile = reader["mobile"].ToString(),
                                UserName = reader["username"].ToString(),
                                Roles = reader["roles"].ToString(),
                                Profilepic = reader["profilepic"].ToString(),
                                Mailtoken = int.Parse(reader["mailtoken"].ToString()),
                                IsActivated = int.Parse(reader["isactivated"].ToString()),
                                Isblocked = int.Parse(reader["isblocked"].ToString()),
                                Qrcodeurl = reader["qrcodeurl"].ToString(),
                                CreatedAt = DateTime.Parse(reader["createdat"].ToString()),
                                UpdatedAt = DateTime.Parse(reader["updatedat"].ToString())
                            };
                            records.Add(record);                        
                        }
                    }
                }
                return records; 
            }
        }

        public async Task<User> GetById(int id)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {            
                string sql = "SELECT id,firstname,lastname,email,mobile,username,password_hash,roles,isactivated,isblocked,mailtoken,qrcodeurl,profilepic,secretkey,createdat,updatedat FROM dbo.users WHERE id = @id";
                using (AseCommand command = new AseCommand(sql, (AseConnection)connection)) {
                    command.Parameters.AddWithValue("@id", id);
                    using (AseDataReader reader = (AseDataReader)await command.ExecuteReaderAsync()) {
                        if (await reader.ReadAsync()) {
                            User user = new User {
                                Id = reader.GetInt32(reader.GetOrdinal("id")),
                                FirstName = reader.GetString(reader.GetOrdinal("firstname")),
                                LastName = reader.GetString(reader.GetOrdinal("lastname")),
                                Email = reader.GetString(reader.GetOrdinal("email")),
                                Mobile = reader.GetString(reader.GetOrdinal("mobile")),
                                UserName = reader.GetString(reader.GetOrdinal("username")),
                                Password_hash = reader.GetString(reader.GetOrdinal("password_hash")),
                                Roles = reader.GetString(reader.GetOrdinal("roles")),
                                IsActivated = reader.GetInt32(reader.GetOrdinal("isactivated")),
                                Isblocked = reader.GetInt32(reader.GetOrdinal("isblocked")),
                                Mailtoken = reader.GetInt32(reader.GetOrdinal("mailtoken")),
                                Qrcodeurl = reader.GetString(reader.GetOrdinal("qrcodeurl")),
                                Profilepic = reader.GetString(reader.GetOrdinal("profilepic")),
                                Secretkey = reader.GetString(reader.GetOrdinal("secretkey")),
                                CreatedAt = reader.GetDateTime(reader.GetOrdinal("createdat")),
                                UpdatedAt = reader.GetDateTime(reader.GetOrdinal("updatedat"))
                            };
                            return user;
                        }                    
                    }
                }
                return null;
            }
        }

        public async Task<bool> UpdateProfile(User userParam)
        {
            DateTime now = DateTime.Now;
            using (var connection = _connectionFactory.CreateConnection())
            {            
                var profileCommand = new AseCommand("UPDATE dbo.users SET firstname=@firstname, lastname=@lastname,mobile=@mobile,updatedat=@updatedat WHERE id = @id", (AseConnection)connection);
                profileCommand.Parameters.AddWithValue("@id", userParam.Id);
                profileCommand.Parameters.AddWithValue("@firstname", userParam.FirstName);
                profileCommand.Parameters.AddWithValue("@lastname", userParam.LastName);
                profileCommand.Parameters.AddWithValue("@mobile", userParam.Mobile);    
                profileCommand.Parameters.AddWithValue("@updatedat", now);    
                await profileCommand.ExecuteReaderAsync();
                return true;
            }
        }

        public async Task<bool> UpdatePassword(User userParam, string password = null)
        {
            DateTime now = DateTime.Now;
            var passWord = "";
            using (var connection = _connectionFactory.CreateConnection())
            {
                var updateCommand = new AseCommand("UPDATE dbo.users SET password_hash = @password, updatedat = @updatedat WHERE id = @id", (AseConnection)connection);
                passWord = BCrypt.Net.BCrypt.HashPassword(userParam.Password_hash);
                updateCommand.Parameters.AddWithValue("@password", passWord);
                updateCommand.Parameters.AddWithValue("@updatedat", now);    
                updateCommand.Parameters.AddWithValue("@id", userParam.Id);
                await updateCommand.ExecuteReaderAsync();
                return true;
            }
        }


        public async Task<bool> ActivateMfa(int id, bool opt, string qrcode_url)
        {
            DateTime now = DateTime.Now;
            using (var connection = _connectionFactory.CreateConnection())
            {
                var updateCommand = new AseCommand("UPDATE dbo.users SET qrcodeurl=@qrcodeurl,updatedat=@updatedat WHERE id = @id", (AseConnection)connection);
                updateCommand.Parameters.AddWithValue("@id", id);
                if (opt) {
                    updateCommand.Parameters.AddWithValue("@qrcodeurl", qrcode_url);
                } else {
                    updateCommand.Parameters.AddWithValue("@qrcodeurl", "/images/qrcode.png");
                }
                updateCommand.Parameters.AddWithValue("@updatedat", now);    
                await updateCommand.ExecuteReaderAsync();
                return true;
            }
      }

        public async Task<bool> UpdatePicture(int id, string file)
        {
            DateTime now = DateTime.Now;
            using (var connection = _connectionFactory.CreateConnection())
            {
                var updateCommand = new AseCommand("UPDATE dbo.users SET profilepic=@profilepic,updatedat=@updatedat WHERE id = @id", (AseConnection)connection);
                updateCommand.Parameters.AddWithValue("@id", id);
                updateCommand.Parameters.AddWithValue("@profilepic", file);
                updateCommand.Parameters.AddWithValue("@updatedat", now);    
                await updateCommand.ExecuteReaderAsync();
                return true;
            }
        }

       public async Task<bool> ActivateUser(int id) 
       {
            using (var connection = _connectionFactory.CreateConnection())
            {
                string sql = "SELECT * FROM dbo.users WHERE id = @id";
                using (AseCommand command = new AseCommand(sql, (AseConnection)connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    using (AseDataReader reader = (AseDataReader)await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            if (reader.GetInt32(reader.GetOrdinal("isblocked")) == 1) {
                            throw new AppException("Account has been blocked.");
                            }
                            if (reader.GetInt32(reader.GetOrdinal("isactivated")) == 1) {
                            throw new AppException("Account is alread activated.");
                            }
                            if (reader.GetInt32(reader.GetOrdinal("id")) == 0) {
                            throw new AppException("User not found");
                            }
                        }
                    }
                }
                DateTime now = DateTime.Now;
                var updateCommand = new AseCommand("UPDATE dbo.users SET isactivated=@isactivated,updatedat=@updatedat WHERE id = @id", (AseConnection)connection);
                updateCommand.Parameters.AddWithValue("@id", id);
                updateCommand.Parameters.AddWithValue("@isactivated", 1);
                updateCommand.Parameters.AddWithValue("@updatedat", now);    
                await updateCommand.ExecuteReaderAsync();
                return true;    
            }
       }

        //CREATE MAILTOKEN AND SENT TO REGISTERED USER EMAIL
        public async Task<int> SendEmailToken(string email)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                var mtoken = 0;
                string sql = "SELECT * FROM dbo.users WHERE email = @email";
                using (AseCommand command = new AseCommand(sql, (AseConnection)connection))
                {
                    command.Parameters.AddWithValue("@email", email);
                    using (AseDataReader reader = (AseDataReader)await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            if (reader.GetString(reader.GetOrdinal("email")) is null) {
                                throw new AppException("Email Address not found...");
                            }
                            mtoken = reader.GetInt32(reader.GetOrdinal("mailtoken"));
                        }
                    }
                }

                DateTime now = DateTime.Now;
                var updateCommand = new AseCommand("UPDATE dbo.users SET mailtoken=@mailtoken,updatedat=@updatedat WHERE email = @email", (AseConnection)connection);
                updateCommand.Parameters.AddWithValue("@email", email);
                var etoken = EmailToken(mtoken);
                updateCommand.Parameters.AddWithValue("@mailtoken", etoken);
                updateCommand.Parameters.AddWithValue("@updatedat", now);    
                await updateCommand.ExecuteReaderAsync();
                return etoken;
            }
        }       

        //CREATE MAILTOKEN
        public int EmailToken(int etoken)
        {
            if (etoken == 0) {
                etoken = 1000;
            }
            int _min = etoken;
            int _max = 9999;
            Random _rdm = new Random();
            return _rdm.Next(_min, _max);
        }

        public async Task<bool> ChangePassword(User userParam)
        {
            DateTime now = DateTime.Now;
            using (var connection = _connectionFactory.CreateConnection())
            {
                string sql = "SELECT * FROM dbo.users WHERE username = @username";
                using (AseCommand command = new AseCommand(sql, (AseConnection)connection))
                {
                    command.Parameters.AddWithValue("@username", userParam.UserName);
                    using (AseDataReader reader = (AseDataReader)await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            if (reader.GetString(reader.GetOrdinal("email")) is null) {
                                throw new AppException("Email Address not found, please user your account email...");
                            }
                        }
                    }
                }

                var updateCommand = new AseCommand("UPDATE dbo.users SET mailtoken=@mailtoken, password_hash=@password, updatedat=@updatedat WHERE username = @username", (AseConnection)connection);
                var etoken = EmailToken(userParam.Mailtoken);
                updateCommand.Parameters.AddWithValue("@mailtoken", etoken);
                var pwd = BCrypt.Net.BCrypt.HashPassword(userParam.Password_hash);
                updateCommand.Parameters.AddWithValue("@password", pwd);    
                updateCommand.Parameters.AddWithValue("@updatedat", now);    
                updateCommand.Parameters.AddWithValue("@username", userParam.UserName);
                await updateCommand.ExecuteReaderAsync();
                return true;
            }
        }
    }
}
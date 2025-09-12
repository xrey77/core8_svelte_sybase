using System.Data;
using System.IO;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using AdoNetCore.AseClient;
using core8_svelte_sybase.Entities;
using core8_svelte_sybase.Helpers;
using core8_svelte_sybase.Models;

namespace core8_svelte_sybase.Services
{
    public interface IProductService {
        IEnumerable<Product> ListAll(int perpage, int offset);        
        Task<int> TotPage();
        IEnumerable<Product> SearchAll(string key, int perpage, int offset);
        Task<int> TotPageSearch(string key, int perpage);
        Task<Product> CreateProduct(Product prod);
        Task<bool> ProductUpdate(Product prod);
        Task<bool> ProductDelete(int id);
        Task<bool> UpdateProdPicture(int id, string file);
        Task<Product> GetProductById(int id);
    }

    public class ProductService : IProductService
    {
        private readonly AppSettings _appSettings;
        private readonly ISybaseConnectionFactory _connectionFactory;

         IConfiguration config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddEnvironmentVariables()
        .Build();

        public ProductService(IOptions<AppSettings> appSettings, ISybaseConnectionFactory connectionFactory)
        {
            _appSettings = appSettings.Value;
            _connectionFactory = connectionFactory;            
        }        

        public async Task<int> TotPage() {
            int perpage = 5;
            int totrecs = 0;

            using (var connection = _connectionFactory.CreateConnection())
            {            
                var sql = "SELECT COUNT(id) FROM dbo.products";
                using (AseCommand command = new AseCommand(sql, (AseConnection)connection))
                {
                    totrecs = (int)await command.ExecuteScalarAsync();                            
                }
            }
            int totpage = (int)Math.Ceiling((float)(totrecs) / perpage);
            return totpage;
        }

        public IEnumerable<Product> ListAll(int perpage, int offset)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                List<Product> records = new List<Product>();
                var sqlQuery = "SET ROWCOUNT @perpage SELECT * FROM dbo.products WHERE id > @offset ORDER BY id";
                using (AseCommand command = new AseCommand(sqlQuery, (AseConnection)connection))            
                {
                    command.Parameters.Add(new AseParameter("@offset", AseDbType.Integer) { Value = offset });                    
                    command.Parameters.Add(new AseParameter("@perpage", AseDbType.Integer) { Value = perpage });
                    
                    using (AseDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            Product record = new Product
                            {
                                Id = int.Parse(reader["id"].ToString()),
                                Category = reader["category"].ToString(),
                                Descriptions = reader["descriptions"].ToString(),
                                Qty = int.Parse(reader["qty"].ToString()),
                                Unit = reader["unit"].ToString(),
                                CostPrice = decimal.Parse(reader["costprice"].ToString()),
                                SellPrice = decimal.Parse(reader["sellprice"].ToString()),
                                SalePrice = decimal.Parse(reader["saleprice"].ToString()),
                                ProductPicture = reader["productpicture"].ToString(),
                                AlertStocks = int.Parse(reader["alertstocks"].ToString()),
                                CriticalStocks = int.Parse(reader["criticalstocks"].ToString()),
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

        public async Task<int> TotPageSearch(string key, int perpage) {
            int totrecs = 0;

            using (var connection = _connectionFactory.CreateConnection())
            {            
                var sql = "SELECT COUNT(*) FROM dbo.products WHERE LOWER(descriptions) LIKE @key ORDER BY id";
                using (AseCommand command = new AseCommand(sql, (AseConnection)connection))
                {
                    command.Parameters.Add(new AseParameter("@key", AseDbType.VarChar) { Value = key });

                    totrecs = (int)await command.ExecuteScalarAsync();                            
                }
                int totpage = (int)Math.Ceiling((float)(totrecs) / perpage);
                return totpage;
            }
        }


        public IEnumerable<Product> SearchAll(string key, int perpage, int offset)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                List<Product> records = new List<Product>();
                var sqlQuery = "SET ROWCOUNT @perpage SELECT * FROM dbo.products WHERE id > @offset AND LOWER(descriptions) LIKE @key ORDER BY id";
                using (AseCommand command = new AseCommand(sqlQuery, (AseConnection)connection))            
                {
                    command.Parameters.Add(new AseParameter("@perpage", AseDbType.Integer) { Value = perpage });
                    command.Parameters.Add(new AseParameter("@key", AseDbType.VarChar) { Value = key });
                    // command.Parameters.AddWithValue("@key", key);
                    command.Parameters.Add(new AseParameter("@offset", AseDbType.Integer) { Value = offset });
                    
                    using (AseDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            Product record = new Product
                            {
                                Id = int.Parse(reader["id"].ToString()),
                                Category = reader["category"].ToString(),
                                Descriptions = reader["descriptions"].ToString(),
                                Qty = int.Parse(reader["qty"].ToString()),
                                Unit = reader["unit"].ToString(),
                                CostPrice = decimal.Parse(reader["costprice"].ToString()),
                                SellPrice = decimal.Parse(reader["sellprice"].ToString()),
                                SalePrice = decimal.Parse(reader["saleprice"].ToString()),
                                ProductPicture = reader["productpicture"].ToString(),
                                AlertStocks = int.Parse(reader["alertstocks"].ToString()),
                                CriticalStocks = int.Parse(reader["criticalstocks"].ToString()),
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


        public async Task<Product> CreateProduct(Product prod) {
            try {
                using (var connection = _connectionFactory.CreateConnection())
                {            
                    string sqlSelect = "SELECT * FROM dbo.products WHERE descriptions = @descriptions";
                    using (AseCommand commandSel = new AseCommand(sqlSelect, (AseConnection)connection))
                    {
                        commandSel.Parameters.AddWithValue("@descriptions", prod.Descriptions);
                        using (AseDataReader reader = (AseDataReader)await commandSel.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                if (reader.GetString(reader.GetOrdinal("descriptions")) is not null) {
                                    throw new AppException("Product Description is already exists...");
                                }
                            }
                            reader.Dispose();
                        }
                        commandSel.Dispose();
                    }
                    
                    String sqlInsert = "INSERT INTO dbo.products(category, descriptions, unit, qty, costprice, sellprice, saleprice, productpicture, alertstocks, criticalstocks, createdat, updatedat) VALUES (@category, @descriptions, @unit, @qty, @costprice, @sellprice, @saleprice, @productpicture, @alertstocks, @criticalstocks, @createdat, @updatedat)";
                    using (AseCommand command = new AseCommand(sqlInsert, (AseConnection)connection))
                    {
                        command.Parameters.AddWithValue("@category", prod.Category);
                        command.Parameters.AddWithValue("@descriptions", prod.Descriptions);
                        command.Parameters.AddWithValue("@unit", prod.Unit);
                        command.Parameters.AddWithValue("@qty", prod.Qty);
                        command.Parameters.AddWithValue("@costprice", prod.CostPrice);
                        command.Parameters.AddWithValue("@sellprice", prod.SellPrice);
                        command.Parameters.AddWithValue("@saleprice", prod.SalePrice);
                        command.Parameters.AddWithValue("@productpicture", prod.ProductPicture);
                        command.Parameters.AddWithValue("@alertstocks", prod.AlertStocks);
                        command.Parameters.AddWithValue("@criticalstocks", prod.CriticalStocks);
                        command.Parameters.AddWithValue("@createdat", prod.CreatedAt);
                        command.Parameters.AddWithValue("@updatedat", prod.UpdatedAt);
                        command.ExecuteNonQuery();
                    }      
                    return prod;
                }
            } catch(Exception ex){
                throw new AppException(ex.Message);              
            }
        }

        public async Task<bool> ProductUpdate(Product prods) {
            using (var connection = _connectionFactory.CreateConnection())
            {            
                string sql = "SELECT * FROM dbo.products WHERE id = @id";
                using (AseCommand getIdCommand = new AseCommand(sql, (AseConnection)connection))
                {
                    getIdCommand.Parameters.AddWithValue("@id", prods.Id);
                    using (AseDataReader reader = (AseDataReader)await getIdCommand.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                         if (reader.GetInt32(reader.GetOrdinal("id")) == 0) {
                                throw new AppException("Product not found");                        
                         }
                        }
                    }
                }

                DateTime now = DateTime.Now;
                var command = new AseCommand("UPDATE dbo.products SET category = @category, descriptions = @descriptions, unit = @unit, qty = @qty, costprice = @costprice, sellprice = @sellprice, saleprice = @saleprice,productpicture = @productpicture, alertstocks = @alertstocks, criticalstocks = @criticalstocks,updatedat=@updatedat WHERE id = @id", (AseConnection)connection);
                command.Parameters.AddWithValue("@id", prods.Id);
                command.Parameters.AddWithValue("@category", prods.Category);
                command.Parameters.AddWithValue("@descriptions", prods.Descriptions);
                command.Parameters.AddWithValue("@unit", prods.Unit);
                command.Parameters.AddWithValue("@qty", prods.Qty);
                command.Parameters.AddWithValue("@costprice", prods.CostPrice);
                command.Parameters.AddWithValue("@sellprice", prods.SellPrice);
                command.Parameters.AddWithValue("@saleprice", prods.SalePrice);
                command.Parameters.AddWithValue("@productpicture", prods.ProductPicture);
                command.Parameters.AddWithValue("@alertstocks", prods.AlertStocks);
                command.Parameters.AddWithValue("@criticalstocks", prods.CriticalStocks);
                command.Parameters.AddWithValue("@updatedat", now);    
                await command.ExecuteReaderAsync();
                return true;
            }
        }

        public async Task<bool> ProductDelete(int id) {
            using (var connection = _connectionFactory.CreateConnection())
            {            
                string sql = "DELETE FROM dbo.products WHERE id = @id";
                using (AseCommand deleteCommand = new AseCommand(sql, (AseConnection)connection))
                {
                    deleteCommand.Parameters.AddWithValue("@id", id);
                    using (AseDataReader reader = (AseDataReader)await deleteCommand.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            if (reader.GetInt32(reader.GetOrdinal("id")) == 0) {
                                throw new AppException("Product not found");                  
                            }
                        }
                    }
                    return true;
                }
            }
        }

        public async Task<bool> UpdateProdPicture(int id, string file) {
            using (var connection = _connectionFactory.CreateConnection())
            {            
                var updateCommand = new AseCommand("UPDATE dbo.products SET productpicture=@productpicture, updatedat=@updatedat WHERE id = @id", (AseConnection)connection);
                updateCommand.Parameters.AddWithValue("@id", id);
                updateCommand.Parameters.AddWithValue("@productpicture", file);
                await updateCommand.ExecuteReaderAsync();
                return true;
            }
        }

        public async Task<Product> GetProductById(int id) {
            using (var connection = _connectionFactory.CreateConnection())
            {            
                string sql = "SELECT * FROM dbo.products WHERE id = @id";
                using (AseCommand command = new AseCommand(sql, (AseConnection)connection)) 
                {           
                    command.Parameters.AddWithValue("@id", id);
                    using (AseDataReader reader = (AseDataReader)await command.ExecuteReaderAsync()) {
                        if (await reader.ReadAsync()) {
                            Product prods = new Product {
                                Id = reader.GetInt32(reader.GetOrdinal("id")),
                                Category = reader.GetString(reader.GetOrdinal("category")),
                                Descriptions = reader.GetString(reader.GetOrdinal("descriptions")),
                                Unit = reader.GetString(reader.GetOrdinal("unit")),
                                Qty = reader.GetInt32(reader.GetOrdinal("qty")),
                                CostPrice = reader.GetDecimal(reader.GetOrdinal("costprice")),
                                SellPrice = reader.GetDecimal(reader.GetOrdinal("sellprice")),
                                SalePrice = reader.GetDecimal(reader.GetOrdinal("sellprice")),
                                ProductPicture = reader.GetString(reader.GetOrdinal("productpicture")),
                                AlertStocks = reader.GetInt32(reader.GetOrdinal("alertstocks")),
                                CriticalStocks = reader.GetInt32(reader.GetOrdinal("criticalstocks"))
                            };
                            return prods;
                        }
                        return null;
                    }
                }
            }
        }        
    }
}
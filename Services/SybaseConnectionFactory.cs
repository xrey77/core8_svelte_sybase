using System.Data;
using AdoNetCore.AseClient;
using Microsoft.Extensions.Configuration;

namespace core8_svelte_sybase.Services
{

    public interface ISybaseConnectionFactory
    {
        IDbConnection CreateConnection();
    }

    public class SybaseConnectionFactory : ISybaseConnectionFactory
    {
        private readonly string _connectionString;


        public SybaseConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SybaseConnection");
        }

        public IDbConnection CreateConnection()
        {
            var connection = new AseConnection(_connectionString);
            connection.Open(); // The connection should be opened by the factory
            return connection;
        }
    }    
    
}
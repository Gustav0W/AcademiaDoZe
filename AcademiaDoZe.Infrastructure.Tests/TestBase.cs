using AcademiaDoZe.Infrastructure.Data;
namespace AcademiaDoZe.Infrastructure.Tests
{
    public abstract class TestBase
    {
        protected string ConnectionString { get; private set; }
        protected DatabaseType DatabaseType { get; private set; }
        protected TestBase()
        {
            var config = CreateSqlServerConfig();
            //var config = CreateMySqlConfig();
            ConnectionString = config.ConnectionString;
            DatabaseType = config.DatabaseType;
        }
        private (string ConnectionString, DatabaseType DatabaseType) CreateSqlServerConfig()
        {
            var connectionString = "Server=GUSTAVOWALTNOTE\\SQLEXPRESS;Database=AcademiaDoZe_TESTES;Trusted_Connection=True;TrustServerCertificate=True;";

            return (connectionString, DatabaseType.SqlServer);

        }
        private (string ConnectionString, DatabaseType DatabaseType) CreateMySqlConfig()
        {
            var connectionString = "Server=localhost;Database=AcademiaDoZe-db;User Id=root;Password=senhaforte;";

            return (connectionString, DatabaseType.MySql);

        }
    }
}
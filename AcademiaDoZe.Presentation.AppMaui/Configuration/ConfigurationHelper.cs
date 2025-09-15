using AcademiaDoZe.Application.DependencyInjection;
using AcademiaDoZe.Application.Enums;

namespace AcademiaDoZe.Presentation.AppMaui.Configuration;

public static class ConfigurationHelper
{
    public static void ConfigureServices(IServiceCollection services)
    {
        // dados conexão

        const string dbServer = "GUSTAVOWALTNOTE\\SQLEXPRESS";
        const string dbDatabase = "AcademiaDoZe_TESTES";
        const string dbTrustedConnection = "True";
        //const string dbPassword = "abcBolinhas12345";
        const string dbComplemento = "TrustServerCertificate=True;Encrypt=True;";
        // se for necessário indicar a porta, incluir junto em dbComplemento

        // Configurações de conexão
        const string connectionString = $"Server={dbServer};Database={dbDatabase};Trusted_Connection={dbTrustedConnection};{dbComplemento}";

        const EAppDatabaseType databaseType = EAppDatabaseType.SqlServer;
        // Configura a fábrica de repositórios com a string de conexão e tipo de banco
        services.AddSingleton(new RepositoryConfig
        {
            ConnectionString = connectionString,
            DatabaseType = databaseType
        });
        // configura os serviços da camada de aplicação
        services.AddApplicationServices();
    }
}
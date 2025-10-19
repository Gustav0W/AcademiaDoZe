using AcademiaDoZe.Application.DependencyInjection;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Presentation.AppMaui.Message;
using CommunityToolkit.Mvvm.Messaging;

namespace AcademiaDoZe.Presentation.AppMaui.Configuration;

public static class ConfigurationHelper
{
    public static void ConfigureServices(IServiceCollection services)
    {
        var (connectionString, databaseType) = ReadDbPreferences();

        var repoConfig = new RepositoryConfig { ConnectionString = connectionString, DatabaseType = databaseType };
        
        services.AddSingleton(repoConfig);
        services.AddApplicationServices();

        WeakReferenceMessenger.Default.Register<RepositoryConfig, BancoPreferencesUpdatedMessage>(
            recipient: repoConfig, handler: static(recipient, message) =>
            {
                var (connectionString, databaseType) = ReadDbPreferences();
                recipient.ConnectionString = connectionString;
                recipient.DatabaseType = databaseType;
            }
            );
    }

    private static (string ConnectionString, EAppDatabaseType DatabaseType) ReadDbPreferences()
    {
        string dbServer = Preferences.Get("Servidor", "localhost");
        string dbDatabase = Preferences.Get("Banco", "db_academia_do_ze");
        string dbUser = Preferences.Get("Usuario", "sa");
        string dbSenha = Preferences.Get("Senha", "abcBolinhas12345");
        string dbComplemento = Preferences.Get("Complemento", "TrustServerCertificate=True;");

        string connectionString = $"Server={dbServer};Database={dbDatabase};User Id={dbUser};Password={dbSenha};{dbComplemento}";
        //var connectionString = "Server=localhost;Database=db_academia_do_ze;User Id=sa;Password=abcBolinhas12345;TrustServerCertificate=True;";


        var dbType = Preferences.Get("DatabaseType", EAppDatabaseType.SqlServer.ToString()) switch
        {
            "SqlServer" => EAppDatabaseType.SqlServer,
            "MySql" => EAppDatabaseType.MySql,
            _ => EAppDatabaseType.SqlServer
        };

        return (connectionString, dbType);
    }
}
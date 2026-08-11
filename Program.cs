using DatabaseSync.Helpers;
using DatabaseSync.Services;
using Microsoft.Extensions.Configuration;

try
{
    // to read the configuration settings
    IConfiguration configuration = 
        new ConfigurationBuilder()// Sources  - Empty list ([]) , Properties - Empty dictionary ({}) , Configuration Data - none
        .SetBasePath(
            AppDomain.CurrentDomain.BaseDirectory)
        .AddJsonFile(
            "appsettings.json",
            optional: false,//must exist
            reloadOnChange: true)
        .Build();

    string sqlConnection =
        configuration.GetConnectionString(
            "SqlServerConnection")!;

    string mariaConnection =
        configuration.GetConnectionString(
            "MariaDBConnection")!;

    Logger.WriteSeparator();

    Logger.Write(
        $"Process Started : {DateTime.Now:dd-MM-yyyy HH:mm:ss}");

    SqlServerService sql =
        new SqlServerService(
            sqlConnection);

    MariaDbService maria =
        new MariaDbService(
            mariaConnection);

    SchemaComparer schema =
        new SchemaComparer(
            sql,
            maria);

    DataSynchronizer sync =
        new DataSynchronizer(
            sql,
            maria,
            schema);


    sync.Start();

    Logger.Write(
        "Job Terminated");

    Logger.Write(
        $"Process Completed : {DateTime.Now:dd-MM-yyyy HH:mm:ss}");

    Logger.WriteSeparator();
}
catch (Exception ex)
{
    Logger.Write(
        $"ERROR : {ex.Message}");

    Logger.Write(
        "Job Terminated");

    Logger.Write(
        $"Process Completed : {DateTime.Now:dd-MM-yyyy HH:mm:ss}");

    Logger.WriteSeparator();
}
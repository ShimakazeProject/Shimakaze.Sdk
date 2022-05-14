using System.Data;
using System.Xml.Serialization;

using Shimakaze.Sdk.Utils;

namespace Shimakaze.Sdk.LanguageServer.Kernel;

internal sealed class DataBase
{
    public const string LanguageServerCacheFolderName = ".ShimakazeLS";
    public const string DataBaseFilePath = ".Shimakaze.Sdk.LS.db.xml";
    public const string DataBaseSchemaFilePath = ".Shimakaze.Sdk.LS.db.xsd";
    private static DataBase? s_instance;

    public static DataBase Instance { get => s_instance ?? throw new("DataBase are not Initialized!"); }

    public DataBase()
    {
        if (s_instance is not null)
            throw new("DataBase has been Initialized!");
        s_instance = this;
    }

    public DataSet KernelDatabase { get; } = new("Shimakaze.Sdk.LanguageServer.Cache");

    public async Task InitializeAsync()
    {
        string dbpath = Path.Combine(Environment.CurrentDirectory, LanguageServerCacheFolderName, DataBaseFilePath);
        if (File.Exists(dbpath))
        {
            await LoadCacheAsync();
        }
    }

    public Task SaveCacheAsync() => Task.Run(() =>
    {
        KernelDatabase.WriteXml(Path.Combine(Environment.CurrentDirectory, LanguageServerCacheFolderName, DataBaseFilePath));
        KernelDatabase.WriteXmlSchema(Path.Combine(Environment.CurrentDirectory, LanguageServerCacheFolderName, DataBaseSchemaFilePath));
    });

    public Task LoadCacheAsync() => Task.Run(() =>
    {
        KernelDatabase.ReadXml(Path.Combine(Environment.CurrentDirectory, LanguageServerCacheFolderName, DataBaseFilePath));
        KernelDatabase.ReadXmlSchema(Path.Combine(Environment.CurrentDirectory, LanguageServerCacheFolderName, DataBaseSchemaFilePath));
    });

}
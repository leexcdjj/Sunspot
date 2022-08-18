namespace Sunspot.EFCore;

/// <summary>
/// EF的常量
/// </summary>
public class EFConstants
{
    public const string Database = "Database";
    public const string DatabasePath = "/database/migration";

    internal static string MigrationAssemblyName { get; set; }
}
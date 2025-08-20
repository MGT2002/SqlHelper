// Ignore Spelling: Dri

using Microsoft.SqlServer.Management.Smo;

namespace SqlHelper.App;

public class OptionBuilder
{
    public static ScriptingOptions CreateDefaultDriAllOptions()
    {
        // --- Scripting options (similar to SSMS “Generate Scripts…/Advanced”) ---
        var opt = new ScriptingOptions
        {
            ScriptDrops = false,
            WithDependencies = false,          // include dependent/related objects
            Indexes = true,                   // script indexes
            DriAll = true,                    // PK, FK, CK, UQ
            FullTextIndexes = true,
            IncludeHeaders = true,
            Triggers = true,                  // table triggers
            ExtendedProperties = true,        // EPs

            IncludeIfNotExists = false,
            ScriptBatchTerminator = true,     // adds "GO"
            SchemaQualify = true,
            NoFileGroup = false,             // include filegroups if present
            ScriptDataCompression = true,
            Statistics = true,
            NoCollation = false,

            TargetServerVersion = SqlServerVersion.VersionLatest,
        };

        return opt;
    }
}

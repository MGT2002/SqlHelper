// Ignore Spelling: Scripter

namespace SqlHelper.Helpers;

public class AppSettings
{
    public ConnectionSettings Connection { get; set; } = new();
    public ScripterDataSettings ScripterData { get; set; } = new();

    public class ConnectionSettings
    {
        public string ServerName { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
    }

    public class ScripterDataSettings
    {
        public string SchemaName { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public string ColumnName { get; set; } = string.Empty;

        public InsertRandomDataInTableScripterSettings InsertRandomDataInTableScripter { get; set; } = new();

        public class InsertRandomDataInTableScripterSettings
        {
            public int RowCount { get; set; }
            public bool DisableConstraints { get; set; }
            public double NullProbability { get; set; }
        }
    }
}

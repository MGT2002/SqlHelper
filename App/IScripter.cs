// Ignore Spelling: Scripter

using Microsoft.SqlServer.Management.Smo;
using SqlHelper.Helpers;
using System.Collections.Specialized;

namespace SqlHelper.App;

public interface IScripter
{
    string OutputFileName { get; }

    static abstract IScripter Create(Server server, Database db, AppSettings appSettings);
    StringCollection Run();
}

using Microsoft.Extensions.Configuration;
using Microsoft.SqlServer.Management.Smo;
using System.Collections.Specialized;

namespace SqlHelper.App;

internal interface IScripter
{
    static abstract IScripter Create(Server server, Database db, IConfigurationRoot config);
    StringCollection Run();
}

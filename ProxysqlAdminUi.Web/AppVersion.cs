using System.Reflection;

namespace ProxysqlAdminUi.Web;
 
public class AppVersion
{
    public string Version = Assembly.GetExecutingAssembly()?
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
        .InformationalVersion ??  DateTime.Now.ToString("yyyy.MM.dd.hh:mm:ss");
}
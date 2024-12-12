using static System.Environment;
namespace ProxysqlAdminUi.Web;
 
public class AppVersion
{
    public string Version = !string.IsNullOrEmpty(GetEnvironmentVariable("BUILD_VERSION")) 
        ? $"{GetEnvironmentVariable("BUILD_VERSION")}-{GetEnvironmentVariable("BUILD_SUFFIX")}" 
        :  DateTime.Now.ToString("yyyy.MM.dd.hh:mm:ss");
}
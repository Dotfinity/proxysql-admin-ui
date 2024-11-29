namespace ProxysqlAdminUi.Web;

public class AppVersion
{
    public string Version = Environment.GetEnvironmentVariable("BUILD_VERSION") ?? DateTime.Now.ToString("yyyy.MM.dd-DEV");
}
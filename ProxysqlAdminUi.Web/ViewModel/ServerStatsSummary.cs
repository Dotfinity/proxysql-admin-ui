namespace ProxysqlAdminUi.Web.ViewModel;

public class ServerStatusSummary
{
    public int Online { get; set; }
    public int Offline { get; set; }
    public int Shunned { get; set; }
    public int Total => Online + Offline + Shunned;
}

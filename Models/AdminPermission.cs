namespace backend.Models;

public class AdminPermission
{
    public string Name { get; set; }  // Ex: "manage_users", "view_reports"
    public bool Allowed { get; set; }
}
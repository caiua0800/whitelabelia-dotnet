using backend.Models;

namespace backend.Models;

public class EnterpriseWithAdminResponse
{
    public Enterprise Enterprise { get; set; }
    public AdminCredentials AdminCredentials { get; set; }
}
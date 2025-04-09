
using backend.Models;
namespace backend.Models;
public class Purchase{

    public string purchaseId { get; set; }
    public double TotalPrice { get; set; }
    public DateTime DateCreated { get; set; }
    public double Comission { get; set; }
    public string ClientId { get; set; }
    
}
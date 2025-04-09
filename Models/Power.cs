using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

[ComplexType] 
public class Power
{
    public PowerState Sales { get; set; } = new PowerState();
    public PowerState Chats { get; set; } = new PowerState();
    public PowerState Users { get; set; } = new PowerState();
}

public class PowerState
{
    public bool View { get; set; } = false;
    public bool Edit { get; set; } = false;
}
namespace backend.Models;

public class Power{
    public PowerState Sales = new PowerState();
    public PowerState Chats = new PowerState();
    public PowerState Users = new PowerState();
}

public class PowerState{

    public bool View { get; set; }
    public bool Edit { get; set; }

    public PowerState(){
        View = false;
        Edit = false;
    }
}
namespace backend.Models
{
    public class ChangePasswordRequest
    {
        public string? CurrentPassword { get; set; } // Obrigatório apenas para auto-alteração
        public string NewPassword { get; set; }
    }
}
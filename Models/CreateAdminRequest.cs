using System.ComponentModel.DataAnnotations;

namespace backend.Models;

public class CreateAdminRequest
{
    [Required] public string Name { get; set; }
    [Required] public string LoginId { get; set; }
    [Required, MinLength(6)] public string Password { get; set; }
    [EmailAddress] public string? Email { get; set; }
    public List<AdminPermission>? Permissions { get; set; }
}
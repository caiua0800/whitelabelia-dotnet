// Admin.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

public class Admin
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Column("name")]
    public string Name { get; set; }

    [Required]
    [Column("login_id")]
    public string LoginId { get; set; }

    [Required]
    [Column("password")]
    public string Password { get; set; }

    [Required]
    [Column("enterprise_id")]
    public int EnterpriseId { get; set; }

    [Column("email")]
    public string? Email { get; set; }

    [Column("permissions", TypeName = "jsonb")]
    public List<AdminPermission> Permissions { get; set; } = new();

    [Column("refresh_token")]
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiryTime { get; set; }
}

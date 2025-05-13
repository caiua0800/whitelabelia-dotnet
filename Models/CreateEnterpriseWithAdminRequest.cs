using System.ComponentModel.DataAnnotations;

namespace backend.Models;

public class CreateEnterpriseWithAdminRequest
{
    [Required] public string EnterpriseName { get; set; }
    [Required] public string Cnpj { get; set; }
    [Required] public string Contact { get; set; }
    [Required, EmailAddress] public string Email { get; set; }
    [Required] public Address Address { get; set; }
    [Required] public string AdminName { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public string? Domain { get; set; }
}

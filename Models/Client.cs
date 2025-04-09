// Models/Client.cs
using System.ComponentModel.DataAnnotations;

namespace backend.Models;

public class Client
{
    [Required]
    public string Id { get; set; }  
    
    [Required]
    public string Name { get; set; }
    
    [Required]
    public string Contact { get; set; }
    
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}
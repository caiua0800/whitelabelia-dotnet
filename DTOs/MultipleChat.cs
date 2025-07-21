using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

public class MultipleChat
{
    [Required]
    [Column("name")]
    public string Name { get; set; }

    [Required]
    [Column("contact")]
    [RegularExpression(@"^\d+$", ErrorMessage = "O contato deve conter apenas números")]
    public string Contact { get; set; } 

    [Column("tags")]
    public List<int>? Tags { get; set; } 
}
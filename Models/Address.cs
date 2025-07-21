using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

public class Address
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("address_id")]
    public int? Id { get; set; } 
    
    [Column("street")]
    public string? Street { get; set; }
    [Column("number")]
    public string? Number { get; set; }
    [Column("complement")]
    public string? Complement { get; set; }
    [Column("neighborhood")]
    public string? Neighborhood { get; set; }
    [Column("city")]
    public string? City { get; set; }
    [Column("zipcode")]
    public string? Zipcode { get; set; }
    [Column("state")]
    public string? State { get; set; }
    [Column("country")]
    public string? Country { get; set; }

    [ForeignKey("EnterpriseId")]
    public int? EnterpriseId { get; set; } 
}


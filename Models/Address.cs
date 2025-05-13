using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

public class Address
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("address_id")]
    public int? Id { get; set; } 
    
    [Column("street")]
    public required string Street { get; set; }
    [Column("number")]
    public required string Number { get; set; }
    [Column("complement")]
    public string? Complement { get; set; }
    [Column("neighborhood")]
    public required string Neighborhood { get; set; }
    [Column("city")]
    public required string City { get; set; }
    [Column("zipcode")]
    public required string Zipcode { get; set; }
    [Column("state")]
    public required string State { get; set; }
    [Column("country")]
    public required string Country { get; set; }

    [ForeignKey("EnterpriseId")]
    public int? EnterpriseId { get; set; } 
}


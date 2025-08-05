using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using backend.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Models;

[Owned] // Adicione esta anotação para indicar que é um tipo owned
public class SignaturePix
{
    [Column("ticket_id")] // Nome da coluna no banco de dados
    public string? TicketId { get; set; }

    [Column("ticket_url")] // Nome da coluna no banco de dados
    public string? TicketUrl { get; set; }

    [Column("qr_code")] // Nome da coluna no banco de dados
    public string? QrCode { get; set; }
}
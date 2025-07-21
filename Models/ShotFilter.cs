using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend.Interfaces;

namespace backend.Models;

public class ShotFilter
{
    [Column("tag_filter_status")]
    public bool TagFilterStatus { get; set; }
    [Column("tag_filter")]
    public List<int>? TagFilter { get; set; }

    [Column("type_filter_status")]
    public bool TypeFilterStatus { get; set; }

    [Column("type_filter")]
    public int? TypeFilter { get; set; }

    [Column("selected_clients_status")]
    public bool SelectedClientsStatus { get; set; }

    [Column("selected_clients")]
    public List<string>? SelectedClients { get; set; }
}
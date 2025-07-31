using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

public class ShotWithMessageModelDto
{
    [Column("shot")]
    public Shot Shot { get; set; }

    [Column("header_text")]
    public string? HeaderText { get; set; }

    [Column("body_text")]
    public string? BodyText { get; set; }

    public ShotWithMessageModelDto()
    {
    }

    public ShotWithMessageModelDto(Shot shot, string headerText, string bodyText)
    {
        Shot = shot;
        HeaderText = headerText;
        BodyText = bodyText;
    }
}
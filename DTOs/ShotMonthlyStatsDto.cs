namespace backend.DTOs;

public class ShotMonthlyStatsDto
{
    public int Month { get; set; }
    public int Year { get; set; }
    public int TotalShots { get; set; }
    public int TotalShotsAllTime { get; set; }
    public int UniqueClientsReached { get; set; }
    public int UniqueClientsAllTime { get; set; }
    public int? AvaliableShots { get; set; }

}
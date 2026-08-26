namespace SmartLibrary.Api.DTOs;

public class DashboardDto
{
    public int CurrentOccupancy { get; set; }
    public int MaxCapacity { get; set; }
    public int AvailableSeats { get; set; }
    public double OccupancyPercentage { get; set; }
    public string LibraryStatus { get; set; } = string.Empty; // "Space Available" | "Almost Full" | "Library Full"
}

public class UpdateCapacityDto
{
    public int MaxCapacity { get; set; }
}

using FileMonitoring.Domain.Enums;

namespace FileMonitoring.Domain.Entities;

public class ProcessedFile
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = null!;
    public string Company { get; set; } = null!;
    public DateTime ProcessingDate { get; set; }
    public DateTime? InitialPeriod { get; set; }
    public DateTime? FinalPeriod { get; set; }
    public string Sequence { get; set; } = null!;
    public FileStatus Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

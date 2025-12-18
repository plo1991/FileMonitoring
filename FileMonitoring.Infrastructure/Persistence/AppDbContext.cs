using FileMonitoring.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FileMonitoring.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<ProcessedFile> ProcessedFiles { get; set; } = null!;

    public async Task AddProcessedFileAsync(ProcessedFile file)
    {
        await ProcessedFiles.AddAsync(file);
    }
}
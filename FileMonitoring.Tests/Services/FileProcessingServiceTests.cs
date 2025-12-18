using FileMonitoring.Application.Services;
using FileMonitoring.Infrastructure.Persistence;
using FileMonitoring.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using System.Text;

namespace FileMonitoring.Tests.Services;

public class FileProcessingServiceTests
{
    private AppDbContext GetContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task ProcessAsync_ValidUfCard_ShouldSaveAsRecepcionado()
    {
        // Arrange
        using var context = GetContext();
        var service = new FileProcessingService(context);
        // Padding with spaces to match the expected format
        var content = "0          20190626        20190625        20190625       0000001UfCard  ";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        // Act
        await service.ProcessAsync(stream, "ufcard.txt");

        // Assert
        var result = await context.ProcessedFiles.FirstAsync();
        result.Status.Should().Be(FileStatus.Recepcionado);
        result.Company.Should().Be("UfCard");
        result.Sequence.Should().Be("0000001");
        result.FileName.Should().Be("ufcard.txt");
    }

    [Fact]
    public async Task ProcessAsync_ValidFagammonCard_ShouldSaveAsRecepcionado()
    {
        // Arrange
        using var context = GetContext();
        var service = new FileProcessingService(context);
        var content = "12019052632165487FagammonCard0002451";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        // Act
        await service.ProcessAsync(stream, "fagammon.txt");

        // Assert
        var result = await context.ProcessedFiles.FirstAsync();
        result.Status.Should().Be(FileStatus.Recepcionado);
        result.Company.Should().Be("FagammonCard");
        result.Sequence.Should().Be("0002451");
        result.FileName.Should().Be("fagammon.txt");
    }

    [Fact]
    public async Task ProcessAsync_InvalidContent_ShouldSaveAsNaoRecepcionado()
    {
        // Arrange
        using var context = GetContext();
        var service = new FileProcessingService(context);
        var content = "INVALID_CONTENT_TOO_SHORT";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        // Act
        await service.ProcessAsync(stream, "invalid.txt");

        // Assert
        var result = await context.ProcessedFiles.FirstAsync();
        result.Status.Should().Be(FileStatus.NaoRecepcionado);
        result.FileName.Should().Be("invalid.txt");
    }
}

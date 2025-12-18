using System.Reflection;
using System.Text;
using FileMonitoring.API.Controllers;
using FileMonitoring.Application.Interfaces;
using FileMonitoring.Domain.Entities;
using FileMonitoring.Domain.Enums;
using FileMonitoring.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace FileMonitoring.Tests.Controllers
{
    public class FileControllerTests
    {
        private AppDbContext GetContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new AppDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        private class StubFileProcessingService : IFileProcessingService
        {
            public bool Called { get; private set; }
            public string? LastFileName { get; private set; }

            public Task ProcessAsync(Stream fileStream, string fileName)
            {
                Called = true;
                LastFileName = fileName;
                return Task.CompletedTask;
            }
        }

        [Fact]
        public async Task Upload_Should_Process_And_ClearCache()
        {
            // Arrange
            var stub = new StubFileProcessingService();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var controller = new FileController(stub, cache);

            var content = "dummy content";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            stream.Position = 0;
            IFormFile file = new FormFile(stream, 0, stream.Length, "file", "test.txt");

            // Put something in cache to ensure it gets removed
            cache.Set("ProcessedFilesList", new List<ProcessedFile> { new ProcessedFile { FileName = "x" } });

            // Act
            var actionResult = await controller.Upload(file);

            // Assert
            actionResult.Should().BeOfType<OkResult>();
            stub.Called.Should().BeTrue();
            cache.TryGetValue("ProcessedFilesList", out var cached).Should().BeFalse();
        }

        [Fact]
        public async Task GetAll_Should_Return_ProcessedFiles_And_PopulateCache()
        {
            // Arrange
            using var context = GetContext();
            var file = new ProcessedFile
            {
                FileName = "a.txt",
                Company = "TestCo",
                Sequence = "0001",
                Status = FileStatus.Recepcionado,
                ProcessingDate = DateTime.UtcNow
            };
            context.ProcessedFiles.Add(file);
            await context.SaveChangesAsync();

            var stub = new StubFileProcessingService();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var controller = new FileController(stub, cache);

            // Act
            var actionResult = await controller.GetAll(context);

            // Assert
            var ok = actionResult as OkObjectResult;
            ok.Should().NotBeNull();
            var list = ok!.Value as List<ProcessedFile>;
            list.Should().NotBeNull();
            list!.Should().HaveCount(1);
            list[0].FileName.Should().Be("a.txt");

            // Cache should be populated
            cache.TryGetValue("ProcessedFilesList", out List<ProcessedFile>? cachedList).Should().BeTrue();
            cachedList.Should().NotBeNull();
            cachedList!.Should().HaveCount(1);
        }

        [Fact]
        public async Task Summary_Should_Return_Correct_Counts()
        {
            // Arrange
            using var context = GetContext();
            context.ProcessedFiles.Add(new ProcessedFile { FileName = "r.txt", Status = FileStatus.Recepcionado, ProcessingDate = DateTime.UtcNow, Company = "A", Sequence = "1" });
            context.ProcessedFiles.Add(new ProcessedFile { FileName = "n.txt", Status = FileStatus.NaoRecepcionado, ProcessingDate = DateTime.UtcNow, Company = "B", Sequence = "2" });
            await context.SaveChangesAsync();

            var stub = new StubFileProcessingService();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var controller = new FileController(stub, cache);

            // Act
            var actionResult = await controller.Summary(context);

            // Assert
            var ok = actionResult as OkObjectResult;
            ok.Should().NotBeNull();
            var obj = ok!.Value!;
            var type = obj.GetType();
            int received = (int)type.GetProperty("received")!.GetValue(obj)!;
            int notReceived = (int)type.GetProperty("notReceived")!.GetValue(obj)!;

            received.Should().Be(1);
            notReceived.Should().Be(1);
        }
    }
}

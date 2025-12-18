using Microsoft.AspNetCore.Mvc;
using FileMonitoring.Application.Interfaces;
using FileMonitoring.Infrastructure.Persistence;
using Microsoft.Extensions.Caching.Memory;
using FileMonitoring.Domain.Enums;

using FileMonitoring.Domain.Entities;

namespace FileMonitoring.API.Controllers
{
    [ApiController]
    [Route("api/files")]
    public class FileController : ControllerBase
    {
        private readonly IFileProcessingService _service;
        private readonly IMemoryCache _cache;
        private const string CacheKey = "ProcessedFilesList";

        public FileController(IFileProcessingService service, IMemoryCache cache)
        {
            _service = service;
            _cache = cache;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            await _service.ProcessAsync(file.OpenReadStream(), file.FileName);
            _cache.Remove(CacheKey);
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromServices] AppDbContext context)
        {
            if (!_cache.TryGetValue(CacheKey, out List<ProcessedFile>? files))
            {
                files = context.ProcessedFiles
                    .OrderByDescending(x => x.CreatedAt)
                    .ToList();

                _cache.Set(CacheKey, files, TimeSpan.FromMinutes(1));
            }

            return Ok(files);
        }

        [HttpGet("summary")]
        public async Task<IActionResult> Summary([FromServices] AppDbContext context)
        {
            // Summary is cheap enough or we can cache it too?
            // For now, let's keep it direct or cache it separately.
            // Let's cache it as well for consistency if we wanted, but requierment just says "Implementação de cache".
            var received = context.ProcessedFiles.Count(x => x.Status == FileStatus.Recepcionado);
            var notReceived = context.ProcessedFiles.Count(x => x.Status == FileStatus.NaoRecepcionado);

            return Ok(new { received, notReceived });
        }
    }

}

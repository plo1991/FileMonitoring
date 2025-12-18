using System.Globalization;
using FileMonitoring.Application.Interfaces;
using FileMonitoring.Domain.Entities;
using FileMonitoring.Domain.Enums;
using FileMonitoring.Infrastructure.Persistence;

namespace FileMonitoring.Application.Services;

public class FileProcessingService : IFileProcessingService
{
    private readonly AppDbContext _context;

    public FileProcessingService(AppDbContext context)
    {
        _context = context;
    }

    public async Task ProcessAsync(Stream fileStream, string fileName)
    {
        // Backup
        var backupDir = Path.Combine(Directory.GetCurrentDirectory(), "Backup");
        Directory.CreateDirectory(backupDir);
        var backupPath = Path.Combine(backupDir, $"{DateTime.UtcNow:yyyyMMddHHmmss}_{fileName}");
        
        using (var fileDest = File.Create(backupPath))
        {
            await fileStream.CopyToAsync(fileDest);
        }
        
        if (fileStream.CanSeek)
            fileStream.Position = 0;

        try
        {
            using var reader = new StreamReader(fileStream);
            var line = await reader.ReadLineAsync();

            if (string.IsNullOrWhiteSpace(line))
                throw new Exception("Arquivo vazio");

            var tipoRegistro = line.Substring(0, 1);

            var entity = tipoRegistro switch
            {
                "0" => ParseUfCard(line),
                "1" => ParseFagammonCard(line),
                _ => throw new Exception("Tipo de registro inválido")
            };

            entity.FileName = fileName;
            entity.Status = FileStatus.Recepcionado;

            await _context.AddProcessedFileAsync(entity);
            await _context.SaveChangesAsync();
        }
        catch
        {
            await _context.AddProcessedFileAsync(new ProcessedFile
            {
                FileName = fileName,
                Status = FileStatus.NaoRecepcionado,
                Company = "Erro",
                Sequence = "0"
            });

            await _context.SaveChangesAsync();
        }
    }

    private ProcessedFile ParseUfCard(string line)
    {
        return new ProcessedFile
        {
            Company = line.Substring(65, 8).Trim(),
            ProcessingDate = DateTime.ParseExact(line.Substring(11, 8), "yyyyMMdd", null),
            InitialPeriod = DateTime.ParseExact(line.Substring(27, 8), "yyyyMMdd", null),
            FinalPeriod = DateTime.ParseExact(line.Substring(43, 8), "yyyyMMdd", null),
            Sequence = line.Substring(58, 7)
        };
    }

    private ProcessedFile ParseFagammonCard(string line)
    {
        return new ProcessedFile
        {
            Company = line.Substring(17, 12).Trim(),
            ProcessingDate = DateTime.ParseExact(line.Substring(1, 8), "yyyyMMdd", null),
            Sequence = line.Substring(29, 7)
        };
    }
}
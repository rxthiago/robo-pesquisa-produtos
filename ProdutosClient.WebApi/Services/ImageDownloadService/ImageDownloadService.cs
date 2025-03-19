namespace ProdutosClient.WebApi.Services.ImageDownloadService;

public class ImageDownloadService
{
    private readonly ILogger<ImageDownloadService> _logger;

    public ImageDownloadService(ILogger<ImageDownloadService> logger)
    {
        _logger = logger;
    }

    public async Task<string> DownloadImageAsync(string imageUrl, string outputFolder, string ean)
    {
        try
        {
            using HttpClient client = new();
            var imageBytes = await client.GetByteArrayAsync(imageUrl);

            // Obtém a extensão do arquivo a partir da URL
            string fileExtension = Path.GetExtension(new Uri(imageUrl).AbsolutePath);
            if (string.IsNullOrEmpty(fileExtension))
            {
                fileExtension = ".jpg"; // Define um padrão caso não consiga obter a extensão
            }

            // Nome do arquivo usando o EAN
            string fileName = $"{ean}{fileExtension}";
            string filePath = Path.Combine(outputFolder, fileName);

            await File.WriteAllBytesAsync(filePath, imageBytes);

            return filePath;

            /*string fileName = Path.GetFileName(new Uri(imageUrl).AbsolutePath);
            string filePath = Path.Combine(outputFolder, fileName);

            await File.WriteAllBytesAsync(filePath, imageBytes);

            return filePath;*/
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao baixar a imagem {ImageUrl}", imageUrl);
            return string.Empty;
        }
    }
}
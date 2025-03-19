namespace ProdutosClient.WebApi.Services.ImageDownload;

public class ImageDownloadService(ILogger<ImageDownloadService> logger)
{
    public async Task<string> DownloadImageAsync(string imageUrl, string outputFolder, string ean, int indice = 0)
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
            string fileName = $"{ean}{(indice > 0 ? "_" + indice.ToString() : "")}{fileExtension}";
            string filePath = Path.Combine(outputFolder, fileName);

            await File.WriteAllBytesAsync(filePath, imageBytes);

            return filePath;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao baixar a imagem {ImageUrl}", imageUrl);
            return string.Empty;
        }
    }
}
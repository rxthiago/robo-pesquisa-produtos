namespace ProdutosClient.WebApi.Services.FileImportService
{
    public class CsvImportService
    {

        public async Task<IEnumerable<string>> LerEans(Stream arquivo)
        {
            using var reader = new StreamReader(arquivo);
            var eans = new List<string>();

            while(!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                var values = line?.Split(';') ?? [];

                eans.Add(values[0]);
            }

            return eans;
        }

    }
}

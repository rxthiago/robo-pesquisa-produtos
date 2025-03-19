namespace ProdutosClient.WebApi.Services.ExportacaoPlanilha
{
    public class SpreadsheetColumnInfo<T>
    {
        public string DisplayName { get; set; } = null!;

        public Func<T, string> Value { get; set; } = null!;

    }
}

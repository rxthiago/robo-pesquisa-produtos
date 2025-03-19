namespace ProdutosClient.WebApi.Services.ExportacaoPlanilha
{
    public static class SpreadsheetBuilderExtensions
    {
        public static SpreadsheetSchemaBuilder<T> WithColumn<T>(this SpreadsheetSchemaBuilder<T> instance, string columnName, Func<T, string> valueFactory)
        {
            instance.Columns.Add(new SpreadsheetColumnInfo<T>()
            {
                DisplayName = columnName,
                Value = valueFactory
            });

            return instance;
        }
    }
}

namespace ProdutosClient.WebApi.Services.ExportacaoPlanilha
{
    public static class SpreadsheetBuilderExtensions
    {
        public static SpreadsheetSchemaBuilder<T> WithColumn<T>(this SpreadsheetSchemaBuilder<T> instance, string columnName, Func<T, string> valueFactory, bool numberAsText = false)
        {
            instance.Columns.Add(new SpreadsheetColumnInfo<T>()
            {
                DisplayName = columnName,
                Value = valueFactory,
                NumberAsText = numberAsText
            });

            return instance;
        }
    }
}

using Spire.Xls;

namespace ProdutosClient.WebApi.Services.ExportacaoPlanilha
{
    public class SpreadsheetSchemaBuilder<T>
    {

        public SpreadsheetSchemaBuilder(Workbook workbook, string name = "")
        {
            Workbook = workbook;

            if (string.IsNullOrEmpty(name))
            {
                name = nameof(T);
            }

            Name = name;
        }

        public Workbook Workbook { get; private set; }

        public string Name { get; private set; }

        public List<SpreadsheetColumnInfo<T>> Columns = [];

    }
}

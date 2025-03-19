using Spire.Xls;
using Spire.Xls.Core;

namespace ProdutosClient.WebApi.Services.ExportacaoPlanilha
{
    public class ExportacaoPlanilhaService
    {
        public IWorksheet CreateWorksheet<T>(IEnumerable<T> data, SpreadsheetSchemaBuilder<T> schemaBuilder)
        {
            // Montar planilha
            using Worksheet worksheet = schemaBuilder.Workbook.CreateEmptySheet();
            worksheet.Name = schemaBuilder.Name;

            for (int columnIndex = 0; columnIndex < schemaBuilder.Columns.Count; columnIndex++)
            {
                worksheet[1, columnIndex + 1].Value2 = schemaBuilder.Columns[columnIndex].DisplayName;
            }

            worksheet.Range[1, 1, 1, schemaBuilder.Columns.Count].Style.Font.IsBold = true;

            var row = 2;

            foreach (var item in data)
            {
                for (int columnIndex = 0; columnIndex < schemaBuilder.Columns.Count; columnIndex++)
                {
                    worksheet[row, columnIndex + 1].Value2 = schemaBuilder.Columns[columnIndex].Value(item);
                }

                row++;
            }

            worksheet.AllocatedRange.AutoFitColumns();
            worksheet.AllocatedRange.BorderAround();
            worksheet.AllocatedRange.BorderInside();

            return worksheet;
        }
    }
}



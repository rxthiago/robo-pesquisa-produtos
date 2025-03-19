using ProdutosClient.WebApi.Dtos;
using ProdutosClient.WebApi.Enums;
using ProdutosClient.WebApi.Factories;
using ProdutosClient.WebApi.Services.ExportacaoPlanilha;
using ProdutosClient.WebApi.Services.FileImportService;
using Spire.Xls;

namespace ProdutosClient.WebApi.Services.ImportacaoListaService
{
    public class ImportacaoListaService(ILogger<ImportacaoListaService> logger,
                                        SiteFactory siteFactory,
                                        CsvImportService csvImportService,
                                        ExportacaoPlanilhaService exportacaoPlanilhaService,
                                        ImageDownloadService imageDownloadService
                                        )
    {

        public async Task<bool> ImportarProdutosPorEan(FileUploadDto file, EnumSite site)
        {
            try
            {
                if (file.File == null)
                {
                    throw new Exception("Nenhum arquivo foi enviado.");
                }

                var sitePesquisa = siteFactory.GetSite(site);

                using var arquivoCsv = new MemoryStream();

                await file?.File?.CopyToAsync(arquivoCsv);
                arquivoCsv.Position = 0;

                var eans = await csvImportService.LerEans(arquivoCsv);

                List<ProdutoDto> produtos = new List<ProdutoDto>();

                foreach (var ean in eans)
                {
                    logger.LogInformation("Buscando dados do produto {Ean}", ean);

                    var produto = await sitePesquisa.PesquisarProdutoPorEan(ean);

                    if (produto == null)
                    {
                        logger.LogInformation("Produto {Ean} não encontrado", ean);
                        continue;
                    }

                    produtos.Add(produto);
                }

                var planilha = new Workbook();
                planilha.Worksheets.Clear();

                var schemaTabela = new SpreadsheetSchemaBuilder<ProdutoDto>(planilha, "Produtos").WithColumn("Ean", p => p.Ean)
                                                                                                 .WithColumn("Site", p => p.Origem.ToString())
                                                                                                 .WithColumn("Nome", p => p.Nome.ToString())
                                                                                                 .WithColumn("Descrição", p => p.Descricao.ToString())
                                                                                                 .WithColumn("Categoria", p => p.Categoria.ToString())
                                                                                                 .WithColumn("Departamento", p => p.Departamento.ToString())
                                                                                                 .WithColumn("Preco", p => p.Preco.ToString());

                // Exportar para Excel
                exportacaoPlanilhaService.CreateWorksheet(produtos, schemaTabela);

                var pastaExportacao = Path.Combine(Environment.CurrentDirectory, "Exportacao");
                var arquivoExportacao = Path.Combine(pastaExportacao, $"Exportacao_{DateTime.Now:dd-MM-yyyy-HH-mm}.xlsx");

                if (!Directory.Exists(pastaExportacao))
                {
                    Directory.CreateDirectory(pastaExportacao);
                }

                planilha.SaveToFile(arquivoExportacao, ExcelVersion.Version2016);

                // Baixar imagens
                var pastaImagens = Path.Combine(Environment.CurrentDirectory, "Exportacao", "Imagens");

                if (!Directory.Exists(pastaImagens))
                {
                    Directory.CreateDirectory(pastaImagens);
                }

                List<string> errosDownload = [];

                foreach (var produto in produtos)
                {
                    foreach (var urlImagem in produto.UrlImagens)
                    {
                        var caminhoImagem = await imageDownloadService.DownloadImageAsync(urlImagem, pastaImagens, produto.Ean);
                        if (!string.IsNullOrEmpty(caminhoImagem))
                        {
                            logger.LogInformation("Imagem {UrlImagem} baixada com sucesso", urlImagem);
                        }
                        else
                        {
                            logger.LogError("Erro ao baixar imagem do Ean:{Ean}", produto.Ean);
                            errosDownload.Add(produto.Ean);
                        }
                    }
                }

                if (errosDownload.Any())
                {
                    var pastaLogs = Path.Combine(Environment.CurrentDirectory, "Exportacao", "Logs");

                    if (!Directory.Exists(pastaLogs))
                    {
                        Directory.CreateDirectory(pastaLogs);
                    }

                    var nomeArquivoLog = Path.Combine(pastaLogs, $"log_erros_download_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt");
                    await File.WriteAllLinesAsync(nomeArquivoLog, errosDownload);

                    logger.LogError("Arquivo de log de erros criado: {LogFilePath}", nomeArquivoLog);
                }

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao pesquisar produto por EAN");

                throw;
            }
        }

    }
}

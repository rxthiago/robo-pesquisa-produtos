using ProdutosClient.WebApi.Dtos;
using ProdutosClient.WebApi.Enums;
using ProdutosClient.WebApi.Factories;
using ProdutosClient.WebApi.Repositories;
using ProdutosClient.WebApi.Services.ExportacaoPlanilha;
using ProdutosClient.WebApi.Services.FileImport;
using ProdutosClient.WebApi.Services.ImageDownload;
using Spire.Xls;
using System.Collections.Concurrent;
using System.Threading.RateLimiting;

namespace ProdutosClient.WebApi.Services.ImportacaoLista
{
    public class ImportacaoListaService(ILogger<ImportacaoListaService> logger,
                                        IConfiguration configuration,
                                        SiteFactory siteFactory,
                                        CsvImportService csvImportService,
                                        ExportacaoPlanilhaService exportacaoPlanilhaService,
                                        ImageDownloadService imageDownloadService,
                                        ProdutosRepository produtosRepository
                                        )
    {

        private int DownloadThreads => int.TryParse(configuration["DownloadThreads"], out int _downloadThreads) ? _downloadThreads : 4;
        private int HorasExpiracaoCache => int.TryParse(configuration["HorasExpiracaoCache"], out int _horasExpiracaoCache) ? _horasExpiracaoCache : 24;

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

                List<ProdutoDto> produtos = [];

                foreach (var ean in eans)
                {
                    logger.LogInformation("Buscando dados do produto {Ean}", ean);

                    // Buscar no cache
                    var produto = produtosRepository.Find(ean, site);
                    var atualizarCache = false;

                    if(produto == null || 
                        (DateTime.Now - (produto?.DataCatalogo ?? DateTime.MinValue)).TotalHours > HorasExpiracaoCache)
                    {
                        produto = await sitePesquisa.PesquisarProdutoPorEan(ean);
                        atualizarCache = true;
                    }

                    if (produto == null)
                    {
                        logger.LogInformation("Produto {Ean} não encontrado", ean);

                        produto = new ProdutoDto()
                        {
                            Ean = ean,
                            Origem = site,
                            ProdutoExiste = false
                        };

                        produtos.Add(produto);
                        produtosRepository.InsertOrUpdate(produto);

                        continue;
                    }

                    if(atualizarCache)
                    {
                        produtosRepository.InsertOrUpdate(produto);
                    }

                    produtos.Add(produto);
                }

                var produtosValidos = produtos.Where(p => p.ProdutoExiste)
                                              .ToList();

                var planilha = new Workbook();
                planilha.Worksheets.Clear();

                var schemaTabela = new SpreadsheetSchemaBuilder<ProdutoDto>(planilha, "Produtos").WithColumn("Ean", p => p.Ean, true)
                                                                                                 .WithColumn("Site", p => p.Origem.ToString())
                                                                                                 .WithColumn("Nome", p => p.Nome.ToString())
                                                                                                 .WithColumn("Descrição", p => p.Descricao.ToString())
                                                                                                 .WithColumn("Categoria", p => p.Categoria.ToString())
                                                                                                 .WithColumn("Departamento", p => p.Departamento.ToString())
                                                                                                 .WithColumn("Preco", p => p.Preco.ToString());

                // Exportar para Excel
                exportacaoPlanilhaService.CreateWorksheet(produtosValidos, schemaTabela);

                var pastaExportacao = Path.Combine(Environment.CurrentDirectory, "Exportacao");
                var arquivoExportacao = Path.Combine(pastaExportacao, $"Exportacao_{site}_{DateTime.Now:dd-MM-yyyy-HH-mm}.xlsx");

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

                ConcurrentBag<string> errosDownload = [];

                var imagensProdutos = produtosValidos.SelectMany((p, i) => p.UrlImagens.Select((url, index) => new ImagemProdutoDto()
                {
                    Ean = p.Ean,
                    Indice = index + 1,
                    UrlImagem = url
                }));

                var rateLimiter = new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 30,                 //  máximo de requisições 
                    Window = TimeSpan.FromMinutes(1),  // janela de 1 minuto
                    QueueLimit = 0,                    // sem requisições extras na fila
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                });

                // Baixar imagens em múltiplos Threads

                await Parallel.ForEachAsync(imagensProdutos, new ParallelOptions()
                {
                    MaxDegreeOfParallelism = DownloadThreads
                }, async (imagemProd, cts) =>
                {
                    await  rateLimiter.AcquireAsync(1);

                    var caminhoImagem = await imageDownloadService.DownloadImageAsync(imagemProd.UrlImagem, pastaImagens, imagemProd.Ean, imagemProd.Indice);
                    
                    if (!string.IsNullOrEmpty(caminhoImagem))
                    {
                        logger.LogInformation("Imagem {UrlImagem} baixada com sucesso", imagemProd.UrlImagem);
                    }
                    else
                    {
                        logger.LogError("Erro ao baixar imagem do Ean: {Ean}", imagemProd.Ean);

                        errosDownload.Add(imagemProd.Ean);
                    }
                });

                if (!errosDownload.IsEmpty)
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

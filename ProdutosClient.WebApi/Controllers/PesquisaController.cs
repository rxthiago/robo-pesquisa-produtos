using Microsoft.AspNetCore.Mvc;
using ProdutosClient.WebApi.Dtos;
using ProdutosClient.WebApi.Enums;
using ProdutosClient.WebApi.Factories;
using ProdutosClient.WebApi.Services.ImportacaoListaService;

namespace ProdutosClient.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class PesquisaController(ILogger<PesquisaController> logger,
                                SiteFactory siteFactory,
                                ImportacaoListaService importacaoListaService) : ControllerBase
{
    [HttpGet]
    [Route("pesquisar-por-ean")]
    public async Task<ActionResult> PesquisarProdutoPorEan(string ean, EnumSite site = EnumSite.Pacheco)
    {
        try
        {
            var sitePesquisa = siteFactory.GetSite(site);

            var produto = await sitePesquisa.PesquisarProdutoPorEan(ean);

            if (produto == null)
            {
                logger.LogError("Produto não encontrado:{Ean}", ean);
                return Ok(new ResultDto<ProdutoDto>()
                {
                    Sucesso = false,
                    Mensagem = "Produto não encontrado"
                });
            }

            return Ok(new ResultDto<ProdutoDto>()
            {
                Sucesso = true,
                Dados = produto
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao pesquisar produto por EAN");

            return Ok(new ResultDto<ProdutoDto>()
            {
                Sucesso = false,
                Mensagem = "Erro ao pesquisar produto por EAN. Verifique os logs para mais detalhes."
            });
        }
    }

    [HttpPost]
    [Route("importar-eans")]
    public async Task<ActionResult> PesquisarProdutoPorEan(FileUploadDto data, EnumSite site = EnumSite.Pacheco)
    {
        try
        {
            _ = await importacaoListaService.ImportarProdutosPorEan(data, site);

            return Ok(new ResultDto<ProdutoDto>()
            {
                Sucesso = true,
                Mensagem = "Lista importada com sucesso."
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao pesquisar produto por EAN");

            return Ok(new ResultDto<ProdutoDto>()
            {
                Sucesso = false,
                Mensagem = $"Ocorreu um erro ao importar a lista de produtos. Verifique os logs para mais detalhes. Mensagem de erro: {ex.Message}"
            });
        }
    }

}

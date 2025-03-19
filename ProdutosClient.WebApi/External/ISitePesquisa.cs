using ProdutosClient.WebApi.Dtos;
using ProdutosClient.WebApi.Enums;

namespace ProdutosClient.WebApi.External
{
    public interface ISitePesquisa
    {
        public EnumSite Site { get; }
        public Task<ProdutoDto?> PesquisarProdutoPorEan(string ean);
    }
}

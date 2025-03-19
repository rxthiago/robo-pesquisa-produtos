using ProdutosClient.WebApi.Enums;

namespace ProdutosClient.WebApi.Dtos
{
    public class ProdutoDto
    {
        public int IdProduto { get; set; }
        public string Ean { get; set; } = null!;
        public EnumSite Origem { get; set; }
        public string Url { get; set; } = null!;
        public IEnumerable<string> UrlImagens { get; set; } = null!;
        public string Nome { get; set; } = null!;
        public string Descricao { get; set; } = null!;
        public string Categoria { get; set; } = null!;
        public string Departamento { get; set; } = null!;
        public decimal Preco { get; set; }
    }
}

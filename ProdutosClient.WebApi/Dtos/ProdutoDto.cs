using LiteDB;
using ProdutosClient.WebApi.Enums;

namespace ProdutosClient.WebApi.Dtos
{
    public class ProdutoDto
    {
        [BsonId]
        public string Id => string.Concat(Ean, "-", Origem);
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
        public DateTime DataCatalogo { get; set; } = DateTime.Now;
        public bool ProdutoExiste { get; set; } = true;

        public static bool ShouldSerializeId()
        {
            return false;
        }
    }
}

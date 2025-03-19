namespace ProdutosClient.WebApi.Dtos
{
    public class ImagemProdutoDto
    {
        public string Ean { get; set; } = null!;
        public string UrlImagem { get; set; } = null!;
        public int Indice { get; set; }
    }
}

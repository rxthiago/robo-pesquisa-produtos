namespace ProdutosClient.WebApi.Dtos
{
    public class ResultDto<T>
    {
        public T? Dados { get; set; }
        public bool Sucesso { get; set; }
        public string Mensagem { get; set; } = null!;
    }
}

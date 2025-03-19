using LiteDB;
using ProdutosClient.WebApi.Dtos;
using ProdutosClient.WebApi.Enums;

namespace ProdutosClient.WebApi.Repositories
{
    public class ProdutosRepository
    {

        private readonly LiteDatabase _database;

        private readonly ILiteCollection<ProdutoDto> _produtos;

        private readonly string DatabasePath = Path.Combine(Environment.CurrentDirectory, "produtos.db");

        public ProdutosRepository() {

            _database = new LiteDatabase(DatabasePath);

            _produtos = _database.GetCollection<ProdutoDto>();
            _produtos.EnsureIndex(p => p.Ean);
            _produtos.EnsureIndex(p => new { p.Ean, p.Origem });
        }

        public ProdutoDto InsertOrUpdate(ProdutoDto produto)
        {
            _produtos.Upsert(produto);
            
            return produto;
        }

        public ProdutoDto? Find(string ean, EnumSite origem)
        {
            return _produtos.FindOne(p => p.Ean == ean && p.Origem == origem);
        }

    }
}

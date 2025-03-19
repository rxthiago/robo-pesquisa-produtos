using ProdutosClient.WebApi.Enums;
using ProdutosClient.WebApi.Exceptions;
using ProdutosClient.WebApi.External;
using ProdutosClient.WebApi.External.Sites.Pacheco;

namespace ProdutosClient.WebApi.Factories
{
    public class SiteFactory(IServiceProvider services)
    {
        public ISitePesquisa GetSite(EnumSite site)
        {
            if (site == EnumSite.Pacheco) return services.GetService<Pacheco>();

            throw new SiteNaoImplementadoException();
        }

    }
}

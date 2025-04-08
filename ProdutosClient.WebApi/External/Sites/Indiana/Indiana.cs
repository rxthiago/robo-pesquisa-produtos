using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using ProdutosClient.WebApi.Dtos;
using ProdutosClient.WebApi.Enums;

namespace ProdutosClient.WebApi.External.Sites.Indiana
{
    public class Indiana : ISitePesquisa, IDisposable
    {
        public EnumSite Site => EnumSite.Indiana;

        private static readonly string UrlBasePesquisa = "https://www.farmaciaindiana.com.br/{0}?_q={0}&map=ft";

        private readonly ChromeDriver driver;

        public Indiana()
        {
            var options = new ChromeOptions();
            options.AddArgument("--remote-allow-origins=*");
            options.AddArgument("headless");

            driver = new(options);
        }

        ~Indiana()
        {
            driver.Quit();
        }

        public async Task<ProdutoDto?> PesquisarProdutoPorEan(string ean)
        {
            try
            {
                string urlPesquisa = UrlBasePesquisa + ean;
                await driver.Navigate().GoToUrlAsync(urlPesquisa);

                var elementoNomeProduto = driver.FindElement(By.CssSelector(".vtex-store-components-3-x-productBrand"));
                if (elementoNomeProduto == null)
                    return default;

                var elementoDescricao = driver.FindElement(By.CssSelector("p[style=\"text-align: justify;\"]"));
                var elementoPrecoInteiro = driver.FindElement(By.CssSelector(".vtex-product-price-1-x-currencyContainer"));
                var elementoImagem = driver.FindElement(By.CssSelector(".vtex-store-components-3-x-productImageTag"));

                string precoTexto = $"{elementoPrecoInteiro.Text}";
                decimal preco = decimal.Parse(precoTexto);

                ProdutoDto produto = new()
                {
                    Ean = ean,
                    Origem = Site,
                    Url = urlPesquisa,
                    Nome = elementoNomeProduto.Text,
                    Descricao = elementoDescricao?.Text ?? string.Empty,
                    Preco = preco,
                    UrlImagens = [elementoImagem.GetAttribute("src")]
                };

                return produto;
            }
            catch
            {
                return default;
            }
        }

        public void Dispose()
        {
            driver.Quit();
            driver.Dispose();
        }
    }
}

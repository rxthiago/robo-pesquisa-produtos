using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using ProdutosClient.WebApi.Dtos;
using ProdutosClient.WebApi.Enums;
using ProdutosClient.WebApi.External.Sites.Pacheco.Models;
using ProdutosClient.WebApi.External.Sites.SaoPaulo.Models;

namespace ProdutosClient.WebApi.External.Sites.Pacheco
{
    public class SaoPaulo : ISitePesquisa, IDisposable
    {
        public EnumSite Site => EnumSite.Pacheco;

        private static readonly string UrlBasePesquisa = "https://www.drogariasaopaulo.com.br/pesquisa?q=";

        private readonly ChromeDriver driver;

        public SaoPaulo()
        {
            var options = new ChromeOptions();
            options.AddArgument("--remote-allow-origins=*");
            options.AddArgument("headless");

            driver = new(options);
        }

        ~SaoPaulo()
        {
            driver.Quit();
        }

        public async Task<ProdutoDto?> PesquisarProdutoPorEan(string ean)
        {
            try
            {
                string urlPesquisa = UrlBasePesquisa + ean;

                await driver.Navigate().GoToUrlAsync(urlPesquisa);

                var element = driver.FindElement(By.CssSelector(".prateleira ul li a.collection-link[href]"));

                if (element == null)
                {
                    return default;
                }

                string urlProduto = element.GetAttribute("href");

                await driver.Navigate().GoToUrlAsync(urlProduto);

                var elementoNomeProduto = driver.FindElement(By.CssSelector(".fn.productName"));

                if (elementoNomeProduto == null)
                {
                    return default;
                }

                // Catalogar propriedades do produto e montar solicitações

                var elementDescricaoProduto = driver.FindElement(By.CssSelector(".productDescriptionShort"));
                var elementPreco = driver.FindElement(By.CssSelector(".productPrice .valor-por .skuBestPrice"));

                var preco = elementPreco == null ? 0 : decimal.Parse(elementPreco.Text.Replace("R$", string.Empty).Replace(".", ","));

                var departmentScriptElement = driver.FindElements(By.TagName("script"))
                                                    .FirstOrDefault(x => x.GetAttribute("innerHTML").Contains("departmentyId"));

                string departmentName = string.Empty;
                string categoryName = string.Empty;

                if (departmentScriptElement != null)
                {
                    string jsonContent = departmentScriptElement.GetAttribute("innerHTML");
                    jsonContent = jsonContent[jsonContent.IndexOf('{')..];
                    jsonContent = jsonContent.Remove(jsonContent.Length - 1, 1);

                    try
                    {
                        var vtexContext = JsonConvert.DeserializeObject<VtexContextS>(jsonContent);

                        departmentName = vtexContext?.DepartmentName ?? string.Empty;
                        categoryName = vtexContext?.CategoryName ?? string.Empty;
                    }
                    catch
                    {
                        // Ignorar erro de deserialização
                    }
                }

                ProdutoDto produto = new()
                {
                    Ean = ean,
                    Origem = Site,
                    Url = urlProduto,
                    Nome = elementoNomeProduto.Text,
                    Descricao = elementDescricaoProduto?.Text ?? string.Empty,
                    Preco = preco,
                    Categoria = categoryName,
                    Departamento = departmentName,
                    UrlImagens = driver.FindElements(By.CssSelector(".thumbs img"))?
                                       .Select(x => x.GetAttribute("src")
                                                     .Replace("70-70", "1000-1000"))? // Tratamento especial para substituir o tamanho da imagem
                                       .ToList() ?? []
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

using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using ProdutosClient.WebApi.Dtos;
using ProdutosClient.WebApi.Enums;
using OpenQA.Selenium.Internal;
using ProdutosClient.WebApi.External.Sites.Pacheco.Models;
using Newtonsoft.Json;

namespace ProdutosClient.WebApi.External.Sites.Pacheco
{
    public class Pacheco : ISitePesquisa
    {
        public EnumSite Site => EnumSite.Pacheco;

        private static readonly string UrlBasePesquisa = "https://www.drogariaspacheco.com.br/pesquisa?q=";

        public async Task<ProdutoDto?> PesquisarProdutoPorEan(string ean)
        {
            var options = new ChromeOptions();
            options.AddArgument("--remote-allow-origins=*");
            options.AddArgument("headless");

            using ChromeDriver driver = new(options);

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

                if(departmentScriptElement != null)
                {
                    string jsonContent = departmentScriptElement.GetAttribute("innerHTML");
                    jsonContent = jsonContent[jsonContent.IndexOf('{')..];
                    jsonContent = jsonContent.Remove(jsonContent.Length - 1, 1);

                    try
                    {
                        var vtexContext = JsonConvert.DeserializeObject<VtexContext>(jsonContent);

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
            finally
            {
                driver.Quit();
            }
        }
    }
}

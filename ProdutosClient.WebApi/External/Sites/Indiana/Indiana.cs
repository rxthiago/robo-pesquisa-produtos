using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using ProdutosClient.WebApi.Dtos;
using ProdutosClient.WebApi.Enums;
using ProdutosClient.WebApi.External.Sites.Pacheco.Models;
using SeleniumExtras.WaitHelpers;

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
                string urlPesquisa = string.Format(UrlBasePesquisa, ean);
                await driver.Navigate().GoToUrlAsync(urlPesquisa);

                var element = driver.FindElement(By.XPath("//div[@id='gallery-layout-container']//a[@href]"));
                if (element == null)
                {
                    return default;
                }
                await driver.Navigate().GoToUrlAsync(element.GetAttribute("href"));

                var elementoNomeProduto = driver.FindElement(By.CssSelector(".vtex-store-components-3-x-productBrand"));
               
                if (elementoNomeProduto == null)
                {
                    return default;
                }
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

                //var elementoDescricao = driver.FindElement(By.CssSelector("p[style=\"text-align: justify;\"]"));

                //var elementoDescricao = driver.FindElement(By.XPath("/html/body/div[2]/div/div[1]/div/div/div/div[3]/div/div[3]/div/section/div/div/div/div[2]/div/div/div/div/div[1]"));

                var elementoDescricao = wait.Until(ExpectedConditions.ElementIsVisible(
                By.XPath("/html/body/div[2]/div/div[1]/div/div/div/div[3]/div/div[3]/div/section/div/div/div/div[2]/div/div/div/div/div[1]")));

                var elementoPrecoInteiro = driver.FindElement(By.CssSelector(".vtex-product-price-1-x-currencyContainer"));
                var elementoImagem = driver.FindElement(By.CssSelector(".vtex-store-components-3-x-productImageTag"));

                string precoTexto = elementoPrecoInteiro?.Text?.Replace("R$", "").Replace(" ", "").Trim();
                decimal preco = decimal.TryParse(precoTexto, NumberStyles.Number, new CultureInfo("pt-BR"), out var valor) ? valor : 0;

                string imagemUrl = elementoImagem.GetAttribute("src");
                imagemUrl = imagemUrl.Replace("width=600", "width=1000").Replace("height=600", "height=1000");

                // Tenta buscar categoria e departamento se possível
                string departamento = string.Empty;
                string categoria = string.Empty;

                var scriptEl = driver.FindElements(By.TagName("script"))
                                     .FirstOrDefault(x => x.GetAttribute("innerHTML").Contains("categoryName"));

                if (scriptEl != null)
                {
                        var json = scriptEl.GetAttribute("innerHTML");
                        json = json[json.IndexOf('{')..];
                        json = json.Remove(json.Length - 1, 1);

                    try 
                    { 
                        var vtexContext = JsonConvert.DeserializeObject<VtexContext>(json);
                        departamento = vtexContext?.DepartmentName ?? string.Empty;
                        categoria = vtexContext?.CategoryName ?? string.Empty;
                    }
                    catch
                    {
                        // Se falhar ao ler o script, tenta buscar pelos breadcrumbs
                    }
                }

                if (string.IsNullOrWhiteSpace(departamento) || string.IsNullOrWhiteSpace(categoria))
                {
                    var breadcrumbs = driver.FindElements(By.CssSelector(".vtex-store-components-3-x-breadcrumbItem"));
                    if (breadcrumbs.Count >= 3)
                    {
                        departamento = breadcrumbs[0].Text;
                        categoria = breadcrumbs[1].Text;
                    }
                }


                ProdutoDto produto = new()
                {
                    Ean = ean,
                    Origem = Site,
                    Url = urlPesquisa,
                    Nome = elementoNomeProduto.Text,
                    Descricao = elementoDescricao?.Text ?? string.Empty,
                    Preco = preco,
                    UrlImagens = [imagemUrl],
                    Departamento = departamento,
                    Categoria = categoria
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

using Newtonsoft.Json;

namespace ProdutosClient.WebApi.External.Sites.Pacheco.Models
{
    public class VtexContext
    {
        [JsonProperty("skus")]
        public string Skus { get; set; } = null!;

        [JsonProperty("searchTerm")]
        public string SearchTerm { get; set; } = null!;

        [JsonProperty("categoryId")]
        public string CategoryId { get; set; } = null!;

        [JsonProperty("categoryName")]
        public string CategoryName { get; set; } = null!;

        [JsonProperty("departmentyId")]
        public string DepartmentyId { get; set; } = null!;

        [JsonProperty("departmentName")]
        public string DepartmentName { get; set; } = null!;

        [JsonProperty("isOrder")]
        public string IsOrder { get; set; } = null!;

        [JsonProperty("isCheck")]
        public string IsCheck { get; set; } = null!;

        [JsonProperty("isCart")]
        public string IsCart { get; set; } = null!;

        [JsonProperty("actionType")]
        public string ActionType { get; set; } = null!;

        [JsonProperty("actionValue")]
        public string ActionValue { get; set; } = null!;

        [JsonProperty("login")]
        public object Login { get; set; } = null!;

        [JsonProperty("url")]
        public string Url { get; set; } = null!;

        [JsonProperty("transurl")]
        public string Transurl { get; set; } = null!;
    }
}

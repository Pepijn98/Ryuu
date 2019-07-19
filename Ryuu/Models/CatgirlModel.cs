using System.Collections.Generic;
using Newtonsoft.Json;
using Ryuu.Interfaces;

namespace Ryuu.Models
{
    public class CatgirlModel : ICatgirl
    {
        [JsonProperty("images")]
        public List<CatgirlIdModel> Images { get; set; }
    }

    public class CatgirlIdModel : ICatgirlId
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
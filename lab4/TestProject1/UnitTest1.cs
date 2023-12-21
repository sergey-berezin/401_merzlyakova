using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using Xunit;
using WebApplication1;
using System.Text;
using System.Net.Http.Json;


namespace TestProject1
{
    public class TextControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> webApplicationFactory;
        public TextControllerTests(WebApplicationFactory<Program> webApplicationFactory)
        {
            this.webApplicationFactory = webApplicationFactory;
        }
        [Fact]
        public async Task Test1()
        {
            var client = webApplicationFactory.CreateClient();
            var postResponse = await client.PostAsJsonAsync("http://localhost:5105/Text", "nothing");
            var textId = await postResponse.Content.ReadAsStringAsync();
            var getResponse = await client.GetAsync($"https://localhost:5105/Text?textId={textId}&question=??????");
            var answer = await getResponse.Content.ReadAsStringAsync();
            Assert.NotNull(answer);
        }
        [Fact]
        public async Task Test2()
        {
            var client = webApplicationFactory.CreateClient();
            var postResponse = await client.PostAsJsonAsync("http://localhost:5105/Text", "I am hobbit");
            var textId = await postResponse.Content.ReadAsStringAsync();
            var getResponse = await client.GetAsync($"https://localhost:5105/Text?textId={textId}&question=Who I am?");
            var answer = await getResponse.Content.ReadAsStringAsync();
            Assert.Equal("ho ##bb ##it", answer);
        }
    }
}
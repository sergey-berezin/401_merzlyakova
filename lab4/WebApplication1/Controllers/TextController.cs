using ClassLibrary1;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TextController : ControllerBase
    {
        private static Dictionary<int, string> Id_Text { get; set; }
        private TextAnalyzer? textAnalyzer { get; set; }

        public TextController(Dictionary<int, string> _Id_Text)
        {
            Id_Text = _Id_Text;
            textAnalyzer = null;
        }

        [HttpPost]
        public int Post([FromBody] string text)
        {
            int textID = text.GetHashCode();
            if (!Id_Text.ContainsKey(textID))
            {
                Id_Text.Add(textID, text);
            }
            return textID;
        }

        [HttpGet]
        public async Task<string> GetAsync(int textID, string question)
        {
            textAnalyzer = await TextAnalyzer.CreateAsync(Id_Text[textID], CancellationToken.None, null);
            return await textAnalyzer.GetAnswerAsync(question, CancellationToken.None);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

using Chat.WebBlazorServer.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Chat.WebBlazorServer.Controllers
{
    public class HomeController(ILogger<HomeController> logger) : Controller
    {
        private readonly ILogger<HomeController> _logger = logger;

        [HttpGet]
        public IActionResult Index()
        {
            var model =
            new ChatModel(
                systemMessage: "Você é uma AI amigável que ajuda os usuários com suas perguntas.Sempre responda em português e formate a resposta em markdown."
            );

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Chat(
                    [FromBody] ChatModel model,
                    [FromServices] IChatCompletionService chatService,
                    [FromServices] Kernel kernel,
                    [FromServices] PromptExecutionSettings promptSettings,
                    CancellationToken cancellationToken
                )
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                model.ChatHistory.AddUserMessage(model.Prompt);
                var history = new ChatHistory(model.ChatHistory);

                // passando o kernel porque o método original exige
                var response = await chatService.GetChatMessageContentAsync(
                    history,
                    promptSettings,
                    kernel,
                    cancellationToken
                );

                model.ChatHistory.Add(response);
                model.Prompt = string.Empty;
                return Ok(model);
            }
            catch (Exception ex)
            {
                // logar ex (ex: ILogger) antes de retornar
                return StatusCode(500, $"Erro ao comunicar com o serviço de IA. {ex.Message}");
            }
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
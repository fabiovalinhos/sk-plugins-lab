using Microsoft.AspNetCore.Mvc;

using Chat.WebBlazorServer.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Chat.WebBlazorServer.Controllers
{
    [Route("[controller]")]
    public class HomeController(ILogger<HomeController> logger) : Controller
    {
        private readonly ILogger<HomeController> _logger = logger;

        // POST: /Home/Chat
        [HttpPost("[action]")]
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
    }
}
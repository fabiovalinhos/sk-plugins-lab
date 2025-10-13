using Microsoft.AspNetCore.Mvc;
using System.Text.Json; // Added for JSON parsing

using Chat.WebBlazorServer.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Http; // Added for HttpOperationException

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
                _logger.LogError(ex, "Erro ao comunicar com o serviço de IA.");

                if (ex is HttpOperationException httpEx && !string.IsNullOrEmpty(httpEx.ResponseContent))
                {
                    try
                    {
                        using JsonDocument doc = JsonDocument.Parse(httpEx.ResponseContent);
                        if (doc.RootElement.TryGetProperty("error", out JsonElement errorElement) &&
                            errorElement.TryGetProperty("code", out JsonElement codeElement) &&
                            int.TryParse(codeElement.GetString(), out int errorCode))
                        {
                            // A variável errorCode agora contém o valor 401 como int32
                            // Você pode usar errorCode aqui. Por exemplo, para retornar um status HTTP específico.
                            return StatusCode(errorCode, $"Erro de serviço de IA: {httpEx.Message}. Código: {errorCode}");
                        }
                    }
                    catch (JsonException jsonEx)
                    {
                        _logger.LogError(jsonEx, "Erro ao parsear ResponseContent do HttpOperationException.");
                    }
                }
                
                return StatusCode(500, $"Erro ao comunicar com o serviço de IA. {ex.Message}");
            }
        }
    }
}
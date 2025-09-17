using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

using Chat.WebBlazorServer.Models;

namespace Chat.WebBlazorServer.Controllers
{
    public class HomeController(ILogger<HomeController> logger) : Controller
    {
        private readonly ILogger<HomeController> _logger = logger;

        [HttpGet]
        public IActionResult Index()
        {
            var model =
            new Chatmodel(
                systemMessage: "Você é uma AI amigável que ajuda os usuários com suas perguntas.Sempre responda em português e formate a resposta em markdown."
            );

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Chat(
            [FromForm] Chatmodel model,
            [FromServices] Kernel kernel,
            [FromServices] PromptExecutionSettings promptSettings
        )
        {
            if (ModelState.IsValid)
            {
                var chatService = kernel.GetRequiredService<IChatCompletionService>();
                model.ChatHistory.AddUserMessage(model.Prompt);
                var history = new ChatHistory(model.ChatHistory);
                var response = await chatService.GetChatMessageContentAsync(
                    history,
                    promptSettings,
                    kernel
                );

                model.ChatHistory.Add(response);
                model.Prompt = string.Empty;
                return PartialView("ChatHistoryPartialView", model);
            }

            return BadRequest(ModelState);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
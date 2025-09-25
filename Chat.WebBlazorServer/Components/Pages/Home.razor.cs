using Chat.WebBlazorServer.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Chat.WebBlazorServer.Components.Pages;

public partial class Home : ComponentBase
{
    [Inject]
    public required IJSRuntime JSRuntime { get; set; }

    [Inject]
    public required HttpClient Http { get; set; }

    [Inject]
    public required NavigationManager Navigation { get; set; }

    private ChatModel chatModel = new ChatModel(
        systemMessage: "Você é uma AI amigável que ajuda os usuários com suas perguntas.Sempre responda em português e formate a resposta em markdown.");
    private string? RequestId { get; set; }
    private bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    protected override void OnInitialized()
    {
        Console.WriteLine("Home component inicializando.");
        // This is where you would typically initialize your chat model or load history
    }

    public async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.CtrlKey && e.Key == "Enter")
        {
            await SubmitChat();
        }
    }

    public async Task SubmitChat()
    {
        if (string.IsNullOrWhiteSpace(chatModel.Prompt))
        {
            return;
        }

        // Add user message to history
        chatModel.ChatHistory.AddUserMessage(chatModel.Prompt);
        StateHasChanged(); // Update UI to show user's message

        chatModel.Prompt = string.Empty; // Clear prompt input

        // This would be an actual API call to your backend
        // For now, let's simulate a response
        try
        {
            //Navigation.ToAbsoluteUri garante que vai ser absoluto. nao tenho no Program.cs o client.baseaddress
            var response = await Http.PostAsJsonAsync(Navigation.ToAbsoluteUri("/Home/Chat"), chatModel);
            
            response.EnsureSuccessStatusCode();
            var updatedChatModel = await response.Content.ReadFromJsonAsync<ChatModel>();
            if (updatedChatModel != null)
            {
                chatModel.ChatHistory = updatedChatModel.ChatHistory;
            } 
        }
        catch (Exception ex)
        {
            // Handle error
            Console.WriteLine($"Error submitting chat: {ex.Message}");
            chatModel.ChatHistory.AddAssistantMessage($"Error: {ex.Message}");
        }
        
        StateHasChanged(); // Update UI with AI's response
        await ScrollToBottom();
    }

    // JavaScript interop for scrolling
    public async Task ScrollToBottom()
    {
        await JSRuntime.InvokeVoidAsync("scrollToBottom");
    }
}
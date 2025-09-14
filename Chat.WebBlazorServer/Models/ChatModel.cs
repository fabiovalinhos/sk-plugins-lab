using Microsoft.SemanticKernel.ChatCompletion;

namespace Chat.WebBlazorServer.Models
{
    public class ChatModel
    {
     public ChatModel()
        {

        }
        
        public ChatModel(string systemMessage)
        {
            ChatHistory = new ChatHistory(systemMessage);
        }

        public ChatHistory ChatHistory { get; set; } = [];

        public string Prompt { get; set; } = string.Empty;   
    }
}
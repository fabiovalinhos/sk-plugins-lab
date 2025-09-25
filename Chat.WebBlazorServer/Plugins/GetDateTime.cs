using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace Chat.WebBlazorServer.Plugins
{
    public class GetDateTime
    {
        [KernelFunction("get_current_date_time")]
        [Description("Obter minha data e hora atuais com fuso horário")]
        [return: Description("data e hora formatadas como dddd, dd MMMM, aaaa HH:mm:ss zzz")]
        public string GetCurrentDateTime()
        {
            return DateTime.Now.ToString("dddd, dd MMMM, yyyy HH:mm:ss zzz");
        }

    }
}

using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace Chat.WebBlazorServer.Plugins
{
    public class PersonalInfo
    {
        [KernelFunction("get_my_information")]
        [Description("chame isto quando minhas informações forem necessárias, incluindo nome, endereço, local ou data de nascimento")]
        [return: Description("retorna meu nome, endereço, localização e data de nascimento formatados como JSON")]
        public Info GetInfo() => new Info();
    }


    public class Info
    {
        public string Name { get => "Bruce Fletcher"; }
        public DateTime Birthdate { get => new DateTime(1973,2,22); }
        public string Address { get => "Austin, TX"; }
    }
}

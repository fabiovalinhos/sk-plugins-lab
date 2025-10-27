using Azure;
using Azure.Search.Documents.Indexes;
using Chat.WebBlazorServer.Components;
using Chat.WebBlazorServer.Plugins;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ModelContextProtocol.Client;


var config = new ConfigurationBuilder()
.SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddHttpClient();
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor(); // Adiciona o serviço IHttpContextAccessor


// Híbrido inútil
///
// builder.Services.AddSingleton<Kernel>(sp =>
// {
//     var builder = Kernel.CreateBuilder();
//     // builder.AddOpenAIChatCompletion("your-model-id", "your-api-key");
//     return builder.Build();
// });




///Adiciona o Semantic Kernel 
var kernelBuilder = builder.Services.AddKernel();

/// Adiciona MCP Servers
await AddFileSystemMcpServerAsync(kernelBuilder);
await AddGitHubMcpServer(kernelBuilder, config["GH_PAT"] ?? string.Empty);


//Registrando os plugins
kernelBuilder.Plugins.AddFromType<GetDateTime>();
kernelBuilder.Plugins.AddFromType<GetWeather>();
kernelBuilder.Plugins.AddFromType<GetGeoCoordinates>();
kernelBuilder.Plugins.AddFromType<PersonalInfo>();

//RAG: Adicionando o plugin Manual de Conduta
kernelBuilder.Plugins.AddFromType<ManualConduta>();


//Obtendo a API externa como um plugin. Não consigo usar o kernelBuilder para isso
//Tenho que usar o buidder services collection para criar um kernel instanciado
//para isso
//
//A solução aqui não é a ideal, tem uma maneira correta de fazer isso com IHostServices
//
// var kernel =
// builder.Services.BuildServiceProvider().GetRequiredService<Kernel>();

// var kernelPlugin = await kernel.ImportPluginFromOpenApiAsync(
// pluginName: "customers",
// uri: new Uri("https://localhost:7287/swagger/v1/swagger.json")
// );

// builder.Services.AddSingleton(kernelPlugin);


var modelid = config["AzureOpenAI:DeploymentName"] ?? string.Empty;
var endpoint = config["AzureOpenAI:Endpoint"] ?? throw new ArgumentNullException("AzureOpenAI:Endpoint not found in configuration.");
var apikey = config["AzureOpenAI:ApiKey"] ?? throw new ArgumentNullException("AzureOpenAI:ApiKey not found in configuration.");

builder.Services.AddAzureOpenAIChatCompletion(modelid, endpoint, apikey);


////
//RAG: Adicionando o Text Embedding Generation

#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
builder.Services.AddAzureOpenAIEmbeddingGenerator(
    deploymentName: config["EMBEDDING_DEPLOYNAME"] ?? string.Empty,
    endpoint: config["AI_SEARCH_ENDPOINT"] ?? string.Empty,
    apiKey: config["AI_SEARCH_KEY"] ?? string.Empty
    );
#pragma warning restore SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

//RAG: Add AI Search Service

builder.Services.AddSingleton(
 sp => new SearchIndexClient(
   new Uri(config["AI_SEARCH_ENDPOINT"] ?? string.Empty
    ),
    new AzureKeyCredential(
        config["AI_SEARCH_KEY"] ?? string.Empty
)));

builder.Services.AddAzureAISearchVectorStore();
////


FunctionChoiceBehaviorOptions options = new()
{
    AllowConcurrentInvocation = true
};

builder.Services.AddTransient<PromptExecutionSettings>(_ => new
OpenAIPromptExecutionSettings
{
    Temperature = 0.7,
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(options: options),
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();


static async Task AddFileSystemMcpServerAsync(IKernelBuilder kernelBuilder)
{
    McpClient mcpClient = await McpClient.CreateAsync(new StdioClientTransport(new()
    {
        Name = "FileSystem",
        Command = "npx",
        Arguments = ["-y", "@modelcontextprotocol/server-filesystem", "C:\\Users\\FabioVilalba\\Documents\\sk-plugins-lab\\Chat.WebBlazorServer\\data"]
    }));

    IList<McpClientTool> tools = await mcpClient.ListToolsAsync();

    kernelBuilder.Plugins.AddFromFunctions("FS",
    tools.Select(skFunc => skFunc.AsKernelFunction()));
}

// este mcp cria permissão para acessar os repositórios privados do github
static async Task AddGitHubMcpServer(IKernelBuilder kernelBuilder, string PAT)
{
    McpClient mcpClient = await McpClient.CreateAsync(new HttpClientTransport(new()
    {
        Name = "GitHub",
        Endpoint = new Uri("https://api.githubcopilot.com/mcp/"),
        AdditionalHeaders = new Dictionary<string, string>
        {
            ["Authorization"] = $"Bearer {PAT}"
        }
    }));

    IList<McpClientTool> tools = await mcpClient.ListToolsAsync();

    kernelBuilder.Plugins.AddFromFunctions("GH",
    tools.Select(skFunc => skFunc.AsKernelFunction()));
}
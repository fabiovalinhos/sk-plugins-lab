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



//Registrando os plugins
kernelBuilder.Plugins.AddFromType<GetDateTime>();
kernelBuilder.Plugins.AddFromType<GetWeather>();
kernelBuilder.Plugins.AddFromType<GetGeoCoordinates>();
kernelBuilder.Plugins.AddFromType<PersonalInfo>();


//Obtendo a API externa como um plugin. Não consigo usar o kernelBuilder para isso
//Tenho que usar o buidder services collection para criar um kernel instanciado
//para isso
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
    IMcpClient mcpClient = await McpClientFactory.CreateAsync(new StdioClientTransport(new()
    {
        Name = "FileSystem",
        Command = "npx",
        Arguments = ["-y", "@modelcontextprotocol/server-filesystem", "/Users/fabiovilalba/Documents/SK_Plugins/Chat.WebBlazorServer/data"]
    }));

    IList<McpClientTool> tools = await mcpClient.ListToolsAsync();

    kernelBuilder.Plugins.AddFromFunctions("FS",
    tools.Select(skFunc => skFunc.AsKernelFunction()));
}
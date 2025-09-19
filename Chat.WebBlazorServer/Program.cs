using Chat.WebBlazorServer.Components;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;


var config = new ConfigurationBuilder()
.SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddHttpClient();


// Híbrido inútil
///
// builder.Services.AddSingleton<Kernel>(sp =>
// {
//     var builder = Kernel.CreateBuilder();
//     // builder.AddOpenAIChatCompletion("your-model-id", "your-api-key");
//     return builder.Build();
// });


///
builder.Services.AddKernel();

var modelid = config["AzureOpenAI:DeploymentName"] ?? string.Empty;
var endpoint = config["AzureOpenAI:Endpoint"] ?? throw new ArgumentNullException("AzureOpenAI:Endpoint not found in configuration.");
var apikey = config["AzureOpenAI:ApiKey"] ?? throw new ArgumentNullException("AzureOpenAI:ApiKey not found in configuration.");

builder.Services.AddAzureOpenAIChatCompletion(modelid, endpoint, apikey);

FunctionChoiceBehaviorOptions options = new()
{
    AllowConcurrentInvocation = false
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


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();


    app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode(); // define globalmente InteractiveServer

app.Run();

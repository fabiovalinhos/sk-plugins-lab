using Chat.WebBlazorServer.Components;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddHttpClient();

// Add Semantic Kernel services
builder.Services.AddSingleton<Kernel>(sp =>
{
    var builder = Kernel.CreateBuilder();
    // Configure your AI service here (e.g., OpenAI, Azure OpenAI)
    // builder.AddOpenAIChatCompletion("your-model-id", "your-api-key");
    return builder.Build();
});

builder.Services.AddTransient<PromptExecutionSettings>(sp => new PromptExecutionSettings());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

using TeejoshSystem.WebUI.Components;
using TeejoshSystem.WebUI.Extensions;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddTeejoshWebUi(builder.Configuration);




var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

await app.Services.ApplyTeejoshDatabaseAsync();

app.UseStaticFiles();
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

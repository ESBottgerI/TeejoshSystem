using TeejoshSystem.WebUI.Components;
using TeejoshSystem.WebUI.Extensions;
using TeejoshSystem.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(TeejoshSystem.Application.Common.Result).Assembly));
builder.Services.AddTeejoshWebUi();
builder.Services.AddSingleton(TimeProvider.System);
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

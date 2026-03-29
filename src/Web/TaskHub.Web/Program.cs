using TaskHub.Web.Auth;
using TaskHub.Web.Components;

var builder = WebApplication.CreateBuilder(args);

var apiBaseAddress = builder.Configuration["Api:BaseAddress"] ?? "https://localhost:7040";

var demoAuthOptionsBuilder = builder.Services
    .AddOptions<DemoAuthOptions>()
    .BindConfiguration(DemoAuthOptions.SectionName);

if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Testing"))
{
    demoAuthOptionsBuilder
        .Validate(options => !string.IsNullOrWhiteSpace(options.Username), "Demo auth username is not configured.")
        .Validate(options => !string.IsNullOrWhiteSpace(options.Password), "Demo auth password is not configured.")
        .ValidateOnStart();
}

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<DemoAccessTokenProvider>();
builder.Services.AddScoped<AuthorizedApiClientFactory>();

builder.Services.AddHttpClient("TaskHubApi", client =>
{
    client.BaseAddress = new Uri(apiBaseAddress);
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
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

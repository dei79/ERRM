using ERRM.Models;
using ERRM.Repository;
using ERRM.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);
var oidcSection = builder.Configuration.GetSection(OidcSettings.SectionName);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddOpenAI(builder.Configuration);
builder.Services.AddSingleton<IEvaluationRepository, JsonFileEvaluationRepository>();
builder.Services.AddSingleton<IEvaluationCriteriaRepository, JsonFileDefaultEvaluationCriteriaRepository>();
builder.Services.AddSingleton<ITemplateEngineService, SimpleTemplateEngineService>();
builder.Services.AddSingleton<IPromptGenerator, EvaluationPromptGenerator>();
builder.Services.AddSingleton<RuleBasedEvaluationFormulationService>();
builder.Services.AddSingleton<IEvaluationFormulationService, AiEvaluationFormulationService>();
builder.Services
    .AddOptions<OidcSettings>()
    .Bind(oidcSection)
    .ValidateDataAnnotations()
    .Validate(
        settings => !string.Equals(settings.Authority, OidcSettings.AuthorityPlaceholder, StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(settings.ClientId, OidcSettings.ClientIdPlaceholder, StringComparison.Ordinal)
                    && !string.Equals(settings.ClientSecret, OidcSettings.ClientSecretPlaceholder, StringComparison.Ordinal),
        $"Update {OidcSettings.SectionName} with your real OIDC Authority, ClientId, and ClientSecret before starting the app.")
    .Validate(
        settings => settings.Scopes is { Length: > 0 },
        $"{OidcSettings.SectionName}:Scopes must include at least one scope.")
    .ValidateOnStart();
var oidcSettings = oidcSection.Get<OidcSettings>() ?? new OidcSettings();

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    })
    .AddOpenIdConnect(options =>
    {
        options.Authority = oidcSettings.Authority;
        options.ClientId = oidcSettings.ClientId;
        options.ClientSecret = oidcSettings.ClientSecret;
        options.CallbackPath = oidcSettings.CallbackPath;
        options.SignedOutCallbackPath = oidcSettings.SignedOutCallbackPath;
        options.ResponseType = "code";
        options.SaveTokens = true;
        options.GetClaimsFromUserInfoEndpoint = true;

        options.Scope.Clear();
        foreach (var scope in oidcSettings.Scopes)
        {
            options.Scope.Add(scope);
        }
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

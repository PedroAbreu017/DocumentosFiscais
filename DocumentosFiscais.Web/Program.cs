using DocumentosFiscais.Data.Extensions;
using DocumentosFiscais.Core.Services;
using DocumentosFiscais.Data.Seed;
using DocumentosFiscais.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

// === CONFIGURAÇÃO DE SERVIÇOS ===
builder.Services.AddControllers();
builder.Services.AddControllersWithViews();

// Serviços de dados e negócio
builder.Services.AddDataServices(builder.Configuration);
builder.Services.AddScoped<IXmlValidationService, XmlValidationService>();
builder.Services.AddScoped<IDocumentoService, DocumentoService>();
builder.Services.AddScoped<SeedDataService>();

// Configurações de segurança moderna
builder.Services.AddSecurityConfiguration();

var app = builder.Build();

// === CONFIGURAÇÃO DO PIPELINE ===

// Inicialização do banco de dados
await app.InitializeDatabaseAsync();

// Pipeline de segurança
app.ConfigureSecurityPipeline();

// Pipeline de desenvolvimento/produção
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Pipeline padrão
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors();
app.UseRouting();
app.UseAuthorization();

// Configuração de rotas
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers().RequireRateLimiting("ApiPolicy");

app.Run();
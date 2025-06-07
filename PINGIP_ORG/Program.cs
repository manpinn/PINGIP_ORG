using PINGIP_ORG.Services;
using Serilog;

//var builder = WebApplication.CreateBuilder(args);

//for linux
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    WebRootPath = "/opt/MEICOTI_LABS/PINGIP_ORG/wwwroot",
    ContentRootPath = "/opt/MEICOTI_LABS/PINGIP_ORG"
});
//for linux

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<GlobalPingIPDictionaryService>();
builder.Services.AddSingleton<GlobalPortCheckIPDictionaryService>();
builder.Services.AddSingleton<GlobalTraceRouteIPDictionaryService>();
builder.Services.AddHostedService<IPCleanupService>();
builder.Services.AddTransient<PingIPService>();
builder.Services.AddTransient<PortCheckService>();
builder.Services.AddTransient<TraceRouteService>();

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File("/opt/MEICOTI_LABS/PINGIP_ORG/LOG/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

//for linux
builder.Configuration
    .SetBasePath("/opt/MEICOTI_LABS/PINGIP_ORG")
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Configuration
    .SetBasePath("/opt/MEICOTI_LABS/PINGIP_ORG")
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

builder.WebHost.UseUrls("http://0.0.0.0:5065");
//for linux


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

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

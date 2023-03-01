using Azure.Identity;
using RefreshIConfigurationproviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();


var configuration = new ConfigurationBuilder()
    .AddAzureKeyVault(new Uri("https://anishdemo1.vault.azure.net/"), new DefaultAzureCredential())
    .Build();
builder.Services.AddSingleton(configuration);

builder.Configuration.AddAzureKeyVault(
        new Uri("https://anishdemo1.vault.azure.net/"),
        new DefaultAzureCredential());

builder.Services.AddAzureAppConfiguration();
builder.Configuration.AddAzureAppConfiguration(options =>
{
    
    options.Connect(builder.Configuration["AppConfigConnectionString"])
 
          .ConfigureRefresh(refreshOptions =>
          {
              refreshOptions.Register("sentinel", refreshAll: true)
             .SetCacheExpiration(TimeSpan.FromSeconds(10));
          } )
          .ConfigureKeyVault(kv =>
          {
              kv.SetCredential(new DefaultAzureCredential());
              kv.SetSecretRefreshInterval(TimeSpan.FromMinutes(1));
          });
});

builder.Services.Configure<SettingsModel>(model =>
{
    model.secret = builder.Configuration["secret"];
    model.name = builder.Configuration["name"];
    model.kvsecret = builder.Configuration["anishkvsecret"];
});


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


configuration.Reload();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAzureAppConfiguration();
app.UseAuthorization();

app.MapControllers();

app.Run();

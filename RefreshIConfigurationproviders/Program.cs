using Azure.Identity;
using RefreshIConfigurationproviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();


var configuration = new ConfigurationBuilder()
    .AddAzureKeyVault(new Uri("https://anishdemo1.vault.azure.net/"), new DefaultAzureCredential())
    .Build();
// tried to reload the providers by setting them with the IConfigurationRoot object (not needed)
builder.Services.AddSingleton(configuration);

// we add the azure keyvault provider to IConfiguration
builder.Configuration.AddAzureKeyVault(
        new Uri("https://anishdemo1.vault.azure.net/"),
        new DefaultAzureCredential());
// The Appconfiguration middleware refreshes the provider, so this extension will register the required dependencies
builder.Services.AddAzureAppConfiguration();

// Configure the AppConfigurationProvider
builder.Configuration.AddAzureAppConfiguration(options =>
{
    
    options.Connect(builder.Configuration["AppConfigConnectionString"])
 
          .ConfigureRefresh(refreshOptions =>
          {
              // create a key named "sentinel" in azure app configuration and watch for changes
              // everytime this key is updated,  the provider will fetch the latest values
              // works with keyvault references in azure keyvault
              refreshOptions.Register("sentinel", refreshAll: true)
             .SetCacheExpiration(TimeSpan.FromSeconds(10));
          } )
          .ConfigureKeyVault(kv =>
          {
              kv.SetCredential(new DefaultAzureCredential());
              // the provider caches the secrets for 1 minute, and will refresh the values if the sentinel key has been changed 
              // and if the cache has expired
              kv.SetSecretRefreshInterval(TimeSpan.FromMinutes(1));
          });
});

// setup this object to read from app config and keyvault, used with IOptionsSnapshot in the controller
builder.Services.Configure<SettingsModel>(model =>
{
    model.secret = builder.Configuration["secret"]; // from app config
    model.name = builder.Configuration["name"]; // from app config
    model.kvsecret = builder.Configuration["anishkvsecret"]; // from KV reference in app config
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

// inject this middleware to enable refershing the app configuration provider
app.UseAzureAppConfiguration();
app.UseAuthorization();

app.MapControllers();

app.Run();

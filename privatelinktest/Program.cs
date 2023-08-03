using Azure.Identity;
using Microsoft.FeatureManagement;
namespace privatelinktest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();

            
            // Load configuration from Azure App Configuration
            builder.Configuration.AddAzureAppConfiguration(options =>
            {
                //string connectionString = "Endpoint=https://jkappconfig.azconfig.io;Id=Hyv5;Secret=ZafIQdMgZVdeozZjBkM3LhDz+bi8A6lmv+zx8j/a/6E=";
                string connectionString = Environment.GetEnvironmentVariable("APP_CONFIG_CONNECTION");
                // string appConfigEndPoint = "https://jkappconfig.azconfig.io";
                options.Connect(connectionString)
                //       options.Connect(new Uri(appConfigEndPoint), new DefaultAzureCredential())
                       // Load all keys that start with `WebDemo:` and have no label
                       .Select("WebDemo:*")
                       
                       // Configure to reload configuration if the registered key 'WebDemo:Test' is modified.
                       // Use the default cache expiration of 30 seconds. It can be overriden via AzureAppConfigurationRefreshOptions.SetCacheExpiration.
                       .ConfigureRefresh(refreshOptions =>
                       {
                           refreshOptions.Register("WebDemo:Test", refreshAll: true);
                       })
                       // Load all feature flags with no label. To load specific feature flags and labels, set via FeatureFlagOptions.Select.
                       // Use the default cache expiration of 30 seconds. It can be overriden via FeatureFlagOptions.CacheExpirationInterval.
                       .UseFeatureFlags();
            });
            builder.Services.AddAzureAppConfiguration()
                .AddFeatureManagement();

            // Bind configuration to the Settings object
            builder.Services.Configure<Settings>(builder.Configuration.GetSection("WebDemo:Test"));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            // Use Azure App Configuration middleware for dynamic configuration refresh and feature flag evaluation     
            app.UseAzureAppConfiguration();
            

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
        }
    }
}
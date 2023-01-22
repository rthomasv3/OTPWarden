using System;
using System.Net;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using OTPWarden.DataAccess;
using OTPWarden.DataAccess.Abstractions;
using OTPWarden.Middleware;
using OTPWarden.Services;
using OTPWarden.Services.Abstractions;
using OTPWarden.UIServices;
using OTPWarden.UIServices.Abstractions;
using Polly;
using Polly.Extensions.Http;

namespace OTPWarden;

public class Program
{
    #region Public Methods

    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        BuildServices(builder.Services);

        WebApplication app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseMiddleware<AuthMiddleware>();

        app.UseHttpsRedirection();

        app.UseStaticFiles();

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapBlazorHub();
            endpoints.MapRazorPages();
            endpoints.MapFallbackToPage("/_Host");
        });

        app.Run();
    }

    #endregion

    #region Private Methods

    private static void BuildServices(IServiceCollection services)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
            .Build();

        string database = configuration.GetValue<string>("Database");
        string databaseConnection = configuration.GetConnectionString("DatabaseConnection") ?? String.Empty;
        string host = configuration.GetValue<string>("Host");
        string jwtKey = configuration.GetValue<string>("JwtKey");

        AppSettings appSettings = new AppSettings()
        {
            Host = host,
            JwtKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };

        services.AddRazorPages();
        services.AddServerSideBlazor();
        services.AddControllers();

        IAsyncPolicy<HttpResponseMessage> retryPolicy = HttpPolicyExtensions
           .HandleTransientHttpError()
           .OrResult(response => response.StatusCode == HttpStatusCode.NotFound)
           .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        services.AddHttpClient("default")
            .AddPolicyHandler(retryPolicy);

        if (String.Equals("postgres", database, StringComparison.OrdinalIgnoreCase))
        {
            services.AddDbContextPool<PostgresDbContext>(options => options.UseNpgsql(databaseConnection));
            services.AddScoped<OTPDbContext, PostgresDbContext>();
        }

        if (String.Equals("sqlite", database, StringComparison.OrdinalIgnoreCase))
        {
            services.AddDbContextPool<SqliteDbContext>(options => options.UseSqlite(databaseConnection));
            services.AddScoped<OTPDbContext, SqliteDbContext>();
        }

        services.AddSingleton(appSettings);
        services.AddSingleton<IKeyService, KeyService>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IVaultService, VaultService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IVaultEntryRepository, VaultEntryRepository>();
        services.AddScoped<IUserSessionRepository, UserSessionRepository>();

        services.AddTransient<IOAuthHttpService, OAuthHttpService>();
        services.AddTransient<ICryptographyService, CryptographyService>();
    }

    #endregion
}
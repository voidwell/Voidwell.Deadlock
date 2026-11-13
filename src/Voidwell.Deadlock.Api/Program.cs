using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Voidwell.Common.Authentication;
using Voidwell.Common.Cache;
using Voidwell.Common.Configuration;
using Voidwell.Common.Logging;
using Voidwell.Common.State;
using Voidwell.Common.Swagger;
using Voidwell.Deadlock.Analytics;
using Voidwell.Deadlock.Api;
using Voidwell.Deadlock.Api.Extensions;
using Voidwell.Deadlock.Api.Filters;
using Voidwell.Deadlock.Api.Options;
using Voidwell.Deadlock.CollectionStore;
using Voidwell.Deadlock.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationLogging();

var configuration = builder.Configuration;
var services = builder.Services;

services.AddControllers(options =>
{
    options.Filters.Add<ClientAbortExceptionFilter>();
    options.Filters.Add<AnalyticsRequestExceptionFilter>();
}).AddApiJsonOptions();

services.AddApiSwagger(configuration);

services.AddOutputCache(options =>
{
    options.AddPolicy(CachePolicies.ReferenceData, policy => policy
        .Expire(TimeSpan.FromMinutes(10))
        .SetVaryByHeader("Origin"));

    // Analytics responses depend on the query string, so each distinct query is cached on its own.
    options.AddPolicy(CachePolicies.Analytics, policy => policy
        .Expire(TimeSpan.FromMinutes(10))
        .SetVaryByQuery("*")
        .SetVaryByHeader("Origin"));
});

services.AddEntityFrameworkContext(configuration);
builder.AddApplicationConfigurationStore();

services.AddCache(options =>
{
    options.RedisConfiguration = configuration.GetValue<string>("RedisConfiguration");
    options.KeyPrefix = configuration.GetApplicationName();
});

var authOptions = configuration.GetSection("Auth").Get<AuthOptions>() ?? throw new InvalidOperationException("Auth configuration section is missing.");

services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddServiceAuthentication(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.Authority = authOptions.Authority;
        options.ClientId = authOptions.ClientId;
        options.ClientSecret = authOptions.ClientSecret;
        options.RoleClaimType = authOptions.RoleClaimType;
        options.SupportedTokens = SupportedTokens.Both;
        options.RequireHttpsMetadata = false;
        options.EnableCaching = true;
        options.CacheDuration = TimeSpan.FromMinutes(2);
    });

services.AddAuthorization();

var allowedOrigins = configuration.GetValue<string>("OriginAddress");
services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(new[] { "http://localhost:4200", allowedOrigins }.Where(o => !string.IsNullOrEmpty(o)).ToArray()!)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

services.AddStatefulServiceDependencies();
services.AddCollectionStores(configuration);
services.AddAnalytics(configuration);

var app = builder.Build();

app.InitializeDatabases();

app.UseForwardedHeaders(GetForwardedHeaderOptions());

app.UseApplicationRequestLogging();

app.UseApiSwagger();

app.UseRouting();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseOutputCache();

app.MapControllers();

await app.RunAsync();

static ForwardedHeadersOptions GetForwardedHeaderOptions()
{
    var options = new ForwardedHeadersOptions
    {
        RequireHeaderSymmetry = false,
        ForwardLimit = 15,
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    };

    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();

    return options;
}

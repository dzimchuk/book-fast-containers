using BookFast.Common.Api;
using BookFast.Common.Api.JsonConverters;
using BookFast.Common.Api.SecurityContext;
using BookFast.Common.Api.Swagger;
using BookFast.Common.Application;
using BookFast.Common.Infrastructure;
using BookFast.Common.Presentation.Authorization;
using BookFast.Common.Presentation.Endpoints;
using BookFast.PropertyManagement.Infrastructure;
using Microsoft.AspNetCore.HttpOverrides;
using OpenIddict.Validation.AspNetCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureOpenTelemetry(builder.Configuration, "BookFast.PropertyManagement");

builder.Services.ConfigureExceptionHandler();

builder.Services.AddSwaggerServices(builder.Configuration);

builder.Services.ConfigureAuthentication(builder.Configuration);
builder.Services.AddAuthorization(options => AuthorizationPolicies.Register(options, OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme));

builder.Services.AddSecurityContext();

builder.Services.AddCorsServices(builder.Configuration);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

builder.Services.AddEndpoints(BookFast.PropertyManagement.Presentation.AssemblyReference.Assembly);

builder.Services.AddApplicationServices(BookFast.PropertyManagement.Application.AssemblyReference.Assembly);

builder.Services.AddCommonInfrastructure(builder.Configuration);

builder.Services.AddPropertyManagementInfrastructure(builder.Configuration);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.Converters.Add(new DecimalToStringConverter());
    options.SerializerOptions.Converters.Add(new NullableDecimalToStringConverter());
    options.SerializerOptions.Converters.Add(new NullableIntToStringConverter());
});

var app = builder.Build();

// make sure to set ASPNETCORE_FORWARDEDHEADERS_ENABLED to true
// see https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer
app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerDefaults(app.Configuration);

    using var scope = app.Services.CreateScope();
    MigrationExtensions.ApplyMigration(scope);
}

app.UseExceptionHandler();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.UseSecurityContext();

app.MapEndpoints();

app.Run();

public partial class Program { }
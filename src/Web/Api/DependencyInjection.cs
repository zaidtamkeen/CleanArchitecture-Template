using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Hosting;

namespace CleanTemplate.Api
{
    using ApiFramework.Attributes;
    using ApiFramework.Swagger;
    using Common;
    using Common.Behaviours;
    using Common.General;
    using Common.Utilities;
    using Filters;
    using FluentValidation;
    using FluentValidation.AspNetCore;
    using HealthChecks.UI.Client;
    using MediatR;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Diagnostics.HealthChecks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.IdentityModel.Tokens;
    using Microsoft.OpenApi.Models;
    using Persistence.Db;
    using PolyCache;
    using Swashbuckle.AspNetCore.SwaggerGen;
    using Swashbuckle.AspNetCore.SwaggerUI;
    using System;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;

    public static class DependencyInjection
    {
        public static IServiceCollection AddWebApi(this IServiceCollection services, IConfiguration configuration, SiteSettings siteSettings)
        {
            services.Configure<SiteSettings>(configuration.GetSection(nameof(SiteSettings)));
            var appOptions = configuration.GetSection(nameof(AppOptions)).Get<AppOptions>();
            var distributedCacheConfig = configuration.GetSection(nameof(DistributedCacheConfig)).Get<DistributedCacheConfig>();

            services.AddApiVersioning(o =>
            {
                o.ReportApiVersions = true;
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.DefaultApiVersion = new ApiVersion(1, 0);
            });

            services.AddSwaggerOptions();
            services.AddHttpContextAccessor();
            services.AddScoped<CleanTemplate.Application.Abstractions.ICurrentUser, CleanTemplate.Api.Auth.CurrentUser>();

            // Configure external authentication against Tamkeen.IdentityService using JwtBearer
            services.AddExternalJwtAuthentication(configuration);

            // Add authorization with a fallback policy (require authenticated users by default)
            services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();

                // Register dynamic, service-name-aware policies (scopes/permissions)
                CleanTemplate.Api.Auth.AuthorizationPolicies.Register(options, configuration);
            });

            // Register dynamic policy provider and handlers (permissions/scopes)
            services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationPolicyProvider, CleanTemplate.Api.Auth.DynamicAuthorizationPolicyProvider>();
            services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, CleanTemplate.Api.Auth.HasPermissionHandler>();
            services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, CleanTemplate.Api.Auth.HasScopeHandler>();

            services.AddCleanArchControllers();
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            services.AddPolyCache(configuration);
            services.AddCustomFluentValidation();

            // Identity token provider (API or gRPC) based on settings
            var identityTransport = (configuration["Identity:Transport"] ?? "Api").ToLowerInvariant();
            if (identityTransport == "grpc")
            {
                services.AddScoped<CleanTemplate.Api.External.IIdentityTokenProvider, CleanTemplate.Api.External.GrpcIdentityTokenProvider>();
            }
            else
            {
                services.AddScoped<CleanTemplate.Api.External.IIdentityTokenProvider, CleanTemplate.Api.External.ApiIdentityTokenProvider>();
            }

            // Example typed HTTP client for PayMobile integration using client credentials
            services.AddHttpClient<CleanTemplate.Api.External.IPayMobileClient, CleanTemplate.Api.External.PayMobileClient>();

            services.AddHealthChecks()
                    .AddSqlServer(appOptions.WriteDatabaseConnectionString)
                    .AddRedis(distributedCacheConfig.ConnectionString);
            services.AddHealthChecksUI()
                    .AddInMemoryStorage();

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CleanTemplate.Application.Behaviors.AuthorizationBehavior<,>));

            // Register the MigrationService
            services.AddScoped<IMigrationService, MigrationService>();

            return services;
        }

        public static IApplicationBuilder UseWebApi(this IApplicationBuilder app,
            IConfiguration configuration,
            IWebHostEnvironment env)
        {
            app.UseCors(builder =>
            {
                builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
            });

            app.UseAppSwagger(configuration);
            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            using (var scope = app.ApplicationServices.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;
                var migrationSvc = serviceProvider.GetRequiredService<IMigrationService>();
                migrationSvc.ApplyMigrations();
            }

            app.UseEndpoints(endpoints =>
            {
                if (env.IsDevelopment() || env.IsStaging())
                {
                    endpoints.MapControllers().AllowAnonymous();
                }
                else
                {
                    endpoints.MapControllers();
                }

                endpoints.MapHealthChecksUI();
                endpoints.MapHealthChecks("/health", new HealthCheckOptions()
                {
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
            });

            return app;
        }

        #region Swagger
        public static IServiceCollection AddSwaggerOptions(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.EnableAnnotations();

                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "CleanTemplate.WebUI",
                    Description = "This is a solution template for Clean Architecture implementation with ASP.NET Core Web Api",
                    Contact = new OpenApiContact
                    {
                        Name = "Tamkeen IT Team",
                        Email = "IT2@tamkeen.com.ye",
                        Url = new Uri("https://www.tamkeen.com.ye"),
                    },
                });
                options.SwaggerDoc("v2", new OpenApiInfo
                {
                    Version = "v2",
                    Title = "CleanTemplate.WebUI",
                    Description = "This is a solution template for Clean Architecture implementation with ASP.NET Core Web Api",
                    Contact = new OpenApiContact
                    {
                        Name = "Tamkeen IT Team",
                        Email = "IT2@tamkeen.com.ye",
                        Url = new Uri("https://www.tamkeen.com.ye"),
                    },
                });

                #region Filters
                //Enable to use [SwaggerRequestExample] & [SwaggerResponseExample]
                //options.ExampleFilters();

                options.OperationFilter<ApplySummariesOperationFilter>();

                //Add 401 response and security requirements (Lock icon) to actions that need authorization
                options.OperationFilter<UnauthorizedResponsesOperationFilter>(true, "OAuth2");

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme."
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                 {
                     {
                           new OpenApiSecurityScheme
                             {
                                 Reference = new OpenApiReference
                                 {
                                     Type = ReferenceType.SecurityScheme,
                                     Id = "Bearer"
                                 }
                             },
                             Array.Empty<string>()
                     }
                 });

                #region Versioning

                options.OperationFilter<RemoveVersionParameters>();

                options.DocumentFilter<SetVersionInPaths>();

                options.DocInclusionPredicate((docName, apiDesc) =>
                {
                    if (!apiDesc.TryGetMethodInfo(out MethodInfo methodInfo)) return false;

                    var versions = methodInfo.DeclaringType
                        .GetCustomAttributes<ApiVersionAttribute>(true)
                        .SelectMany(attr => attr.Versions);

                    return versions.Any(v => $"v{v}" == docName);
                });
                #endregion

                //If use FluentValidation then must be use this package to show validation in swagger (MicroElements.Swashbuckle.FluentValidation)
                //options.AddFluentValidationRules();
                #endregion
            });

            return services;
        }

        public static IApplicationBuilder UseAppSwagger(this IApplicationBuilder app, IConfiguration configuration)
        {
            app.UseSwagger();

            //Swagger middleware for generate UI from swagger.json
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "CleanTemplate.WebUI v1");
                options.SwaggerEndpoint("/swagger/v2/swagger.json", "CleanTemplate.WebUI v2");

                options.DocExpansion(DocExpansion.None);
            });

            //ReDoc UI middleware. ReDoc UI is an alternative to swagger-ui
            app.UseReDoc(options =>
            {
                options.SpecUrl("/swagger/v1/swagger.json");
                //options.SpecUrl("/swagger/v2/swagger.json");

                #region Customizing
                //By default, the ReDoc UI will be exposed at "/api-docs"
                //options.RoutePrefix = "docs";
                //options.DocumentTitle = "My API Docs";

                options.EnableUntrustedSpec();
                options.ScrollYOffset(10);
                options.HideHostname();
                options.HideDownloadButton();
                options.ExpandResponses("200,201");
                options.RequiredPropsFirst();
                options.NoAutoAuth();
                options.PathInMiddlePanel();
                options.HideLoading();
                options.NativeScrollbars();
                options.DisableSearch();
                options.OnlyRequiredInSamples();
                options.SortPropsAlphabetically();
                #endregion
            });

            return app;
        }
        #endregion

        // External JwtBearer auth against Tamkeen.IdentityService (OIDC/OAuth2)
        public static void AddExternalJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var authority = configuration["Identity:Authority"];
            var audience = configuration["Identity:Audience"];
            var requireHttps = true;
            if (bool.TryParse(configuration["Identity:RequireHttpsMetadata"], out var r))
                requireHttps = r;

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.Authority = authority;
                options.Audience = audience;
                options.RequireHttpsMetadata = requireHttps;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        if (context.AuthenticateFailure != null)
                            throw new CleanArchAppException(ApiResultStatusCode.UnAuthorized, "Authenticate failure.", HttpStatusCode.Unauthorized, context.AuthenticateFailure, null);
                        throw new CleanArchAppException(ApiResultStatusCode.UnAuthorized, "You are unauthorized to access this resource.", HttpStatusCode.Unauthorized);
                    }
                };
            });
        }

        public static void AddCleanArchControllers(this IServiceCollection services)
        {
            services.AddControllers(options =>
            {
                options.Filters.Add(typeof(ValidateModelStateAttribute));
                options.Filters.Add(new ApiExceptionFilter());
            });

            services.AddCors();
        }


        public static void AddCustomFluentValidation(this IServiceCollection services)
        {
            services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();
        }
    }
}


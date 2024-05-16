using System.Text;
using Common.Library.Kafka.Common.Extensions;
using Fake.Detection.Auth.Configure;
using Fake.Detection.Auth.Extensions;
using Fake.Detection.Auth.Helpers;
using Fake.Detection.Auth.Interceptors;
using Fake.Detection.Post.Monitoring.Client.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Fake.Detection.Auth;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.Configure<JWTOptions>(_configuration.GetSection(nameof(JWTOptions)));
        services.Configure<AuthOptions>(_configuration.GetSection(nameof(AuthOptions)));

        services.AddCommonKafka(_configuration);
        services.AddMonitoring(_configuration);

        services.AddGrpcReflection();
        services.AddGrpc(o =>
        {
            o.Interceptors.Add<ExceptionInterceptor>();
        });

        services.AddControllers();
        services.AddEndpointsApiExplorer();

        services.AddSingleton<JWTHelper>();
            
        services.AddDal(_configuration);
        
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _configuration.GetSection("JWTOptions:Issuer").Value,
                    ValidAudience = _configuration.GetSection("JWTOptions:Audience").Value,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(_configuration.GetSection("JWTOptions:SecretKey").Value!))
                };
            });
            
            services.AddAuthorization();
    }

    public void Configure(
        IHostEnvironment environment,
        IApplicationBuilder app)
    {
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(o =>
        {
            o.MapGrpcService<Services.AuthService>();
            o.MapGrpcReflectionService();
            o.MapControllers();
        });
    }
}
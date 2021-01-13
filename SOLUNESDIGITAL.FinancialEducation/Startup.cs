using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using SOLUNESDIGITAL.Connector.Token.Mangers;
using SOLUNESDIGITAL.FinancialEducation.Connector.Email.Managers;
using SOLUNESDIGITAL.FinancialEducation.DataAccess;
using SOLUNESDIGITAL.FinancialEducation.DataAccess.V1;
using SOLUNESDIGITAL.FinancialEducation.Extensions;
using SOLUNESDIGITAL.Framework.Logs;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SOLUNESDIGITAL.FinancialEducation
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            string webApiName = Configuration.GetValue<string>("WebApi:Name");
            string webApiShortName = Configuration.GetValue<string>("WebApi:ShortName");

            #region Config DataBase
            string dbApiDatabase = Configuration.GetValue<string>("ConnectionStrings_DB:Database");
            string dbApiPassword = Configuration.GetValue<string>("ConnectionStrings_DB:Password");
            string dbApiServer = Configuration.GetValue<string>("ConnectionStrings_DB:Server");
            string dbApiUser = Configuration.GetValue<string>("ConnectionStrings_DB:User");
            int dbApiTimeout = Configuration.GetValue<int>("ConnectionStrings_DB:Timeout");
            //dbApiPassword = securityManager.EncryptDecrypt(false, dbApiPassword); ENCRIPTAR
            string connectionDbApi = Connection.DBApi(dbApiServer, dbApiDatabase, dbApiUser, dbApiPassword, webApiName);
            #endregion

            #region Logs
            string pathLogFile = Configuration.GetValue<string>("Logs:Path_Log_File");
            string logLevel = Configuration.GetValue<string>("Logs:Level");

            services.AddSingleton<ILogger, Logger>(f => new Logger(pathLogFile, logLevel));
            #endregion

            #region DataAccess
            services.AddSingleton<IClient, Client>(f => new Client(connectionDbApi, dbApiTimeout));
            services.AddSingleton<IConsumptionHistory, ConsumptionHistory>(f => new ConsumptionHistory(connectionDbApi, dbApiTimeout));
            services.AddSingleton<IUser, User>(f => new User(connectionDbApi, dbApiTimeout));
            services.AddSingleton<IUserPolicy, UserPolicy>(f => new UserPolicy(connectionDbApi, dbApiTimeout));
            services.AddSingleton<IRefreshToken, RefreshToken>(f => new RefreshToken(connectionDbApi, dbApiTimeout));
            services.AddSingleton<IModule, Module>(f => new Module(connectionDbApi, dbApiTimeout));
            services.AddSingleton<IClienAnswer, ClienAnswer>(f => new ClienAnswer(connectionDbApi, dbApiTimeout));
            services.AddSingleton<IClientModule, ClientModule>(f => new ClientModule(connectionDbApi, dbApiTimeout));            
            #endregion


            #region Connectors
            double minutesExpiratioTime = Configuration.GetValue<double>("JwtSettings:MinutesExpiratioTime");
            string validIssuer = Configuration.GetValue<string>("JwtSettings:ValidIssuer");
            string validAudience = Configuration.GetValue<string>("JwtSettings:ValidAudience");

            services.AddTransient<ITokenManger, TokenManger>(f => new TokenManger(minutesExpiratioTime, validIssuer, validAudience));

            /*string host = Configuration.GetValue<string>("Connectors_Email:Host");
            string port = Configuration.GetValue<string>("Connectors_Email:Port");
            string from = Configuration.GetValue<string>("Connectors_Email:From");
            string smtpUser = Configuration.GetValue<string>("Connectors_Email:User");
            string smtpPassword = Configuration.GetValue<string>("Connectors_Email:Password");*/

            string host = Environment.GetEnvironmentVariable("EMAILHOST");
            string port = Environment.GetEnvironmentVariable("EMAILPORT");
            string from = Environment.GetEnvironmentVariable("EMAILFROM");
            string smtpUser = Environment.GetEnvironmentVariable("EMAILUSER");
            string smtpPassword = Environment.GetEnvironmentVariable("EMAILPASSWORD");

            bool flagEnableUserPassword = Configuration.GetValue<bool>("Connectors_Email:FlagEnableUserPassword");
            //string message = Configuration.GetValue<string>("Connectors_Email:Message");
            string message = File.ReadAllText(string.Format(@"{0}/Resources/Template/TemplateMail.html", Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)));

            services.AddSingleton<IEmailManager, EmailManager>(f => new EmailManager(host, port, from, smtpUser, smtpPassword, flagEnableUserPassword, message));

            #endregion

            #region API versioning
            services.AddApiVersioning();

            services.AddVersionedApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VV";
                options.SubstituteApiVersionInUrl = true;
            });
            #endregion

            #region Swagger
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, SwaggerGenConfiguration>();

            services.AddSwaggerGen(c =>
            {
                c.OperationFilter<AddHeaderParameter>();
                c.AddSecurityDefinition("basic", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "basic",
                    In = ParameterLocation.Header,
                    Description = "Basic Authorization header using the Bearer scheme."
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference{Type = ReferenceType.SecurityScheme,Id = "basic"}
                        },
                        new string[] {}
                    }
                });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please insert JWT with Bearer into field",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference{Type = ReferenceType.SecurityScheme,Id = "Bearer"}
                        },
                        new string[] {}
                    }
                });

            });
            #endregion
            services.AddControllers();
            services.ConfigureCors();
            //services.AddCors();
            services.ConfigureIISIntegration();
            services.ConfigureJWT(Configuration);
            //services.ConfigureNonBreakingSameSiteCookies();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(Configuration.GetValue<string>("LicencePdf"));
            app.UseHttpsRedirection();
            
            app.UseStaticFiles();
            
            
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.All
            });

            app.UseRouting();

            app.UseCors("CorsPolicy");

            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            #region Swagger
            app.UseSwagger();
            // string swagger = Configuration.GetValue<string>("Swagger:Host");
            app.UseSwaggerUI(options =>
            {
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    //string content = swagger.Replace("@GroupName", description.GroupName);
                    //options.SwaggerEndpoint(content, description.GroupName.ToUpperInvariant());
                    options.SwaggerEndpoint($"../swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                }
            });
            #endregion
        }
    }
}
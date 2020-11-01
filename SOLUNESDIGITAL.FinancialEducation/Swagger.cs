using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SOLUNESDIGITAL.FinancialEducation
{
    internal class SwaggerGenConfiguration : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider;

        public SwaggerGenConfiguration(IApiVersionDescriptionProvider provider)
        {
            this._provider = provider;
        }

        public void Configure(SwaggerGenOptions options)
        {
            options.OperationFilter<AddApiVersion>();
            foreach (ApiVersionDescription description in _provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(
                    description.GroupName,
                    new OpenApiInfo
                    {
                        Title = "SOLUNESDIGITAL.FinancialEducation Web API",
                        Version = description.ApiVersion.ToString(),
                        Description = "ASP.NET Core Web API",
                    });
            }
        }
    }

    public class AddApiVersion : IOperationFilter
    {
        private const string ApiVersionQueryParameter = "version";
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            OpenApiParameter apiVersionParameter = operation.Parameters.SingleOrDefault(p => p.Name == ApiVersionQueryParameter);

            if (apiVersionParameter == null)
                return;

            ApiVersionAttribute attribute = context?.MethodInfo?.DeclaringType?
                .GetCustomAttributes(typeof(ApiVersionAttribute), false)
                .Cast<ApiVersionAttribute>()
                .SingleOrDefault();

            string version = attribute?.Versions?.SingleOrDefault()?.ToString();

            if (version != null)
            {
                apiVersionParameter.Example = new OpenApiString(version);
                apiVersionParameter.Schema.Example = new OpenApiString(version);
            }
        }
    }

    class AddHeaderParameter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
                operation.Parameters = new List<OpenApiParameter>();

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "Correlation-Id",
                In = ParameterLocation.Header,
                //Required = true,
                Schema = new OpenApiSchema
                {
                    Type = "String"
                }
            });
        }
    }
}

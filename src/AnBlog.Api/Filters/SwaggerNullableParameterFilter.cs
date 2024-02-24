using AutoMapper.Internal;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AnBlog.Api.Filters
{
    public class SwaggerNullableParameterFilter : IParameterFilter
    {
        public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
        {
            //var name = parameter.Name;
            //var docName = context.DocumentName;
            //var c = context.ApiParameterDescription;
            //var c1 = !parameter.Schema.Nullable;
            //var c2 = context.ApiParameterDescription.Type.IsNullableType();
            //var c3 = context.ApiParameterDescription.Type.IsValueType;

            if (!parameter.Schema.Nullable &&
                (context.ApiParameterDescription.Type.IsNullableType() || context.ApiParameterDescription.Type.IsValueType))
            {
                parameter.Schema.Nullable = true;
            }
        }
    }
}

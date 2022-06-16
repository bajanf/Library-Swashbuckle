using Library.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace Library.API.OperationFilters
{
    public class GetBookOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
            // --Swashbuckle 4
            //    if (operation.OperationId != "GetBook")
            //{
            //    return;
            //}

            //if (operation.Responses.Any(a => a.Key == StatusCodes.Status200OK.ToString()))
            //{
            //    operation.Responses[StatusCodes.Status200OK.ToString()].Content.Add(
            //        "application/vnd.marvin.bookwithconcatenatedauthorname+json",
            //        new OpenApiMediaType()
            //        {
            //            Schema = context.SchemaRegistry.GetOrRegister(typeof(BookWithConcatenatedAuthorName))
            //        });
            //}


            // -- Swashbuckle 5
            if (context.ApiDescription.ActionDescriptor.RouteValues["action"] != "GetBook")
            {
                return;
            }

            var schema = context.SchemaGenerator.GenerateSchema(typeof(BookWithConcatenatedAuthorName), context.SchemaRepository);
            operation.Responses[StatusCodes.Status200OK.ToString()].Content.Add(
                "application/vnd.marvin.bookwithconcatenatedauthorname+json",
                new OpenApiMediaType()
                {
                    Schema = schema
                }); 
        }
    }
}

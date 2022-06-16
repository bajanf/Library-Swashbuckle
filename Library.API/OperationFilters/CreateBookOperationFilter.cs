using Library.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Library.API.OperationFilters
{
    public class CreateBookOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // -- Swashbuckle 5
            if (context.ApiDescription.ActionDescriptor.RouteValues["action"] != "CreateBook")
            {
                return;
            }

            var schema = context.SchemaGenerator.GenerateSchema(typeof(BookForCreationWithAmountOfPages), context.SchemaRepository);
            operation.Responses[StatusCodes.Status201Created.ToString()].Content.Add(
                "application/vnd.marvin.bookforcreationwithamountofpages+json",
                new OpenApiMediaType()
                {
                    Schema = schema
                });
        }
    
    }
}

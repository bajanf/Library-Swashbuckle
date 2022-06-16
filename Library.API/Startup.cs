using AutoMapper;
using Library.API.Contexts;
using Library.API.OperationFilters;
using Library.API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

[assembly: ApiConventionType(typeof(DefaultApiConventions))]
namespace Library.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(setupAction =>
            {
                //set up response type globably in order to not mention them in every controller
                //!!!be carefull, those overrides the DefaultApiConventions/CustomConvestions annotations if are any
                //setupAction.Filters.Add(
                //    new ProducesResponseTypeAttribute(StatusCodes.Status500InternalServerError));
                //setupAction.Filters.Add(
                //    new ProducesResponseTypeAttribute(StatusCodes.Status406NotAcceptable));
                //setupAction.Filters.Add(
                //    new ProducesResponseTypeAttribute(StatusCodes.Status400BadRequest));

                // if ReturnHttpNotAcceptable is false or is missing
                // not ok because we can constrain  [Produces] annotation to a specific type => ignore the specified Accept headers for responses
                //imagine a site that suport only application/xml type will crush after the server sends the format in json => set allways to true
                setupAction.ReturnHttpNotAcceptable = true;

                setupAction.OutputFormatters.Add(new XmlSerializerOutputFormatter());

                var jsonOutputFormatter = setupAction.OutputFormatters
                    .OfType<JsonOutputFormatter>().FirstOrDefault();

                if (jsonOutputFormatter != null)
                {
                    // remove text/json as it isn't the approved media type
                    // for working with JSON at API level
                    if (jsonOutputFormatter.SupportedMediaTypes.Contains("text/json"))
                    {
                        jsonOutputFormatter.SupportedMediaTypes.Remove("text/json");
                    }
                }
            })
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // register the DbContext on the container, getting the connection string from
            // appSettings (note: use this during development; in a production environment,
            // it's better to store the connection string in an environment variable)
            var connectionString = Configuration["ConnectionStrings:LibraryDBConnectionString"];
            services.AddDbContext<LibraryContext>(o => o.UseSqlServer(connectionString));
            
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = actionContext =>
                {
                    var actionExecutingContext =
                        actionContext as Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext;

                    // if there are modelstate errors & all keys were correctly
                    // found/parsed we're dealing with validation errors
                    if (actionContext.ModelState.ErrorCount > 0
                        && actionExecutingContext?.ActionArguments.Count == actionContext.ActionDescriptor.Parameters.Count)
                    {
                        return new UnprocessableEntityObjectResult(actionContext.ModelState);
                    }

                    // if one of the keys wasn't correctly found / couldn't be parsed
                    // we're dealing with null/unparsable input
                    return new BadRequestObjectResult(actionContext.ModelState);
                };
            });

            services.AddScoped<IBookRepository, BookRepository>();
            services.AddScoped<IAuthorRepository, AuthorRepository>();

            services.AddAutoMapper();

            services.AddSwaggerGen(setupAction =>
            {
                setupAction.SwaggerDoc(
                    "OpenAPISpecAuthors",
                    new Microsoft.OpenApi.Models.OpenApiInfo()
                    {
                        Title = "Library API (Authors)",
                        Version = "v1",
                        Description = "Throught this API you can access authors.",
                        Contact = new Microsoft.OpenApi.Models.OpenApiContact()
                        {
                            Email ="flavia.bajan@gamil.com",
                            Name ="Flavia Demo",
                            Url =  new Uri ("https://github.com/bajanf"),
                        },
                        License =new Microsoft.OpenApi.Models.OpenApiLicense() 
                        { 
                            Name = "MIT License",
                            Url=new Uri("https://opensource.org/licenses/MIT")
                        }
                    });

                setupAction.SwaggerDoc(
                    "OpenAPISpecBooks",
                    new Microsoft.OpenApi.Models.OpenApiInfo()
                    {
                        Title = "Library API (Books)",
                        Version = "v1",
                        Description = "Throught this API you can access Books.",
                        Contact = new Microsoft.OpenApi.Models.OpenApiContact()
                        {
                            Email = "flavia.bajan@gamil.com",
                            Name = "Flavia Demo",
                            Url = new Uri("https://github.com/bajanf"),
                        },
                        License = new Microsoft.OpenApi.Models.OpenApiLicense()
                        {
                            Name = "MIT License",
                            Url = new Uri("https://opensource.org/licenses/MIT")
                        }
                    });

                //setupAction.ResolveConflictingActions(apiDescriptions =>
                //{
                //    var firstDescription = apiDescriptions.First();
                //    var secondDescription = apiDescriptions.ElementAt(1);

                //    firstDescription.SupportedResponseTypes.AddRange(secondDescription.SupportedResponseTypes.Where(a => a.StatusCode == 200));

                //    //return firstDescription;

                //    return apiDescriptions.First();
                //});

                setupAction.OperationFilter<GetBookOperationFilter>();
                setupAction.OperationFilter<CreateBookOperationFilter>();

                var xmlCommentFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlCommentFullPath = Path.Combine(AppContext.BaseDirectory, xmlCommentFile);
                setupAction.IncludeXmlComments(xmlCommentFullPath);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. 
                // You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint(
                    "/swagger/OpenAPISpecAuthors/swagger.json",
                    "Library API (Authors)");
                c.SwaggerEndpoint(
                    "/swagger/OpenAPISpecBooks/swagger.json",
                    "Library API (Books)");
                c.RoutePrefix = ""; //sets the endpoint of documentation at root
            });

            app.UseStaticFiles();

            app.UseMvc();
        }
    }
}

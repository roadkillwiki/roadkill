using System.Collections.Generic;
using Swashbuckle.Swagger;

namespace Roadkill.Core.Mvc.Setup
{
	public class SwashbuckleApplyApiKeySecurity : IDocumentFilter, IOperationFilter
	{
		public string Description { get; }
		public string In { get; }
		public string Key { get; }
		public string Name { get; }

		public SwashbuckleApplyApiKeySecurity(string key, string name, string description, string @in)
		{
			Key = key;
			Name = name;
			Description = description;
			In = @in;
		}

		public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, System.Web.Http.Description.IApiExplorer apiExplorer)
		{
			IList<IDictionary<string, IEnumerable<string>>> security = new List<IDictionary<string, IEnumerable<string>>>();
			security.Add(new Dictionary<string, IEnumerable<string>> {
				{Key, new string[0]}
			});

			swaggerDoc.security = security;
		}

		public void Apply(Operation operation, SchemaRegistry schemaRegistry, System.Web.Http.Description.ApiDescription apiDescription)
		{
			operation.parameters = operation.parameters ?? new List<Parameter>();
			operation.parameters.Add(new Parameter
			{
				name = Name,
				description = Description,
				@in = In,
				required = true,
				type = "string"
			});
		}

		public void Apply(Swashbuckle.Application.SwaggerDocsConfig c)
		{
			c.ApiKey(Key)
				.Name(Name)
				.Description(Description)
				.In(In);
			c.DocumentFilter(() => this);
			c.OperationFilter(() => this);
		}
	}
}
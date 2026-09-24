using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Roadkill.Core.DependencyResolution;

namespace Roadkill.Web
{
	public class Program
	{
		public static void Main(string[] args)
		{
			WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
			builder.Services.AddRoadkill(builder.Environment.ContentRootPath);

			WebApplication app = builder.Build();

			if (!app.Environment.IsDevelopment())
				app.UseExceptionHandler("/wiki/servererror");

			app.UseRoadkill();
			app.Run();
		}
	}
}

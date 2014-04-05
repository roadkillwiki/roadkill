using Roadkill.Core.Configuration;
using Roadkill.Core.Services;
using StructureMap;
using System.Web;
using System.Web.Routing;

namespace Roadkill.Core.Attachments
{
	/// <summary>
	/// A route handler for the attachments virtual folder. This route handler doesn't swallow MVC routes.
	/// </summary>
	public class AttachmentRouteHandler : IRouteHandler
	{
		private ApplicationSettings _settings;
		private readonly IFileService _fileService;

		/// <summary>
		/// Initializes a new instance of the <see cref="AttachmentRouteHandler"/> class.
		/// </summary>
		/// <param name="settings">The current application settings.</param>
		public AttachmentRouteHandler(ApplicationSettings settings, IFileService fileService)
		{
			_settings = settings;
			_fileService = fileService ?? ObjectFactory.GetInstance<IFileService>(); //This is a hack, but I'm not seeing the good solution.
		}

		/// <summary>
		/// Registers the attachments path route, using the settings given in the application settings.
		/// </summary>
		/// <param name="settings">The settings.</param>
		/// <param name="routes">The routes.</param>
		/// <exception cref="ConfigurationException">
		/// The configuration is missing an attachments route path.
		/// or
		/// The attachmentsRoutePath in the config is set to 'files' which is not an allowed route path.
		/// </exception>
		public static void RegisterRoute(ApplicationSettings settings, RouteCollection routes)
		{
			if (string.IsNullOrEmpty(settings.AttachmentsRoutePath))
				throw new ConfigurationException("The configuration is missing an attachments route path, please enter one using attachmentsRoutePath=\"Attachments\"", null);

			if (settings.AttachmentsRoutePath.ToLower() == "files")
				throw new ConfigurationException("The attachmentsRoutePath in the config is set to 'files' which is not an allowed route path. Please change it to something else.", null);


			Route route = new Route(settings.AttachmentsRoutePath + "/{*filename}", new AttachmentRouteHandler(settings, null));
			route.Constraints = new RouteValueDictionary();
			route.Constraints.Add("MvcContraint", new IgnoreMvcConstraint(settings));

			routes.Add(route);
		}

		/// <summary>
		/// Provides the object that processes the request.
		/// </summary>
		/// <param name="requestContext">An object that encapsulates information about the request.</param>
		/// <returns>
		/// An object that processes the request.
		/// </returns>
		public IHttpHandler GetHttpHandler(RequestContext requestContext)
		{
			return new AttachmentFileHandler(_settings, _fileService);
		}

		/// <summary>
		/// A route constraint for the attachments route, that ignores any controller or action route requests so only the
		/// /attachments/{filename} routes get through.
		/// </summary>
		public class IgnoreMvcConstraint : IRouteConstraint
		{
			private ApplicationSettings _settings;

			public IgnoreMvcConstraint(ApplicationSettings settings)
			{
				_settings = settings;
			}

			public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
			{
				if (routeDirection == RouteDirection.UrlGeneration)
					return false;
				if (values.ContainsKey("controller") || values.ContainsKey("action"))
					return false;

				if (route.Url.StartsWith(_settings.AttachmentsRoutePath +"/{*filename}"))
					return true;
				else
					return false;
			}
		}
	}
}

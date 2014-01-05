using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using RestSharp;
using Roadkill.Core.Mvc.Controllers.Api;

namespace Roadkill.Tests.Integration.WebApi
{
	public class WebApiResponse<T> where T : new()
	{
		public string Url { get; set; }
		public string Content { get; set; }
		public HttpStatusCode HttpStatusCode { get; set; }
		public T Result { get; set; }

		/// <summary>
		/// Allows a string to be implicitly cast from a <c>WebApiResponse{T}</c>.
		/// </summary>
		/// <param name="pageHtml"></param>
		/// <returns></returns>
		public static implicit operator string(WebApiResponse<T> response)
		{
			return response.ToString();
		}

		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendLine("---- WebApiResponse<T> debug: ----");
			builder.AppendLine("Url - " + Url);
			builder.AppendLine("HttpStatusCode - " + HttpStatusCode);

			if (Result != null)
				builder.AppendLine("Result - " + Result.ToString());
			else
				builder.AppendLine("Result - (null)");

			builder.AppendLine("Content - " + Content);
			builder.AppendLine("---------------------------------------");
			return builder.ToString();
		}
	}

	public class WebApiResponse : WebApiResponse<object>
	{
		/// <summary>
		/// Allows a string to be implicitly cast from a <c>WebApiResponse</c>.
		/// </summary>
		/// <param name="pageHtml"></param>
		/// <returns></returns>
		public static implicit operator string(WebApiResponse response)
		{
			return response.ToString();
		}
	}
}

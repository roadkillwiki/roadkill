using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Administration;

namespace Roadkill.Core.Attachments
{
	/// <summary>
	/// Contains a  list of common mime type, used when IIS mime type lookups fail.
	/// </summary>
	public class MimeTypes
	{
		private static Dictionary<string, string> ExtensionMap = new Dictionary<string, string>();

		static MimeTypes()
		{
			ExtensionMap.Add(".323", "text/h323");
			ExtensionMap.Add(".asx", "video/x-ms-asf");
			ExtensionMap.Add(".acx", "application/internet-property-stream");
			ExtensionMap.Add(".ai", "application/postscript");
			ExtensionMap.Add(".aif", "audio/x-aiff");
			ExtensionMap.Add(".aiff", "audio/aiff");
			ExtensionMap.Add(".axs", "application/olescript");
			ExtensionMap.Add(".aifc", "audio/aiff");
			ExtensionMap.Add(".asr", "video/x-ms-asf");
			ExtensionMap.Add(".avi", "video/x-msvideo");
			ExtensionMap.Add(".asf", "video/x-ms-asf");
			ExtensionMap.Add(".au", "audio/basic");
			ExtensionMap.Add(".application", "application/x-ms-application");
			ExtensionMap.Add(".bin", "application/octet-stream");
			ExtensionMap.Add(".bas", "text/plain");
			ExtensionMap.Add(".bcpio", "application/x-bcpio");
			ExtensionMap.Add(".bmp", "image/bmp");
			ExtensionMap.Add(".cdf", "application/x-cdf");
			ExtensionMap.Add(".cat", "application/vndms-pkiseccat");
			ExtensionMap.Add(".crt", "application/x-x509-ca-cert");
			ExtensionMap.Add(".c", "text/plain");
			ExtensionMap.Add(".css", "text/css");
			ExtensionMap.Add(".cer", "application/x-x509-ca-cert");
			ExtensionMap.Add(".crl", "application/pkix-crl");
			ExtensionMap.Add(".cmx", "image/x-cmx");
			ExtensionMap.Add(".csh", "application/x-csh");
			ExtensionMap.Add(".cod", "image/cis-cod");
			ExtensionMap.Add(".cpio", "application/x-cpio");
			ExtensionMap.Add(".clp", "application/x-msclip");
			ExtensionMap.Add(".crd", "application/x-mscardfile");
			ExtensionMap.Add(".deploy", "application/octet-stream");
			ExtensionMap.Add(".dll", "application/x-msdownload");
			ExtensionMap.Add(".dot", "application/msword");
			ExtensionMap.Add(".doc", "application/msword");
			ExtensionMap.Add(".dvi", "application/x-dvi");
			ExtensionMap.Add(".dir", "application/x-director");
			ExtensionMap.Add(".dxr", "application/x-director");
			ExtensionMap.Add(".der", "application/x-x509-ca-cert");
			ExtensionMap.Add(".dib", "image/bmp");
			ExtensionMap.Add(".dcr", "application/x-director");
			ExtensionMap.Add(".disco", "text/xml");
			ExtensionMap.Add(".exe", "application/octet-stream");
			ExtensionMap.Add(".etx", "text/x-setext");
			ExtensionMap.Add(".evy", "application/envoy");
			ExtensionMap.Add(".eml", "message/rfc822");
			ExtensionMap.Add(".eps", "application/postscript");
			ExtensionMap.Add(".flr", "x-world/x-vrml");
			ExtensionMap.Add(".fif", "application/fractals");
			ExtensionMap.Add(".gtar", "application/x-gtar");
			ExtensionMap.Add(".gif", "image/gif");
			ExtensionMap.Add(".gz", "application/x-gzip");
			ExtensionMap.Add(".hta", "application/hta");
			ExtensionMap.Add(".htc", "text/x-component");
			ExtensionMap.Add(".htt", "text/webviewhtml");
			ExtensionMap.Add(".h", "text/plain");
			ExtensionMap.Add(".hdf", "application/x-hdf");
			ExtensionMap.Add(".hlp", "application/winhlp");
			ExtensionMap.Add(".html", "text/html");
			ExtensionMap.Add(".htm", "text/html");
			ExtensionMap.Add(".hqx", "application/mac-binhex40");
			ExtensionMap.Add(".isp", "application/x-internet-signup");
			ExtensionMap.Add(".iii", "application/x-iphone");
			ExtensionMap.Add(".ief", "image/ief");
			ExtensionMap.Add(".ivf", "video/x-ivf");
			ExtensionMap.Add(".ins", "application/x-internet-signup");
			ExtensionMap.Add(".ico", "image/x-icon");
			ExtensionMap.Add(".jfif", "image/pjpeg");
			ExtensionMap.Add(".jpe", "image/jpeg");
			ExtensionMap.Add(".jpeg", "image/jpeg");
			ExtensionMap.Add(".jpg", "image/jpeg");
			ExtensionMap.Add(".js", "application/x-javascript");
			ExtensionMap.Add(".lsx", "video/x-la-asf");
			ExtensionMap.Add(".latex", "application/x-latex");
			ExtensionMap.Add(".lsf", "video/x-la-asf");
			ExtensionMap.Add(".manifest", "application/x-ms-manifest");
			ExtensionMap.Add(".mhtml", "message/rfc822");
			ExtensionMap.Add(".mny", "application/x-msmoney");
			ExtensionMap.Add(".mht", "message/rfc822");
			ExtensionMap.Add(".mid", "audio/mid");
			ExtensionMap.Add(".mpv2", "video/mpeg");
			ExtensionMap.Add(".man", "application/x-troff-man");
			ExtensionMap.Add(".mvb", "application/x-msmediaview");
			ExtensionMap.Add(".mpeg", "video/mpeg");
			ExtensionMap.Add(".m3u", "audio/x-mpegurl");
			ExtensionMap.Add(".mdb", "application/x-msaccess");
			ExtensionMap.Add(".mpp", "application/vnd.ms-project");
			ExtensionMap.Add(".m1v", "video/mpeg");
			ExtensionMap.Add(".mpa", "video/mpeg");
			ExtensionMap.Add(".me", "application/x-troff-me");
			ExtensionMap.Add(".m13", "application/x-msmediaview");
			ExtensionMap.Add(".movie", "video/x-sgi-movie");
			ExtensionMap.Add(".m14", "application/x-msmediaview");
			ExtensionMap.Add(".mpe", "video/mpeg");
			ExtensionMap.Add(".mp2", "video/mpeg");
			ExtensionMap.Add(".mov", "video/quicktime");
			ExtensionMap.Add(".mp3", "audio/mpeg");
			ExtensionMap.Add(".mpg", "video/mpeg");
			ExtensionMap.Add(".ms", "application/x-troff-ms");
			ExtensionMap.Add(".nc", "application/x-netcdf");
			ExtensionMap.Add(".nws", "message/rfc822");
			ExtensionMap.Add(".oda", "application/oda");
			ExtensionMap.Add(".ods", "application/oleobject");
			ExtensionMap.Add(".pmc", "application/x-perfmon");
			ExtensionMap.Add(".p7r", "application/x-pkcs7-certreqresp");
			ExtensionMap.Add(".p7b", "application/x-pkcs7-certificates");
			ExtensionMap.Add(".p7s", "application/pkcs7-signature");
			ExtensionMap.Add(".pmw", "application/x-perfmon");
			ExtensionMap.Add(".ps", "application/postscript");
			ExtensionMap.Add(".p7c", "application/pkcs7-mime");
			ExtensionMap.Add(".pbm", "image/x-portable-bitmap");
			ExtensionMap.Add(".png", "image/png");
			ExtensionMap.Add(".ppm", "image/x-portable-pixmap");
			ExtensionMap.Add(".pub", "application/x-mspublisher");
			ExtensionMap.Add(".pnm", "image/x-portable-anymap");
			ExtensionMap.Add(".pml", "application/x-perfmon");
			ExtensionMap.Add(".p10", "application/pkcs10");
			ExtensionMap.Add(".pfx", "application/x-pkcs12");
			ExtensionMap.Add(".p12", "application/x-pkcs12");
			ExtensionMap.Add(".pdf", "application/pdf");
			ExtensionMap.Add(".pps", "application/vnd.ms-powerpoint");
			ExtensionMap.Add(".p7m", "application/pkcs7-mime");
			ExtensionMap.Add(".pko", "application/vndms-pkipko");
			ExtensionMap.Add(".ppt", "application/vnd.ms-powerpoint");
			ExtensionMap.Add(".pmr", "application/x-perfmon");
			ExtensionMap.Add(".pma", "application/x-perfmon");
			ExtensionMap.Add(".pot", "application/vnd.ms-powerpoint");
			ExtensionMap.Add(".prf", "application/pics-rules");
			ExtensionMap.Add(".pgm", "image/x-portable-graymap");
			ExtensionMap.Add(".qt", "video/quicktime");
			ExtensionMap.Add(".ra", "audio/x-pn-realaudio");
			ExtensionMap.Add(".rgb", "image/x-rgb");
			ExtensionMap.Add(".ram", "audio/x-pn-realaudio");
			ExtensionMap.Add(".rmi", "audio/mid");
			ExtensionMap.Add(".ras", "image/x-cmu-raster");
			ExtensionMap.Add(".roff", "application/x-troff");
			ExtensionMap.Add(".rtf", "application/rtf");
			ExtensionMap.Add(".rtx", "text/richtext");
			ExtensionMap.Add(".sv4crc", "application/x-sv4crc");
			ExtensionMap.Add(".spc", "application/x-pkcs7-certificates");
			ExtensionMap.Add(".setreg", "application/set-registration-initiation");
			ExtensionMap.Add(".snd", "audio/basic");
			ExtensionMap.Add(".stl", "application/vndms-pkistl");
			ExtensionMap.Add(".setpay", "application/set-payment-initiation");
			ExtensionMap.Add(".stm", "text/html");
			ExtensionMap.Add(".shar", "application/x-shar");
			ExtensionMap.Add(".sh", "application/x-sh");
			ExtensionMap.Add(".sit", "application/x-stuffit");
			ExtensionMap.Add(".spl", "application/futuresplash");
			ExtensionMap.Add(".sct", "text/scriptlet");
			ExtensionMap.Add(".scd", "application/x-msschedule");
			ExtensionMap.Add(".sst", "application/vndms-pkicertstore");
			ExtensionMap.Add(".src", "application/x-wais-source");
			ExtensionMap.Add(".sv4cpio", "application/x-sv4cpio");
			ExtensionMap.Add(".swf", "application/x-shockwave-flash");
			ExtensionMap.Add(".tex", "application/x-tex");
			ExtensionMap.Add(".tgz", "application/x-compressed");
			ExtensionMap.Add(".t", "application/x-troff");
			ExtensionMap.Add(".tar", "application/x-tar");
			ExtensionMap.Add(".tr", "application/x-troff");
			ExtensionMap.Add(".tif", "image/tiff");
			ExtensionMap.Add(".txt", "text/plain");
			ExtensionMap.Add(".texinfo", "application/x-texinfo");
			ExtensionMap.Add(".trm", "application/x-msterminal");
			ExtensionMap.Add(".tiff", "image/tiff");
			ExtensionMap.Add(".tcl", "application/x-tcl");
			ExtensionMap.Add(".texi", "application/x-texinfo");
			ExtensionMap.Add(".tsv", "text/tab-separated-values");
			ExtensionMap.Add(".ustar", "application/x-ustar");
			ExtensionMap.Add(".uls", "text/iuls");
			ExtensionMap.Add(".vcf", "text/x-vcard");
			ExtensionMap.Add(".wps", "application/vnd.ms-works");
			ExtensionMap.Add(".wav", "audio/wav");
			ExtensionMap.Add(".wrz", "x-world/x-vrml");
			ExtensionMap.Add(".wri", "application/x-mswrite");
			ExtensionMap.Add(".wks", "application/vnd.ms-works");
			ExtensionMap.Add(".wmf", "application/x-msmetafile");
			ExtensionMap.Add(".wcm", "application/vnd.ms-works");
			ExtensionMap.Add(".wrl", "x-world/x-vrml");
			ExtensionMap.Add(".wdb", "application/vnd.ms-works");
			ExtensionMap.Add(".wsdl", "text/xml");
			ExtensionMap.Add(".xml", "text/xml");
			ExtensionMap.Add(".xlm", "application/vnd.ms-excel");
			ExtensionMap.Add(".xaf", "x-world/x-vrml");
			ExtensionMap.Add(".xla", "application/vnd.ms-excel");
			ExtensionMap.Add(".xls", "application/vnd.ms-excel");
			ExtensionMap.Add(".xof", "x-world/x-vrml");
			ExtensionMap.Add(".xlt", "application/vnd.ms-excel");
			ExtensionMap.Add(".xlc", "application/vnd.ms-excel");
			ExtensionMap.Add(".xsl", "text/xml");
			ExtensionMap.Add(".xbm", "image/x-xbitmap");
			ExtensionMap.Add(".xlw", "application/vnd.ms-excel");
			ExtensionMap.Add(".xpm", "image/x-xpixmap");
			ExtensionMap.Add(".xwd", "image/x-xwindowdump");
			ExtensionMap.Add(".xsd", "text/xml");
			ExtensionMap.Add(".z", "application/x-compress");
			ExtensionMap.Add(".zip", "application/x-zip-compressed");
			ExtensionMap.Add(".*", "application/octet-stream");
		}

		/// <summary>
		/// Gets the mimetype for the extension provided, using IIS to lookup the mimetype, or this fails,
		/// a dictionary lookup of common extensions to mimetypes.
		/// </summary>
		/// <param name="fileExtension">The file extension to lookup, which should include the "." e.g. ".jpg"</param>
		/// <returns>The mimetype for the extension, or "application/octet-stream" if the mimetype cannot be found.</returns>
		public static string GetMimeType(string fileExtension)
		{
			if (string.IsNullOrEmpty(fileExtension))
				return "application/octet-stream";

			fileExtension = fileExtension.ToLower();

#if MONO || DEBUG
			return MimeTypes.GetMimeMapping(fileExtension);
#endif
			try
			{
				using (ServerManager serverManager = new ServerManager())
				{
					string mimeType = "application/octet-stream";

					Microsoft.Web.Administration.Configuration config = serverManager.GetApplicationHostConfiguration();
					ConfigurationSection staticContentSection = config.GetSection("system.webServer/staticContent");
					ConfigurationElementCollection mimemaps = staticContentSection.GetCollection();

					ConfigurationElement element = mimemaps.FirstOrDefault(m => m.Attributes["fileExtension"].Value.ToString() == fileExtension);

					if (element != null)
						mimeType = element.Attributes["mimeType"].Value.ToString();

					return mimeType;
				}
			}
			catch (Exception)
			{
				// Shared hosting won't have access to the applicationhost.config file (UnauthorizedAccessException)
				// also IIS Express doesn't have ServerManager registered as a COM type (COMException)
				return MimeTypes.GetMimeMapping(fileExtension);
			}
		}

		private static string GetMimeMapping(string fileExtension)
		{
			if (string.IsNullOrEmpty(fileExtension))
				return ExtensionMap[".*"];

			fileExtension = fileExtension.ToLower();

			if (ExtensionMap.ContainsKey(fileExtension))
				return ExtensionMap[fileExtension];
			else
				return ExtensionMap[".*"];
		}
	}
}

namespace LiteRpc.Server
{
	using System;
	using System.Linq;
	using System.Web.Mvc;
	using System.Reflection;
	using System.Text;
	using System.IO;
	using System.Web.UI;

	public class RpcController : Controller
	{
		private const string MIME_JSON = "application/json";

		public ActionResult Index(string method, params object[] @params)
		{
			if (this.Request.ContentType.Equals(MIME_JSON, StringComparison.InvariantCultureIgnoreCase))
			{
				var items = method.Split('.');

				var type = HttpContext.ApplicationInstance.GetType().BaseType.Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Controller)) && t.Name == items[0] + "Controller").Single();
				var controller = DependencyResolver.Current.GetService(type) as IController;

				MethodInfo methodInfo;
				if (@params != null)
				{
					methodInfo = type.GetMethod(items[1], Array.ConvertAll(@params, o => o.GetType()));
				}
				else
				{
					methodInfo = type.GetMethod(items[1], Type.EmptyTypes);
				}

				if (methodInfo.GetCustomAttributes(typeof(ExposedAttribute), false).Length > 0)
				{
					this.ControllerContext.RequestContext.RouteData.Values["controller"] = items[0];
					this.ControllerContext.RequestContext.RouteData.Values["action"] = items[1];
					var methodArgs = methodInfo.GetParameters();

					for (int i = 0; i < methodArgs.Length; i++)
					{
						this.ControllerContext.RequestContext.RouteData.Values[methodArgs[i].Name] = @params[i];
					}

					var json = new JsonResult();

					try
					{
						controller.Execute(this.ControllerContext.RequestContext);
						json.ExecuteResult(this.ControllerContext);
						return json;
					}
					catch (Exception e)
					{
						return this.Json(new { error = e.Message });
					}
				}
				else
				{
					return this.Json(new { error = "This action is not exposed." });
				}
			}
			else
			{
				Assembly assembly = HttpContext.ApplicationInstance.GetType().BaseType.Assembly;
				if (Request.QueryString.Count > 0 && Request.Params.GetValues(null).Contains("version", StringComparer.InvariantCultureIgnoreCase))
				{
					return Json(new VersionInfo { Title = "Financial Subsystem Service Entry", Provider = "samasoft", Version = assembly.GetName().Version.ToString() }, JsonRequestBehavior.AllowGet);
				}
				else
				{
					var jsonrpc =
						(
						from c in assembly.GetTypes()
						where c.BaseType == typeof(Controller)
						from a in c.GetMethods()
						where Attribute.IsDefined(a, typeof(ExposedAttribute))
						group a by c into g
						select new RpcProcInfo { DomainName = g.Key.Name.Replace("Controller", ""), Methods = (from m in g.Key.GetMethods() where m.GetCustomAttributes(typeof(ExposedAttribute), false).Length > 0 select m) }
						);


					using (var writer = new StringWriter())
					{
						using (var html = new HtmlTextWriter(writer))
						{
							html.RenderBeginTag(HtmlTextWriterTag.Body);
							html.RenderBeginTag(HtmlTextWriterTag.H2);
							html.Write("List of Json-RPC usable actions");
							html.RenderEndTag();
							foreach (var domain in jsonrpc)
							{
								html.RenderBeginTag(HtmlTextWriterTag.H3);
								html.Write(domain.DomainName);
								html.RenderEndTag();
								html.RenderBeginTag(HtmlTextWriterTag.Table);
								foreach (var proc in domain.Methods)
								{
									html.RenderBeginTag(HtmlTextWriterTag.Tr);
									html.RenderBeginTag(HtmlTextWriterTag.Td);
									html.Write(string.Concat(domain.DomainName, "." ,proc.Name, "(", string.Join(",", proc.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name)), ")"));
									html.RenderEndTag();
									html.RenderEndTag();
								}
								html.RenderEndTag();
							}
							html.RenderEndTag();
						}
						return Content(writer.ToString());
					}

					ViewBag.Version = assembly.GetName().Version.ToString();
					return View(jsonrpc.ToList());
				}
			}

		}

		protected override JsonResult Json(object data, string contentType, Encoding contentEncoding)
		{
			if (this.ControllerContext.HttpContext.Request.ContentType == MIME_JSON)
			{
				return Json(new { result = data }, contentType, contentEncoding);
			}
			else
			{
				return Json(data, contentType, contentEncoding);
			}
		}
	}
}

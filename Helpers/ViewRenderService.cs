using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Abstractions;


namespace JewelEase.Service
{
    public class ViewRenderService : IViewRenderService
    {
        private readonly IRazorViewEngine _viewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;

        public ViewRenderService(
            IRazorViewEngine viewEngine,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider)
        {
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
        }

        public async Task<string> RenderViewToStringAsync(string viewName, object model)
        {
            var actionContext = new ActionContext(
                new DefaultHttpContext { RequestServices = _serviceProvider },
                new RouteData(),
                new ActionDescriptor());

            var viewResult = _viewEngine.FindView(actionContext, viewName, false);

            if (viewResult.View == null)
            {
                throw new ArgumentNullException($"The view '{viewName}' was not found.");
            }

            using (var sw = new StringWriter())
            {
                var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                {
                    Model = model
                };

                var tempData = new TempDataDictionary(actionContext.HttpContext, _tempDataProvider);
                var viewContext = new ViewContext(
                    actionContext,
                    viewResult.View,
                    viewData,
                    tempData,
                    sw,
                    new HtmlHelperOptions());

                await viewResult.View.RenderAsync(viewContext);
                return sw.ToString();
            }
        }
    }
}

namespace RHAds.Helpers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ViewEngines;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
    using System.IO;
    using System.Threading.Tasks;

    public static class ViewRenderHelper
    {
        public static async Task<string> RenderViewAsync(this Controller controller, string viewName, object model, bool partial = false)
        {
            controller.ViewData.Model = model;

            using (var writer = new StringWriter())
            {
                var serviceProvider = controller.HttpContext.RequestServices;
                var engine = serviceProvider.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;

                var viewResult = engine.FindView(controller.ControllerContext, viewName, !partial);

                if (!viewResult.Success)
                    throw new FileNotFoundException($"No se encontró la vista {viewName}");

                var viewContext = new ViewContext(
                    controller.ControllerContext,
                    viewResult.View,
                    controller.ViewData,
                    controller.TempData,
                    writer,
                    new HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);

                return writer.ToString();
            }
        }
    }
}

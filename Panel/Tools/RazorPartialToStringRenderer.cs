using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Panel.Tools
{
    public class RazorPartialToStringRenderer(IRazorViewEngine viewEngine, ITempDataProvider tempDataProvider, IServiceProvider serviceProvider)
    {
        public async Task<string> RenderViewToStringAsync(ControllerContext controllerContext, string viewName, string styleFolder, object model)
        {
            var viewEngineResult = viewEngine.FindView(controllerContext, viewName, false);

            if (!viewEngineResult.Success)
            {
                throw new InvalidOperationException($"Could not find the view '{viewName}'");
            }

            var view = viewEngineResult.View;
            using (var writer = new StringWriter())
            {
                var viewContext = new ViewContext(
                    controllerContext,
                    view,
                    new ViewDataDictionary(serviceProvider.GetRequiredService<IModelMetadataProvider>(), new ModelStateDictionary())
                    {
                        Model = model
                    },
                    new TempDataDictionary(controllerContext.HttpContext, tempDataProvider),
                    writer,
                    new HtmlHelperOptions()
                );

                // Read the CSS file
                // Get all files in the folder
                string[] files = Directory.GetFiles(styleFolder);
                var cssContent = "";
                foreach (string file in files)
                {
                    cssContent += await System.IO.File.ReadAllTextAsync(file);
                }

                var cssTag = $"<style>{cssContent}</style>";
                // Render the view
                await view.RenderAsync(viewContext);

                // Inject the CSS into the rendered HTML
                var htmlContent = writer.ToString();
                htmlContent = htmlContent.Replace("</head>", $"{cssTag}</head>"); 
                return htmlContent;
            }
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Panel.Tools;
using PuppeteerSharp;

namespace Panel.Controllers
{
    public class ExportPdfController(RazorPartialToStringRenderer renderer) : Controller
    {
        public async Task<IActionResult> TestExport()
        {
            try
            {
                // Check if the OS is Windows
                bool isWindows = OperatingSystem.IsWindows();

                // put your google chrome path here
                string googleChromePath = isWindows ? "C:/Program Files/Google/Chrome/Application/chrome.exe" : "/usr/bin/google-chrome";

                // launch browser
                var browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true,
                    ExecutablePath = googleChromePath
                });

                // put your css folder path here
                var styleFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "css");

                var html = await renderer.RenderViewToStringAsync(ControllerContext, "TestExport", styleFolderPath, "put your cshtml model here");

                // Create a new page and set the HTML content
                var page = await browser.NewPageAsync();
                await page.SetContentAsync(html);

                // Generate the PDF
                var pdfBytes = await page.PdfDataAsync();

                // Close the browser
                await browser.CloseAsync();

                //return pdfBytes;
                return File(pdfBytes, "application/pdf", "output.pdf");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return View("Error");
            }
        }
    }
}

using Microsoft.AspNetCore.Mvc.Razor.Extensions;
using Microsoft.AspNetCore.Razor.Language;

namespace KeepItSafer.Web
{
    public class KeepItSaferCompilationService : MvcRazorTemplateEngine
    {
        public KeepItSaferCompilationService(RazorEngine engine, RazorProject project)
            : base(engine, project)
        {
        }

        public override RazorCSharpDocument GenerateCode(RazorCodeDocument codeDocument)
        {
            RazorCSharpDocument razorCSharpDocument = base.GenerateCode(codeDocument);
            // Set breakpoint here for inspecting the generated C# code in razorCSharpDocument.GeneratedCode
            // The razor code can be inspected in the Autos or Locals window in codeDocument.Source._innerSourceDocument._content 
            return razorCSharpDocument;
        }
    }
}
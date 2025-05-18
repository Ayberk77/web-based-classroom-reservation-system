using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class TestErrorModel : PageModel
{
    public IActionResult OnGet()
    {
        throw new Exception("🔥 TEST: Global exception middleware working!");
    }
}

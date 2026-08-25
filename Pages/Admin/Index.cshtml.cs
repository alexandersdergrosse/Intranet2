using Intranet2.Sicherheit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Intranet2.Pages.Admin
{
    [Authorize(Policy = Berechtigungen.AdminBereich)]
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
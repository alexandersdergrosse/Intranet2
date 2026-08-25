using Intranet2.Services.Jobs;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Intranet2.Pages.Jobs
{
    public class IndexModel : PageModel
    {
        private readonly StellenService _stellenService;

        public IndexModel(StellenService stellenService)
        {
            _stellenService = stellenService;
        }


        public List<Stellenangebot> Stellenangebote { get; set; } = new();


        public bool Ladefehler { get; set; }


        public async Task OnGetAsync()
        {
            Stellenangebote = await _stellenService.GetStellenangeboteAsync();

            Ladefehler = Stellenangebote.Count == 0;
        }
    }
}
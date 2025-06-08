using BLL.Facade;
using BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace API.Controllers
{
    [ApiController]
    [Route("api/reports")]
    public class ReportsController : ControllerBase
    {
        private readonly ILibraryFacade _facade;

        public ReportsController(ILibraryFacade facade)
        {
            _facade = facade;
        }

        [HttpGet("paper-availability")]
        public async Task<IActionResult> ReadPaperAvailability()
        {
            var books = await _facade.ReadAllBooksAsync();
            var report = books.Select(b => new
            {
                b.Id,
                b.Title,
                AvailableNow = b.AvailableCopies,
                InitialTotal = b.InitialCopies
            });
            return Ok(report);
        }

        [HttpGet("paper-orders")]
        public async Task<IActionResult> ReadPaperOrdersReport()
        {
            var orders = await _facade.ReadAllOrdersAsync();
            var paperOrders = orders
                .Where(o => o.OrderType == Domain.Enums.BookType.Paper)
                .GroupBy(o => o.BookId)
                .Select(g => new
                {
                    BookId = g.Key,
                    Count = g.Count()
                });
            return Ok(paperOrders);
        }

        [HttpGet("digital-metrics")]
        public async Task<IActionResult> ReadDigitalMetrics()
        {
            var books = await _facade.ReadAllBooksAsync();
            var metrics = books.Select(b => new
            {
                b.Id,
                b.Title,
                ElectronicDownloads = b.DownloadCount,
                AudioPlays = b.ListenCount
            });
            return Ok(metrics);
        }
    }
}

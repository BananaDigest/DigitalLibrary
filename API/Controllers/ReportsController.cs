using BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace API.Controllers
{
    [ApiController]
    [Route("api/reports")]
    [Authorize(Roles = "Manager")]
    public class ReportsController : ControllerBase
    {
        private readonly IBookService _bookSvc;
        private readonly IOrderService _orderSvc;

        public ReportsController(IBookService bookSvc, IOrderService orderSvc)
        {
            _bookSvc = bookSvc;
            _orderSvc = orderSvc;
        }

        /// <summary>
        /// Скільки паперових примірників кожної книги доступні зараз
        /// </summary>
        [HttpGet("paper-availability")]
        public async Task<IActionResult> GetPaperAvailability()
        {
            //  в BookDto нема полів: AvailableCopies, InitialCopies
            var books = await _bookSvc.ReadAllAsync();
            var report = books.Select(b => new
            {
                b.Id,
                b.Title,
                AvailableNow = b.AvailableCopies,
                InitialTotal = b.InitialCopies
            });
            return Ok(report);
        }

        /// <summary>
        /// Скільки паперових книг замовлено користувачами
        /// </summary>
        [HttpGet("paper-orders")]
        public async Task<IActionResult> GetPaperOrdersReport()
        {
            var orders = await _orderSvc.ReadAllAsync();
            // Фільтруємо тільки замовлення паперових копій
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

        /// <summary>
        /// Скільки разів завантажували електронні та прослуховували аудіо
        /// </summary>
        [HttpGet("digital-metrics")]
        public async Task<IActionResult> GetDigitalMetrics()
        {
            var books = await _bookSvc.ReadAllAsync();
            var metrics = books.Select(b => new
            {
                b.Id,
                b.Title,
                ElectronicDownloads = b.DownloadCount,   // зі CreateBookDto/BookDto має бути поле DownloadCount
                AudioPlays = b.ListenCount      // і поле ListenCount
            });
            return Ok(metrics);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DocManager.Models;
using DocManager.Data;
using Microsoft.EntityFrameworkCore;
using DocManager_2.Models;

namespace DocManager.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _ctx;

        public HomeController(ApplicationDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<IActionResult> Index()
        {
            // Coleta apenas datas para processamento em memória
            var dados = await _ctx.Documents
                .Select(d => d.CriadoEm)
                .ToListAsync();

            var agrupamento = dados
                .GroupBy(data => data.Date)
                .Select(g => new {
                    Data = g.Key.ToString("dd/MM"),
                    Total = g.Count()
                })
                .OrderBy(g => DateTime.ParseExact(g.Data, "dd/MM", null))
                .ToList();

            ViewBag.GraficoLabels = agrupamento.Select(a => a.Data).ToArray();
            ViewBag.GraficoValores = agrupamento.Select(a => a.Total).ToArray();

            ViewBag.TotalDocs = dados.Count;

            ViewBag.Ultimos = await _ctx.Documents
                .OrderByDescending(d => d.CriadoEm)
                .Take(5)
                .ToListAsync();

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}

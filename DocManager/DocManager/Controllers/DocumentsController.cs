using DocManager.Services;
using DocManager.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using DocManager.Data;
using X.PagedList;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;

namespace DocManager.Controllers
{
    [Route("Documentos")]
public class DocumentosController : Controller
{
    private readonly IDocumentoService _svc;
    public DocumentosController(IDocumentoService svc) => _svc = svc;


        private readonly ApplicationDbContext _ctx;



      

        [HttpGet("")]
        public async Task<IActionResult> Index(string filtro, int pagina = 1, string ordenacao = "Titulo")
        {
            var lista = await _svc.ListarAsync(filtro, pagina, ordenacao);
            return View(lista);
        }

        [HttpGet("Criar")]
    public IActionResult Criar() => View(new Document());

    [HttpPost("Criar")]
    public async Task<IActionResult> Criar(Document doc, IFormFile file)
    {
            try
            {
                await _svc.CriarAsync(doc, file);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {

                throw ex;
            }
       
    }


        [HttpGet("Editar/{id}")]
        public async Task<IActionResult> Editar(int id)
        {
            var doc = await _svc.ObterPorIdAsync(id);
            return View(doc);
        }

        [HttpPost("Editar/{id}")]
        [ValidateAntiForgeryToken]
       
        public async Task<IActionResult> Editar(Document doc, IFormFile file)
        {
            //if (!ModelState.IsValid) return View(doc);
            await _svc.AtualizarAsync(doc, file);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("Detalhes/{id}")]
        public async Task<IActionResult> Detalhes(int id)
        {
            var doc = await _svc.ObterPorIdAsync(id);
            return View(doc);
        }
        [HttpGet("Download/{id}")]
        public async Task<IActionResult> Download(int id)
        {
            try
            {
                var docDown = await _svc.DownloadAsync(id);

                // Retorna FileResult prontinho
                return File(
                    docDown.Conteudo,
                    docDown.ContentType,
                    docDown.NomeArquivo
                );
            }
            catch (FileNotFoundException)
            {
                return NotFound();
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpPost("Excluir/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Excluir(int id)
        {
            await _svc.ExcluirAsync(id);
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        [Route("api/grafico-documentos")]
        public async Task<IActionResult> GraficoDocumentos()
        {
            var dados = await _ctx.Documents
                .Select(d => d.CriadoEm)
                .ToListAsync();

            var agrupado = dados
                .GroupBy(data => data.Date)
                .Select(g => new {
                    label = g.Key.ToString("dd/MM"),
                    value = g.Count()
                })
                .OrderBy(g => DateTime.ParseExact(g.label, "dd/MM", null))
                .ToList();

            return Json(agrupado);
        }
    }

}

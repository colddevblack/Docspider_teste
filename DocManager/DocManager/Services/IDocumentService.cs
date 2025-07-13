// IDocumentService.cs
using DocManager.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocManager.Services
{
   public interface IDocumentoService
{
        Task<Paginacao<Document>> ListarAsync(string filtro, int pagina, string ordenacao);
    Task<Document> ObterPorIdAsync(int id);
    Task CriarAsync(Document doc, IFormFile file);
    Task AtualizarAsync(Document doc, IFormFile file);
    Task ExcluirAsync(int id);
        Task<DocumentoDownload> DownloadAsync(int id);
    }

}

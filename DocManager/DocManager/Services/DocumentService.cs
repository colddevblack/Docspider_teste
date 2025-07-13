// DocumentService.cs
using X.PagedList;
using DocManager.Data;
using DocManager.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;


namespace DocManager.Services
{
    public class DocumentoService : IDocumentoService
    {
        private readonly ApplicationDbContext _ctx;
        private readonly IWebHostEnvironment _env;
        private readonly string[] _extensoesBloqueadas = { ".exe", ".zip", ".bat" };

        public DocumentoService(ApplicationDbContext ctx, IWebHostEnvironment env)
        {
            _ctx = ctx;
            _env = env;
        }
        public async Task<Paginacao<Document>> ListarAsync(string filtro, int pagina, string ordenacao)
        {
            const int tamanhoPagina = 10;
            var query = _ctx.Documents.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtro))
                query = query.Where(d => d.Titulo.Contains(filtro));

            query = ordenacao switch
            {
                "Titulo" => query.OrderBy(d => d.Titulo),
                "CriadoEm" => query.OrderByDescending(d => d.CriadoEm),
                _ => query.OrderBy(d => d.Titulo),
            };

            var totalItens = await query.CountAsync();
            var itens = await query
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .ToListAsync();

            return new Paginacao<Document>
            {
                PaginaAtual = pagina,
                TamanhoPagina = tamanhoPagina,
                TotalItens = totalItens,
                Itens = itens
            };
        }

        public async Task CriarAsync(Document doc, IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new Exception("Arquivo não fornecido.");

            // Gera caminho físico e nome seguro
            var pastaUploads = Path.Combine("wwwroot", "uploads");

            // Cria o diretório se não existir
            if (!Directory.Exists(pastaUploads))
                Directory.CreateDirectory(pastaUploads);

            var nomeArquivo = Path.GetFileName(file.FileName);
            var caminho = Path.Combine(pastaUploads, nomeArquivo);

            using (var stream = new FileStream(caminho, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            doc.NomeArquivo = nomeArquivo;
            doc.CaminhoFisico = caminho;
            doc.CriadoEm = DateTime.Now;

            _ctx.Documents.Add(doc);
            await _ctx.SaveChangesAsync();


        }


        public async Task AtualizarAsync(Document doc, IFormFile file)
        {
            var existente = await _ctx.Documents.FindAsync(doc.Id)
                           ?? throw new Exception("Documento não encontrado.");

            existente.Titulo = doc.Titulo;
            existente.Descricao = doc.Descricao;

            if (file != null && file.Length > 0)
            {
                ValidarExtensao(file);

                // Deleta o arquivo antigo
                var velho = Path.Combine(_env.WebRootPath, existente.CaminhoFisico.TrimStart('/'));
                if (File.Exists(velho))
                    File.Delete(velho);

                // Salva o novo
                var nomeSeguro = Guid.NewGuid() + "_" + Path.GetFileName(file.FileName);
                var uploads = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploads);

                var caminhoFisico = Path.Combine(uploads, nomeSeguro);
                await using var stream = new FileStream(caminhoFisico, FileMode.Create);
                await file.CopyToAsync(stream);

                existente.NomeArquivo = nomeSeguro;
                existente.CaminhoFisico = "/uploads/" + nomeSeguro;
            }

            await _ctx.SaveChangesAsync();
        }

        // Implementar ListarAsync, ObterPorIdAsync, ExcluirAsync,...

        private void ValidarExtensao(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (_extensoesBloqueadas.Contains(ext))
                throw new InvalidOperationException($"Upload de {ext} não permitido.");
        }



        public async Task<Document> ObterPorIdAsync(int id)
        {
            var doc = await _ctx.Documents.FindAsync(id);
            if (doc == null) throw new Exception("Documento não encontrado.");
            return doc;
        }

        public async Task ExcluirAsync(int id)
        {
            var doc = await _ctx.Documents.FindAsync(id)
                  ?? throw new Exception("Documento não encontrado.");

            // Exclui o arquivo físico
            var caminhoFisico = Path.Combine(_env.WebRootPath, doc.CaminhoFisico.TrimStart('/'));
            if (File.Exists(caminhoFisico))
                File.Delete(caminhoFisico);

            _ctx.Documents.Remove(doc);
            await _ctx.SaveChangesAsync();
        }

        public async Task<DocumentoDownload> DownloadAsync(int id)
        {
            // 1) Busca metadados
            var doc = await _ctx.Documents.FindAsync(id)
                      ?? throw new FileNotFoundException("Documento não encontrado no banco.");

            // 2) Monta o caminho absoluto: wwwroot/uploads/<NomeArquivo>
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
            var absPath = Path.Combine(uploadsFolder, doc.NomeArquivo);

            // 3) Garante que o arquivo exista
            if (!File.Exists(absPath))
                throw new FileNotFoundException($"Arquivo não existe em disco: {absPath}", absPath);

            // 4) Lê bytes e infere MIME
            var bytes = await File.ReadAllBytesAsync(absPath);
            var ext = Path.GetExtension(absPath).ToLowerInvariant();
            var contentType = ext switch
            {
                ".pdf" => "application/pdf",
                ".txt" => "text/plain",
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                _ => "application/octet-stream"
            };

            return new DocumentoDownload
            {
                Conteudo = bytes,
                ContentType = contentType,
                NomeArquivo = doc.NomeArquivo
            };
        }

    }
}

using System;
using System.IO;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using DocManager.Controllers;
using DocManager.Models;
using DocManager.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace DocManager.Tests.Controllers
{
    public class DocumentosControllerTests
    {
        private readonly Mock<IDocumentoService> _svcMock;
        private readonly DocumentosController _ctrl;

        public DocumentosControllerTests()
        {
            _svcMock = new Mock<IDocumentoService>(MockBehavior.Strict);
            _ctrl = new DocumentosController(_svcMock.Object);
        }

        // Helper para criar um IFormFile falso
        private static IFormFile MakeFakeFile(
            string fileName = "teste.txt",
            string content = "conteúdo de teste")
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;

            return new FormFile(stream, 0, stream.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "text/plain"
            };
        }

        [Fact]
        public async Task Index_DeveRetornarViewComModelo()
        {
            // arrange
            var page = new Paginacao<Models.Document>
            {
                PaginaAtual = 2,
                TamanhoPagina = 10,
                TotalItens = 1,
                Itens = { new Models.Document { Id = 42, Titulo = "Doc42" } }
            };
            _svcMock
                .Setup(s => s.ListarAsync("filtro", 2, "CriadoEm"))
                .ReturnsAsync(page);

            // act
            var result = await _ctrl.Index("filtro", pagina: 2, ordenacao: "CriadoEm");

            // assert
            var vr = Assert.IsType<ViewResult>(result);
            Assert.Same(page, vr.Model);
            _svcMock.VerifyAll();
        }

        [Fact]
        public void Criar_Get_DeveRetornarViewComDocumentoVazio()
        {
            // act
            var result = _ctrl.Criar();

            // assert
            var vr = Assert.IsType<ViewResult>(result);
            Assert.IsType<Models.Document>(vr.Model);
        }

        [Fact]
        public async Task Criar_Post_Sucesso_DeveRedirecionarParaIndex()
        {
            // arrange
            var doc = new Models.Document { Titulo = "Novo" };
            var file = MakeFakeFile("arquivo.pdf", "dados");
            _svcMock
                .Setup(s => s.CriarAsync(doc, file))
                .Returns(Task.CompletedTask);

            // act
            var result = await _ctrl.Criar(doc, file);

            // assert
            var rr = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(_ctrl.Index), rr.ActionName);
            _svcMock.Verify(s => s.CriarAsync(doc, file), Times.Once);
        }

        [Fact]
        public async Task Criar_Post_Falha_DevePropagarExcecao()
        {
            // arrange
            var doc = new Models.Document();
            var file = MakeFakeFile();
            _svcMock
                .Setup(s => s.CriarAsync(doc, file))
                .ThrowsAsync(new InvalidOperationException("erro"));

            // act & assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _ctrl.Criar(doc, file));
        }

        [Fact]
        public async Task Editar_Get_DeveRetornarViewComDocumento()
        {
            // arrange
            var doc = new Models.Document { Id = 7, Titulo = "T7" };
            _svcMock
                .Setup(s => s.ObterPorIdAsync(7))
                .ReturnsAsync(doc);

            // act
            var result = await _ctrl.Editar(7);

            // assert
            var vr = Assert.IsType<ViewResult>(result);
            Assert.Same(doc, vr.Model);
        }

        [Fact]
        public async Task Editar_Post_Sucesso_DeveRedirecionarParaIndex()
        {
            // arrange
            var doc = new Models.Document { Id = 13, Titulo = "Atualizado" };
            var file = MakeFakeFile("upd.txt", "xyz");
            _svcMock
                .Setup(s => s.AtualizarAsync(doc, file))
                .Returns(Task.CompletedTask);

            // act
            var result = await _ctrl.Editar(doc, file);

            // assert
            var rr = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(_ctrl.Index), rr.ActionName);
            _svcMock.Verify(s => s.AtualizarAsync(doc, file), Times.Once);
        }

        [Fact]
        public async Task Detalhes_DeveRetornarViewComDocumento()
        {
            // arrange
            var doc = new Models.Document { Id = 5, Titulo = "D5" };
            _svcMock
                .Setup(s => s.ObterPorIdAsync(5))
                .ReturnsAsync(doc);

            // act
            var result = await _ctrl.Detalhes(5);

            // assert
            var vr = Assert.IsType<ViewResult>(result);
            Assert.Same(doc, vr.Model);
        }

        [Fact]
        public async Task Excluir_DeveRedirecionarParaIndex()
        {
            // arrange
            _svcMock
                .Setup(s => s.ExcluirAsync(99))
                .Returns(Task.CompletedTask);

            // act
            var result = await _ctrl.Excluir(99);

            // assert
            var rr = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(_ctrl.Index), rr.ActionName);
            _svcMock.Verify(s => s.ExcluirAsync(99), Times.Once);
        }
    }
}

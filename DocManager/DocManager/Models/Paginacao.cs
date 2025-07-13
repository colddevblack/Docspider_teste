using System.Collections.Generic;
using System;

namespace DocManager.Models
{
    public class Paginacao<T>
    {
        public int PaginaAtual { get; set; }
        public int TamanhoPagina { get; set; }
        public int TotalItens { get; set; }
        public int TotalPaginas => (int)Math.Ceiling((double)TotalItens / TamanhoPagina);
        public List<T> Itens { get; set; } 
    }
}

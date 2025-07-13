using System;
using System.ComponentModel.DataAnnotations;

namespace DocManager.Models
{


public class Document
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Titulo { get; set; }

    [StringLength(2000)]
    public string Descricao { get; set; }

    [Required]
    public string NomeArquivo { get; set; }

    [Required]
    public string CaminhoFisico { get; set; }

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}

}
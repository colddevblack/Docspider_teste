namespace DocManager.Models
{
    public class DocumentoDownload
    {
        public byte[] Conteudo { get; set; }
        public string ContentType { get; set; }
        public string NomeArquivo { get; set; }
    }
}

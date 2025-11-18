namespace Avant.Api.Dtos
{
    public class LinkDto
    {
        public string Rel { get; set; } = default!;
        public string Href { get; set; } = default!;
        public string Metodo { get; set; } = default!;
    }

    public class ResultadoPaginadoDto<T>
    {
        public IEnumerable<T> Itens { get; set; } = Enumerable.Empty<T>();
        public int Pagina { get; set; }
        public int TamanhoPagina { get; set; }
        public int TotalItens { get; set; }
        public IEnumerable<LinkDto> Links { get; set; } = new List<LinkDto>();
    }
}

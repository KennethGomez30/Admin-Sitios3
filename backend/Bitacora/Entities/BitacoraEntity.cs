namespace Bitacora.Entities
{
    public class BitacoraEntity
    {
        public int IdBitacora { get; set; }
        public DateTime FechaBitacora { get; set; }
        public string? Usuario { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }

    public class BitacoraRequest
    {
        public string? Usuario { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }

    public class BusinessLogicResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? ResponseObject { get; set; }
    }
}
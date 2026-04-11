namespace proyecto_SISIE.Models.DTOs;

public class CategoriaDTO
{
    public int Id { get; set; }
    public string NombreCategoria { get; set; } = string.Empty;
}

public class CategoriaCreateDTO
{
    public string NombreCategoria { get; set; } = string.Empty;
}
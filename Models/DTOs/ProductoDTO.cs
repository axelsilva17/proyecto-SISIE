namespace proyecto_SISIE.Models.DTOs;

public class ProductoDTO
{
    public int Id { get; set; }
    
    public string NombreProducto { get; set; } = string.Empty;
    
    public string? Descripcion { get; set; }
    
    public decimal PrecioUnitario { get; set; }
    
    public int Stock { get; set; }
    
    public int IdCategoria { get; set; }
    
    public string? NombreCategoria { get; set; }
    public DateTime FechaCreacion { get; set; }
    public bool Activo { get; set; }
}

public class ProductoListDTO
{
    public int Id { get; set; }
    
    public string NombreProducto { get; set; } = string.Empty;
    
    public string? Descripcion { get; set; }
    
    public decimal PrecioUnitario { get; set; }
    
    public int Stock { get; set; }
    
    public int IdCategoria { get; set; }
    
    public string? NombreCategoria { get; set; }
    public DateTime FechaCreacion { get; set; }
    public bool Activo { get; set; }
}

public class ProductoCreateDTO
{
    public string NombreProducto { get; set; } = string.Empty;
    
    public string? Descripcion { get; set; }
    
    public decimal PrecioUnitario { get; set; }
    
    public int Stock { get; set; }
    
    public int IdCategoria { get; set; }
}

public class ProductoUpdateDTO
{
    public int Id { get; set; }
    
    public string NombreProducto { get; set; } = string.Empty;
    
    public string? Descripcion { get; set; }
    
    public decimal PrecioUnitario { get; set; }
    
    public int Stock { get; set; }
    
    public int IdCategoria { get; set; }
}

public class ProductoPagedResult
{
    public IEnumerable<ProductoListDTO> Items { get; set; } = Enumerable.Empty<ProductoListDTO>();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
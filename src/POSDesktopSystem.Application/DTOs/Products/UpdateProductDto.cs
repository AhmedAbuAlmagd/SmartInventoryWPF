namespace POSDesktopSystem.Application.DTOs.Products;

public class UpdateProductDto : CreateProductDto
{
    public bool IsActive { get; set; }
}

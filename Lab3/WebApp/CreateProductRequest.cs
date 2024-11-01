namespace WebApp;

public class CreateProductRequest
{
    public string ProductName { get; set; } = string.Empty;

    public decimal ProductPrice { get; set; }
}
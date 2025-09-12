namespace WIB.Domain;

public class Store
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Chain { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
}

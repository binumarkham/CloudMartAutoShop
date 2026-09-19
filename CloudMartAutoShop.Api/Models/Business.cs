using System.ComponentModel.DataAnnotations;

namespace CloudMartAutoShop.Api.Models;

public class Business
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<User> Users { get; set; }
        = new List<User>();

    public ICollection<Customer> Customers { get; set; }
        = new List<Customer>();

    public ICollection<Supplier> Suppliers { get; set; }
        = new List<Supplier>();
}
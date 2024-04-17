using System.ComponentModel.DataAnnotations.Schema;
using Shared.Models.Identity;

namespace Shared.Models;

public class Collection
{
    public Guid CollectionId { get; set; }
    public string UserEmail { get; set; }
    public string Name { get; set; } = null!;
    public List<Plant> Plants { get; set; } = new();
}
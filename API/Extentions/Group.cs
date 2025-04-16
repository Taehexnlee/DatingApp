using System;
using System.ComponentModel.DataAnnotations;

namespace API.Extentions;

public class Group
{
    [Key]
    public required string Name { get; set; }
    public ICollection<Connection> Connections { get; set; } =[];
}

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Entities
{
    public class Group
    {
        public Group()
        {
            
        }

        public Group(string name)
        {
            Name = name;
        }

        // AKA GroupName
        [Key]
        public string Name { get; set; }
        public ICollection<Connection> Connections { get; set; } = new List<Connection>();
    }
}
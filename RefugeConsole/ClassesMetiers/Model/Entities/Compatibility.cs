using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RefugeConsole.ClassesMetiers.Model.Entities
{
    internal class Compatibility
    {        
        public Compatibility(string type)
            : this(Guid.NewGuid(), type) 
        {
        
        }

        public Compatibility(Guid id, string type) {

            this.Id = id;
            this.Type = type;
        }

        [Key]
        public Guid Id { get; private set; }
        [Required]
        public string Type { get; set; }

        public override string ToString()
        {
            return string.Format(
                "Compatibility{{ id = {0}, type = {1}  }}",
                this.Id,
                this.Type
            );
        }
    }
}

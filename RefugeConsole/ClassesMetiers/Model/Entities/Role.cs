using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RefugeConsole.ClassesMetiers.Model.Entities
{
    internal class Role
    {

        public Role(string name) 
            :this(Guid.NewGuid(), name) 
        {
        }

        public Role(Guid id, string name)
        {
            this.Id = id;
            this.Name = name;
        }

        [Key]
        public Guid Id { get; private set; }
        [Required]
        public string Name {  get; set; }


        public override string ToString()
        {
            return string.Format(
                "Role{{ id = {0}, type = {1} }}",
                this.Id,
                this.Name
            );
        }
    }
}

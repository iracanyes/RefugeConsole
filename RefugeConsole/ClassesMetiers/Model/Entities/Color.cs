using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RefugeConsole.ClassesMetiers.Model.Entities
{
    internal class Color
    {
        public Color(string name) 
            : this(Guid.NewGuid(), name)
        { }

        public Color(Guid id, string name)
        {
            Id = id;
            Name = name;
        }

        [Key]
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }


    }
}

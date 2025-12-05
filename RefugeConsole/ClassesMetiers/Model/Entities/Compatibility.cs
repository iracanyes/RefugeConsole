using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RefugeConsole.ClassesMetiers.Model.Entities
{
    internal class Compatibility
    {        
        public Compatibility(string type, string value, string description, Animal animal)
            : this(Guid.NewGuid(), type, value, description, animal) 
        {
        
        }

        public Compatibility(Guid id, string type, string value, string description, Animal animal) {
            ArgumentNullException.ThrowIfNull(animal, nameof(animal));

            this.Id = id;
            this.Type = type;
            this.Value = value;
            this.Description = description;
            
            this.Animal = animal;
            this.AnimalId = animal.Id;
        }

        [Key]
        public Guid Id { get; private set; }
        [Required]
        public string Type { get; set; }
        [Required]
        public string Value { get; set; }

        public string Description { get; set; }
        
        public string AnimalId { get; set; }

        public Animal Animal { get; set; }

        public override string ToString()
        {
            return string.Format(
                "Compatibility{{ id = {0}, type = {1}, value = {2}, description = {3}, animal = {4} }}",
                this.Id,
                this.Type,
                this.Value,
                this.Description,
                this.Animal
            );
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RefugeConsole.ClassesMetiers.Model.Entities
{
    internal class AnimalCompatibility
    {        
        public AnimalCompatibility(string value, string description, Animal animal, Compatibility compatibility)
            : this(Guid.NewGuid(), value, description, animal, compatibility) 
        { }

        public AnimalCompatibility(Guid id, string value, string description, Animal animal, Compatibility compatibility) {
            ArgumentNullException.ThrowIfNull(animal, nameof(animal));
            ArgumentNullException.ThrowIfNull(compatibility, nameof(compatibility));


            this.Id = id;
            this.Value = value;
            this.Description = description;
            
            this.Animal = animal;
            this.AnimalId = animal.Id;

            this.Compatibility = compatibility;
            this.CompatibilityId = compatibility.Id;
        }

        [Key]
        public Guid Id { get; private set; }
        
        [Required]
        public string Value { get; set; }

        public string Description { get; set; }
        
        public string AnimalId { get; set; }

        public Animal Animal { get; set; }

        public Guid CompatibilityId { get; set; }

        public Compatibility Compatibility { get; set; }

        public override string ToString()
        {
            return string.Format(
                "AnimalCompatibility{{ id = {0}, value = {2}, description = {3}, compatibility = {4}, animal = {5},  }}",
                this.Id,
                this.Value,
                this.Description,
                this.Animal,
                this.Compatibility
            );
        }
    }
}

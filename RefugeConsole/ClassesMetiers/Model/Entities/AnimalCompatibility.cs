using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RefugeConsole.ClassesMetiers.Model.Entities
{
    internal class AnimalCompatibility
    {        
        public AnimalCompatibility(Animal animal, Compatibility compatibility, bool? value, string? description)
            : this(Guid.NewGuid(), animal, compatibility, value, description) 
        { }

        public AnimalCompatibility(Guid id, Animal animal, Compatibility compatibility, bool? value, string? description) {
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
        public bool? Value { get; set; }

        public string? Description { get; set; }
        
        public string AnimalId { get; set; }

        public Animal Animal { get; set; }

        public Guid CompatibilityId { get; set; }

        public Compatibility Compatibility { get; set; }

        public override string ToString()
        {
            return string.Format(
                "AnimalCompatibility{{ id = {0}, value = {1}, description = {2}, compatibility = {3}, animal = {4}  }}",
                this.Id,
                this.Value,
                this.Description,
                this.Animal,
                this.Compatibility
            );
        }
    }
}

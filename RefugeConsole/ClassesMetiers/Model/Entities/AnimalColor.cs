using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RefugeConsole.ClassesMetiers.Model.Entities
{
    internal class AnimalColor
    {
        public AnimalColor(Animal animal, Color color)
            : this(Guid.NewGuid(), animal, color) { }

        public AnimalColor(Guid id, Animal animal, Color color)
        {
            ArgumentNullException.ThrowIfNull(animal, nameof(animal));
            ArgumentNullException.ThrowIfNull(color, nameof(color));  
            
            Id = id;

            AnimalId  = animal.Id;
            Animal = animal;

            ColorId = color.Id;
            Color = color;
        }

        [Key]
        public Guid Id { get; set; }

        public string AnimalId { get; set; }

        public Animal Animal { get; set; }

        public Guid ColorId { get; set; }

        public Color Color { get; set; }
    }
}

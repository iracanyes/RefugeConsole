using RefugeConsole.ClassesMetiers.Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RefugeConsole.ClassesMetiers.Model.Entities
{
    internal class Release
    {
        public Release(DateTime dateCreated, DateOnly dateStart, DateOnly dateEnd, Contact contact, Animal animal)
            : this(Guid.NewGuid(), dateCreated, dateStart, dateEnd, contact, animal) 
        { }

        public Release(Guid id, DateTime dateCreated, DateOnly dateStart, DateOnly dateEnd, Contact contact, Animal animal)
        {
            ArgumentNullException.ThrowIfNull(contact, nameof(contact));
            ArgumentNullException.ThrowIfNull(animal, nameof(animal));

            this.Id = id;
            this.DateCreated = dateCreated;
            this.DateStart = dateStart;
            this.DateEnd = dateEnd;

            this.ContactId = contact.Id;
            this.Contact = contact;

            this.AnimalId = animal.Id;
            this.Animal = animal;
        }

        [Key]
        public Guid Id { get; private set; }
        [Required]
        public DateTime DateCreated { get; set; }
        [Required]
        public DateOnly DateStart { get; set; }

        public DateOnly DateEnd { get; set; }

        [Required]
        public Guid ContactId { get; set; }
        public Contact Contact { get; set; }

        [Required]
        public string AnimalId { get; set; }
        public Animal Animal { get; set; }



        public override string ToString()
        {
            return string.Format(
                "Release{{ id = {0}, type = {1}, dateCreated = {2}, dateStart = {3}, dateEnd = {4}, contact = {5}, animal = {6}}}",
                this.Id, 
                this.DateCreated, 
                this.DateStart,
                this.DateEnd,
                this.Contact,
                this.Animal
            );
        }
    }
}

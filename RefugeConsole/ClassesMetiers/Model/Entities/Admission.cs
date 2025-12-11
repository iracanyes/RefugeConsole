using Microsoft.Extensions.Logging;
using RefugeConsole.ClassesMetiers.Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RefugeConsole.ClassesMetiers.Model.Entities
{
    internal class Admission
    {
        public static readonly ILogger MyLogger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger(nameof(Admission));


        public Admission(string reason, DateTime dateCreated, Contact contact, Animal animal) 
            : this(Guid.NewGuid(), reason, dateCreated, contact, animal)
        { }

        public Admission(Guid id, string reason, DateTime dateCreated, Contact contact, Animal animal)
        {
            ArgumentNullException.ThrowIfNull(contact, nameof(contact));
            ArgumentNullException.ThrowIfNull(animal, nameof(animal));

            this.Id = id;
            this.Reason = reason;
            this.DateCreated = dateCreated;

            this.ContactId = contact.Id;
            this.Contact = contact;

            this.AnimalId = animal.Id;
            this.Animal = animal;
        }

        [Key]
        public Guid Id { get; private set; }

        [Required]
        public string Reason { get; set; }

        [Required]
        public DateTime DateCreated { get; set; }

        
        public Guid ContactId { get; set; }
        
        public Contact Contact { get; set; }

        
        public string AnimalId { get; set; }
        
        public Animal Animal { get; set; }

        

        public override string ToString() {
            return string.Format(
                "Admission{{ id = {0}, reason = {1}, dateCreated = {2}, contact = {3}, animal = {4} }}",
                this.Id,
                this.Reason, 
                this.DateCreated,
                this.Contact,
                this.Animal
            );
        }

    }
}

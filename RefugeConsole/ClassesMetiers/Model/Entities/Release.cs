using RefugeConsole.ClassesMetiers.Helper;
using RefugeConsole.ClassesMetiers.Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RefugeConsole.ClassesMetiers.Model.Entities
{
    internal class Release
    {
        public Release(ReleaseType reason, DateTime dateCreated, Animal animal, Contact? contact = null)
            : this(Guid.NewGuid(), reason, dateCreated, animal, contact) 
        { }

        public Release(Guid id, ReleaseType reason, DateTime dateCreated, Animal animal, Contact? contact = null)
        {
            
            ArgumentNullException.ThrowIfNull(animal, nameof(animal));

            this.Id = id;
            this.Reason = MyEnumHelper.GetEnumDescription<ReleaseType>(reason);
            this.DateCreated = dateCreated;

            if(contact != null)
            {
                this.ContactId = contact.Id;
                this.Contact = contact;

            }
            

            this.AnimalId = animal.Id;
            this.Animal = animal;
        }

        [Key]
        public Guid Id { get; private set; }

        [Required]
        public string Reason { get; set; }

        [Required]
        public DateTime DateCreated { get; set; }

        
        public Guid? ContactId { get; set; }
        public Contact? Contact { get; set; }

        [Required]
        public string AnimalId { get; set; }
        public Animal Animal { get; set; }



        public override string ToString()
        {
            return string.Format(
                "Release{{ id = {0}, reason = {1}, dateCreated = {2}, contact = {5}, animal = {6}}}",
                this.Id, 
                this.Reason,
                this.DateCreated, 
                this.Contact,
                this.Animal
            );
        }
    }
}

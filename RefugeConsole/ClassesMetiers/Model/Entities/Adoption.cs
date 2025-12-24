using RefugeConsole.ClassesMetiers.Helper;
using RefugeConsole.ClassesMetiers.Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RefugeConsole.ClassesMetiers.Model.Entities
{
    internal class Adoption
    {
        public Adoption(ApplicationStatus status, DateTime dateCreated, DateOnly dateStart, DateOnly? dateEnd, Contact contact, Animal animal)
            : this(Guid.NewGuid(), status, dateCreated, dateStart, dateEnd, contact, animal)
        {
        }

        public Adoption(Guid id, ApplicationStatus status, DateTime dateCreated, DateOnly dateStart, DateOnly? dateEnd, Contact contact, Animal animal)
        {
            ArgumentNullException.ThrowIfNull(contact, nameof(contact));
            ArgumentNullException.ThrowIfNull(animal, nameof(animal));


            this.Id = id;
            this.Status = MyEnumHelper.GetEnumDescription<ApplicationStatus>(status);
            this.DateCreated = dateCreated;
            DateStart = dateStart;
            DateEnd = dateEnd;

            this.Contact = contact;
            this.ContactId = contact.Id;

            this.Animal = animal;
            this.AnimalId = animal.Id;
        }


        [Required]
        public Guid Id { get; set; }

        [Required]
        public string Status { get; set; }

        [Required] 
        public DateTime DateCreated { get; set; }

        [Required]
        public DateOnly DateStart { get; set; }
        public DateOnly? DateEnd {
            get;
            set
            {
                if (value != null && DateStart > value)
                    throw new ArgumentOutOfRangeException("End date can't be before start date!");
                field = value;
            } 
        }

        public string AnimalId { get; set; }
        public Animal Animal { get; set; }

        
        public Guid ContactId {  get; set; }
        public Contact Contact { get; set; }

        public override string ToString()
        {
            return string.Format(
                "Adoption{{ id = {0}, contactType = {1}, dateCreated = {2}, DateStart = {3}, dateEnd = {4}, contact = {5}, animal = {6} }}",
                this.Id,
                this.Status,
                this.DateCreated,
                this.DateStart,
                this.DateEnd, 
                this.Contact,
                this.Animal
            );
        }
    }
}

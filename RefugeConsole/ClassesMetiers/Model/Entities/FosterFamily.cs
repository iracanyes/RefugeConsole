using RefugeConsole.ClassesMetiers.Helper;
using RefugeConsole.ClassesMetiers.Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RefugeConsole.ClassesMetiers.Model.Entities
{
    internal class FosterFamily
    {
        public FosterFamily(DateTime dateCreated, DateOnly dateStart, DateOnly? dateEnd, Contact contact, Animal animal)
            : this(Guid.NewGuid(), dateCreated, dateStart, dateEnd, contact, animal)
        {
        }

        public FosterFamily(Guid id, DateTime dateCreated, DateOnly dateStart, DateOnly? dateEnd, Contact contact, Animal animal)
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

        [Required]
        public Guid Id { get; set; }

        [Required] 
        public DateTime DateCreated { get; set; }

        [Required]
        public DateOnly DateStart {  get; set; }
        public DateOnly? DateEnd
        {
            get;
            set
            {
                if (DateStart > value)
                    throw new ArgumentOutOfRangeException("End date can't be before start date!");
                field = value;
            }
        }

        public string AnimalId { get; set; }
        public Animal Animal { get; set; }


        public Guid ContactId { get; set; }
        public Contact Contact { get; set; }


        public override string ToString()
        {
            return string.Format(
                "FosterFamily{{ id = {0}, dateCreated = {1}, DateStart = {2}, dateEnd = {3}, contact  = {4}, animal  = {5} }}",
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

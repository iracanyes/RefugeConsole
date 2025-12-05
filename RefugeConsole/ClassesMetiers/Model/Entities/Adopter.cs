using RefugeConsole.ClassesMetiers.Helper;
using RefugeConsole.ClassesMetiers.Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RefugeConsole.ClassesMetiers.Model.Entities
{
    internal class Adopter: Contact
    {
        public Adopter(Guid id, DateTime dateCreated, DateOnly dateStart, DateOnly dateEnd, Release release)
            : base(id, MyEnumHelper.GetEnumDescription<ContactType>(ContactType.Adopter), dateCreated)
        {
            DateStart = dateStart;
            DateEnd = dateEnd;

            Release = release;
            ReleaseId = release.Id;
        }

        public Adopter(Guid id, DateTime dateCreated, ContactInfo contactInfo, DateOnly dateStart, DateOnly dateEnd, Release release)
            : base(id, MyEnumHelper.GetEnumDescription<ContactType>(ContactType.Adopter), dateCreated, contactInfo)
        {
            DateStart = dateStart;
            DateEnd = dateEnd;

            Release = release;
            ReleaseId = release.Id;
        }

        [Required]
        public DateOnly DateStart { get; set; }
        public DateOnly DateEnd {
            get;
            set
            {
                if (DateStart > value)
                    throw new ArgumentOutOfRangeException("End date can't be before start date!");
                field = value;
            } 
        }

        public Guid ReleaseId { get; set; }
        public Release Release { get; set; }

        public override string ToString()
        {
            return string.Format(
                "Adopter{{ id = {0}, contactType = {1}, dateCreated = {2}, DateStart = {3}, dateEnd = {4} }}",
                this.Id,
                this.Type,
                this.DateCreated,
                this.DateStart,
                this.DateEnd
            );
        }
    }
}

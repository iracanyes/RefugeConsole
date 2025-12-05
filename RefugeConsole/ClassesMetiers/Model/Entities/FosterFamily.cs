using RefugeConsole.ClassesMetiers.Helper;
using RefugeConsole.ClassesMetiers.Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RefugeConsole.ClassesMetiers.Model.Entities
{
    internal class FosterFamily: Contact
    {
        public FosterFamily(Guid id, DateTime dateCreated, DateOnly dateStart, DateOnly dateEnd, Release release)
            : base(id, MyEnumHelper.GetEnumDescription<ContactType>(ContactType.FosterFamily), dateCreated)
        {
            DateStart = dateStart;
            DateEnd = dateEnd;

            Release = release;
            ReleaseId = release.Id;
        }

        public FosterFamily(Guid id, DateTime dateCreated, ContactInfo contactInfo, DateOnly dateStart, DateOnly dateEnd, Release release)
            : base(id, MyEnumHelper.GetEnumDescription<ContactType>(ContactType.FosterFamily), dateCreated, contactInfo) 
        {
            DateStart = dateStart;
            DateEnd = dateEnd;

            Release = release;
            ReleaseId = release.Id;
        }

        [Required]
        public DateOnly DateStart {  get; set; }
        public DateOnly DateEnd
        {
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
                "FosterFamily{{ id = {0}, contactType = {1}, dateCreated = {2}, contactInfo = {3} DateStart = {4}, dateEnd = {5} }}",
                this.Id,
                this.Type,
                this.DateCreated,
                this.ContactInfo,
                this.DateStart,
                this.DateEnd
            );
        }
    }
}

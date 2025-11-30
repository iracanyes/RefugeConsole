using RefugeConsole.ClassesMetiers.Helper;
using RefugeConsole.ClassesMetiers.Model.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace RefugeConsole.ClassesMetiers.Model.Entities
{
    internal class Volunteer: Contact
    {
        public Volunteer(Guid id, DateTime dateCreated)
            : base(id, MyEnumHelper.GetEnumDescription<ContactType>(ContactType.Volunteer), dateCreated)
        {
        }

        public Volunteer(Guid id, DateTime dateCreated, ContactInfo contactInfo) 
            : base(id, MyEnumHelper.GetEnumDescription<ContactType>(ContactType.Volunteer), dateCreated, contactInfo)
        {
        }

        public override string ToString()
        {
            return string.Format(
                "Volunteer{{ id = {0}, contactType = {1}, dateCreated = {2}, contactInfo = {3} }}",
                this.Id,
                this.Type,
                this.DateCreated,
                this.ContactInfo
            );
        }
    }
}

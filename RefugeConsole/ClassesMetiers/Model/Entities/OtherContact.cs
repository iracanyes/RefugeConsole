using RefugeConsole.ClassesMetiers.Helper;
using RefugeConsole.ClassesMetiers.Model.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace RefugeConsole.ClassesMetiers.Model.Entities
{
    internal class OtherContact: Contact
    {
        public OtherContact(Guid id, DateTime dateCreated)
            : base(id, MyEnumHelper.GetEnumDescription<ContactType>(ContactType.OtherContact), dateCreated)
        {
        }

        public OtherContact(Guid id, DateTime dateCreated, ContactInfo contactInfo) 
            : base(id, MyEnumHelper.GetEnumDescription<ContactType>(ContactType.OtherContact), dateCreated, contactInfo)
        {
        }

        public Admission? Admission { get; set; }

        public override string ToString()
        {
            return string.Format(
                "OtherContact{{ id = {0}, contactType = {1}, dateCreated = {2}, contactInfo = {3} }}",
                this.Id,
                this.Type,
                this.DateCreated,
                this.ContactInfo
            );
        }
    }
}

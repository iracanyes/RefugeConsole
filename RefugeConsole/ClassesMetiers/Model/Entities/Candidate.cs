using RefugeConsole.ClassesMetiers.Helper;
using RefugeConsole.ClassesMetiers.Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RefugeConsole.ClassesMetiers.Model.Entities
{
    internal class Candidate: Contact
    {
        public Candidate(Guid id, DateTime dateCreated, string applicationType, string status)
            : base(id, MyEnumHelper.GetEnumDescription<ContactType>(ContactType.Candidate), dateCreated)
        {
            this.ApplicationType = applicationType;
            this.Status = status;
        }

        public Candidate(Guid id, DateTime dateCreated, ContactInfo contactInfo, string applicationType, string status)
            : base(id, MyEnumHelper.GetEnumDescription<ContactType>(ContactType.Candidate), dateCreated, contactInfo) 
        { 
            this.ApplicationType = applicationType;
            this.Status = status;
        }

        [Required]
        public string ApplicationType { get; set; }
        [Required]
        public string Status { get; set; }

        public override string ToString()
        {
            return string.Format(
                "FosterFamily{{ id = {0}, contactType = {1}, dateCreated = {2}, applicationType = {3}, status = {4} }}",
                this.Id,
                this.Type,
                this.DateCreated,
                this.ApplicationType,
                this.Status
            );
        }
    }
}

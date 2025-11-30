using RefugeConsole.ClassesMetiers.Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefugeConsole.ClassesMetiers.Model.Entities
{
    internal class Contact
    {
        public Contact(Guid id, string type, DateTime dateCreated)
        {

            this.Id = id;
            this.Type = type;
            this.DateCreated = dateCreated;

        }

        public Contact(Guid id, string type, DateTime dateCreated, ContactInfo contactInfo) {
            ArgumentNullException.ThrowIfNull(contactInfo, nameof(contactInfo));

            this.Id = id;
            this.Type = type;
            this.DateCreated = dateCreated;

            this.ContactInfoId = contactInfo.Id;
            this.ContactInfo = contactInfo;

        }

        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Type { get; set; }

        [Required]
        public DateTime DateCreated { get; set; }
        
        public Guid ContactInfoId { get; set; }
        public ContactInfo ContactInfo { get; set; }

        public override string ToString()
        {
            return string.Format(
                "Contact{{ id = {0}, contactType = {1}, dateCreated = {2}, contactInfo = {3} }}",
                this.Id,
                this.Type,
                this.DateCreated,
                this.ContactInfo
            );
        }
    }
}

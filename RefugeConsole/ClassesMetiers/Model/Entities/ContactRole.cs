using System;
using System.Collections.Generic;
using System.Text;

namespace RefugeConsole.ClassesMetiers.Model.Entities
{
    internal class ContactRole
    {

        public ContactRole(Contact contact, Role role) 
            : this(Guid.NewGuid(), contact, role)
        { }

        public ContactRole(Guid id, Contact contact, Role role)
        {
            ArgumentNullException.ThrowIfNull(contact, nameof(contact));
            ArgumentNullException.ThrowIfNull(role, nameof(contact));

            this.Id = id;

            this.ContactId = contact.Id;
            this.Contact = contact;

            this.RoleId = role.Id;
            this.Role = role;
        }

        public Guid Id { get; private set; }

        public Guid ContactId { get; set; } = Guid.Empty;
        public Contact Contact { get; set; }
        public Guid RoleId { get; set; } = Guid.Empty;
        public Role Role { get; set; }
    }
}

using Npgsql;
using RefugeConsole.ClassesMetiers.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace RefugeConsole.CoucheAccesDB
{
    internal interface IContactDataService
    {
        public bool CreateAddress(Address address, NpgsqlTransaction transaction);

        public bool CreateContactRole(ContactRole contactRole, NpgsqlTransaction transaction);

        public Contact GetContactByRegistryNumber(string registryNumber);

        public Contact CreateContact(Contact contact);

        public bool UpdateAddress(Address address, NpgsqlTransaction transaction);

        public Contact UpdateContact(Contact contact);

        public bool DeleteContact(Contact contact);

        public HashSet<Role> GetRoles();

    }
}

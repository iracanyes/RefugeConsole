using Npgsql;
using RefugeConsole.ClassesMetiers.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace RefugeConsole.CoucheAccesDB
{
    internal interface IContactDataService
    {
        bool CreateAddress(Address address, NpgsqlTransaction? transaction = null);

        bool CreateContactRole(ContactRole contactRole, NpgsqlTransaction transaction);

        Contact GetContactByRegistryNumber(string registryNumber);

        Contact CreateContact(Contact contact);

        bool UpdateAddress(Address address, NpgsqlTransaction? transaction = null);

        Contact UpdateContact(Contact contact);

        bool DeleteContactRole(ContactRole contactRole, NpgsqlTransaction? transaction = null);

        bool DeleteContact(Contact contact);

        HashSet<Role> GetRoles();
        
        HashSet<ContactRole> GetContactRoles(Contact contact);



    }
}

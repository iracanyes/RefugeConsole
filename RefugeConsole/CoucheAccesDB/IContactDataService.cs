using RefugeConsole.ClassesMetiers.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace RefugeConsole.CoucheAccesDB
{
    internal interface IContactDataService
    {
        public Contact GetContactByRegistryNumber(string registryNumber);

        public Contact CreateContact(Contact contact);

        public Contact UpdateContact(Contact contact);

        public bool DeleteContact(Contact contact);

    }
}

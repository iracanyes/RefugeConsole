using Microsoft.Extensions.Logging;
using RefugeConsole.ClassesMetiers.Helper;
using RefugeConsole.ClassesMetiers.Model.Entities;
using RefugeConsole.ClassesMetiers.Model.Enums;
using RefugeConsole.CoucheAccesDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace RefugeConsole.CouchePresentation.View
{
    internal class ContactView
    {
        private static readonly ILogger MyLogger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger(nameof(ContactView));

        public static Contact AddContact()
        {
            Contact? contact = null;

            Console.Clear();

            Console.WriteLine("Ajouter une personne de contact: (type de contact, nom, prenom, numéro de registre national, adresse, email, numéro de téléphone, numéro de portable)");
            // Get contact type

            ContactType contactType = SharedView.EnumChoice<ContactType>("Choisissez le type de personne de contact");
            string? firstname = SharedView.InputString("Entrez votre nom : ");
            string? lastname = SharedView.InputString("Entrez votre nom : ");

            string registryNumber = SharedView.InputString("Entrez votre numéro de registre national : ");
            string address = SharedView.InputString("Entrez votre adresse : ");
            string email = SharedView.InputString("Entrez votre email : ");
            string mobileNumber = SharedView.InputString("Entrez votre numéro de portable : ");
            string phoneNumber = SharedView.InputString("Entrez votre numéro de téléphone fixe : ");

            ContactInfo contactInfo = new ContactInfo(Guid.NewGuid(), firstname, lastname, registryNumber, address, email, phoneNumber, mobileNumber);
            contact = new Contact(Guid.NewGuid(), MyEnumHelper.GetEnumDescription(contactType), new DateTime(), contactInfo);

            ArgumentNullException.ThrowIfNull(contact, nameof(Contact));

            return contact;


        }
    }
}

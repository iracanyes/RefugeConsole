using Microsoft.Extensions.Logging;
using RefugeConsole.ClassesMetiers.Helper;
using RefugeConsole.ClassesMetiers.Model.Entities;
using RefugeConsole.ClassesMetiers.Model.Enums;
using RefugeConsole.CoucheAccesDB;
using RefugeConsole.CouchePresentation.View;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace RefugeConsole.CouchePresentation.ViewModel
{
    internal class ContactViewModel
    {
        private static readonly ILogger MyLogger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger(nameof(ContactViewModel));
        private readonly IContactDataService contactDataService;
        private readonly HashSet<Role> roles = new HashSet<Role>();

        public ContactViewModel(IContactDataService contactDataService)
        {
            this.contactDataService = contactDataService;

            this.roles = contactDataService.GetRoles();
        }

        public void AddContactRole(Contact contact)
        {
            Console.WriteLine("Ajouter un rôle à un contact : ");
            try
            {

                RoleNameType roleName = SharedView.EnumChoice<RoleNameType>("Sélectionner le rôle désiré : ");
                
                Role role = roles.First(r => r.Name == MyEnumHelper.GetEnumDescription(roleName));

                contact.AddContactRole(new ContactRole(contact, role));
            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached)
                    Debug.WriteLine($"Unable to add a role to a contact.Role name : {contact}. Exception message : {ex.Message}.\nException : {ex}");
                throw new Exception($"Unable to add a role to a contact.Role name : {contact}. Exception message : {ex.Message}.\nException : {ex}");
            }
        }

        public void ViewContact() {
            try
            {
                string registryNumber = SharedView.InputString("Entrez le numéro de registre national de la personne de contact : ");

                Contact contactInfo = contactDataService.GetContactByRegistryNumber(registryNumber);                

                ContactView.DisplayContactInfo(contactInfo);


            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to add contact info!\nReason : {ex.Message}\nException : \n{ex}");
                throw new Exception($"Unable to add contact info!\nReason : {ex.Message}\nException : \n{ex}");
            }
        }

        public void AddContact() {
            bool addRole = false;
            
            try
            {
                // Capture user input for a contact instance
                Contact contactInfoData = ContactView.AddContactInfo();

                // Add roles
                addRole = SharedView.InputBoolean("Voulez-vous ajouter un rôle à la personne de contact?");
                do
                {
                    this.AddContactRole(contactInfoData);
                    addRole = SharedView.InputBoolean("Voulez-vous ajouter un rôle à la personne de contact?");

                } while (addRole);

                // Save new contact
                Contact contactInfo = contactDataService.CreateContact(contactInfoData);

                ContactView.DisplayContactInfo(contactInfo);


            }catch(Exception ex)
            {
                Debug.WriteLine($"Unable to add contact info!\nReason : {ex.Message}\nException : \n{ex}");
                throw new Exception($"Unable to add contact info!\nReason : {ex.Message}\nException : \n{ex}");
            }

            
        }

        public void UpdateContact() {
            try
            {
                // Get national registry number
                string registryNumber = SharedView.InputString("Entrez le numéro de registre national de la personne de contact : ");

                // Get contact info by registry number
                Contact contactInfo = contactDataService.GetContactByRegistryNumber(registryNumber);

                // Get updated contact info from user input
                Contact contactInfoDataToUpdate = ContactView.UpdateContactInfo(contactInfo);

                // Save updated contact info 
                Contact contactInfoUpdated = contactDataService.UpdateContact(contactInfoDataToUpdate);

                ContactView.DisplayContactInfo(contactInfoUpdated);


            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to add contact info!\nReason : {ex.Message}\nException : \n{ex}");
                throw new Exception($"Unable to add contact info!\nReason : {ex.Message}\nException : \n{ex}");
            }
        }

        public void RemoveContact() {
            try
            {
                // Get national registry number
                string registryNumber = SharedView.InputString("Entrez le numéro de registre national de la personne de contact : ");

                // Get contact info by registry number
                Contact contactInfo = contactDataService.GetContactByRegistryNumber(registryNumber);

                // Save updated contact info 
                bool result = contactDataService.DeleteContact(contactInfo);

                if(result)
                    Console.WriteLine($"Le contact avec le numéro de registre national ({registryNumber}) a été supprimé!") ;


            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to add contact info!\nReason : {ex.Message}\nException : \n{ex}");
                throw new Exception($"Unable to add contact info!\nReason : {ex.Message}\nException : \n{ex}");
            }
        }
    }
}

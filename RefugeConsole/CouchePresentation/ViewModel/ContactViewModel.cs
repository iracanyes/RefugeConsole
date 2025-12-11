using Microsoft.Extensions.Logging;
using RefugeConsole.ClassesMetiers.Model.Entities;
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

        public ContactViewModel(IContactDataService contactDataService)
        {
            this.contactDataService = contactDataService;
        }

        public void ViewContact() {
            try
            {
                string registryNumber = SharedView.InputString("Entrez le numéro de registre national de la personne de contact : ");

                Contact contactInfo = contactDataService.GetContactInfoByRegistryNumber(registryNumber);                

                ContactView.DisplayContactInfo(contactInfo);


            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to add contact info!\nReason : {ex.Message}\nException : \n{ex}");
                throw new Exception($"Unable to add contact info!\nReason : {ex.Message}\nException : \n{ex}");
            }
        }

        public void AddContact() {
            try
            {
                Contact contactInfoData = ContactView.AddContactInfo();
                Contact contactInfo = contactDataService.CreateContactInfo(contactInfoData);

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
                Contact contactInfo = contactDataService.GetContactInfoByRegistryNumber(registryNumber);

                // Get updated contact info from user input
                Contact contactInfoDataToUpdate = ContactView.UpdateContactInfo(contactInfo);

                // Save updated contact info 
                Contact contactInfoUpdated = contactDataService.UpdateContactInfo(contactInfoDataToUpdate);

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
                Contact contactInfo = contactDataService.GetContactInfoByRegistryNumber(registryNumber);

                // Save updated contact info 
                bool result = contactDataService.DeleteContactInfo(contactInfo);

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

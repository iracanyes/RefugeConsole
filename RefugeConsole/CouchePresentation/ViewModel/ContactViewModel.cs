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

        /**
         * <summary>
         *  Fonctionnalité : Ajouter un rôle à une personne de contact
         * </summary>
         * <param name="contact">
         *  Personne de contact
         * </param>
         */
        public void AddContactRole(Contact contact)
        {
            Console.WriteLine("Ajouter un rôle à un contact : ");
            try
            {
                // Sélection par l'utilisateur d'un rôle pour la personne de contact
                RoleNameType roleName = SharedView.EnumChoice<RoleNameType>("Sélectionner le rôle désiré : ");
                
                // Récupération du rôle en base de donnée
                Role role = roles.First(r => r.Name == MyEnumHelper.GetEnumDescription<RoleNameType>(roleName));

                // Ajout du rôle pour la personne de contact
                contact.AddContactRole(new ContactRole(contact, role));
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Unable to add a role to a contact.Role name : {contact}. Exception message : {ex.Message}.\nException : {ex}");

                if (Debugger.IsAttached)
                    Debug.WriteLine($"Unable to add a role to a contact.Role name : {contact}. Exception message : {ex.Message}.\nException : {ex}");
                throw new Exception($"Unable to add a role to a contact.Role name : {contact}. Exception message : {ex.Message}.\nException : {ex}");
            }
        }

        /**
         * <summary>
         *  Fonctionnalité : Ajouter une adresse à une personne de contact
         * </summary>
         * <param name="contact">
         *  Personne de contact
         * </param>
         */
        public void AddAdress(Contact contact)
        {
           
            try
            {

                // Sauvegarde de l'adresse de la personne de contact
                bool result = contactDataService.CreateAddress(contact.Address);

                // Si erreur inconnu
                if (!result)
                {
                    throw new Exception($"Unknown error while saving an address into DB.");
                }                    
                

            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached)
                    Debug.WriteLine($"Error while saving new address.\nError : {ex.Message}.\nException : {ex}");

                MyLogger.LogError($"Error while saving new address. Error : {ex.Message}. Exception : {ex}");
                
            }
        }

        /**
         * <summary>
         *  Fonctionnalité : Consulter une personne de contact
         * </summary>
         */
        public void ViewContact() {
            try
            {
                // Saisie par l'utilisateur du numéro de registre national
                string registryNumber = SharedView.InputString("Entrez le numéro de registre national de la personne de contact : ");

                // Récupération de la personne de contact
                Contact contactInfo = contactDataService.GetContactByRegistryNumber(registryNumber);
                
                // Affichage
                ContactView.DisplayContactInfo(contactInfo);


            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Unable to add contact info!\nReason : {ex.Message}\nException : \n{ex}");
                
                if(Debugger.IsAttached)
                    Debug.WriteLine($"Unable to add contact info!\nReason : {ex.Message}\nException : \n{ex}");
                
                throw new Exception($"Unable to add contact info!\nReason : {ex.Message}\nException : \n{ex}");
            }

            // Saisie par l'utilisateur d'une touche pour continuer
            SharedView.WaitForKeyPress();
        }

        /**
         * <summary>
         *  Fonctionnalité : Ajouter une personne de contact
         * </summary>
         */
        public Contact AddContact() {
            Contact? contactInfoData = null;
            Contact? result = null;
            bool addRole = false;
            bool correctInfo = false;

            // Raffraichir l'écran
            Console.Clear();
            Console.WriteLine("Ajouter une personne de contact\n=======================================\n\n");

            try
            {
                do {
                    // Saisie par l'utilisateur des informatiosn de la personne de contact
                    contactInfoData = ContactView.AddContactInfo();

                    if (contactInfoData == null) continue;

                    // Contrainte : Numéro de registre national unique!
                    if (!this.contactDataService.RegistryNumberExists(contactInfoData.RegistryNumber))
                    {
                        correctInfo = true;
                    }
                } while (!correctInfo);


                if (contactInfoData == null)
                    throw new Exception("Unexpected error while receiving contact's info from user");


                // Sauvegarde de l'adresse
                this.AddAdress(contactInfoData);
               
                // Ajout des rôles de la personne de contact
                do
                {
                    this.AddContactRole(contactInfoData);
                    addRole = SharedView.InputBoolean("Voulez-vous ajouter un rôle à la personne de contact? (§Oui/Non)");

                } while (addRole);

                // Sauvegarde de la personne de contact
                result = contactDataService.CreateContact(contactInfoData);

                // Affichage de la personne de contact
                ContactView.DisplayContactInfo(result);

               


            }catch(Exception ex)
            {
                Debug.WriteLine($"Unable to add contact info!\nReason : {ex.Message}\nException : \n{ex}");
                throw new Exception($"Unable to add contact info!\nReason : {ex.Message}\nException : \n{ex}");
            }

            // Saisie d'une touche pour continuer
            SharedView.WaitForKeyPress();

            // Raffraichir l'écran
            Console.Clear();

            return result;
        }

        /**
         * <summary>
         *  Fonctionnalité : Mettre à jour une personne de contact
         * </summary>
         */
        public void UpdateContact() {
            // Raffraichir l'écran
            Console.Clear();
            Console.WriteLine("Mettre à jour une personne de contact\n=======================================\n\n");

            try
            {
                // Saisie par l'utilisateur du numéro de registre national de la personne de contact
                string registryNumber = SharedView.InputString("Entrez le numéro de registre national de la personne de contact : ");

                // Récupération de la personne de contact par son numéro de registre national
                Contact contactInfo = contactDataService.GetContactByRegistryNumber(registryNumber);

                // Saisie des informations mises à jour pour la personne de contact
                Contact contactInfoDataToUpdate = ContactView.UpdateContactInfo(contactInfo);

                // Sauvegarde des nouvelles informations
                Contact contactInfoUpdated = contactDataService.UpdateContact(contactInfoDataToUpdate);

                // Affichage des informations mises à jour de la personne de contact
                ContactView.DisplayContactInfo(contactInfoUpdated);


            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Unable to add contact info!\nReason : {ex.Message}\nException : \n{ex}"); 

                if(Debugger.IsAttached)
                    Debug.WriteLine($"Unable to add contact info!\nReason : {ex.Message}\nException : \n{ex}");
            }

            // Saisie d'une touche pour continuer
            SharedView.WaitForKeyPress();
        }

        /**
         * <summary>
         *  Fonctionnalité : Supprimer une personne de contact
         * </summary>
         */
        public void RemoveContact() {
            // Raffraichir l'écran
            Console.Clear();
            Console.WriteLine("Supprimer une personne de contact\n=======================================\n\n");

            try
            {
                // Saisie par l'utilisateur du numéro de registre national de la personne de contact
                string registryNumber = SharedView.InputString("Entrez le numéro de registre national de la personne de contact : ");

                // Récupération de la personne de contact par son numéro de registre national
                Contact contactInfo = contactDataService.GetContactByRegistryNumber(registryNumber);

                // Supprimer la personne de contact en base de données
                bool result = contactDataService.DeleteContact(contactInfo);

                if(result)
                    Console.WriteLine($"Le contact avec le numéro de registre national ({registryNumber}) a été supprimé!") ;

                
            }
            catch (Exception ex)
            {
                if(Debugger.IsAttached)
                    Debug.WriteLine($"Unable to add contact info!\nReason : {ex.Message}\nException : \n{ex}");

                Console.Error.WriteLine($"Unable to add contact info!\nReason : {ex.Message}\nException : \n{ex}");
            }

            // Saisie d'une touche pour continuer
            SharedView.WaitForKeyPress();
        }
    }
}

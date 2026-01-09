using Microsoft.Extensions.Logging;
using RefugeConsole.ClassesMetiers.Helper;
using RefugeConsole.ClassesMetiers.Model.Entities;
using RefugeConsole.ClassesMetiers.Model.Enums;
using RefugeConsole.CoucheAccesDB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;

namespace RefugeConsole.CouchePresentation.View
{
    internal class ContactView
    {
        private static readonly ILogger MyLogger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger(nameof(ContactView));

        public static Address AddAddress()
        {
            Address? address = null;

            Console.WriteLine("Ajouter une addresse (rue, ville, province/état/département, code postal, pays");
            

            try
            {
                string street = SharedView.InputString("Entrez votre rue : ");
                string city = SharedView.InputString("Entrez votre ville : ");
                string state = SharedView.InputString("Entrez votre province : ");
                string zipCode = SharedView.InputString("Entrez votre code postal : ");
                string country = SharedView.InputString("Entrez votre pays de résidence : ");

                address = new Address(street, city, state, zipCode, country);
            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached)
                {
                    Debug.WriteLine($"Unable to accept input for an address instance. \nException message : {ex.Message}\nException : {ex}");
                }

                throw new Exception($"Unable to accept input for an address instance.\nException message : {ex.Message}\nException : {ex}");
            }

            return address;

        }



        public static Contact AddContactInfo()
        {
            Contact? contactInfo = null;
            string? registryNumber = null;
            string? email = null;
            string? mobileNumber = null;
            string? phoneNumber = null;
            bool addEmail = false;
            bool addMobileNumber = false;
            bool addPhoneNumber = false;

            Console.Clear();

            Console.WriteLine("Ajouter une personne de contact: (type de contact, nom, prenom, numéro de registre national, email, numéro de téléphone, numéro de portable, adresse)");
            // Get contact type


            string firstname = SharedView.InputString("Entrez votre nom : ", @"^\w{2,}$");
            string lastname = SharedView.InputString("Entrez votre prénom : ", @"^\w{2,}$");

            registryNumber = SharedView.InputString("Entrez votre numéro de registre national : ", @"^(\d{2})\.(0[1-9]|1[0-2])\.(0[1-9]|[1-2]\d|3[0-1])-(\d{3})\.(\d{2})$", "Le numéro de registre national doit avoir le format (yy.mm.dd-999.99)");



            // Contrainte : Au moins un moyen de contact : gsm OR telephone OR email NOT NULL
            Console.WriteLine("Ajouter au moins un moyen de contact (email, gsm, téléphone fixe) : ");

            do {
                addEmail = SharedView.InputBoolean("Ajouter un email ? (Oui/Non)");
                if (addEmail)
                    email = SharedView.InputString("Entrez votre email : ", @"^[A-Za-z0-9.%\!#$/?^|~]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$", "Email doit être dans un format valide!");

                addMobileNumber = SharedView.InputBoolean("Ajouter un numéro de portable ? (Oui/Non)");
                if (addMobileNumber)
                    mobileNumber = SharedView.InputString("Entrez votre numéro de portable : ");

                addPhoneNumber = SharedView.InputBoolean("Ajouter un numéro de téléphone fixe ? (Oui/Non)");

                if (addPhoneNumber)
                    phoneNumber = SharedView.InputString("Entrez votre numéro de téléphone fixe : ");
            } while (email == null && mobileNumber == null && phoneNumber == null);           
            



            Address address = ContactView.AddAddress();

            try
            {
               

               contactInfo = new Contact(Guid.NewGuid(), firstname, lastname, registryNumber, email, phoneNumber, mobileNumber, address);
                
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error while creating contact info from user input.\nReason :\n{ex.Message}\nException :\n{ex}");
                throw new Exception($"Error while creating contact info from user input.\nReason :\n{ex.Message}\nException :\n{ex}");
            }


            return contactInfo;


        }

        public static void DisplayContactInfo(Contact contactInfo)
        {
            Console.WriteLine(
                $"""
                
                ============================================
                Contact ID : {contactInfo.Id}
                ============================================
                firstname = {contactInfo.Firstname}
                lastname = {contactInfo.Lastname}
                registryNumber = {contactInfo.RegistryNumber}                
                email = {contactInfo.Email}
                phoneNumber = {contactInfo.PhoneNumber}
                mobileNumber = {contactInfo.MobileNumber}
                address = {contactInfo.Address}
                roles = {string.Join(", ", contactInfo.ContactRoles.Select(cr => cr.Role.Name).ToList())}
                ==============================================
                """
                    
            );

            
        }

        

        public static Contact UpdateContactInfo(Contact contactInfo) {
            Contact? result = null;

            Console.WriteLine("Mettre à jour une personne de contact: (nom, prenom, numéro de registre national, adresse, email, numéro de téléphone, numéro de portable)");
            // Get contact type


            string? firstname = SharedView.InputString($"Entrez votre nom : (Actuel = {contactInfo.Firstname})");
            string? lastname = SharedView.InputString($"Entrez votre nom : (Actuel = {contactInfo.Lastname})");

            string registryNumber = SharedView.InputString($"Entrez votre numéro de registre national : (Actuel = {contactInfo.RegistryNumber})");

            string? email = null;
            string? mobileNumber = null;
            string? phoneNumber = null;
            bool addEmail = false;
            bool addMobileNumber = false;
            bool addPhoneNumber = false;

            // Contrainte : Au moins un moyen de contact : gsm OR telephone OR email NOT NULL
            Console.WriteLine("Mettre à les informations de contact, au moins un moyen de contact obligatoire (email, gsm, téléphone fixe) : ");

            do
            {
                addEmail = SharedView.InputBoolean("Ajouter un email ? (Oui/Non)");
                if (addEmail)
                    email = SharedView.InputString($"Entrez votre email : (Actuel = {contactInfo.Email})");

                addMobileNumber = SharedView.InputBoolean("Ajouter un numéro de portable ? (Oui/Non)");
                if (addMobileNumber)
                    mobileNumber = SharedView.InputString($"Entrez votre numéro de portable : (Actuel = {contactInfo.MobileNumber})");

                addPhoneNumber = SharedView.InputBoolean("Ajouter un numéro de téléphone fixe ? (Oui/Non)");

                if (addPhoneNumber)
                    phoneNumber = SharedView.InputString($"Entrez votre numéro de téléphone fixe : (Actuel = {contactInfo.PhoneNumber})");

            } while (email == null && mobileNumber == null && phoneNumber == null);



            // Update the address 
            bool updateAddress = SharedView.InputBoolean($"?Voulez-vous mettre à jour votre addresse ? (Oui/Non) \n Données actuelle : {contactInfo.Address}");
            Address? updatedAddress = null;

            if (updateAddress) {
                Console.WriteLine("Mettre à jour votre addresse : ");

                string street = SharedView.InputString($"Entrez votre rue : ({contactInfo.Address.Street})");
                string city = SharedView.InputString($"Entrez votre ville : ({contactInfo.Address.City})");
                string state = SharedView.InputString($"Entrez votre province : ({contactInfo.Address.State})");
                string zipCode = SharedView.InputString($"Entrez votre code postal : ({contactInfo.Address.ZipCode})");
                //
                // string country = SharedView.InputString($"Entrez votre pays de résidence : ({contactInfo.Address.Country})");
                string country = "Belgique";

                updatedAddress = new Address(
                    contactInfo.Address.Id, 
                    street, 
                    city, 
                    state, 
                    zipCode, 
                    country
                );
            }

            try
            {

                
                result = new Contact(
                    contactInfo.Id, 
                    firstname, 
                    lastname, 
                    registryNumber, 
                    email, 
                    phoneNumber, 
                    mobileNumber, 
                    updateAddress ? updatedAddress! : contactInfo.Address
                );

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error while updating contact info from user input.\nReason :\n{ex.Message}\nException :\n{ex}");
                throw new Exception($"Error while updating contact info from user input.\nReason :\n{ex.Message}\nException :\n{ex}");
            }

            return result;
        }
    }
}

using Microsoft.Extensions.Logging;
using RefugeConsole.ClassesMetiers.Helper;
using RefugeConsole.ClassesMetiers.Model.Entities;
using RefugeConsole.ClassesMetiers.Model.Enums;
using RefugeConsole.CoucheAccesDB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace RefugeConsole.CouchePresentation.View
{
    internal class ContactView
    {
        private static readonly ILogger MyLogger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger(nameof(ContactView));

        public static Contact AddContactInfo()
        {
            Contact? contactInfo = null;

            Console.Clear();

            Console.WriteLine("Ajouter une personne de contact: (type de contact, nom, prenom, numéro de registre national, adresse, email, numéro de téléphone, numéro de portable)");
            // Get contact type

            
            string? firstname = SharedView.InputString("Entrez votre nom : ");
            string? lastname = SharedView.InputString("Entrez votre nom : ");

            string registryNumber = SharedView.InputString("Entrez votre numéro de registre national : ");
            string address = SharedView.InputString("Entrez votre adresse : ");
            string email = SharedView.InputString("Entrez votre email : ");
            string mobileNumber = SharedView.InputString("Entrez votre numéro de portable : ");
            string phoneNumber = SharedView.InputString("Entrez votre numéro de téléphone fixe : ");

            try
            {
               contactInfo = new Contact(Guid.NewGuid(), firstname, lastname, registryNumber, address, email, phoneNumber, mobileNumber);
                
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
                ContactInfo ID : {contactInfo.Id}
                ============================================
                firstname = {contactInfo.Firstname}
                lastname = {contactInfo.Lastname}
                registryNumber = {contactInfo.RegistryNumber}
                address = {contactInfo.Address}
                email = {contactInfo.Email}
                phoneNumber = {contactInfo.PhoneNumber}
                mobileNumber = {contactInfo.MobileNumber}
                ==============================================
                """
                    
            );

            SharedView.WaitForKeyPress();
        }

        public static Contact UpdateContactInfo(Contact contactInfo) {
            Contact? result = null;

            Console.WriteLine("Mettre à jour une personne de contact: (nom, prenom, numéro de registre national, adresse, email, numéro de téléphone, numéro de portable)");
            // Get contact type


            string? firstname = SharedView.InputString($"Entrez votre nom : (Actuel = {contactInfo.Firstname})");
            string? lastname = SharedView.InputString($"Entrez votre nom : (Actuel = {contactInfo.Lastname})");

            string registryNumber = SharedView.InputString($"Entrez votre numéro de registre national : (Actuel = {contactInfo.RegistryNumber})");
            string address = SharedView.InputString($"Entrez votre adresse : (Actuel = {contactInfo.Address})");
            string email = SharedView.InputString($"Entrez votre email : (Actuel = {contactInfo.Email})");
            string mobileNumber = SharedView.InputString($"Entrez votre numéro de portable : (Actuel = {contactInfo.MobileNumber})");
            string phoneNumber = SharedView.InputString($"Entrez votre numéro de téléphone fixe : (Actuel = {contactInfo.PhoneNumber})");

            try
            {
                result = new Contact(contactInfo.Id, firstname, lastname, registryNumber, address, email, phoneNumber, mobileNumber);

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

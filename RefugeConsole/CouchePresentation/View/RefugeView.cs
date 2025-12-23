using RefugeConsole.ClassesMetiers.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace RefugeConsole.CouchePresentation.View
{
    internal class RefugeView
    {
        public static void DisplayAdmission(Admission admission)
        {
            Console.WriteLine(
                $"""
                ============================================================
                ADMISSION ID : {admission.Id}
                ============================================================
                Raison : {admission.Reason}
                Crée le : {admission.DateCreated}
                
                Animal
                =======
                Nom : {admission.Animal.Name}
                Type :  {admission.Animal.Type}
                Sexe : {admission.Animal.Gender}
                Coleurs = [{string.Join(", ", admission.Animal.AnimalColors.Select(ac => ac.Color.Name).ToList())}]
                Date de naissance : {admission.Animal.BirthDate}
                Date de décès : {admission.Animal.DeathDate}
                Stérilisé : {(admission.Animal.IsSterilized ? "Oui" : "Non") }
                Date stérilisation :  {admission.Animal.DateSterilization}
                Particularité :
                {admission.Animal.Particularity}
                Description : 
                {admission.Animal.Description}

                Contact 
                =======
                Nom: {admission.Contact.Firstname}
                Prenom :  {admission.Contact.Lastname}
                N° de registre national : {admission.Contact.RegistryNumber}
                Email : {admission.Contact.Email}
                ============================================================
                """    
            );
        }

        public static void DisplayReleaseForFosterFamily(Release release, FosterFamily fm)
        {
            string address = $"{fm.Contact.Address.Street}, {fm.Contact.Address.City}, {fm.Contact.Address.ZipCode} {fm.Contact.Address.State}, {fm.Contact.Address.Country}";
            Console.WriteLine(
                $"""
                ============================================================
                FosterFamily ID : {fm.Id}
                ============================================================
                Sortie
                ========
                Raison : {release.Reason}
                Crée le : {release.DateCreated}
                
                Animal
                =======
                Nom : {fm.Animal.Name}
                Type :  {fm.Animal.Type}
                Sexe : {fm.Animal.Gender}
                Coleurs = [{string.Join(", ", fm.Animal.AnimalColors.Select(ac => ac.Color.Name).ToList())}]
                Date de naissance : {fm.Animal.BirthDate}
                Date de décès : {fm.Animal.DeathDate}
                Stérilisé : {(fm.Animal.IsSterilized ? "Oui" : "Non")}
                Date stérilisation :  {fm.Animal.DateSterilization}
                Particularité :
                {fm.Animal.Particularity}
                Description : 
                {fm.Animal.Description}

                Famille d'accueil 
                ===================
                Nom: {fm.Contact.Firstname}
                Prenom :  {fm.Contact.Lastname}
                N° de registre national : {fm.Contact.RegistryNumber}
                Email : {fm.Contact.Email}
                GSM : {fm.Contact.MobileNumber}
                Adresse : {address}
                ============================================================
                """
            );
        }

        public static void DisplayFosterFamily(FosterFamily fm)
        {
            string address = $"{fm.Contact.Address.Street}, {fm.Contact.Address.City}, {fm.Contact.Address.ZipCode} {fm.Contact.Address.State}, {fm.Contact.Address.Country}";
            Console.WriteLine(
                $"""
                ============================================================
                FosterFamily ID : {fm.Id}
                ============================================================
                Créé le : {fm.DateCreated}
                Début : {fm.DateStart}
                Fin : {fm.DateEnd}

                Animal
                =======
                Nom : {fm.Animal.Name}
                Type :  {fm.Animal.Type}
                Sexe : {fm.Animal.Gender}
                Coleurs = [{string.Join(", ", fm.Animal.AnimalColors.Select(ac => ac.Color.Name).ToList())}]
                Date de naissance : {fm.Animal.BirthDate}
                Date de décès : {fm.Animal.DeathDate}
                Stérilisé : {(fm.Animal.IsSterilized ? "Oui" : "Non")}
                Date stérilisation :  {fm.Animal.DateSterilization}
                Particularité :
                {fm.Animal.Particularity}
                Description : 
                {fm.Animal.Description}

                Famille d'accueil 
                ===================
                Nom: {fm.Contact.Firstname}
                Prenom :  {fm.Contact.Lastname}
                N° de registre national : {fm.Contact.RegistryNumber}
                Email : {fm.Contact.Email}
                GSM : {fm.Contact.MobileNumber}
                Adresse : {address}
                ============================================================
                """
            );
        }


    }
}

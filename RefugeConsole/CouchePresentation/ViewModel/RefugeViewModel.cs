using RefugeConsole.ClassesMetiers.Helper;
using RefugeConsole.ClassesMetiers.Model.Entities;
using RefugeConsole.ClassesMetiers.Model.Enums;
using RefugeConsole.CoucheAccesDB;
using RefugeConsole.CouchePresentation.View;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;

namespace RefugeConsole.CouchePresentation.ViewModel
{
    internal class RefugeViewModel
    {
        private readonly AnimalViewModel animalViewModel;
        private readonly ContactViewModel contactViewModel;
        private readonly IAnimalDataService animalDataService;
        private readonly IContactDataService contactDataService;
        private readonly IRefugeDataService refugeDataService;

        public RefugeViewModel(
            AnimalViewModel animalVM,
            ContactViewModel contactVM,
            IAnimalDataService animalDataService, 
            IContactDataService contactDataService, 
            IRefugeDataService refugeDataService
        ) {
            this.animalViewModel = animalVM;
            this.contactViewModel = contactVM;

            this.animalDataService = animalDataService;
            this.contactDataService = contactDataService;
            this.refugeDataService = refugeDataService;
        }

        public Contact GetContact()
        {
            Contact? contact = null;

            try
            {
                // Récupérer la personne de contact si elle existe déjà, sinon créer une nouvelle personne de contact
                bool contactExists = SharedView.InputBoolean("Le contact est-il déjà enregistré ? (Oui/Non)");

                if (contactExists)
                {
                    string registryNumber = SharedView.InputString("Entrez le numéro de registre national?");
                    contact = this.contactDataService.GetContactByRegistryNumber(registryNumber);
                }
                else
                {
                    contact = this.contactViewModel.AddContact();

                }
            }
            catch (Exception ex)
            {

                if (Debugger.IsAttached)
                    Debug.WriteLine($"Error while asking for contact info to user.\nError : {ex.Message}.\nException : {ex}.");
            }

            if (contact == null) throw new Exception("Unknown error while  asking for contact info to user.");

            return contact;

        }

        public void AddAdmission()
        {
            bool result = false;
            Contact? contact = null;
            Animal? animal = null;

            try
            {
                // Récupérer la raison de l'admission
                AdmissionType admissionType = SharedView.EnumChoice<AdmissionType>("Quelle est la raison de l'admission de l'animal? ");
                string reason = MyEnumHelper.GetEnumDescription<AdmissionType>(admissionType);

                // Ask for contact info,
                contact = this.GetContact();

                // If it's a return, find animal in database, else create an animal
                if (admissionType == AdmissionType.ReturnAdoption || admissionType == AdmissionType.ReturnFosterFamily)
                {
                    animal = animalViewModel.GetAnimalByName();
                }
                else
                {
                    animal = this.animalViewModel.AddAnimal();
                }
                    

                if (animal == null) throw new Exception($"Unknown error while adding a new animal.");

                // Handle creating a new admission
                result = this.refugeDataService.HandleAdmission(new Admission(reason, DateTime.Now, contact, animal));

                if (!result) throw new Exception("Unknown error while adding an admission to the refuge.");
                

                Console.WriteLine($"Admission enregistré de {animal.Name} par {contact.Firstname + " " + contact.Lastname} en raison de {reason}.");

                SharedView.WaitForKeyPress();

            }
            catch (Exception ex) {
                if (Debugger.IsAttached)
                    Debug.WriteLine($"Error while adding an admission.\nError : {ex.Message}.\nException : {ex}.");
            }
        }

        /**
         * Fonctionnalité : Lister les animaux présents au refuge (toutes les entrées)
         */
        public void ListAdmissions() {
            try
            {
                HashSet<Admission> admissions = refugeDataService.GetAdmissions();

                Console.WriteLine("Lister les admissions : ");

                foreach (Admission admission in admissions) {
                    this.animalDataService.GetAnimalColors(admission.Animal);

                    RefugeView.DisplayAdmission(admission);
                }
                    

                SharedView.WaitForKeyPress();
            }
            catch (Exception ex) {
                if (Debugger.IsAttached)
                    Debug.WriteLine($"Error while listing admissions. Error {ex.Message}. Exception : {ex} ");
            }
        }

        /**
         * Fonctionnalité : Lister les familles d’accueil par où l’animal est passé
         */
        public void ListAnimalPastFosterFamilies() {
            try
            {
                Animal? animal = this.animalViewModel.GetAnimalByName();

                if (animal == null) throw new Exception("Unable to retrieve the corresponding animal!");

                HashSet<FosterFamily> pastFosterFamilies = this.refugeDataService.GetFosterFamiliesForAnimal(animal);

                foreach( FosterFamily family in pastFosterFamilies )
                    RefugeView.DisplayFosterFamily(family);

                SharedView.WaitForKeyPress();
            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached)
                    Debug.WriteLine($"Error while retrieving all past foster family for an animal. Error: {ex.Message}. Exception : {ex}.");
            }
        }

        /**
         * Fonctionnalité : Lister les animaux accueillis par une famille d’accueil
         */
        public void ListReleaseInFosterFamily()
        {
            HashSet<FosterFamily> fosterFamilies = new HashSet<FosterFamily>();

            try
            {
                fosterFamilies = this.refugeDataService.GetFosterFamilies();

                foreach( FosterFamily fosterFamily in fosterFamilies ) 
                    RefugeView.DisplayFosterFamily(fosterFamily);

                SharedView.WaitForKeyPress();

            }
            catch (Exception ex) {

                if (Debugger.IsAttached)
                    Debug.WriteLine($"Error while listing all release in foster family. Error : {ex.Message}. Exception : {ex}.");


            } 
        }

        

        /**
         * Fonctionnalité : Ajouter une nouvelle famille d’accueil à un animal (la date d’arrivée et la personne de contact sont obligatoires)
         */
        public void ReleaseToFosterFamily() {
            Contact? contact = null;
            Animal? animal = null;

            try
            {
                // Récupérer la personne de contact si elle existe déjà, sinon créer une nouvelle personne de contact
                contact = this.GetContact();

                // Récupération de l'animal 
                animal = this.animalViewModel.GetAnimalByName();

                if (contact == null || animal == null) throw new Exception("Unable to retrieve the contact and the animal to release for foster family");

                DateOnly dateStart = SharedView.InputDateOnly("Entrez la date de début de l'accueil : ");
                DateOnly? dateEnd = null;

                bool dateEndKnown = SharedView.InputBoolean("Connaissez-vous la date de fin de l'accueil? (Oui/Non)");

                if (dateEndKnown)
                    dateEnd = SharedView.InputDateOnly("Entrez la date de début de l'accueil : ");

                FosterFamily fosterFamily = new FosterFamily(DateTime.Now, dateStart, dateEnd, contact, animal);

                Release release = new Release(ReleaseType.FosterFamily, DateTime.Now, contact, animal);

                bool result = this.refugeDataService.CreateReleaseForFosterFamily(fosterFamily, release);

                if (!result)
                    throw new Exception("Unable to create a release for foster family.");

                RefugeView.DisplayReleaseForFosterFamily(release, fosterFamily);

                SharedView.WaitForKeyPress();

            }
            catch (Exception ex)
            {

                if (Debugger.IsAttached)
                    Debug.WriteLine($"Error while listing all release in foster family. Error : {ex.Message}. Exception : {ex}.");


            }
        }

        /**
         * Fonctionnalité : Lister les adoptions et leur statut
         */
        public void ListAdoptionWithStatus() {

            try
            {
                HashSet<Adoption> adoptions = this.refugeDataService.GetAdoptions();

                Console.WriteLine("Liste des adoptions\n===================");

                foreach (Adoption ad in adoptions)
                    RefugeView.DisplayAdoption(ad);

                SharedView.WaitForKeyPress();
            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached)
                    Debug.WriteLine($"Error while listing all adoption. Error : {ex.Message}. Exception : {ex}");
            }
        }

        
        /**
         * Fonctionnalité : Ajouter une adoption (candidature)
         * 
         */
        public void CreateAdoption()
        {
            Animal? animal = null;
            Contact? contact = null;
            Adoption? adoption = null;

            try
            {

                animal = this.animalViewModel.GetAnimalByName();

                contact = this.GetContact();

                if (animal == null || contact == null) throw new Exception("Unable to retrieve the animal or the contact for creating an adoption.");

                DateOnly dateStart = SharedView.InputDateOnly("Quel est la date de début d'adoption : ");


                adoption = new Adoption(
                    ApplicationStatus.Application,
                    DateTime.Now,
                    dateStart,
                    null,
                    contact,
                    animal
                );

                bool result = this.refugeDataService.CreateAdoption(adoption);

                if (!result) throw new Exception("Unable to create an adoption's application.");

                RefugeView.DisplayAdoption(adoption);

                SharedView.WaitForKeyPress();
            }
            catch (Exception ex)
            {

                if (Debugger.IsAttached)
                    Debug.WriteLine($"Error while retrieving an adoption. Error : {ex.Message}. Exception : {ex}.");
            }

        }

        /**
         * Fonctionnalité : Valider une demande d'adoption
         */
        public void ReleaseForAdoption() {
            Contact? contact = null;
            Animal? animal = null;

            try
            {
                // Récupérer la personne de contact si elle existe déjà, sinon créer une nouvelle personne de contact
                contact = this.GetContact();

                // Récupération de l'animal 
                animal = this.animalViewModel.GetAnimalByName();

                if (contact == null || animal == null) throw new Exception("Unable to retrieve the contact or the animal to release for adoption");

                DateOnly dateStart = SharedView.InputDateOnly("Entrez la date de début de l'adoption : ");

                // Récupérer la demande d'adoption
                Adoption adoption = this.refugeDataService.GetAdoption(contact, animal); 

                adoption.Status = MyEnumHelper.GetEnumDescription(ApplicationStatus.Accepted);

                Release release = new Release(ReleaseType.Adoption, DateTime.Now, contact, animal);

                bool result = this.refugeDataService.CreateReleaseForAdoption(adoption, release);

                if (!result)
                    throw new Exception("Unable to create a release for adoption.");

                RefugeView.DisplayReleaseForAdoption(release, adoption);

                SharedView.WaitForKeyPress();

            }
            catch (Exception ex)
            {

                if (Debugger.IsAttached)
                    Debug.WriteLine($"Error while releasing an animal for adoption. Error : {ex.Message}. Exception : {ex}.");


            }
        }

        /**
         * Fonctionnalité : Modifier le statut d’une adoption
         */
        public void UpdateAdoptionStatus() {
            bool result = false;

            try
            {
                
                string registryNumber = SharedView.InputString("Entrez le numéro de registre national de l'adoptant : ");
                
                Contact contact = this.contactDataService.GetContactByRegistryNumber(registryNumber);

                Animal? animal = this.animalViewModel.GetAnimalByName();

                if (animal == null) throw new Exception("Unable to find the animal specified!");

                ApplicationStatus status = SharedView.EnumChoice<ApplicationStatus>("Quel est le nouveau statut de l'adoption : ");                


                Adoption adoption = this.refugeDataService.GetAdoption(contact, animal);

                adoption.Status = MyEnumHelper.GetEnumDescription<ApplicationStatus>(status);

                if(status == ApplicationStatus.Accepted)
                {
                    Release release = new Release(ReleaseType.Adoption, DateTime.Now, contact, animal);

                    result = this.refugeDataService.CreateReleaseForAdoption(adoption, release);
                }
                else
                {
                    result = this.refugeDataService.UpdateAdoption(adoption);
                }

                if (!result) throw new Exception("Unable to update adoption's status.");

                Console.WriteLine($"Adoption (ID : {adoption.Id}) a été mis à jour avec un statut ({adoption.Status}).");

                SharedView.WaitForKeyPress();

            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached)
                    Debug.WriteLine($"Error while updating an adoption's status. Error : {ex.Message}. Exception : {ex}");
            }
        }

        /**
         * Fonctionnalité : Ajouter un vaccin (date du vaccin, nom du vaccin, fait ou non fait) à un animal
         */
        public void AddVaccination() {
            Animal? animal = null;
            Vaccine? vaccine = null;

            try
            {
                animal = this.animalViewModel.GetAnimalByName();

                string vaccineName = SharedView.InputString("Quel est le nom du vaccin : ");
                vaccine = this.refugeDataService.GetVaccine(vaccineName);

                if (vaccine == null)
                    vaccine = this.refugeDataService.CreateVaccine(new Vaccine(vaccineName));

                DateOnly dateVaccination = SharedView.InputDateOnly("Quelle est la date de la vaccination : (format : dd/dd/yyyy)");
                bool done = SharedView.InputBoolean("La vaccination a été réalisé ? (Oui/Non)");

                Vaccination vaccination = new Vaccination(dateVaccination, done, animal, vaccine);

                bool result = this.refugeDataService.CreateVaccination(vaccination);

                RefugeView.DisplayVaccination(vaccination);

                SharedView.WaitForKeyPress();

            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached)
                    Debug.WriteLine($"Error while adding an vaccination. Error : {ex.Message}. Exception : {ex}");
            }
        }
    }
}

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

            try
            {
                // Récupérer la raison de l'admission
                AdmissionType admissionType = SharedView.EnumChoice<AdmissionType>("Quelle est la raison de l'admission de l'animal? ");
                string reason = MyEnumHelper.GetEnumDescription<AdmissionType>(admissionType);

                // Ask for contact info,
                contact = this.GetContact();


                // Create animal
                Animal? animal = this.animalViewModel.AddAnimal();

                if (animal == null) throw new Exception($"Unknown error while adding a new animal.");



                result = this.refugeDataService.CreateAdmission(new Admission(reason, DateTime.Now, contact, animal));

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
         * Fonctionnalité : Ajouter une candidature pour adoption
         */
        public void AddCandidate() { }

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
        public void ListReleaseAdoptionWithStatus() { }

        /**
         * Fonctionnalité : Ajouter une adoption
         */
        public void ReleaseForAdoption() { }

        /**
         * Fonctionnalité : Modifier le statut d’une adoption
         */
        public void UpdateReleaseForAdoption() { }

        /**
         * Fonctionnalité : Ajouter un vaccin (date du vaccin, nom du vaccin, fait ou non fait) à un animal
         */
        public void AddVaccination() { }
    }
}

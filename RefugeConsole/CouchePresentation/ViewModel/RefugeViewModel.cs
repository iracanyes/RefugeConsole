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

            // Raffraichir l'écran
            Console.Clear();

            return contact;

        }

        /**
         * <summary>
         *  Gère la vue pour l'ajout une admission
         * </summary>
         */ 
        public void AddAdmission()
        {
            bool result = false;
            Contact? contact = null;
            Animal? animal = null;

            // Raffraichir l'écran
            Console.Clear();
            Console.WriteLine("Ajouter une admission\n===================\n");

            try
            {
                // Récupération des données de la personne de contact
                contact = this.GetContact();

                // 


                // Saisie du type d'admission par l'utilisateur
                AdmissionType admissionType = SharedView.EnumChoice<AdmissionType>("Quelle est la raison de l'admission de l'animal? ");
                string reason = MyEnumHelper.GetEnumDescription<AdmissionType>(admissionType);               

                
                // Si l'admission concerne un 'retour d'adoption' ou un 'retour de famille d'accueil', alors l'animal existe en base de donnée
                // Sinon on ajoute un animal
                if (admissionType == AdmissionType.ReturnAdoption || admissionType == AdmissionType.ReturnFosterFamily)
                {
                    // Affiche la vue pour rechercher et récupérer un animal en base de donnée
                    animal = animalViewModel.GetAnimalByName();


                    // Vérification des contraintes suivantes :
                    // Contrainte 1 : Date_entree: Un animal ne peut être entrée plus d’une fois depuis une sortie
                    // Contrainte 2 : Si la raison d’entrée est 'retour_adoption', alors ce n'est pas la première entrée de l'animal,
                    // il existe une sortie précédente avec comme raison de sortie 'adoption’.
                    if (this.refugeDataService.IsAdmittedSinceLastRelease(animal.Id))
                        throw new Exception($"L'animal ({animal.Name}) a déjà été admis depuis sa dernière sortie.");                    


                }
                else
                {
                    // afficher la vue pour créer et sauvegarder un nouveau animal 
                    animal = this.animalViewModel.AddAnimal();
                }
                    

                if (animal == null) 
                    throw new Exception($"Unknown error while retrieving or adding a animal.");
                

                // gère l'ajout d'une nouvelle admission
                result = this.refugeDataService.HandleCreateAdmission(new Admission(reason, DateTime.Now, contact, animal));

                if (!result) 
                    throw new Exception("Unknown error while adding an admission to the refuge.");
                

                Console.WriteLine($"Admission enregistré de {animal.Name} par {contact.Firstname + " " + contact.Lastname} en raison de {reason}.");

                
            }
            catch (Exception ex) {

                Console.Error.WriteLine($"Error while adding an admission.\nError : {ex.Message}.\nException : {ex}.");
                if (Debugger.IsAttached)
                    Debug.WriteLine($"Error while adding an admission.\nError : {ex.Message}.\nException : {ex}.");

                
            }

            // Bloquer l'écran jusqu'à l'appui sur une touche
            SharedView.WaitForKeyPress();
        }

        /**
         * <summary>
         *  Fonctionnalité :  la mise à jour d'un animal
         * </summary>
         */
        public void UpdateAdmission()
        {
            Animal? animalInfo = null;

            // Raffraichir l'écran
            Console.Clear();
            Console.WriteLine("Mise à jour d'une admission\n===================\n");

            try
            {
                animalInfo = this.animalViewModel.GetAnimalByName();

                // Si aucun animal
                if (animalInfo == null)
                {
                    Console.WriteLine($"Animal inconnu!");
                    SharedView.WaitForKeyPress();
                    return;
                }

                // Si l'animal est déjà mort, aucune action permise
                if (animalInfo.DeathDate != null)
                {
                    Console.Error.WriteLine($"Animal '{animalInfo.Name}' is dead, no more action allowed!");
                    SharedView.WaitForKeyPress();
                    return;
                }

                // Saisie des informations à jour de l'animal
                Animal updatedAnimalInfo = AnimalView.UpdateAnimal(animalInfo);

                // Si la nouvelle date de décès est non nulle, on crée une dernière sortie avec comme raison 'deces_animal'
                if (updatedAnimalInfo.DeathDate != null)
                    this.refugeDataService.CreateRelease(new Release(ReleaseType.Death, DateTime.Now, updatedAnimalInfo));

                // Mise à jour des informations de l'animal en base de donnée
                Animal updatedAnimal = animalDataService.UpdateAnimal(updatedAnimalInfo);

                // Ajout de nouvelles compatibilités pour l'animal
                this.animalViewModel.AddAnimalCompatibilities(updatedAnimal);

                // Affichage des informations mises à jour
                AnimalView.DisplayAnimal(updatedAnimal);


            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error while updating the animal! Reason : {ex.Message}\nException:\n{ex}");

                Debug.WriteLine($"Error while updating the animal! Reason : {ex.Message}\nException:\n{ex}");
            }

            // Saisie d'une touche pour continuer
            SharedView.WaitForKeyPress();

        }

        /**
         * <summary>
         *  Fonctionnalité : Lister les animaux présents au refuge (toutes les entrées)
         * </summary>
         */
        public void ListAdmissions() {

            // Raffraichir l'écran
            Console.Clear();
            Console.WriteLine("Liste des admissions\n===================\n");

            try
            {
                HashSet<Admission> admissions = refugeDataService.GetAdmissions();

                // Affichage
                foreach (Admission admission in admissions) {
                    this.animalDataService.GetAnimalColors(admission.Animal);

                    RefugeView.DisplayAdmission(admission);
                }
                    

            }
            catch (Exception ex) {
                if (Debugger.IsAttached)
                    Debug.WriteLine($"Error while listing admissions. Error {ex.Message}. Exception : {ex} ");
            }

            // Saisie d'une touche pour continuer
            SharedView.WaitForKeyPress();
        }

        /**
         * <summary>
         *  Fonctionnalité : Lister les familles d’accueil par où l’animal est passé
         * </summary>
         */
        public void ListAnimalPastFosterFamilies() {

            // Raffraichir l'écran
            Console.Clear();
            Console.WriteLine("Liste des familles d'accueil passées d'un animal\n=========================================\n");

            try
            {
                // Recherche de l'animal par nom
                Animal? animal = this.animalViewModel.GetAnimalByName();

                // Retour si aucun animal existe
                if (animal == null) throw new Exception("Unable to retrieve the corresponding animal!");

                // Lister les familles d'accueil passés de l'animal
                HashSet<FosterFamily> pastFosterFamilies = this.refugeDataService.GetFosterFamiliesForAnimal(animal);

                // Affichage des familles d'accueil
                foreach( FosterFamily family in pastFosterFamilies )
                    RefugeView.DisplayFosterFamily(family);

                
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error while retrieving all past foster family for an animal. Error: {ex.Message}. Exception : {ex}.");

                if (Debugger.IsAttached)
                    Debug.WriteLine($"Error while retrieving all past foster family for an animal. Error: {ex.Message}. Exception : {ex}.");
            }

            // Saisie d'une touche pour continuer
            SharedView.WaitForKeyPress();
        }

        /**
         * <summary>
         *  Fonctionnalité : Lister les animaux accueillis par une famille d’accueil
         * </summary>
         */
        public void ListReleaseInFosterFamily()
        {
            HashSet<FosterFamily> fosterFamilies = new HashSet<FosterFamily>();

            // Raffraichir l'écran
            Console.Clear();
            Console.WriteLine("Liste des animaux en famille d'accueil\n=======================================\n\n");

            try
            {
                // Lister les animaux en familles d'accueil 
                fosterFamilies = this.refugeDataService.GetFosterFamilies();

                // Affichage 
                foreach( FosterFamily fosterFamily in fosterFamilies ) 
                    RefugeView.DisplayFosterFamily(fosterFamily);

                

            }
            catch (Exception ex) {
                Console.Error.WriteLine($"Error while listing all release in foster family. Error : {ex.Message}. Exception : {ex}.");

                if (Debugger.IsAttached)
                    Debug.WriteLine($"Error while listing all release in foster family. Error : {ex.Message}. Exception : {ex}.");


            }

            // Saisie d'une touche pour continuer
            SharedView.WaitForKeyPress();
        }



        /**
         * <summary>
         *  Fonctionnalité : Ajouter une nouvelle famille d’accueil à un animal (la date d’arrivée et la personne de contact sont obligatoires)
         * </summary>
         */
        public void ReleaseToFosterFamily() {
            Contact? contact = null;
            Animal? animal = null;

            // Raffraichir l'écran
            Console.Clear();
            Console.WriteLine("Ajouter une famille d'accueil à un animal \n=======================================\n\n");

            try
            {
                // Récupération de l'animal 
                animal = this.animalViewModel.GetAnimalByName();

                // Si animal mort, aucune action permise
                if (animal.DeathDate != null)
                {
                    Console.Error.WriteLine($"L'animal '{animal.Name}' est mort, aucune action permis!");
                    SharedView.WaitForKeyPress();
                    return;
                }

                // Contrainte : Date_sortie: il n’y a qu’une seule sortie depuis la plus récente date d’entrée de l’animal
                if (this.refugeDataService.IsReleasedSinceLastAdmission(animal.Id))
                {
                    Console.Error.WriteLine($"L'animal ({animal.Name}) est déjà sorti!");
                    SharedView.WaitForKeyPress();
                    return;
                }

                // Contrainte : Un animal ne peut pas avoir plusieurs accueils actifs simultanément (un seul ACCUEIL avec date_fin = NULL à la fois)
                if (this.refugeDataService.IsInFosterFamily(animal.Id))
                {
                    Console.Error.WriteLine($"L'animal ({animal.Name}) est déjà en famille d'accueil!");
                    SharedView.WaitForKeyPress();
                    return;
                }


                // Récupérer la personne de contact si elle existe déjà, sinon créer une nouvelle personne de contact
                contact = this.GetContact();

                
                if (contact == null || animal == null) throw new Exception("Unable to retrieve the contact and the animal to release for foster family");

                // Gestion des dates de l'accueil
                DateOnly dateStart = SharedView.InputDateOnly("Entrez la date de début de l'accueil : ");
                DateOnly? dateEnd = null;

                bool dateEndKnown = SharedView.InputBoolean("Connaissez-vous la date de fin de l'accueil? (Oui/Non)");

                if (dateEndKnown)
                    dateEnd = SharedView.InputDateOnly("Entrez la date de début de l'accueil : ", dateStart, DateComparator.GreaterThanOrEqual);
                
                // Sauvegarde de la sortie et de la famille d'accueil
                FosterFamily fosterFamily = new FosterFamily(DateTime.Now, dateStart, dateEnd, contact, animal);

                Release release = new Release(ReleaseType.FosterFamily, DateTime.Now, animal, contact);

                
                bool result = this.refugeDataService.CreateReleaseForFosterFamily(fosterFamily, release);

                if (!result)
                    throw new Exception("Unable to create a release for foster family.");

                // Affichage
                RefugeView.DisplayReleaseForFosterFamily(release, fosterFamily);

                

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error while creating a release in foster family.\n Error : {ex.Message}.\n Exception : {ex}.");

                if (Debugger.IsAttached)
                    Debug.WriteLine($"Error while creating a release in foster family.\n Error : {ex.Message}.\n Exception : {ex}.");


            }

            // Saisie d'une touche pour continuer
            SharedView.WaitForKeyPress();
        }

        /**
         * <summary>
         *  Fonctionnalité : Lister les adoptions et leur statut
         * </summary>
         */
        public void ListAdoptionWithStatus() {

            // Raffraichir l'écran
            Console.Clear();
            Console.WriteLine("Liste des adoptions\n=======================================\n\n");

            try
            {
                // Récupération de la liste des adoptions
                HashSet<Adoption> adoptions = this.refugeDataService.GetAdoptions();


                // Affichage
                foreach (Adoption ad in adoptions)
                    RefugeView.DisplayAdoption(ad);

                
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error while listing all adoption. Error : {ex.Message}. Exception : {ex}");

                if (Debugger.IsAttached)
                    Debug.WriteLine($"Error while listing all adoption. Error : {ex.Message}. Exception : {ex}");
            }

            // Saisie d'une touche pour continuer
            SharedView.WaitForKeyPress();
        }


        /**
         * <summary>
         *  Fonctionnalité : Ajouter une adoption (candidature)
         * </summary>
         * 
         */
        public void CreateAdoption()
        {
            Animal? animal = null;
            Contact? contact = null;
            Adoption? adoption = null;

            // Raffraichir l'écran
            Console.Clear();
            Console.WriteLine("Ajouter une adoption\n=======================================\n\n");

            try
            {

                animal = this.animalViewModel.GetAnimalByName();

                // Si animal mort, aucune action permise
                if (animal.DeathDate != null)
                {
                    Console.WriteLine($"Animal '{animal.Name}' is dead, no more action allowed!");
                    SharedView.WaitForKeyPress();
                    return;
                }

                // Récupération de la personne de contact si elle existe, sinon crée un contact
                contact = this.GetContact();

                if (animal == null || contact == null) throw new Exception("Unable to retrieve the animal or the contact for creating an adoption.");

                // Date de début de l'adoption
                DateOnly dateStart = SharedView.InputDateOnly("Quel est la date de début d'adoption : ");

                // Sauvegarde de l'adoption

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

                
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error while retrieving an adoption. Error : {ex.Message}. Exception : {ex}.");

                if (Debugger.IsAttached)
                    Debug.WriteLine($"Error while retrieving an adoption. Error : {ex.Message}. Exception : {ex}.");
            }

            // Saisie d'une touche pour continuer
            SharedView.WaitForKeyPress();

        }

        /**
         * <summary>
         *  Fonctionnalité : Valider une demande d'adoption
         * </summary>
         */
        public void ReleaseForAdoption() {
            Contact? contact = null;
            Animal? animal = null;

            // Raffraichir l'écran
            Console.Clear();
            Console.WriteLine("Valider une demande d'adoption\n=======================================\n\n");

            try
            {
                // Récupération de l'animal 
                animal = this.animalViewModel.GetAnimalByName();

                // Contrainte : Date_sortie: il n’y a qu’une seule sortie depuis la plus récente date d’entrée de l’animal
                if (this.refugeDataService.IsReleasedSinceLastAdmission(animal.Id))
                {
                    Console.Error.WriteLine($"L'animal ({animal.Name}) est déjà sorti!");
                    SharedView.WaitForKeyPress();
                    return;
                }

                // Récupérer la personne de contact si elle existe déjà, sinon créer une nouvelle personne de contact
                contact = this.GetContact();                

                if (contact == null || animal == null) throw new Exception("Unable to retrieve the contact or the animal to release for adoption");

                DateOnly dateStart = SharedView.InputDateOnly("Entrez la date de début de l'adoption : ");

                // Récupérer la demande d'adoption
                Adoption adoption = this.refugeDataService.GetAdoption(contact, animal); 

                // Accepter la demande d'adoption
                adoption.Status = MyEnumHelper.GetEnumDescription(ApplicationStatus.Accepted);

                // Créer une sortie pour adoption
                Release release = new Release(ReleaseType.Adoption, DateTime.Now, animal, contact);

                // Sauvegarde de l'adoption et de la sortie
                bool result = this.refugeDataService.CreateReleaseForAdoption(adoption, release);

                if (!result)
                    throw new Exception("Unable to create a release for adoption.");

                // Affichage de l'adoption et de la sortie
                RefugeView.DisplayReleaseForAdoption(release, adoption);

                

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error while releasing an animal for adoption. Error : {ex.Message}. Exception : {ex}.");

                if (Debugger.IsAttached)
                    Debug.WriteLine($"Error while releasing an animal for adoption. Error : {ex.Message}. Exception : {ex}.");


            }

            // Saisie d'une touche pour continuer
            SharedView.WaitForKeyPress();
        }

        /**
         * <summary>
         *  Fonctionnalité : Modifier le statut d’une adoption
         * </summary>
         */
        public void UpdateAdoptionStatus() {
            bool result = false;

            // Raffraichir l'écran
            Console.Clear();
            Console.WriteLine("Mettre à jour une adoption\n=======================================\n\n");

            try
            {
                // Récupération de l'animal
                Animal? animal = this.animalViewModel.GetAnimalByName();

                if (animal == null) throw new Exception("Unable to find the animal specified!");

                // Si animal mort, aucune action permise
                if (animal.DeathDate != null)
                {
                    Console.WriteLine($"L'animal '{animal.Name}' est mort, aucune action n'est permis!");
                    SharedView.WaitForKeyPress();
                    return;
                }

                // Récupération de la personne de contact par son numéro de registre national
                string registryNumber = SharedView.InputString("Entrez le numéro de registre national de l'adoptant : ");
                
                Contact contact = this.contactDataService.GetContactByRegistryNumber(registryNumber);                
                             

                // Récupération de l'adoption
                Adoption adoption = this.refugeDataService.GetAdoption(contact, animal);

                // Saisie du status de la demande d'adoption
                ApplicationStatus status = SharedView.EnumChoice<ApplicationStatus>("Quel est le nouveau statut de l'adoption : ");                

                // Si le nouveau statut de l'adoption est 'acceptee' et l'adoption récupéré en base de donnée a un statut de 'candidat',
                // On ajoute une nouvelle sortie pour adoption
                if(status == ApplicationStatus.Accepted && adoption.Status == MyEnumHelper.GetEnumDescription<ApplicationStatus>(ApplicationStatus.Application))
                {
                    // Mise à jour du status de l'adoption
                    adoption.Status = MyEnumHelper.GetEnumDescription<ApplicationStatus>(status);

                    // Ajout d'une sortie pour l'animal
                    Release release = new Release(ReleaseType.Adoption, DateTime.Now, animal, contact);

                    // Sauvegarde de l'adoption et de la sortie
                    result = this.refugeDataService.CreateReleaseForAdoption(adoption, release);
                }
                else
                {
                    // Mise à jour du status de l'adoption
                    adoption.Status = MyEnumHelper.GetEnumDescription<ApplicationStatus>(status);

                    // Ajout d'une sortie pour l'animal
                    result = this.refugeDataService.UpdateAdoption(adoption);
                }

                if (!result) throw new Exception("Unable to update adoption's status.");

                Console.WriteLine($"Adoption (ID : {adoption.Id}) a été mis à jour avec un statut ({adoption.Status}).");

                

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error while updating an adoption's status. Error : {ex.Message}. Exception : {ex}");
                if (Debugger.IsAttached)
                    Debug.WriteLine($"Error while updating an adoption's status. Error : {ex.Message}. Exception : {ex}");
            }

            // Saisie d'une touche pour continuer
            SharedView.WaitForKeyPress();
        }

        /**
         * <summary>
         *  Fonctionnalité : Ajouter un vaccin (date du vaccin, nom du vaccin, fait ou non fait) à un animal
         * </summary>
         */
        public void AddVaccination() {
            Animal? animal = null;
            Vaccine? vaccine = null;

            // Raffraichir l'écran
            Console.Clear();
            Console.WriteLine("Ajouter une vaccination pour un animal\n=======================================\n\n");

            try
            {
                animal = this.animalViewModel.GetAnimalByName();

                // Si animal mort, aucune action permise
                if (animal.DeathDate != null)
                {
                    Console.WriteLine($"L'nimal '{animal.Name}' est mort, aucune action n'est permis!");
                    SharedView.WaitForKeyPress();
                    return;
                }

                // Saisie du nom du vaccin
                string vaccineName = SharedView.InputString("Quel est le nom du vaccin : ");
                vaccine = this.refugeDataService.GetVaccine(vaccineName);

                // Si le vaccin n'existe en DB, on ajoute le vaccin
                if (vaccine == null)
                    vaccine = this.refugeDataService.CreateVaccine(new Vaccine(vaccineName));

                // Saisie de la date de vaccination
                bool dateVaccinationKnown = SharedView.InputBoolean("Connaissez-vous la date de vaccination? (Oui/Non)");
                
                DateOnly? dateVaccination = null;
                bool done = false;

                if (dateVaccinationKnown)
                {
                    dateVaccination = SharedView.InputDateOnly("Quelle est la date de la vaccination : (format : dd/mm/yyyy)", animal.BirthDate, DateComparator.GreaterThan);

                    // Contrainte : Vaccination_date: est soit nulle ou soit supérieure à la date de naissance
                    if (dateVaccination <= animal.BirthDate)
                        throw new Exception("La date de vaccination doit être supérieur à la date de naissance de l'animal");
                }

                // Saisie de vaccination réalisé
                done = dateVaccination == null || dateVaccination > DateOnly.FromDateTime(DateTime.Now) ? false : SharedView.InputBoolean("La vaccination a-t-elle été réalisé? (Oui/Non)");

                // Sauvegarde de la vaccination
                Vaccination vaccination = new Vaccination(dateVaccination, done, animal, vaccine);

                bool result = this.refugeDataService.CreateVaccination(vaccination);

                // Affichage
                RefugeView.DisplayVaccination(vaccination);

                

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error while adding an vaccination. Error : {ex.Message}. Exception : {ex}");

                if (Debugger.IsAttached)
                    Debug.WriteLine($"Error while adding an vaccination. Error : {ex.Message}. Exception : {ex}");
            }

            // Saisie d'une touche pour continuer
            SharedView.WaitForKeyPress();
        }
    }
}

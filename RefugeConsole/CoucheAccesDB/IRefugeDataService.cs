using Npgsql;
using RefugeConsole.ClassesMetiers.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace RefugeConsole.CoucheAccesDB
{
    internal interface IRefugeDataService
    {
        /**
         * <summary>
         *  Gère la création d'une nouvelle admission
         * </summary>
         * <param name="admission">
         *  Entrée pour un animal
         * </param>
         */
        bool HandleCreateAdmission(Admission admission);

        /**
         * <summary>
         *  Ajouter une admission pour un animal
         * </summary>
         * <param name="admission">
         *  Entrée pour un animal
         * </param>
         * 
         */
        bool CreateAdmission(Admission admission, NpgsqlTransaction transaction );

        /**
         * <summary>
         * Lister l'ensemble des admissions
         * </summary>
         */
        HashSet<Admission> GetAdmissions();


        /**
         * <summary>
         *  Lister l'ensemble des famiiles d'accueil 
         * </summary>
         */
        HashSet<FosterFamily> GetFosterFamilies();

        /**
         * <summary>
         *  Lister les familles d'accueil par lesquelles un animal est passé
         * </summary>
         */
        HashSet<FosterFamily> GetFosterFamiliesForAnimal(Animal animal);

        /**
         * <summary>
         *  Ajouter une sortie en famille d'accueil
         * </summary>
         */
        bool CreateReleaseForFosterFamily(FosterFamily fosterFamily, Release release);

        /**
         * <summary>
         *  Ajouter une sortie 
         * </summary>
         */
        bool CreateRelease(Release release, NpgsqlTransaction? transaction = null);

        /**
         * <summary>
         *  Récupérer une famille d'accueil à partir des informations de contact et de l'animal
         * </summary>
         */
        FosterFamily GetFosterFamily(Contact contact, Animal animal);

        /**
         * <summary>
         *  Vérifie que l'animal est déjà en famille d'accueil
         * </summary>
         */
        bool IsInFosterFamily(string animalId);

        /**
         * <summary>
         *  Ajoute d'une famille d'accueil pour un animal
         * </summary>
         */
        bool CreateFosterFamily(FosterFamily fosterFamily, NpgsqlTransaction transaction);


        /**
         * <summary>
         *   Mettre à jour une famille d'accueil pour un animal
         * </summary>
         */
        bool UpdateFosterFamily(FosterFamily fosterFamily, NpgsqlTransaction transaction);

        /**
         * <summary>
         *  Lister les adoptions
         * </summary>
         */
        HashSet<Adoption> GetAdoptions();

        /**
         * <summary>
         *  Récupérer une adoption 
         * </summary>
         */
        Adoption GetAdoption(Contact contact, Animal animal);

        /**
         * <summary>
         *  Ajouter une adoption
         * </summary>
         */
        bool CreateAdoption(Adoption adoption);

        /**
         * <summary>
         *   Créer une sortie pour adoption
         * </summary>
         */
        bool CreateReleaseForAdoption(Adoption adoption, Release release);

        /**
         * <summary>
         *   Mettre à jour une adoption
         * </summary>
         */
        bool UpdateAdoption(Adoption adoption, NpgsqlTransaction? transaction = null);

        /**
         * <summary>
         *  Récupérer un vaccin
         * </summary>
         */
        Vaccine? GetVaccine(string name);

        /**
         * <summary>
         *  Ajouter un vaccin
         * </summary>
         */
        Vaccine CreateVaccine(Vaccine vaccine);

        /**
         * <summary>
         *  Ajouter une vaccination pour un animal
         * </summary>
         */
        bool CreateVaccination(Vaccination vaccination);

        /**
         * <summary>
         *  Vérifie la contrainte  : Date_entree: Un animal ne peut être entrée plus d’une fois depuis une sortie
         * </summary>
         */
        bool IsAdmittedSinceLastRelease(string animalId);

        /**
         * <summary>
         *  Vérifie la contrainte : Date_sortie: il n’y a qu’une seule sortie depuis la plus récente date d’entrée de l’animal
         * </summary>
         */
        bool IsReleasedSinceLastAdmission(string animalId);
    }
}

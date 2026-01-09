using Npgsql;
using RefugeConsole.ClassesMetiers.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace RefugeConsole.CoucheAccesDB
{
    internal interface IAnimalDataService
    {

        /**
         * <summary>
         *  Ajouter un animal
         * </summary>
         */
        Animal CreateAnimal(Animal animal);

        /**
          * <summary>
          *     Récupérer les couleurs d'un animal
          * </summary>
          */
        HashSet<AnimalColor> GetAnimalColors(Animal animal);

        /**
         * <summary>
         *  Récupérer un animal par son nom
         * </summary>
         */
        List<Animal> GetAnimalByName(string name);

        /**
         * <summary>
         *  Récupérer un animal par son Identifiant unique
         * </summary>
         */
        Animal? GetAnimalById(string id);

        /**
         * <summary>
         *  Mettre à jour un animal 
         * </summary>
         */
        Animal UpdateAnimal(Animal animal);

        /**
         * <summary>
         *  Supprimer un animal
         * </summary>
         */
        bool RemoveAnimal(Animal animal);

        /**
          * <summary>
          *     Ajouter une compatibilité 
          * </summary>
          */
        bool CreateCompatibility(Compatibility compatibility, NpgsqlTransaction transaction);

        /**
         * <summary>
         *  Ajouter une compatibilité pour un animal
         * </summary>
         */
        bool CreateAnimalCompatibility(AnimalCompatibility animalCompatibility);

        /**
         * <summary>
         *  Récupérer la liste des compatibilités
         * </summary>
         */
        HashSet<Compatibility> GetCompatibilities();

        /**
         * <summary>
         *  Récupérer les couleurs d'animaux disponibles
         * </summary>
         */
        HashSet<Color> GetColors();

        /**
         * <summary>
         *  Ajouter une couleur pour un animal
         * </summary>
         */
        bool CreateAnimalColor(AnimalColor animalColor, NpgsqlTransaction? transaction = null);
    }
}

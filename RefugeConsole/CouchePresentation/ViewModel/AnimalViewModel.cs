using Microsoft.Extensions.Logging;
using Npgsql;
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
    internal class AnimalViewModel
    {
        private static readonly ILogger MyLogger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger(nameof(AnimalViewModel));
        private readonly IAnimalDataService animalDataService;
        private readonly HashSet<Compatibility> compatibilities = new HashSet<Compatibility>();

        public AnimalViewModel(IAnimalDataService animalDataService) { 
            this.animalDataService = animalDataService;

            this.compatibilities = animalDataService.GetCompatibilities();
        }

        /**
         * <summary>
         *  Fonctionnalité : Gère la vue pour rechercher un animal par son nom
         * </summary>
         */
        public Animal GetAnimalByName()
        {
            Animal? animalInfo = null;

            // Raffraichir l'écran
            Console.Clear();
            Console.WriteLine("Recherche d'animaux par leur nom\n=======================================\n\n");

            try
            {
                // Demander le nom de l'animal
                string name = SharedView.InputString("Entrez le nom de l'animal : ");

                // Récupére les animaux ayant le même nom
                List<Animal> animalInfos = animalDataService.GetAnimalByName(name);
                

                // Si un seul résultat, retourne l'objet
                // si plusieurs résultats, afficher les résultats, demande l'identifiant de l'animal et retourné l'objet correspondant 
                if (animalInfos.Count == 1)
                {
                    animalInfo = animalInfos[0];
                }
                else if (animalInfos.Count > 1)
                {

                    // Affichage des animaux trouvés
                    foreach (Animal animal in animalInfos)
                    {
                        AnimalView.DisplayAnimal(animal);
                    }

                    // Saisie de l'ID de l'animal par l'utilisateur
                    string id = SharedView.InputString("Entrez l'identifiant de l'animal désiré : ");
                    animalInfo = animalDataService.GetAnimalById(id);
                }

                // Stop if no animal found with the name
                if (animalInfo == null)
                {
                    Console.WriteLine($"Animal with name '{name}' not found!");
                    SharedView.WaitForKeyPress();
                    
                }
            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached)
                    Debug.WriteLine($"Error while retrieving an animal by name. Error : {ex.Message}. Exception : {ex}");


            }

            

            return animalInfo!;
        }

        /**
         * <summary>
         *  Fonctionnalité : Consulter un animal
         * </summary>
         */
        public void ViewAnimal()
        {
            try
            {
                // Saisie du nom de l'animal
                string name = SharedView.InputString("Entrez le nom de l'animal : ");
                // Lister les animaux ayant le même nom
                List<Animal> animals = animalDataService.GetAnimalByName(name);

                if (animals.Count == 0)
                {
                    Console.WriteLine($"Animal with name '{name}' not found!");
                    SharedView.WaitForKeyPress();
                    return;
                }

                // Affichage
                foreach(Animal animal in  animals) 
                    AnimalView.DisplayAnimal(animal);

                
            }
            catch (Exception ex) {
                Debug.WriteLine($"Error while retrieving an animal! Reason : {ex.Message} \nException: {ex}");
            }

            // Saisie d'une touche pour continuer
            SharedView.WaitForKeyPress();

        }

        /**
         * <summary>
         *  Fonctionnalité : Ajouter un animal
         * </summary>
         */
        public Animal AddAnimal()
        {
            Animal? result = null;

            // Raffraichir l'écran
            Console.Clear();
            Console.WriteLine("Ajouter un animal\n=======================================\n\n");

            try
            {
                
                // Saisie par utilisateur des données de l'animal
                Animal animalInfo = AnimalView.AddAnimal();

                // Récupération de la liste des couleurs d'animaux
                List<Color> colors = animalDataService.GetColors().ToList();

                // Saisie par utilisateur des couleurs de l'animal
                AnimalView.AddColors(animalInfo, colors);


                // Sauvegarde de l'animal
                result = animalDataService.CreateAnimal(animalInfo);

                // Sauvegarde des couleurs de l'animal
                foreach (AnimalColor animalColor in animalInfo.AnimalColors) {
                    animalDataService.CreateAnimalColor(animalColor);
                }
                
                // Ajout des compatibilités pour l'animal
                this.AddAnimalCompatibilities(result);

                // Affichage de l'animal
                AnimalView.DisplayAnimal(result);

                

            }
            catch (Exception ex) {
                Console.Error.WriteLine($"Error while creating an animal. Reason : {ex.Message}.Exception : {ex}");

                if (Debugger.IsAttached)
                    Debug.WriteLine($"Error while creating an animal. Reason : {ex.Message}.Exception : {ex}");
            }

            // Saisie d'une touche pour continuer
            SharedView.WaitForKeyPress();

            return result!;
        }


        /**
         * <summary>
         *  Fonctionnalité : Supprimer un animal
         * </summary>
         */
        public void RemoveAnimal() {
            Animal? animalInfo = null;

            // Raffraichir l'écran
            Console.Clear();
            Console.WriteLine("Supprimer un animal\n=======================================\n\n");

            try
            {
                // Récupération de l'animal par son nom
                animalInfo = this.GetAnimalByName();

                // Si aucun animal
                if (animalInfo == null)
                {
                    Console.WriteLine($"L'animal n'existe pas dans la base de donnée!");
                    SharedView.WaitForKeyPress();
                    return;
                }

                // Supprimer l'animal en base de donnée
                bool result = animalDataService.RemoveAnimal(animalInfo);

                if (result) Console.WriteLine($"L'animal nommé {animalInfo.Name} a été supprimé!");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error while removing an animal! Reason: {ex.Message}");
                MyLogger.LogError($"Error while removing an animal! Reason: {ex.Message}");
            }

            // Saisie d'une touche pour continuer
            SharedView.WaitForKeyPress();
        }

        /**
         * <summary>
         *  Fonctionnalité : Ajouter une compatibilité pour un animal
         * </summary>
         */
        public void AddAnimalCompatibilities(Animal? animal = null)
        {
            
            try
            {
                // Si animal est nulle, rechercher l'animal par son nom
                if(animal == null)
                {
                    animal = this.GetAnimalByName();
                }

                // Récupérer la liste des compatibilités
                HashSet<Compatibility> compatibilities = animalDataService.GetCompatibilities();

                // Affiche la vue qui gère l'ajout de compatibilité à un animal
                AnimalView.AddAnimalCompatibilities(animal!, compatibilities);

                // Si l'animal contient des objets "AnimalCompatiblility", alors on sauvegarde les nouvelles compatibilités
                if (animal!.AnimalCompatibilities.Count != 0)
                {
                    foreach (AnimalCompatibility animalCompatibility in animal.AnimalCompatibilities)
                    {
                        animalDataService.CreateAnimalCompatibility(animalCompatibility);
                    }

                }


            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error while adding compatibilities to an animal! Reason : {ex.Message}\nException:\n{ex}");

                if(Debugger.IsAttached)
                    Debug.WriteLine($"Error while adding compatibilities to an animal! Reason : {ex.Message}\nException:\n{ex}");
            }


        }
    }
}

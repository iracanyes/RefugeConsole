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

        public void ViewAnimal()
        {
            try
            {
                string name = SharedView.InputString("Entrez le nom de l'animal : ");
                List<Animal> animals = animalDataService.GetAnimalByName(name);

                if (animals.Count == 0)
                {
                    Console.WriteLine($"Animal with name '{name}' not found!");
                    SharedView.WaitForKeyPress();
                    return;
                }

                foreach(Animal animal in  animals) 
                    AnimalView.DisplayAnimal(animal);

                SharedView.WaitForKeyPress();
            }
            catch (Exception ex) {
                Debug.WriteLine($"Error while retrieving an animal! Reason : {ex.Message} \nException: {ex}");
            }

        }

        public void AddAnimal()
        {
            try
            {
                // Require a animal name which is unique
                // Display view to get animal info from user
                Animal animalInfo = AnimalView.AddAnimal();

                // retrieve animal colors in DB
                List<Color> colors = animalDataService.GetColors().ToList();

                // Require animal's colors input from user
                AnimalView.AddColors(animalInfo, colors);


                //Debug.WriteLine($"Animal ID : {animalInfo.Id}"); 
                //MyLogger.LogInformation($"Animal ID : {animalInfo.Id}");

                // Save the animal's information
                Animal animal = animalDataService.CreateAnimal(animalInfo);

                // Save animal's colors in database
                foreach (AnimalColor animalColor in animalInfo.AnimalColors) {
                    animalDataService.CreateAnimalColor(animalColor);
                }
                
                // Handle adding animal compatibilities
                this.AddAnimalCompatibilities(animal);

                // Display the newly created animal
                AnimalView.DisplayAnimal(animal);

                // Lock screen until any key press
                SharedView.WaitForKeyPress();

            }
            catch (Exception ex) {
                if(Debugger.IsAttached)
                    Debug.WriteLine($"Error while creating an animal. Reason : {ex.Message}.Exception : {ex}");
            }
        }

        public void UpdateAnimal()
        {
            try
            {
                // Get the animal's name from user
                string name = SharedView.InputString("Entrez le nom de l'animal : ");

                // Retrieve animals with same name from DB
                List<Animal> animalInfos = animalDataService.GetAnimalByName(name);
                Animal? animalInfo = null;

                // If one result set to animalInfo object,
                // else if more than one result display retrieved animals' info and handle asking for the id of the animal desired 
                if (animalInfos.Count == 1)
                {
                    animalInfo = animalInfos[0];
                }
                else if (animalInfos.Count > 1)
                {

                    foreach (Animal animal in animalInfos)
                    {
                        AnimalView.DisplayAnimal(animal);
                    }

                    string id = SharedView.InputString("Entrez l'identifiant de l'animal désiré : ");
                    animalInfo = animalDataService.GetAnimalById(id);
                }

                // Stop if no animal found with the name
                if (animalInfo == null)
                {
                    Console.WriteLine($"Animal with name '{name}' not found!");
                    SharedView.WaitForKeyPress();
                    return;
                }

                // Get updated animal's infos from user
                Animal updatedAnimalInfo = AnimalView.UpdateAnimal(animalInfo);

                // Update animal's infos in DB
                Animal updatedAnimal = animalDataService.UpdateAnimal(updatedAnimalInfo);

                // Handle adding new compatibilities for the animal
                this.AddAnimalCompatibilities(updatedAnimal);

                // Display animal's infos
                AnimalView.DisplayAnimal(updatedAnimal);

                // Lock screen until any key press
                SharedView.WaitForKeyPress();

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error while updating the animal! Reason : {ex.Message}\nException:\n{ex}");
            }
        }

        public void RemoveAnimal() {
            try
            {
                // Get the animal's name from user
                string name = SharedView.InputString("Entrez le nom de l'animal : ");

                // Retrieve animals with same name from DB
                List<Animal> animalInfos = animalDataService.GetAnimalByName(name);
                Animal? animalInfo = null;

                // If one result set to animalInfo object,
                // else if more than one result display retrieved animals' info and handle asking for the id of the animal desired 
                if (animalInfos.Count == 1)
                {
                    animalInfo = animalInfos[0];
                }
                else if(animalInfos.Count > 1)
                {

                    foreach (Animal animal in animalInfos)
                    {
                        AnimalView.DisplayAnimal(animal);
                    }

                    string id = SharedView.InputString("Entrez l'identifiant de l'animal désiré : ");
                    animalInfo = animalDataService.GetAnimalById(id);
                }

                // Stop if no animal found with the name
                if (animalInfo == null)
                {
                    Console.WriteLine($"L'animal nommé '{name}' n'existe pas dans la base de donnée!");
                    SharedView.WaitForKeyPress();
                    return;
                }

                // Remove animal from DB
                bool result = animalDataService.RemoveAnimal(animalInfo);

                if (result) Console.WriteLine($"L'animal nommé {name} a été supprimé!");

                // Lock screen until key press
                SharedView.WaitForKeyPress();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error while removing an animal! Reason: {ex.Message}");
                MyLogger.LogError($"Error while removing an animal! Reason: {ex.Message}");
            }
        }

        public void AddAnimalCompatibilities(Animal? animal = null)
        {
            
            try
            {
                // If animal is null, Ask the user to input the animal's name and retrieve it
                if(animal == null)
                {
                    string name = SharedView.InputString("Entrez le nom de l'animal : ");
                    List<Animal> animalInfos = animalDataService.GetAnimalByName(name);

                    if (animalInfos.Count == 1)
                    {
                        animal = animalInfos[0];
                    }
                    else if (animalInfos.Count > 1)
                    {

                        foreach (Animal animalFound in animalInfos)
                        {
                            AnimalView.DisplayAnimal(animalFound);
                        }

                        string id = SharedView.InputString("Entrez l'identifiant de l'animal désiré : ");
                        animal = animalDataService.GetAnimalById(id);
                    }
                }

                // Get a list of all available compatibilities
                HashSet<Compatibility> compatibilities = animalDataService.GetCompatibilities();

                // Ask the user to input compatibilities for the animal and add AnimalCompatibility instances for the animal receive in argument
                AnimalView.AddAnimalCompatibilities(animal!, compatibilities);

                // If AnimalCompatibilities list isn't empty, save all n
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

                Debug.WriteLine($"Error while adding compatibilities to an animal! Reason : {ex.Message}\nException:\n{ex}");
            }
        }
    }
}

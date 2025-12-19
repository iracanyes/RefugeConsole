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
                Animal? animal = animalDataService.GetAnimal(name);

                if (animal == null)
                {
                    Console.WriteLine($"Animal with name '{name}' not found!");
                    SharedView.WaitForKeyPress();
                    return;
                }

                AnimalView.DisplayAnimal(animal);
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

                //Debug.WriteLine($"Animal ID : {animalInfo.Id}"); 
                //MyLogger.LogInformation($"Animal ID : {animalInfo.Id}");

                // Save the animal's information
                Animal animal = animalDataService.CreateAnimal(animalInfo);
                
                // Handle adding animal compatibilities
                this.AddAnimalCompatibilities(animal);

                // Display the newly created animal
                AnimalView.DisplayAnimal(animal);

            }
            catch (Exception ex) {
                Debug.WriteLine("Error while creating an animal. Reason : {0}", ex.Message);
                Debug.WriteLine("Exception : {0}", ex);
            }
        }

        public void UpdateAnimal()
        {
            try
            {
                string name = SharedView.InputString("Entrez le nom de l'animal : ");
                Animal? animalInfo = animalDataService.GetAnimal(name);

                if (animalInfo == null)
                {
                    Console.WriteLine($"Animal with name '{name}' not found!");
                    SharedView.WaitForKeyPress();
                    return;
                }


                Animal updatedAnimalInfo = AnimalView.UpdateAnimal(animalInfo);

                Animal updatedAnimal = animalDataService.UpdateAnimal(updatedAnimalInfo);

                this.AddAnimalCompatibilities(updatedAnimal);

                AnimalView.DisplayAnimal(updatedAnimal);

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error while updating the animal! Reason : {ex.Message}\nException:\n{ex}");
            }
        }

        public void RemoveAnimal() {
            try
            {
                // G
                string name = SharedView.InputString("Entrez le nom de l'animal : ");
                Animal? animalInfo = animalDataService.GetAnimal(name);

                if (animalInfo == null)
                {
                    Console.WriteLine($"L'animal nommé '{name}' n'existe pas dans la base de donnée!");
                    SharedView.WaitForKeyPress();
                    return;
                }

                bool result = animalDataService.RemoveAnimal(animalInfo);

                if (result) Console.WriteLine($"L'animal nommé {name} a été supprimé!");

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
                    string name = SharedView.InputString("Entrez le nom de l'animal :");
                    animal = animalDataService.GetAnimal(name);
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

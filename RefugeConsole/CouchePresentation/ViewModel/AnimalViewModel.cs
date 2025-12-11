using Microsoft.Extensions.Logging;
using RefugeConsole.ClassesMetiers.Model.Entities;
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

        public AnimalViewModel(IAnimalDataService animalDataService) { 
            this.animalDataService = animalDataService;
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
                // Display view to get animal info from user
                Animal animalInfo = AnimalView.AddAnimal();

                Debug.WriteLine($"Animal ID : {animalInfo.Id}"); 
                MyLogger.LogInformation($"Animal ID : {animalInfo.Id}");

                Animal animal = animalDataService.CreateAnimal(animalInfo);
                
                this.AddCompatibilities(animal);

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

                //Debug.WriteLine($"Animal info to update : \n{updatedAnimalInfo}");

                Animal updatedAnimal = animalDataService.UpdateAnimal(updatedAnimalInfo);

                this.AddCompatibilities(updatedAnimal);

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

        public void AddCompatibilities(Animal? animal)
        {
            
            try
            {
                if(animal == null)
                {
                    string name = SharedView.InputString("Entrez le nom de l'animal :");
                    animal = animalDataService.GetAnimal(name);
                }
                
                List<Compatibility> result = AnimalView.AddCompatibility(animal!);

                if (result.Count != 0)
                {
                    foreach(Compatibility compatibility in result) 
                        animalDataService.CreateCompatibility(compatibility);
                }


            }
            catch (Exception ex)
            {

                Debug.WriteLine($"Error while adding compatibilities to an animal! Reason : {ex.Message}\nException:\n{ex}");
            }
        }
    }
}

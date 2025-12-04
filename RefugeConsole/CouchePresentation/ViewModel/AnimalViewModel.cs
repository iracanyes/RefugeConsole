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
                Animal animal = animalDataService.GetAnimal(name);

                if (animal == null)
                {
                    throw new Exception($"Animal named ({name}) not found!");
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

                AnimalView.DisplayAnimal(animal);

            }
            catch (Exception ex) {
                Debug.WriteLine("Error while creating an animal. Reason : {0}", ex.Message);
                Debug.WriteLine("Exception : {0}", ex);
            }
        }

        public void UpdateAnimal() { }

        public void RemoveAnimal() { }
    }
}

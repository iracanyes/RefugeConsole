using Microsoft.Extensions.Logging;
using RefugeConsole.ClassesMetiers.Helper;
using RefugeConsole.ClassesMetiers.Model.Entities;
using RefugeConsole.ClassesMetiers.Model.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace RefugeConsole.CouchePresentation.View
{
    internal class AnimalView
    {
        private static readonly ILogger MyLogger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger(nameof(AnimalView));

        public static void AddColors(Animal animal, List<Color> colors)
        {
            bool addMoreColor = false;
            List<string> colorsNames = colors.Select(c => c.Name).ToList();
            List<string> selectedColors = new List<string>();

            try
            {
                do {

                    // Ask for color name input
                    string colorName = SharedView.ChoiceInList<string>("Quel est la couleur de l'animal?", colorsNames);

                    // Select corresponding Color object
                    Color selectedColor = colors.First(c => c.Name == colorName);

                    
                    animal.AddAnimalColor(new AnimalColor(animal, selectedColor));

                    colorsNames.Remove(colorName);

                    addMoreColor = SharedView.InputBoolean($"Voulez-vous ajouter une couleur pour l'animal ({animal.Name}) ? (Oui/Non)");


                } while (addMoreColor);
            }
            catch (Exception ex)
            {
                if(Debugger.IsAttached)
                    Debug.WriteLine($"Error while asking color name input.\n{ex.Message}");
                MyLogger.LogError($"Error while adding animal's color from user input. {ex.Message}");
                throw new Exception($"Error while asking color name input.\n{ex.Message}");
            }



        }

        public static Animal AddAnimal()
        {
            Animal? animal = null;

            Console.Clear();

            Console.WriteLine("Ajouter un animal: (nom, type, sexe, couleur, date de naissance, date de décès, est sterilisé, date stérilisation, particularité, description)");
            
            
            string name = SharedView.InputString("Entrez le nom de l'animal: ");
            AnimalType type = SharedView.EnumChoice<AnimalType>("Entrez le type de l'animal (chat, chien) : ");
            GenderType gender = SharedView.EnumChoice<GenderType>("Entrez le sexe de l'animal : ");

            
            DateOnly birthDate = SharedView.InputDateOnly("Entrez la date de naissance de l'animal : (Format : dd/mm/yyyy ) ", DateOnly.FromDateTime(DateTime.Now), DateComparator.LessThanOrEqual);
            
            // Check if is alive or record death date
            bool isAlive = SharedView.InputBoolean("Est-il vivant?");
            DateOnly? deathDate = null;
            if (!isAlive) {
                deathDate = SharedView.InputDateOnly("Entrez la date de décès de l'animal : ", birthDate, DateComparator.GreaterThan);
            }

            // 
            bool isSterilized = SharedView.InputBoolean("L'animal est-il stérilisé? (Oui/Non) ");

            // Check if is sterilized or record the sterilization's date
            bool isSterilizationDateKnown = false;
            DateOnly? dateSterilization = null;

            if (isSterilized)
            {
                isSterilizationDateKnown = SharedView.InputBoolean("Connaissez-vous la date de stérilisation? (Oui/Non)");

                if (isSterilizationDateKnown)
                {
                    dateSterilization = SharedView.InputDateOnly($"Entrez la date de stérilisation de l'animal?", birthDate, DateComparator.GreaterThan);
                }
            }
        
             
            string particularity = SharedView.InputMultipleLines("Entrez les particularités de l'animal: ");
            string description = SharedView.InputMultipleLines("Entrez la description de l'animal: ");

            try
            {
                animal = new Animal(
                    name, 
                    type, 
                    gender,  
                    birthDate, 
                    deathDate, 
                    isSterilized, 
                    dateSterilization, 
                    particularity, 
                    description
                );

                
                
            }
            catch (Exception ex) {
                Debug.WriteLine($"Error while creating an animal. Reason : {ex.Message} . \nException : {ex}");
                Debug.WriteLine(ex);
            }

            if (animal == null) throw new Exception("Unknown error while creating an animal.");

            return (Animal) animal;


        }

        public static void DisplayAnimal(Animal animal)
        {
            Console.WriteLine(
                $"""
                ============================================
                Animal ID : {animal.Id}
                ============================================
                name = {animal.Name}
                type = {animal.Type}
                gender = {animal.Gender}
                colors = [{string.Join(", ", animal.AnimalColors.Select(ac => ac.Color.Name).ToList())}]
                birthDate = {animal.BirthDate}
                deathDate = {animal.DeathDate}
                isSterilized = {animal.IsSterilized}
                dateSterilization = {animal.DateSterilization}
                particularity :
                {animal.Particularity}
                
                description :
                {animal.Description}
                ==============================================
                """
            );

        }

        public static Animal UpdateAnimal(Animal animal)
        {
            Animal? result = null;

            Console.Clear();

            Console.WriteLine("Mettre à jour un animal: (nom, type, sexe, couleur, date de naissance, date de décès, est sterilisé, date stérilisation, particularité, description)");


            string name = SharedView.InputString($"Entrez le nom de l'animal: (Actuel = {animal.Name})");



            AnimalType type = SharedView.EnumChoice<AnimalType>($"Entrez le type de l'animal (chat, chien) : (Actuel = {animal.Type})");
            GenderType gender = SharedView.EnumChoice<GenderType>($"Entrez le sexe de l'animal : (Actuel = {animal.Gender})");
            DateOnly birthDate = SharedView.InputDateOnly($"Entrez la date de naissance de l'animal (Format : 26/11/1989) : (Actuel = {animal.BirthDate})");

            // Check if is alive or record death date
            bool isAlive = SharedView.InputBoolean("Est-il vivant?");
            DateOnly? deathDate = null;
            if (!isAlive)
            {
                deathDate = SharedView.InputDateOnly($"Entrez la date de décès de l'animal : (Actuel = {animal.DeathDate})");
            }

            // 
            bool isSterilized = SharedView.InputBoolean($"L'animal est-il stérilisé? (Oui/Non)   (Actuel = {animal.IsSterilized})");

            // Check if is sterilized or record the sterilization's date
            bool isSterilizationDateKnown = false;
            DateOnly? dateSterilization = null;

            if (isSterilized) {                

                isSterilizationDateKnown = SharedView.InputBoolean("Connaissez-vous la date de stérilisation? (Oui/Non)");

                if (isSterilizationDateKnown)
                {
                    dateSterilization = SharedView.InputDateOnly($"Entrez la date de stérilisation de l'animal? (Actuel = {animal.DateSterilization}) ");
                }
            }             
            

            string particularity = SharedView.InputMultipleLines($"Entrez les particularités de l'animal: \nValeur actuel\n=============\n {animal.Particularity}\nRéponse\n================ ");

            string description = SharedView.InputMultipleLines($"Entrez la description de l'animal: \nValeur actuel\n=============\n {animal.Description}\nRéponse\n================ ");

            try
            {
                result = new Animal(
                    animal.Id,
                    name,
                    type,
                    gender,
                    birthDate,
                    deathDate,
                    isSterilized,
                    dateSterilization,
                    particularity,
                    description
                );

                

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error while updating an animal. \nReason : {ex.Message}.\nException : {ex} ");
                Console.WriteLine($"Error while updating an animal. \nReason : {ex.Message}.\nException : {ex} ");
            }

            if (result == null) throw new Exception("Unknown error while updating an animal.");

            return result!;
        }

        public static void AddAnimalCompatibilities(Animal animal, HashSet<Compatibility> compatibilities)
        {
            
            bool addNext = false;
            List<string> compatiblitiesNames = compatibilities.Select(p => p.Type).ToList();
            List<string> alreadySelected = new List<string>();


            try
            {
                do
                {
                    addNext = SharedView.InputBoolean($"Voulez-vous ajouter une nouvelle compatibilité pour {animal.Name} ? (Oui/Non)");

                    if (addNext)
                    {

                        string type = SharedView.ChoiceInList<string>("Quel est le type de compatibilité?", compatiblitiesNames);

                        // If already selected type, skip 
                        if (alreadySelected.Contains(type))
                        {
                            Console.WriteLine("Vous avez déjà entrée des données pour cette compatibilité!");
                            continue;
                        }

                        Compatibility compatibility = compatibilities.First(c => c.Type == type);
                        bool value = SharedView.InputBoolean("Quel est sa valeur? (Oui/Non)");

                        string? description = null;

                        if (value)
                            description = SharedView.InputMultipleLines("Décrivez cette compatibilité?");

                        AnimalCompatibility animalCompatibility = new AnimalCompatibility( animal, compatibility, value, description);

                        animal.AddAnimalCompatibility(animalCompatibility);

                        // Add the compatibility type to already selected list
                        alreadySelected.Add(type);

                    }
                } while (addNext);
            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached)
                    Debug.WriteLine($"Unable to get user input for animal compatibilities. Error message : {ex.Message}.\nException : {ex}");
                throw new Exception($"Unable to get user input for animal compatibilities. Error message : {ex.Message}.\nException : {ex}");
            }
        }
    }
}

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


        public static Animal AddAnimal()
        {
            Animal? animal = null;

            Console.Clear();

            Console.WriteLine("Ajouter un animal: (nom, type, sexe, couleur, date de naissance, date de décès, est sterilisé, date stérilisation, particularité, description)");
            
            
            string name = SharedView.InputString("Entrez le nom de l'animal: ");
            AnimalType type = SharedView.EnumChoice<AnimalType>("Entrez le type de l'animal (chat, chien) : ");
            GenderType gender = SharedView.EnumChoice<GenderType>("Entrez le sexe de l'animal : ");
            string color = SharedView.InputString("Entrez la couleur de l'animal : ");
            DateOnly birthDate = SharedView.InputDateOnly("Entrez la date de naissance de l'animal : (Format : ) ");
            
            // Check if is alive or record death date
            bool isAlive = SharedView.InputBoolean("Est-il vivant?");
            DateOnly? deathDate = null;
            if (!isAlive) {
                deathDate = SharedView.InputDateOnly("Entrez la date de décès de l'animal : ");
            }

            // 
            bool isSterilized = SharedView.InputBoolean("L'animal est-il stérilisé? (Oui/Non) ");

            // Check if is sterilized or record the sterilization's date
            bool isSterilizationDateKnown = SharedView.InputBoolean("Connaissez-vous la date de stérilisation? (Oui/Non)");
            DateOnly? dateSterilization = null;
            if (isSterilizationDateKnown)
            {
                dateSterilization = SharedView.InputDateOnly("Entrez la date de stérilisation de l'animal? ");
            }
             
            string particularity = SharedView.InputMultipleLines("Entrez les particularités de l'animal: ");
            string description = SharedView.InputMultipleLines("Entrez la description de l'animal: ");

            try
            {
                animal = new Animal(
                    name, 
                    type, 
                    gender, 
                    color, 
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
                color = {animal.Color}
                birthDate = {animal.BirthDate}
                deathDate = {animal.DeathDate}
                isSterilized = {animal.IsSterilized}
                dateSterilization = {animal.DateSterilization}
                particularity = {animal.Particularity}
                description = {animal.Description}
                ==============================================
                """
            );


            SharedView.WaitForKeyPress();
        }

        public static Animal UpdateAnimal(Animal animal)
        {
            Animal? result = null;

            Console.Clear();

            Console.WriteLine("Mettre à jour un animal: (nom, type, sexe, couleur, date de naissance, date de décès, est sterilisé, date stérilisation, particularité, description)");


            string name = SharedView.InputString($"Entrez le nom de l'animal: (Actuel = {animal.Name})");
            AnimalType type = SharedView.EnumChoice<AnimalType>($"Entrez le type de l'animal (chat, chien) : (Actuel = {animal.Type})");
            GenderType gender = SharedView.EnumChoice<GenderType>($"Entrez le sexe de l'animal : (Actuel = {animal.Gender})");
            string color = SharedView.InputString($"Entrez la couleur de l'animal : (Actuel = {animal.Color})");
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
            bool isSterilizationDateKnown = SharedView.InputBoolean("Connaissez-vous la date de stérilisation? (Oui/Non)");
            DateOnly? dateSterilization = null;
            if (isSterilizationDateKnown)
            {
                dateSterilization = SharedView.InputDateOnly($"Entrez la date de stérilisation de l'animal? (Actuel = {animal.DateSterilization}) ");
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
                    color,
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
                Debug.WriteLine($"Error while updating an animal. \nReason : {ex.Message} ");
                Debug.WriteLine($"Exception: {ex}");
            }

            if (result == null) throw new Exception("Unknown error while updating an animal.");

            return result!;
        }

        public static List<Compatibility> AddCompatibility(Animal animal)
        {
            List<Compatibility> result = [];
            bool addNext = false;

            do
            {
                addNext = SharedView.InputBoolean($"Voulez-vous ajouter une nouvelle compatibilité pour {animal.Name} ? (Oui/Non)");

                if (addNext)
                {
                    string type = SharedView.InputString("Quel est le type de compatibilité? (Chat, Chien, Jeune enfant, Enfant, Jardin, Poney)");
                    CompatibilityValueType value = SharedView.EnumChoice<CompatibilityValueType>("Quel est sa valeur?");
                    string description = SharedView.InputMultipleLines("Décrivez cette compatibilité?");

                    Compatibility compatibility = new Compatibility(type, MyEnumHelper.GetEnumDescription(value), description, animal);

                    animal.AddCompatibility(compatibility);
                    result.Add(compatibility);

                }
            } while (addNext);


            return result;
        }
    }
}

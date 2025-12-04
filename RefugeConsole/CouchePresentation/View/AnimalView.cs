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


            Console.WriteLine("Pressez une touche pour continuer...");
            Console.ReadKey(true);
        }
    }
}

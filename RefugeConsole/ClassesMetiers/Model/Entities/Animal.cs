using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.Extensions.Logging;
using RefugeConsole.ClassesMetiers.Helper;
using RefugeConsole.ClassesMetiers.Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace RefugeConsole.ClassesMetiers.Model.Entities
{
    internal class Animal
    {
        private static ILogger MyLogger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger(nameof(Animal));
        private static Random randomGenerator = new Random();
        private static DateTime yesterday = DateTime.Now;

        public Animal(
            string name,
            AnimalType type,
            GenderType gender,
            string color,
            DateOnly? birthDate,
            DateOnly? deathDate,
            bool isSterilized,
            DateOnly? dateSterilization,
            string particularity,
            string description
        ){
                                   

            this.Id = DateOnly.FromDateTime(DateTime.Now).ToString("yyMMdd") + randomGenerator.Next(0, 99999).ToString("D5");
            this.Name = name;
            this.Type = MyEnumHelper.GetEnumDescription(type);
            this.Gender = MyEnumHelper.GetEnumDescription(gender);
            this.Color = color;
            this.BirthDate = birthDate;
            this.DeathDate = deathDate;
            this.IsSterilized = isSterilized;
            this.DateSterilization = dateSterilization;
            this.Particularity = particularity;
            this.Description = description;
        }
        

        public Animal(
            string id,
            string name,
            AnimalType type,
            GenderType gender,
            string color,
            DateOnly? birthDate,
            DateOnly? deathDate,
            bool isSterilized,
            DateOnly? dateSterilization,
            string particularity,
            string description
        )
        {
            this.Id = id;
            this.Name = name;
            this.Type = MyEnumHelper.GetEnumDescription(type);
            this.Gender = MyEnumHelper.GetEnumDescription(gender);
            this.Color = color;
            this.BirthDate = birthDate;
            this.DeathDate = deathDate;
            this.IsSterilized = isSterilized;
            this.DateSterilization = dateSterilization;
            this.Particularity = particularity;
            this.Description = description;
        }



        [Key]
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Type { get; set; }
        [Required]
        public string Gender { get; set; }
        [Required]
        public string Color { get; set; }
        
        public DateOnly? BirthDate { get; set; }

        public DateOnly? DeathDate { get; set; }
        [Required]
        public Boolean IsSterilized { get; set; }

        public DateOnly? DateSterilization { get; set; }

        public string Particularity { get; set; }

        public string Description { get; set; }

        public HashSet<Admission> Admissions { get; set; } = new HashSet<Admission>();

        public HashSet<Vaccination> Vaccinations { get; set; } = new HashSet<Vaccination>();

        public HashSet<Compatibility> Compatibilities { get; set; } = new HashSet<Compatibility>();

        /*============= M&thodes d'instances =========================================================*/
        public void AddAdmission(Admission admission)
        {
            ArgumentNullException.ThrowIfNull(admission, nameof(admission));

            try
            {
                Admissions.Add(admission);
            }
            catch (ArgumentException ex)
            {
                MyLogger.LogError("Unable to add an admission. Reason : {0} ", ex.Message);
            }
        }

        public void RemoveAdmission(Admission admission)
        {
            ArgumentNullException.ThrowIfNull(admission, nameof(admission));

            try
            {
                Admissions.Remove(admission);
            }
            catch (ArgumentException ex)
            {
                MyLogger.LogError("Unable to remove an admission. Reason : {0} ", ex.Message);
            }
        }



        public void AddVaccination(Vaccination vaccination)
        {
            ArgumentNullException.ThrowIfNull(vaccination, nameof(vaccination));

            try
            {
                Vaccinations.Add(vaccination);
            }
            catch (ArgumentException ex)
            {
                MyLogger.LogError("Unable to add a vaccination. Reason : {0} ", ex.Message);
            }
        }

        public void RemoveVaccination(Vaccination vaccination)
        {
            ArgumentNullException.ThrowIfNull(vaccination, nameof(vaccination));

            try
            {
                Vaccinations.Remove(vaccination);
            }
            catch (ArgumentException ex)
            {
                MyLogger.LogError("Unable to remove a vaccination. Reason : {0} ", ex.Message);
            }
        }

        public void AddCompatibility(Compatibility compatibility)
        {
            ArgumentNullException.ThrowIfNull(compatibility, nameof(compatibility));

            try
            {
                Compatibilities.Add(compatibility);
            }
            catch (ArgumentException ex)
            {
                MyLogger.LogError("Unable to add a compatibility. Reason : {0} ", ex.Message);
            }
        }

        public void RemoveCompatibility(Compatibility compatibility)
        {
            ArgumentNullException.ThrowIfNull(compatibility, nameof(compatibility));

            try
            {
                Compatibilities.Remove(compatibility);
            }
            catch (ArgumentException ex)
            {
                MyLogger.LogError("Unable to remove a compatibility. Reason : {0} ", ex.Message);
            }
        }

        public override string ToString()
        {
            return string.Format(
                "Animal{{ id = {0}, name = {1}, type = {2}, gender = {3}, color = {4}, birthDate = {5}, deathDate = {6}, sterilized = {7}, dateSterilization = {8}, particularity = {9}, description = {10} }}",
                this.Id,
                this.Name,
                this.Type,
                this.Gender,
                this.Color,
                this.BirthDate,
                this.DeathDate,
                this.IsSterilized,
                this.DateSterilization,
                this.Particularity,
                this.Description
            );
        }
    }
}

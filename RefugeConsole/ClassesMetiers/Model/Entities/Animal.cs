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


        public HashSet<Vaccination> Vaccinations { get; set; } = new HashSet<Vaccination>();

        public HashSet<AnimalCompatibility> AnimalCompatibilities { get; set; } = new HashSet<AnimalCompatibility>();

        public HashSet<Admission> Admissions { get; } = new HashSet<Admission>();

        public HashSet<Release> Releases { get; } = new HashSet<Release>();

        public HashSet<Adoption> Adoptions { get; } = new HashSet<Adoption>();

        public HashSet<FosterFamily> FosterFamilies { get; } = new HashSet<FosterFamily>();

        /*================ Méthodes d'instance =========================================*/
        

        public void AddAdmission(Admission admission)
        {
            ArgumentNullException.ThrowIfNull(admission, nameof(admission));
            try
            {
                this.Admissions.Add(admission);
            }
            catch (ArgumentException ex)
            {
                MyLogger.LogError("Unable to add the following adoption : " + admission.ToString() + "\nThe reason :" + ex.Message);
            }

        }

        public void RemoveAdmission(Admission admission)
        {
            try
            {
                this.Admissions.Remove(admission);
            }
            catch (ArgumentException ex)
            {
                MyLogger.LogError("Unable to remove the following adoption : " + admission.ToString() + "\nThe reason :" + ex.Message);
            }
        }

        public void AddRelease(Release release)
        {
            ArgumentNullException.ThrowIfNull(release, nameof(release));
            try
            {
                this.Releases.Add(release);
            }
            catch (ArgumentException ex)
            {
                MyLogger.LogError("Unable to add the following release : " + release.ToString() + "\nThe reason :" + ex.Message);
            }

        }

        public void RemoveRelease(Release release)
        {
            try
            {
                this.Releases.Remove(release);
            }
            catch (ArgumentException ex)
            {
                MyLogger.LogError("Unable to remove the following release : " + release.ToString() + "\nThe reason :" + ex.Message);
            }
        }

        public void AddAdoption(Adoption adoption)
        {
            ArgumentNullException.ThrowIfNull(adoption, nameof(adoption));
            try
            {
                this.Adoptions.Add(adoption);
            }
            catch (ArgumentException ex)
            {
                MyLogger.LogError("Unable to add the following adoption : " + adoption.ToString() + "\nThe reason :" + ex.Message);
            }

        }

        public void RemoveAdoption(Adoption adoption)
        {
            try
            {
                this.Adoptions.Remove(adoption);
            }
            catch (ArgumentException ex)
            {
                MyLogger.LogError("Unable to remove the following adoption : " + adoption.ToString() + "\nThe reason :" + ex.Message);
            }
        }

        public void AddFosterFamily(FosterFamily fosterFamily)
        {
            ArgumentNullException.ThrowIfNull(fosterFamily, nameof(fosterFamily));
            try
            {
                this.FosterFamilies.Add(fosterFamily);
            }
            catch (ArgumentException ex)
            {
                MyLogger.LogError("Unable to add the following animal welcomed as foster family : " + fosterFamily.ToString() + "\nThe reason :" + ex.Message);
            }

        }

        public void RemoveFosterFamily(FosterFamily fosterFamily)
        {
            try
            {
                this.FosterFamilies.Remove(fosterFamily);
            }
            catch (ArgumentException ex)
            {
                MyLogger.LogError("Unable to remove the following animal welcomed as foster family : " + fosterFamily.ToString() + "\nThe reason :" + ex.Message);
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

        public void AddAnimalCompatibility(AnimalCompatibility animalCompatibility)
        {
            ArgumentNullException.ThrowIfNull(animalCompatibility, nameof(animalCompatibility));

            try
            {
                AnimalCompatibilities.Add(animalCompatibility);

            }
            catch (ArgumentException ex)
            {
                MyLogger.LogError("Unable to add a animalCompatibility. Reason : {0} ", ex.Message);
            }
        }

        public void RemoveCompatibility(AnimalCompatibility animalCompatibility)
        {
            ArgumentNullException.ThrowIfNull(animalCompatibility, nameof(animalCompatibility));

            try
            {
                AnimalCompatibilities.Remove(animalCompatibility);
            }
            catch (ArgumentException ex)
            {
                MyLogger.LogError("Unable to remove a animalCompatibility. Reason : {0} ", ex.Message);
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

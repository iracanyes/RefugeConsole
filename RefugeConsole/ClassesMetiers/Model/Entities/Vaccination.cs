using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RefugeConsole.ClassesMetiers.Model.Entities
{
    internal class Vaccination
    {
        // Constructor used by EntityFramework
        public Vaccination(DateOnly? dateVaccination, bool done, Animal animal, Vaccine vaccine)
            : this(Guid.NewGuid(), DateTime.Now, dateVaccination, done, animal, vaccine)
        { }

        public Vaccination(Guid id, DateTime dateCreated, DateOnly? dateVaccination, bool done, Animal animal, Vaccine vaccine) {
            ArgumentNullException.ThrowIfNull(animal, nameof(animal));
            ArgumentNullException.ThrowIfNull(vaccine, nameof(vaccine));

            this.Id = id;
            this.DateCreated = dateCreated;
            this.DateVaccination = dateVaccination;
            this.Done = done;

            this.Animal = animal;
            this.AnimalId = animal.Id;

            this.Vaccine = vaccine;
            this.VaccineId = vaccine.Id;
        }

        [Key]
        public Guid Id { get; private set; }

        [Required]
        public DateTime DateCreated { get; set; }

        [Required]
        public DateOnly? DateVaccination { get; set; }
        [Required]
        public bool Done { get; set; }
        [Required]
        public string AnimalId {  get; set; }
        public Animal Animal { get; set; }
        [Required]
        public Guid VaccineId { get; set; }

        public Vaccine Vaccine { get; set; }

        public override string ToString()
        {
            return string.Format(
                "Vaccination{{ id = {0}, dateCreated = {1}, dateVaccination = {2}, done = {3}, vaccin = {4} animal = {5} }}",
                this.Id,
                this.DateCreated,
                this.DateVaccination,
                this.Done,
                this.Vaccine,
                this.Animal
            );
        }
    }
}

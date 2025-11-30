using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RefugeConsole.ClassesMetiers.Model.Entities
{
    internal class Vaccination
    {
        // Constructor used by EntityFramework
        public Vaccination(Guid id, DateOnly dateVaccination, bool done)
        {

            this.Id = id;
            this.DateVaccination = dateVaccination;
            this.Done = done;
        }

        public Vaccination(Guid id, DateOnly dateVaccination, bool done, Animal animal, Vaccine vaccin) {
            ArgumentNullException.ThrowIfNull(animal, nameof(animal));
            ArgumentNullException.ThrowIfNull(vaccin, nameof(vaccin));

            this.Id = id;
            this.DateVaccination = dateVaccination;
            this.Done = done;

            this.Animal = animal;
            this.AnimalId = animal.Id;

            this.Vaccin = vaccin;
            this.VaccinId = vaccin.Id;
        }

        [Key]
        public Guid Id { get; private set; }
        [Required]
        public DateOnly DateVaccination { get; set; }
        [Required]
        public bool Done { get; set; }
        [Required]
        public string AnimalId {  get; set; }
        public Animal Animal { get; set; }
        [Required]
        public Guid VaccinId { get; set; }

        public Vaccine Vaccin { get; set; }

        public override string ToString()
        {
            return string.Format(
                "Vaccination{{ id = {0}, dateVaccination = {1}, done = {2}, vaccin = {3} animal = {4} }}",
                this.Id,
                this.DateVaccination,
                this.Done,
                this.Vaccin,
                this.Animal
            );
        }
    }
}

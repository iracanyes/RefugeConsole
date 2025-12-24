using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RefugeConsole.ClassesMetiers.Model.Entities
{
    internal class Vaccine
    {
        private static readonly ILogger MyLogger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger(nameof(Vaccine));

        public Vaccine(string name)
            : this(Guid.NewGuid(), name)
        { }

        public Vaccine(Guid id, string name) { 
            this.Id = id;
            this.Name = name;
        }

        [Key]
        public Guid Id { get; private set; }

        [Required]
        public string Name { get; set; }

        public HashSet<Vaccination> Vaccinations { get; set; } = new HashSet<Vaccination>();

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

        public override string ToString()
        {
            return string.Format(
                "Vaccin{{ id = {0}, name = {1}",
                this.Id,
                this.Name
            );
        }
    }
}

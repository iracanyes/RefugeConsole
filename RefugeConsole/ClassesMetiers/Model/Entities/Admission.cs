using Microsoft.Extensions.Logging;
using RefugeConsole.ClassesMetiers.Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RefugeConsole.ClassesMetiers.Model.Entities
{
    internal class Admission
    {
        public static readonly ILogger MyLogger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger(nameof(Admission));
        
        public Admission(Guid id, string type, DateTime dateCreated)
        {
            this.Id = id;
            this.Type = type;
            this.DateCreated = dateCreated;
        }

        public Admission(Guid id, string type, DateTime dateCreated, OtherContact otherContact, Animal animal)
        {
            ArgumentNullException.ThrowIfNull(otherContact, nameof(OtherContact));
            ArgumentNullException.ThrowIfNull(animal, nameof(animal));

            this.Id = id;
            this.Type = type;
            this.DateCreated = dateCreated;

            this.OtherContactId = otherContact.Id;
            this.OtherContact = otherContact;

            this.AnimalId = animal.Id;
            this.Animal = animal;
        }

        [Key]
        public Guid Id { get; private set; }

        [Required]
        public string Type { get; set; }

        [Required]
        public DateTime DateCreated { get; set; }

        
        public Guid OtherContactId { get; set; }
        
        public OtherContact OtherContact { get; set; }

        
        public string AnimalId { get; set; }
        
        public Animal Animal { get; set; }

        public IList<Candidate> Candidates { get; set; } = new List<Candidate>();


        public void AddCandidate(Candidate candidate) { 
            ArgumentNullException.ThrowIfNull(candidate, nameof(candidate));

            try
            {
                this.Candidates.Add(candidate);
            }
            catch (Exception ex)
            {

                MyLogger.LogError("Unable to add a candidate. Reason : {0}", ex.Message);
            }
        }

        public void RemoveCandidate(Candidate candidate)
        {
            ArgumentNullException.ThrowIfNull(candidate, nameof(candidate));

            try
            {
                this.Candidates.Remove(candidate);
            }
            catch (Exception ex)
            {

                MyLogger.LogError("Unable to remove a candidate. Reason : {0}", ex.Message);
            }
        }

        public override string ToString() {
            return string.Format(
                "Admission{{ id = {0}, type = {1}, dateCreated = {2}, contact = {3}, animal = {4} }}",
                this.Id,
                this.Type, 
                this.DateCreated,
                this.OtherContact,
                this.Animal
            );
        }

    }
}

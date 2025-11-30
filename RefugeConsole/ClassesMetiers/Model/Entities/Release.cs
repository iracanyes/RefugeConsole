using RefugeConsole.ClassesMetiers.Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RefugeConsole.ClassesMetiers.Model.Entities
{
    internal class Release
    {
        public Release(Guid id, string type, DateTime dateCreated)
        {
            this.Id = id;
            this.Type = type;
            this.DateCreated = dateCreated;
        }

        public Release(Guid id, string type, DateTime dateCreated, Admission admission)
        {
            ArgumentNullException.ThrowIfNull(admission, nameof(admission));
            this.Id = id;
            this.Type = type;
            this.DateCreated = dateCreated;

            this.AdmissionId = admission.Id;
            this.Admission = admission;
        }

        [Key]
        public Guid Id { get; private set; }
        [Required]
        public string Type { get; set; }
        [Required]
        public DateTime DateCreated { get; set; }
        [Required]
        public Guid AdmissionId { get; set; }
        public Admission Admission { get; set; }

        public override string ToString()
        {
            return string.Format(
                "Release{{ id = {0}, type = {1}, dateCreated = {2}, admission = {3}}}",
                this.Id, this.Type, this.DateCreated, this.Admission
            );
        }
    }
}

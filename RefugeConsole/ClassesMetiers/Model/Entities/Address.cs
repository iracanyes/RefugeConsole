using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RefugeConsole.ClassesMetiers.Model.Entities
{
    internal class Address
    {
        public Address(string street, string city, string state, string zipCode, string country)
            : this(Guid.NewGuid(), street, city, state, zipCode, country) { }

        public Address(Guid id, string street, string city, string state, string zipCode, string country) {
            this.Id = id;
            this.Street = street;
            this.City = city;
            this.State = state;
            this.ZipCode = zipCode;
            this.Country = country;
        }

        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Street { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string State { get; set; }
        [Required]
        public string ZipCode { get; set; }
        [Required]
        public string Country { get; set; }
    }
}

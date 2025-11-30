using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefugeConsole.ClassesMetiers.Model.Entities
{
    internal class ContactInfo
    {
        /*================ variables statiques =========================================*/
        private static readonly ILogger MyLogger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger(nameof(ContactInfo));

        /*================ Constructeurs =========================================*/
        public ContactInfo(Guid id, string firstname,  string lastname, string registryNumber, string email, string address, string phoneNumber, string mobileNumber)
        {
            this.Id = id;
            this.Firstname = firstname;
            this.Lastname = lastname;
            this.RegistryNumber = registryNumber;
            this.Email = email;
            this.Address = address;
            this.PhoneNumber = phoneNumber;
            this.MobileNumber = mobileNumber;
        }

        /*================ Propriétés =========================================*/
        [Key]
        public Guid Id { get; private set; }
        [Required]
        public string Firstname { get; set; }
        [Required]
        public string Lastname { get; set; }
        [Required]
        public string RegistryNumber { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        [Required]
        public string MobileNumber { get; set; }

        public HashSet<Contact> Contacts { get; } = new HashSet<Contact>();

        /*================ Méthodes d'instance =========================================*/
        public void AddContact(Contact contact) { 
            ArgumentNullException.ThrowIfNull(contact, nameof(contact));
            try
            {
                this.Contacts.Add(contact);
            }
            catch (ArgumentException ex) {
                MyLogger.LogError("Unable to add the following contact : " + contact.ToString() + "\nThe reason :" + ex.Message);
            }
            
        }

        public void RemoveContact(Contact contact)
        {
            try
            {
                this.Contacts.Remove(contact);
            }
            catch (ArgumentException ex) {
                MyLogger.LogError("Unable to remove the following contact : " + contact.ToString() + "\nThe reason :" + ex.Message);
            }
        }

        public override string ToString()
        {
            return string.Format("ContactInfo{{ Id = {0}," +
                " firstname = {1}, " +
                "lastname = {2}, " +
                "registryNumber = {3}, " +
                "email =  {4}, " +
                "phoneNumber  = {5}, " +
                "mobileNumber = {6} }}",
                this.Id, this.Firstname, this.Lastname, this.RegistryNumber, this.Email, this.PhoneNumber, this.MobileNumber
            
                );
        }

    }
}

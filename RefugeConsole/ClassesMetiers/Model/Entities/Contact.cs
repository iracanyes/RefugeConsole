using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefugeConsole.ClassesMetiers.Model.Entities
{
    internal class Contact
    {
        /*================ variables statiques =========================================*/
        private static readonly ILogger MyLogger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger(nameof(Contact));

        /*================ Constructeurs =========================================*/
        public Contact(Guid id, string firstname,  string lastname, string registryNumber, string email, string phoneNumber, string mobileNumber, Address address)
        {

            ArgumentNullException.ThrowIfNull(address, nameof(address));


            this.Id = id;
            this.Firstname = firstname;
            this.Lastname = lastname;
            this.RegistryNumber = registryNumber;
            this.Email = email;
            this.Address = address;
            this.PhoneNumber = phoneNumber;
            this.MobileNumber = mobileNumber;

            this.Address = address;
            this.AddressId = address.Id;
        }

        /*================ Propriétés =========================================*/
        [Key]
        public Guid Id { get; private set; }
        [Required]
        public string Firstname { 
            get;
            set {
                if (value.Length < 2)
                    throw new ArgumentException("Le nom doit avoir une longueur d'au moins 2 caractères!");
                field = value;
            } 
        }

        [Required]
        public string Lastname
        {
            get;
            set
            {
                if (value.Length < 2)
                    throw new ArgumentException("Le nom doit avoir une longueur d'au moins 2 caractères!");
                // Set the value on the backed properties
                field = value;
            }
        }

        [Required]
        public string RegistryNumber { get; set; }
        
        
        [Required]
        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        [Required]
        public string MobileNumber { get; set; }
        
        [Required]
        public Guid AddressId { get; set; }
        public Address Address { get; set; }

        public HashSet<ContactRole> ContactRoles { get; } = new HashSet<ContactRole>();

        public HashSet<Admission> Admissions { get; } = new HashSet<Admission>();

        public HashSet<Release> Releases { get; } = new HashSet<Release>();

        public HashSet<Adoption> Adoptions { get; } = new HashSet<Adoption>();

        public HashSet<FosterFamily> FosterFamilies { get; } = new HashSet<FosterFamily>();

        /*================ Méthodes d'instance =========================================*/
        public void AddContactRole(ContactRole contactRole) { 
            ArgumentNullException.ThrowIfNull(contactRole, nameof(contactRole));
            try
            {
                this.ContactRoles.Add(contactRole);
            }
            catch (ArgumentException ex) {
                MyLogger.LogError("Unable to add the following contactRole : " + contactRole.ToString() + "\nThe reason :" + ex.Message);
            }
            
        }

        public void RemoveContactRole(ContactRole contactRole)
        {
            try
            {
                this.ContactRoles.Remove(contactRole);
            }
            catch (ArgumentException ex) {
                MyLogger.LogError("Unable to remove the following contactRole : " + contactRole.ToString() + "\nThe reason :" + ex.Message);
            }
        }

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

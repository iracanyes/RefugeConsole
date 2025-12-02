using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RefugeConsole.ClassesMetiers.Helper;
using RefugeConsole.ClassesMetiers.Model.Entities;
using RefugeConsole.ClassesMetiers.Model.Enums;

namespace RefugeConsole.ClassesMetiers.Config
{
    internal class RefugeDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            

            //Console.WriteLine($"Connection string : {Environment.GetEnvironmentVariable("REFUGE_DB_CONNECTION_STRING")}");
            options.UseNpgsql(Environment.GetEnvironmentVariable("REFUGE_DB_CONNECTION_STRING"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            /*-- Primary keys are defined in the model ----*/

            /* Index and Unicity constraints */
            modelBuilder.Entity<ContactInfo>()
                .HasIndex(ci => ci.RegistryNumber)
                .IsUnique();
            modelBuilder.Entity<ContactInfo>()
                .HasIndex(ci => ci.Email)
                .IsUnique();

            modelBuilder.Entity<Animal>()
                .HasIndex(a => a.Name);

            modelBuilder.Entity<Vaccine>()
                .HasIndex(v =>  v.Name)
                .IsUnique();

            /*- Inheritance -*/
            // Using the default strategy : TPH 
            // One single table with a discriminator property
            /*
            modelBuilder.Entity<Contact>()
                .HasDiscriminator<string>("Type")
                .HasValue<OtherContact>(MyEnumHelper.GetEnumDescription(ContactType.OtherContact))
                .HasValue<Volunteer>(MyEnumHelper.GetEnumDescription(ContactType.Volunteer))
                .HasValue<FosterFamily>(MyEnumHelper.GetEnumDescription(ContactType.FosterFamily))
                .HasValue<Candidate>(MyEnumHelper.GetEnumDescription(ContactType.Candidate))
                .HasValue<Adopter>(MyEnumHelper.GetEnumDescription(ContactType.Adopter));
            */

            // Create table for each type
            modelBuilder.Entity<Contact>()
                .UseTpcMappingStrategy()
                .ToTable("Contacts");
            modelBuilder.Entity<OtherContact>().ToTable("OtherContacts");
            modelBuilder.Entity<Volunteer>().ToTable("Volunteers");
            modelBuilder.Entity<FosterFamily>().ToTable("FosterFamilies");
            modelBuilder.Entity<Candidate>().ToTable("Candidates");
            modelBuilder.Entity<Adopter>().ToTable("Adopters");


            /*- Other constraints -*/
            modelBuilder.Entity<Animal>()
                .Property("Id")
                .HasMaxLength(11);



        }

        public DbSet<ContactInfo> ContactInfos { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Volunteer> Volunteers { get; set; }
        public DbSet<OtherContact> OtherContacts { get; set; }
        public DbSet<Candidate> Candidates { get; set; }
        public DbSet<FosterFamily> FosterFamilies { get; set; }
        public DbSet<Adopter> Adopters { get; set; }
        public DbSet<Admission> Admissions { get; set; }
        public DbSet<Release> Releases { get; set; }
        public DbSet<Animal> Animals { get; set; }
        public DbSet<Vaccine> Vaccines { get; set; }
        public DbSet<Vaccination> Vaccinations { get; set; }
        public DbSet<Compatibility> Compatibilities { get; set; }




    }
}

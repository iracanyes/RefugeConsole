using Npgsql;
using RefugeConsole.ClassesMetiers.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace RefugeConsole.CoucheAccesDB
{
    internal interface IRefugeDataService
    {
        bool HandleAdmission(Admission admission);

        bool CreateAdmission(Admission admission, NpgsqlTransaction transaction );

        HashSet<Admission> GetAdmissions();

        HashSet<FosterFamily> GetFosterFamilies();

        HashSet<FosterFamily> GetFosterFamiliesForAnimal(Animal animal);

        bool CreateReleaseForFosterFamily(FosterFamily fosterFamily, Release release);

        bool CreateRelease(Release release, NpgsqlTransaction transaction);

        FosterFamily GetFosterFamily(Contact contact, Animal animal);

        bool CreateFosterFamily(FosterFamily fosterFamily, NpgsqlTransaction transaction);

        bool UpdateFosterFamily(FosterFamily fosterFamily, NpgsqlTransaction transaction);

        HashSet<Adoption> GetAdoptions();

        Adoption GetAdoption(Contact contact, Animal animal);

        bool CreateAdoption(Adoption adoption);

        bool CreateReleaseForAdoption(Adoption adoption, Release release);

        bool UpdateAdoption(Adoption adoption, NpgsqlTransaction? transaction = null);

        Vaccine? GetVaccine(string name);

        Vaccine CreateVaccine(Vaccine vaccine);

        bool CreateVaccination(Vaccination vaccination);


    }
}

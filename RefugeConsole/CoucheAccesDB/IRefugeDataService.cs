using Npgsql;
using RefugeConsole.ClassesMetiers.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace RefugeConsole.CoucheAccesDB
{
    internal interface IRefugeDataService
    {
        bool CreateAdmission(Admission admission);

        HashSet<Admission> GetAdmissions();

        HashSet<FosterFamily> GetFosterFamilies();

        HashSet<FosterFamily> GetFosterFamiliesForAnimal(Animal animal);

        bool CreateReleaseForFosterFamily(FosterFamily fosterFamily, Release release);

        bool CreateRelease(Release release, NpgsqlTransaction transaction);

        bool CreateFosterFamily(FosterFamily fosterFamily, NpgsqlTransaction transaction);

        HashSet<Adoption> GetAdoptions();

        bool CreateAdoption(Adoption adoption, NpgsqlTransaction transaction);

        bool UpdateAdoption(Adoption adoption);

        bool CreateVaccination(Vaccination vaccination);


    }
}

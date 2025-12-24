using Npgsql;
using Npgsql.Internal;
using RefugeConsole.ClassesMetiers.Exceptions;
using RefugeConsole.ClassesMetiers.Helper;
using RefugeConsole.ClassesMetiers.Model.Entities;
using RefugeConsole.ClassesMetiers.Model.Enums;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System.Text;

namespace RefugeConsole.CoucheAccesDB
{
    internal class RefugeDataService: AccessDb, IRefugeDataService
    {
        public bool HandleAdmission(Admission admission)
        {
            bool result = false;
            NpgsqlTransaction transaction = this.SqlConn.BeginTransaction();

            try
            {
                if(admission.Reason == MyEnumHelper.GetEnumDescription<AdmissionType>(AdmissionType.ReturnAdoption))
                {
                    Adoption adoption = this.GetAdoption(admission.Contact, admission.Animal);
                    adoption.DateEnd = DateOnly.FromDateTime(DateTime.Now);

                    result = this.UpdateAdoption(adoption, transaction);
                }

                if(admission.Reason == MyEnumHelper.GetEnumDescription<AdmissionType>(AdmissionType.ReturnFosterFamily))
                {
                    FosterFamily ff = this.GetFosterFamily(admission.Contact, admission.Animal);
                    ff.DateEnd = DateOnly.FromDateTime(DateTime.Now);

                    result = this.UpdateFosterFamily(ff, transaction);
                }

                result = this.CreateAdmission(admission, transaction);

                transaction.Commit();

                
            }
            catch (Exception ex)
            {
                if (transaction != null && transaction.Connection != null) transaction.Rollback();

                if (Debugger.IsAttached)
                    Debug.WriteLine($"Error while creating admission in DB. Error : {ex.Message}. Exception: {ex}");

                throw new AccessDbException("sqlCmd is NULL", $"Error while creating admission in  DB. Error : {ex.Message}. Exception: {ex}");

            }

            return result;
        }
        public bool CreateAdmission(Admission admission, NpgsqlTransaction transaction)
        {
            bool result = false;
            NpgsqlCommand? sqlCmd = null;

            try
            {
                sqlCmd = new NpgsqlCommand(
                    """
                    INSERT INTO public."Admissions" ("Id", "Reason", "DateCreated", "ContactId", "AnimalId")
                    VALUES (:id, :reason, :dateCreated, :contactId, :animalId)
                    """,
                    this.SqlConn,
                    transaction
                );

                sqlCmd.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Uuid);
                sqlCmd.Parameters.Add("reason", NpgsqlTypes.NpgsqlDbType.Text);
                sqlCmd.Parameters.Add("dateCreated", NpgsqlTypes.NpgsqlDbType.TimestampTz);
                sqlCmd.Parameters.Add("contactId", NpgsqlTypes.NpgsqlDbType.Uuid);
                sqlCmd.Parameters.Add("animalId", NpgsqlTypes.NpgsqlDbType.Varchar);

                sqlCmd.Prepare();

                sqlCmd.Parameters["id"].Value = admission.Id;
                sqlCmd.Parameters["reason"].Value = admission.Reason;
                sqlCmd.Parameters["dateCreated"].Value = admission.DateCreated.ToUniversalTime();
                sqlCmd.Parameters["contactId"].Value = admission.ContactId;
                sqlCmd.Parameters["animalId"].Value = admission.AnimalId;

                int nbRowAffected = sqlCmd.ExecuteNonQuery();

                if (nbRowAffected == 0) throw new Exception($"Unknown error while creating admission instance in DB.Object : {admission}");

                result = true;

            }
            catch (Exception ex) {
                if (Debugger.IsAttached)
                    Debug.WriteLine($"Error while creating admission in DB. Error : {ex.Message}. Exception: {ex}");

                if (sqlCmd != null)
                    throw new AccessDbException(sqlCmd.CommandText, $"Error while creating admission in DB. Error : {ex.Message}. Exception: {ex}");
                else
                    throw new AccessDbException("sqlCmd is NULL", $"Error while creating admission in  DB. Error : {ex.Message}. Exception: {ex}");
            }

            return result;
        }

        public HashSet<Admission> GetAdmissions() {
            HashSet<Admission> result = new HashSet<Admission>();
            NpgsqlCommand? sqlCmd = null;
            NpgsqlDataReader? reader = null;

            try
            {
                sqlCmd = new NpgsqlCommand(
                    """
                    SELECT adm."Id" AS "Id",
                            adm."Reason" AS "Reason",
                            adm."DateCreated" AS "DateCreated",
                            adm."AnimalId" AS "AnimalId",
                            adm."ContactId" AS "ContactId",
                            a."Name" AS "Name",
                            a."Type" AS "Type",
                            a."Gender" AS "Gender",
                            a."BirthDate" AS "BirthDate",
                            a."DeathDate" AS "DeathDate",
                            a."IsSterilized" AS "IsSterilized",
                            a."DateSterilization" AS "DateSterilization",
                            a."Particularity" AS "Particularity",
                            a."Description" AS "Description",
                            c."Firstname" AS "Firstname",
                            c."Lastname" AS "Lastname",
                            c."RegistryNumber" AS "RegistryNumber",
                            c."Email" AS "Email",
                            c."PhoneNumber" AS "PhoneNumber",
                            c."MobileNumber" AS "MobileNumber",
                            c."AddressId" AS "AddressId",
                            add."Street" AS "Street",
                            add."City" AS "City",
                            add."State" AS "State",
                            add."ZipCode" AS "ZipCode",
                            add."Country" AS "Country"
                    FROM public."Admissions" adm
                    INNER JOIN public."Animals" a
                        ON adm."AnimalId" = a."Id"
                    INNER JOIN public."Contacts" c
                        ON adm."ContactId" = c."Id"
                    INNER JOIN public."Addresses" add
                        ON c."AddressId" = add."Id"
                    """,
                    this.SqlConn
                );

                sqlCmd.Prepare();

                reader  = sqlCmd.ExecuteReader();

                while (reader.Read()) {
                    DateOnly? birthDate = reader["BirthDate"] != DBNull.Value ? (DateOnly)reader["BirthDate"] : null;
                    DateOnly? deathDate = reader["DeathDate"] != DBNull.Value ? (DateOnly)reader["DeathDate"] : null;
                    DateOnly? dateSterilization = reader["dateSterilization"] != DBNull.Value ? (DateOnly)reader["dateSterilization"] : null;

                    //
                    Animal animal = new Animal(
                        Convert.ToString(reader["AnimalId"])!,
                        Convert.ToString(reader["Name"])!,
                        MyEnumHelper.GetEnumFromDescription<AnimalType>(Convert.ToString(reader["Type"])!),
                        MyEnumHelper.GetEnumFromDescription<GenderType>(Convert.ToString(reader["Gender"])!),
                        birthDate,
                        deathDate,
                        Convert.ToBoolean(reader["IsSterilized"])!,
                        dateSterilization,
                        Convert.ToString(reader["Particularity"])!,
                        Convert.ToString(reader["Description"])!

                    );

                    Address address = new Address(
                        new Guid(Convert.ToString(reader["AddressId"])!),
                        Convert.ToString(reader["Street"])!,
                        Convert.ToString(reader["City"])!,
                        Convert.ToString(reader["State"])!,
                        Convert.ToString(reader["ZipCode"])!,
                        Convert.ToString(reader["Country"])!
                    );

                    Contact contact = new Contact(
                        new Guid(Convert.ToString(reader["ContactId"])!),
                        Convert.ToString(reader["Firstname"])!,
                        Convert.ToString(reader["Lastname"])!,
                        Convert.ToString(reader["RegistryNumber"])!,
                        Convert.ToString(reader["Email"])!,
                        Convert.ToString(reader["PhoneNumber"])!,
                        Convert.ToString(reader["MobileNumber"])!,
                        address
                    );

                    

                    result.Add(new Admission(
                        new Guid(Convert.ToString(reader["Id"])!),
                        Convert.ToString(reader["Reason"])!,
                        Convert.ToDateTime(reader["DateCreated"])!,
                        contact,
                        animal
                    ));
                    
                }

                reader.Close();


            }
            catch (Exception ex)
            {
                if (reader != null) reader.Close();

                if (sqlCmd != null)
                    throw new AccessDbException(sqlCmd.CommandText, $"Error while retrieving admissions from DB. Error : {ex.Message}. Exception : {ex}");
                else
                    throw new AccessDbException("CreateAdmission : sqlCmd is null!", $"Error while retrieving admissions from DB. Error : {ex.Message}. Exception : {ex}");

                
            }


            return result;

        }

        

        public HashSet<FosterFamily> GetFosterFamiliesForAnimal(Animal animal)
        {
            HashSet<FosterFamily> result = new HashSet<FosterFamily>();
            NpgsqlCommand? sqlCmd = null;
            NpgsqlDataReader? reader = null;

            try
            {
                sqlCmd = new NpgsqlCommand(
                    """
                    SELECT ff."Id" AS "Id",
                           ff."DateCreated" AS "DateCreated",
                           ff."DateStart" AS "DateStart",
                           ff."DateEnd" AS "DateEnd",
                           ff."AnimalId" AS "AnimalId",
                           ff."ContactId" AS "ContactId",
                           a."Name" AS "Name",
                           a."Type" AS "Type",
                           a."Gender" AS "Gender",
                           a."BirthDate" AS "BirthDate",
                           a."DeathDate" AS "DeathDate",
                           a."IsSterilized" AS "IsSterilized",
                           a."DateSterilization" AS "DateSterilization",
                           a."Particularity" AS "Particularity",
                           a."Description" AS "Description",
                           c."Firstname" AS "Firstname",
                           c."Lastname" AS "Lastname",
                           c."RegistryNumber" AS "RegistryNumber",
                           c."Email" AS "Email",
                           c."PhoneNumber" AS "PhoneNumber",
                           c."MobileNumber" AS "MobileNumber",
                           c."AddressId" AS "AddressId",
                           add."Street" AS "Street",
                           add."City" AS "City",
                           add."State" AS "State",
                           add."ZipCode" AS "ZipCode",
                           add."Country" AS "Country"
                    FROM public."FosterFamilies" ff
                    INNER JOIN public."Animals" a
                        ON ff."AnimalId" = a."Id"
                    INNER JOIN public."Contacts" c
                        ON ff."ContactId" = c."Id"
                    INNER JOIN public."Addresses" add
                        ON c."AddressId" = add."Id"
                    WHERE ff."AnimalId" = :animalId
                    ORDER BY ff."DateCreated" DESC
                    """,
                    this.SqlConn
                );

                sqlCmd.Parameters.Add(new NpgsqlParameter("animalId", NpgsqlTypes.NpgsqlDbType.Varchar));

                sqlCmd.Prepare();

                sqlCmd.Parameters["animalId"].Value = animal.Id;

                reader = sqlCmd.ExecuteReader();

                while (reader.Read())
                {
                    // Dates for foster family instance
                    DateOnly dateStart = (DateOnly)reader["DateStart"];
                    DateOnly? dateEnd = reader["DateEnd"] != DBNull.Value ? (DateOnly)reader["DateEnd"] : null;

                    // Dates for animal's instance
                    DateOnly? birthDate = reader["BirthDate"] != DBNull.Value ? (DateOnly)reader["BirthDate"] : null;
                    DateOnly? deathDate = reader["DeathDate"] != DBNull.Value ? (DateOnly)reader["DeathDate"] : null;
                    DateOnly? dateSterilization = reader["dateSterilization"] != DBNull.Value ? (DateOnly)reader["dateSterilization"] : null;

                    //
                    Animal animalInDb = new Animal(
                        Convert.ToString(reader["AnimalId"])!,
                        Convert.ToString(reader["Name"])!,
                        MyEnumHelper.GetEnumFromDescription<AnimalType>(Convert.ToString(reader["Type"])!),
                        MyEnumHelper.GetEnumFromDescription<GenderType>(Convert.ToString(reader["Gender"])!),
                        birthDate,
                        deathDate,
                        Convert.ToBoolean(reader["IsSterilized"])!,
                        dateSterilization,
                        Convert.ToString(reader["Particularity"])!,
                        Convert.ToString(reader["Description"])!

                    );

                    Address address = new Address(
                        new Guid(Convert.ToString(reader["AddressId"])!),
                        Convert.ToString(reader["Street"])!,
                        Convert.ToString(reader["City"])!,
                        Convert.ToString(reader["State"])!,
                        Convert.ToString(reader["ZipCode"])!,
                        Convert.ToString(reader["Country"])!
                    );

                    Contact contact = new Contact(
                        new Guid(Convert.ToString(reader["ContactId"])!),
                        Convert.ToString(reader["Firstname"])!,
                        Convert.ToString(reader["Lastname"])!,
                        Convert.ToString(reader["RegistryNumber"])!,
                        Convert.ToString(reader["Email"])!,
                        Convert.ToString(reader["PhoneNumber"])!,
                        Convert.ToString(reader["MobileNumber"])!,
                        address
                    );



                    result.Add(new FosterFamily(
                        new Guid(Convert.ToString(reader["Id"])!),
                        Convert.ToDateTime(reader["DateCreated"])!,
                        dateStart,
                        dateEnd,
                        contact,
                        animalInDb
                    ));

                }

                reader.Close();
            }
            catch (Exception ex)
            {
                if(reader != null) reader.Close();
                
                if (Debugger.IsAttached)
                    Debug.WriteLine($"Error while listing past foster family for an animal. Error : {ex.Message}. Exception : {ex}");

                if(sqlCmd != null)
                    throw new AccessDbException(sqlCmd.CommandText, $"Error while listing past foster family for an animal. Error : {ex.Message}. Exception : {ex}");
                else
                    throw new AccessDbException("GetFosterFamiliesForAnimal : sqlCmd is null", $"Error while listing past foster family for an animal. Error : {ex.Message}. Exception : {ex}");

            }

            return result;
        }

        public HashSet<FosterFamily> GetFosterFamilies()
        {
            HashSet<FosterFamily> result = new HashSet<FosterFamily>();
            NpgsqlCommand? sqlCmd = null;
            NpgsqlDataReader? reader = null;

            try
            {
                sqlCmd = new NpgsqlCommand(
                    """
                    SELECT ff."Id" AS "Id",
                           ff."DateCreated" AS "DateCreated",
                           ff."DateStart" AS "DateStart",
                           ff."DateEnd" AS "DateEnd",
                           ff."AnimalId" AS "AnimalId",
                           ff."ContactId" AS "ContactId",
                           a."Name" AS "Name",
                           a."Type" AS "Type",
                           a."Gender" AS "Gender",
                           a."BirthDate" AS "BirthDate",
                           a."DeathDate" AS "DeathDate",
                           a."IsSterilized" AS "IsSterilized",
                           a."DateSterilization" AS "DateSterilization",
                           a."Particularity" AS "Particularity",
                           a."Description" AS "Description",
                           c."Firstname" AS "Firstname",
                           c."Lastname" AS "Lastname",
                           c."RegistryNumber" AS "RegistryNumber",
                           c."Email" AS "Email",
                           c."PhoneNumber" AS "PhoneNumber",
                           c."MobileNumber" AS "MobileNumber",
                           c."AddressId" AS "AddressId",
                           add."Street" AS "Street",
                           add."City" AS "City",
                           add."State" AS "State",
                           add."ZipCode" AS "ZipCode",
                           add."Country" AS "Country"
                    FROM public."FosterFamilies" ff
                    INNER JOIN public."Animals" a
                        ON ff."AnimalId" = a."Id"
                    INNER JOIN public."Contacts" c
                        ON ff."ContactId" = c."Id"
                    INNER JOIN public."Addresses" add
                        ON c."AddressId" = add."Id"
                    WHERE ff."DateEnd" IS NULL
                    """,
                    this.SqlConn
                );

                sqlCmd.Prepare();

                reader = sqlCmd.ExecuteReader();

                while (reader.Read())
                {
                    // Dates for foster family instance
                    DateOnly dateStart = (DateOnly)reader["DateStart"];
                    DateOnly? dateEnd = reader["DateEnd"] != DBNull.Value ? (DateOnly)reader["DateEnd"] : null;

                    // Dates for animal's instance
                    DateOnly? birthDate = reader["BirthDate"] != DBNull.Value ? (DateOnly)reader["BirthDate"] : null;
                    DateOnly? deathDate = reader["DeathDate"] != DBNull.Value ? (DateOnly)reader["DeathDate"] : null;
                    DateOnly? dateSterilization = reader["dateSterilization"] != DBNull.Value ? (DateOnly)reader["dateSterilization"] : null;

                    //
                    Animal animal = new Animal(
                        Convert.ToString(reader["AnimalId"])!,
                        Convert.ToString(reader["Name"])!,
                        MyEnumHelper.GetEnumFromDescription<AnimalType>(Convert.ToString(reader["Type"])!),
                        MyEnumHelper.GetEnumFromDescription<GenderType>(Convert.ToString(reader["Gender"])!),
                        birthDate,
                        deathDate,
                        Convert.ToBoolean(reader["IsSterilized"])!,
                        dateSterilization,
                        Convert.ToString(reader["Particularity"])!,
                        Convert.ToString(reader["Description"])!

                    );

                    Address address = new Address(
                        new Guid(Convert.ToString(reader["AddressId"])!),
                        Convert.ToString(reader["Street"])!,
                        Convert.ToString(reader["City"])!,
                        Convert.ToString(reader["State"])!,
                        Convert.ToString(reader["ZipCode"])!,
                        Convert.ToString(reader["Country"])!
                    );

                    Contact contact = new Contact(
                        new Guid(Convert.ToString(reader["ContactId"])!),
                        Convert.ToString(reader["Firstname"])!,
                        Convert.ToString(reader["Lastname"])!,
                        Convert.ToString(reader["RegistryNumber"])!,
                        Convert.ToString(reader["Email"])!,
                        Convert.ToString(reader["PhoneNumber"])!,
                        Convert.ToString(reader["MobileNumber"])!,
                        address
                    );



                    result.Add(new FosterFamily(
                        new Guid(Convert.ToString(reader["Id"])!),
                        Convert.ToDateTime(reader["DateCreated"])!,
                        dateStart,
                        dateEnd,
                        contact,
                        animal
                    ));

                }

                reader.Close();


            }
            catch (Exception ex)
            {
                if(reader != null) reader.Close();

                if(sqlCmd != null)
                    throw new AccessDbException(sqlCmd.CommandText, $"Error while retrieving foster families. Error : {ex.Message}. Exception : {ex}");
                else
                    throw new AccessDbException("GetFosterFamilies : sqlCmd is null", $"Error while retrieving foster families. Error : {ex.Message}. Exception : {ex}");

            }

            return result;
        }

        public bool CreateReleaseForFosterFamily(FosterFamily fosterFamily, Release release)
        {
            bool result = false;
            NpgsqlTransaction transaction = this.SqlConn.BeginTransaction();

            try
            {
                bool releaseSaved = this.CreateRelease(release, transaction);

                bool fosterFamilySaved = this.CreateFosterFamily(fosterFamily, transaction);

                // Commit transaction to DB
                if(releaseSaved && fosterFamilySaved) 
                    transaction.Commit();


            }
            catch (Exception ex)
            {
                if (transaction != null && transaction.Connection != null) transaction.Rollback();
                
                if (Debugger.IsAttached)
                    Debug.WriteLine($"Error while creating a foster family. Error : {ex.Message}. Exception : {ex}");


                throw;
            }

            return result;
        }

        public bool CreateRelease(Release release, NpgsqlTransaction transaction)
        {
            bool result = false;
            NpgsqlCommand? sqlCmd = null;

            try
            {
                sqlCmd = new NpgsqlCommand(
                    """
                    INSERT INTO public."Releases" ("Id", "Reason", "DateCreated", "AnimalId", "ContactId")
                    VALUES (:id, :reason, :dateCreated, :animalId, :contactId)
                    """,
                    this.SqlConn,
                    transaction
                );

                sqlCmd.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Uuid));
                sqlCmd.Parameters.Add(new NpgsqlParameter("reason", NpgsqlTypes.NpgsqlDbType.Text));
                sqlCmd.Parameters.Add(new NpgsqlParameter("dateCreated", NpgsqlTypes.NpgsqlDbType.TimestampTz));
                sqlCmd.Parameters.Add(new NpgsqlParameter("animalId", NpgsqlTypes.NpgsqlDbType.Varchar));
                sqlCmd.Parameters.Add(new NpgsqlParameter("contactId", NpgsqlTypes.NpgsqlDbType.Uuid));

                sqlCmd.Prepare();

                sqlCmd.Parameters["id"].Value = release.Id;
                sqlCmd.Parameters["reason"].Value = release.Reason;
                sqlCmd.Parameters["dateCreated"].Value = release.DateCreated.ToUniversalTime();
                sqlCmd.Parameters["animalId"].Value = release.AnimalId;
                sqlCmd.Parameters["contactId"].Value = release.ContactId;


                int nbRowAffected = sqlCmd.ExecuteNonQuery();

                if (nbRowAffected == 0)
                    throw new Exception($"Unable to save new release in DB.");

                result = true;
            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached)
                    Debug.WriteLine($"Error while creating a release in DB. Error : {ex.Message}. Exception : {ex}.");

                if (sqlCmd != null)
                    throw new AccessDbException(sqlCmd.CommandText, $"Error while creating a release in DB. Error : {ex.Message}. Exception : {ex}.");
                else
                    throw new AccessDbException("CreateRelease : sqlCmd is null", $"Error while creating a release in DB. Error : {ex.Message}. Exception : {ex}.");

            }


            return result;
        }

        public bool CreateFosterFamily(FosterFamily fosterFamily, NpgsqlTransaction transaction)
        {
            bool result = false;
            NpgsqlCommand? sqlCmd = null;

            try
            {
                sqlCmd = new NpgsqlCommand(
                     """
                    INSERT INTO public."FosterFamilies" ("Id", "DateCreated", "DateStart", "DateEnd", "AnimalId", "ContactId")
                    VALUES (:id, :dateCreated, :dateStart, :dateEnd, :animalId, :contactId)
                    """,
                    this.SqlConn,
                    transaction
                );

                sqlCmd.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Uuid));
                sqlCmd.Parameters.Add(new NpgsqlParameter("dateCreated", NpgsqlTypes.NpgsqlDbType.TimestampTz));
                sqlCmd.Parameters.Add(new NpgsqlParameter("dateStart", NpgsqlTypes.NpgsqlDbType.Date));
                sqlCmd.Parameters.Add(new NpgsqlParameter("dateEnd", NpgsqlTypes.NpgsqlDbType.Date));
                sqlCmd.Parameters.Add(new NpgsqlParameter("animalId", NpgsqlTypes.NpgsqlDbType.Varchar));
                sqlCmd.Parameters.Add(new NpgsqlParameter("contactId", NpgsqlTypes.NpgsqlDbType.Uuid));

                sqlCmd.Prepare();

                sqlCmd.Parameters["id"].Value = fosterFamily.Id;
                sqlCmd.Parameters["dateCreated"].Value = fosterFamily.DateCreated.ToUniversalTime();
                sqlCmd.Parameters["dateStart"].Value = fosterFamily.DateStart;
                sqlCmd.Parameters["dateEnd"].Value = fosterFamily.DateEnd == null ? DBNull.Value : fosterFamily.DateEnd;
                sqlCmd.Parameters["animalId"].Value = fosterFamily.AnimalId;
                sqlCmd.Parameters["contactId"].Value = fosterFamily.ContactId;

                int nbRowAffected = sqlCmd.ExecuteNonQuery();

                if (nbRowAffected == 0)
                    throw new Exception($"Unable to create a foster family");

                result = true;
            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached)
                    Debug.WriteLine($"Error while creating a foster family in DB. Error : {ex.Message}. Exception : {ex}.");

                if (sqlCmd != null)
                    throw new AccessDbException(sqlCmd.CommandText, $"Error while creating a foster family in DB. Error : {ex.Message}. Exception : {ex}.");
                else
                    throw new AccessDbException("CreateFosterFamily : sqlCmd is null", $"Error while creating a foster family in DB. Error : {ex.Message}. Exception : {ex}.");

            }


            return result;
        }

        public FosterFamily GetFosterFamily(Contact contact, Animal animal)
        {
            FosterFamily? result = null;
            NpgsqlCommand? sqlCmd = null;
            NpgsqlDataReader? reader = null;

            try
            {
                sqlCmd = new NpgsqlCommand(
                    """
                    SELECT ff."Id" AS "Id",
                           ff."DateCreated" AS "DateCreated",
                           ff."DateStart" AS "DateStart",
                           ff."DateEnd" AS "DateEnd",
                           ff."AnimalId" AS "AnimalId",
                           ff."ContactId" AS "ContactId",
                           a."Name" AS "Name",
                           a."Type" AS "Type",
                           a."Gender" AS "Gender",
                           a."BirthDate" AS "BirthDate",
                           a."DeathDate" AS "DeathDate",
                           a."IsSterilized" AS "IsSterilized",
                           a."DateSterilization" AS "DateSterilization",
                           a."Particularity" AS "Particularity",
                           a."Description" AS "Description",
                           c."Firstname" AS "Firstname",
                           c."Lastname" AS "Lastname",
                           c."RegistryNumber" AS "RegistryNumber",
                           c."Email" AS "Email",
                           c."PhoneNumber" AS "PhoneNumber",
                           c."MobileNumber" AS "MobileNumber",
                           c."AddressId" AS "AddressId",
                           add."Street" AS "Street",
                           add."City" AS "City",
                           add."State" AS "State",
                           add."ZipCode" AS "ZipCode",
                           add."Country" AS "Country"
                    FROM public."FosterFamilies" ff
                    INNER JOIN public."Animals" a
                        ON ff."AnimalId" = a."Id"
                    INNER JOIN public."Contacts" c
                        ON ff."ContactId" = c."Id"
                    INNER JOIN public."Addresses" add
                        ON c."AddressId" = add."Id"
                    WHERE ff."DateEnd" IS NULL AND ff."AnimalId" = :animalId AND ff."ContactId" = :contactId
                    """,
                    this.SqlConn
                );

                sqlCmd.Parameters.Add(new NpgsqlParameter("animalId", NpgsqlTypes.NpgsqlDbType.Varchar));
                sqlCmd.Parameters.Add(new NpgsqlParameter("contactId", NpgsqlTypes.NpgsqlDbType.Uuid));


                sqlCmd.Prepare();

                sqlCmd.Parameters["animalId"].Value = animal.Id;
                sqlCmd.Parameters["contactId"].Value = contact.Id;

                reader = sqlCmd.ExecuteReader();

                if (reader.Read())
                {
                    // Dates for foster family instance
                    DateOnly dateStart = (DateOnly)reader["DateStart"];
                    DateOnly? dateEnd = reader["DateEnd"] != DBNull.Value ? (DateOnly)reader["DateEnd"] : null;

                    // Dates for animal's instance
                    DateOnly? birthDate = reader["BirthDate"] != DBNull.Value ? (DateOnly)reader["BirthDate"] : null;
                    DateOnly? deathDate = reader["DeathDate"] != DBNull.Value ? (DateOnly)reader["DeathDate"] : null;
                    DateOnly? dateSterilization = reader["dateSterilization"] != DBNull.Value ? (DateOnly)reader["dateSterilization"] : null;

                    //
                    Animal animalInDb = new Animal(
                        Convert.ToString(reader["AnimalId"])!,
                        Convert.ToString(reader["Name"])!,
                        MyEnumHelper.GetEnumFromDescription<AnimalType>(Convert.ToString(reader["Type"])!),
                        MyEnumHelper.GetEnumFromDescription<GenderType>(Convert.ToString(reader["Gender"])!),
                        birthDate,
                        deathDate,
                        Convert.ToBoolean(reader["IsSterilized"])!,
                        dateSterilization,
                        Convert.ToString(reader["Particularity"])!,
                        Convert.ToString(reader["Description"])!

                    );

                    Address address = new Address(
                        new Guid(Convert.ToString(reader["AddressId"])!),
                        Convert.ToString(reader["Street"])!,
                        Convert.ToString(reader["City"])!,
                        Convert.ToString(reader["State"])!,
                        Convert.ToString(reader["ZipCode"])!,
                        Convert.ToString(reader["Country"])!
                    );

                    Contact contactInDb = new Contact(
                        new Guid(Convert.ToString(reader["ContactId"])!),
                        Convert.ToString(reader["Firstname"])!,
                        Convert.ToString(reader["Lastname"])!,
                        Convert.ToString(reader["RegistryNumber"])!,
                        Convert.ToString(reader["Email"])!,
                        Convert.ToString(reader["PhoneNumber"])!,
                        Convert.ToString(reader["MobileNumber"])!,
                        address
                    );



                    result = new FosterFamily(
                        new Guid(Convert.ToString(reader["Id"])!),
                        Convert.ToDateTime(reader["DateCreated"])!,
                        dateStart,
                        dateEnd,
                        contactInDb,
                        animalInDb
                    );

                }

                reader.Close();


            }
            catch (Exception ex)
            {
                if (reader != null) reader.Close();

                if (sqlCmd != null)
                    throw new AccessDbException(sqlCmd.CommandText, $"Error while retrieving foster families. Error : {ex.Message}. Exception : {ex}");
                else
                    throw new AccessDbException("GetFosterFamilies : sqlCmd is null", $"Error while retrieving foster families. Error : {ex.Message}. Exception : {ex}");

            }

            return result!;
        }

        public bool UpdateFosterFamily(FosterFamily fosterFamily, NpgsqlTransaction transaction)
        {
            bool result = false;
            NpgsqlCommand? sqlCmd = null;

            try
            {
                sqlCmd = new NpgsqlCommand(
                    """
                    UPDATE public."FosterFamilies"
                    SET "DateStart" = :dateStart,
                        "DateEnd" = :dateEnd
                    WHERE "AnimalId" = :animalId AND "ContactId" = :contactId
                    """,
                    this.SqlConn,
                    transaction
                );

                sqlCmd.Parameters.Add(new NpgsqlParameter("animalId", NpgsqlTypes.NpgsqlDbType.Varchar));
                sqlCmd.Parameters.Add(new NpgsqlParameter("contactId", NpgsqlTypes.NpgsqlDbType.Uuid));
                sqlCmd.Parameters.Add(new NpgsqlParameter("dateStart", NpgsqlTypes.NpgsqlDbType.Date));
                sqlCmd.Parameters.Add(new NpgsqlParameter("dateEnd", NpgsqlTypes.NpgsqlDbType.Date));

                sqlCmd.Prepare();

                sqlCmd.Parameters["animalId"].Value = fosterFamily.AnimalId;
                sqlCmd.Parameters["contactId"].Value = fosterFamily.ContactId;
                sqlCmd.Parameters["dateStart"].Value = fosterFamily.DateStart;
                sqlCmd.Parameters["dateEnd"].Value = fosterFamily.DateEnd != null ? fosterFamily.DateEnd : DBNull.Value;

                int nbRowAffected = sqlCmd.ExecuteNonQuery();

                if (nbRowAffected == 0) throw new Exception("Unable to update a foster family.");


                result = true;
            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached)
                    Debug.WriteLine($"Error while retrieving foster families. Error : {ex.Message}. Exception : {ex}");

                if (sqlCmd != null)
                    throw new AccessDbException(sqlCmd.CommandText, $"Error while retrieving foster families. Error : {ex.Message}. Exception : {ex}");
                else
                    throw new AccessDbException("GetFosterFamilies : sqlCmd is null", $"Error while retrieving foster families. Error : {ex.Message}. Exception : {ex}");
            }

            return result;

        }

        public HashSet<Adoption> GetAdoptions()
        {
            HashSet<Adoption> result = new HashSet<Adoption>();
            NpgsqlCommand? sqlCmd = null;
            NpgsqlDataReader? reader = null;

            try
            {
                sqlCmd = new NpgsqlCommand(
                    """
                    SELECT ado."Id" AS "Id",
                           ado."Status" AS "Status",
                           ado."DateCreated" AS "DateCreated",
                           ado."DateStart" AS "DateStart",
                           ado."DateEnd" AS "DateEnd",
                           ado."AnimalId" AS "AnimalId",
                           ado."ContactId" AS "ContactId",
                           a."Name" AS "Name",
                           a."Type" AS "Type",
                           a."Gender" AS "Gender",
                           a."BirthDate" AS "BirthDate",
                           a."DeathDate" AS "DeathDate",
                           a."IsSterilized" AS "IsSterilized",
                           a."DateSterilization" AS "DateSterilization",
                           a."Particularity" AS "Particularity",
                           a."Description" AS "Description",
                           c."Firstname" AS "Firstname",
                           c."Lastname" AS "Lastname",
                           c."RegistryNumber" AS "RegistryNumber",
                           c."Email" AS "Email",
                           c."PhoneNumber" AS "PhoneNumber",
                           c."MobileNumber" AS "MobileNumber",
                           c."AddressId" AS "AddressId",
                           add."Street" AS "Street",
                           add."City" AS "City",
                           add."State" AS "State",
                           add."ZipCode" AS "ZipCode",
                           add."Country" AS "Country"
                    FROM public."Adoptions" ado
                    INNER JOIN public."Animals" a
                        ON ado."AnimalId" = a."Id"
                    INNER JOIN public."Contacts" c
                        ON ado."ContactId" = c."Id"
                    INNER JOIN public."Addresses" add
                        ON c."AddressId" = add."Id"
                    """,
                    this.SqlConn
                );


                sqlCmd.Prepare();

                reader = sqlCmd.ExecuteReader();

                while (reader.Read())
                {
                    // Dates for foster family instance
                    DateOnly dateStart = (DateOnly)reader["DateStart"];
                    DateOnly? dateEnd = reader["DateEnd"] != DBNull.Value ? (DateOnly)reader["DateEnd"] : null;

                    // Dates for animal's instance
                    DateOnly? birthDate = reader["BirthDate"] != DBNull.Value ? (DateOnly)reader["BirthDate"] : null;
                    DateOnly? deathDate = reader["DeathDate"] != DBNull.Value ? (DateOnly)reader["DeathDate"] : null;
                    DateOnly? dateSterilization = reader["dateSterilization"] != DBNull.Value ? (DateOnly)reader["dateSterilization"] : null;

                    //
                    Animal animalInDb = new Animal(
                        Convert.ToString(reader["AnimalId"])!,
                        Convert.ToString(reader["Name"])!,
                        MyEnumHelper.GetEnumFromDescription<AnimalType>(Convert.ToString(reader["Type"])!),
                        MyEnumHelper.GetEnumFromDescription<GenderType>(Convert.ToString(reader["Gender"])!),
                        birthDate,
                        deathDate,
                        Convert.ToBoolean(reader["IsSterilized"])!,
                        dateSterilization,
                        Convert.ToString(reader["Particularity"])!,
                        Convert.ToString(reader["Description"])!

                    );

                    Address address = new Address(
                        new Guid(Convert.ToString(reader["AddressId"])!),
                        Convert.ToString(reader["Street"])!,
                        Convert.ToString(reader["City"])!,
                        Convert.ToString(reader["State"])!,
                        Convert.ToString(reader["ZipCode"])!,
                        Convert.ToString(reader["Country"])!
                    );

                    Contact contactInDb = new Contact(
                        new Guid(Convert.ToString(reader["ContactId"])!),
                        Convert.ToString(reader["Firstname"])!,
                        Convert.ToString(reader["Lastname"])!,
                        Convert.ToString(reader["RegistryNumber"])!,
                        Convert.ToString(reader["Email"])!,
                        Convert.ToString(reader["PhoneNumber"])!,
                        Convert.ToString(reader["MobileNumber"])!,
                        address
                    );



                    result.Add( new Adoption(
                        new Guid(Convert.ToString(reader["Id"])!),
                        MyEnumHelper.GetEnumFromDescription<ApplicationStatus>(Convert.ToString(reader["Status"])!),
                        Convert.ToDateTime(reader["DateCreated"])!,
                        dateStart,
                        dateEnd,
                        contactInDb,
                        animalInDb
                    ));

                }

                reader.Close();
            }
            catch (Exception ex)
            {
                if (reader != null) reader.Close();

                if (Debugger.IsAttached)
                    Debug.WriteLine($"Error while retrieving an adoption. Error : {ex.Message}. Exception : {ex}");

                if (sqlCmd != null)
                    throw new AccessDbException(sqlCmd.CommandText, $"Error while retrieving an adoption in DB. Error : {ex.Message}. Exception : {ex}.");
                else
                    throw new AccessDbException("GetAdoption : sqlCmd is null", $"Error while retrieving an adoption in DB. Error : {ex.Message}. Exception : {ex}.");
            }

            return result;
        }

        public Adoption GetAdoption(Contact contact, Animal animal)
        {
            Adoption? result = null;
            NpgsqlCommand? sqlCmd = null;
            NpgsqlDataReader? reader = null;

            try
            {
                sqlCmd = new NpgsqlCommand(
                    """
                    SELECT ado."Id" AS "Id",
                           ado."Status" AS "Status",
                           ado."DateCreated" AS "DateCreated",
                           ado."DateStart" AS "DateStart",
                           ado."DateEnd" AS "DateEnd",
                           ado."AnimalId" AS "AnimalId",
                           ado."ContactId" AS "ContactId",
                           a."Name" AS "Name",
                           a."Type" AS "Type",
                           a."Gender" AS "Gender",
                           a."BirthDate" AS "BirthDate",
                           a."DeathDate" AS "DeathDate",
                           a."IsSterilized" AS "IsSterilized",
                           a."DateSterilization" AS "DateSterilization",
                           a."Particularity" AS "Particularity",
                           a."Description" AS "Description",
                           c."Firstname" AS "Firstname",
                           c."Lastname" AS "Lastname",
                           c."RegistryNumber" AS "RegistryNumber",
                           c."Email" AS "Email",
                           c."PhoneNumber" AS "PhoneNumber",
                           c."MobileNumber" AS "MobileNumber",
                           c."AddressId" AS "AddressId",
                           add."Street" AS "Street",
                           add."City" AS "City",
                           add."State" AS "State",
                           add."ZipCode" AS "ZipCode",
                           add."Country" AS "Country"
                    FROM public."Adoptions" ado
                    INNER JOIN public."Animals" a
                        ON ado."AnimalId" = a."Id"
                    INNER JOIN public."Contacts" c
                        ON ado."ContactId" = c."Id"
                    INNER JOIN public."Addresses" add
                        ON c."AddressId" = add."Id"
                    WHERE ado."AnimalId" = :animalId
                    ORDER BY ado."DateCreated" DESC
                    """,
                    this.SqlConn
                );

                sqlCmd.Parameters.Add(new NpgsqlParameter("animalId", NpgsqlTypes.NpgsqlDbType.Varchar));

                sqlCmd.Prepare();

                sqlCmd.Parameters["animalId"].Value = animal.Id;

                reader = sqlCmd.ExecuteReader();

                if (reader.Read())
                {
                    // Dates for foster family instance
                    DateOnly dateStart = (DateOnly)reader["DateStart"];
                    DateOnly? dateEnd = reader["DateEnd"] != DBNull.Value ? (DateOnly)reader["DateEnd"] : null;

                    // Dates for animal's instance
                    DateOnly? birthDate = reader["BirthDate"] != DBNull.Value ? (DateOnly)reader["BirthDate"] : null;
                    DateOnly? deathDate = reader["DeathDate"] != DBNull.Value ? (DateOnly)reader["DeathDate"] : null;
                    DateOnly? dateSterilization = reader["dateSterilization"] != DBNull.Value ? (DateOnly)reader["dateSterilization"] : null;

                    //
                    Animal animalInDb = new Animal(
                        Convert.ToString(reader["AnimalId"])!,
                        Convert.ToString(reader["Name"])!,
                        MyEnumHelper.GetEnumFromDescription<AnimalType>(Convert.ToString(reader["Type"])!),
                        MyEnumHelper.GetEnumFromDescription<GenderType>(Convert.ToString(reader["Gender"])!),
                        birthDate,
                        deathDate,
                        Convert.ToBoolean(reader["IsSterilized"])!,
                        dateSterilization,
                        Convert.ToString(reader["Particularity"])!,
                        Convert.ToString(reader["Description"])!

                    );

                    Address address = new Address(
                        new Guid(Convert.ToString(reader["AddressId"])!),
                        Convert.ToString(reader["Street"])!,
                        Convert.ToString(reader["City"])!,
                        Convert.ToString(reader["State"])!,
                        Convert.ToString(reader["ZipCode"])!,
                        Convert.ToString(reader["Country"])!
                    );

                    Contact contactInDb = new Contact(
                        new Guid(Convert.ToString(reader["ContactId"])!),
                        Convert.ToString(reader["Firstname"])!,
                        Convert.ToString(reader["Lastname"])!,
                        Convert.ToString(reader["RegistryNumber"])!,
                        Convert.ToString(reader["Email"])!,
                        Convert.ToString(reader["PhoneNumber"])!,
                        Convert.ToString(reader["MobileNumber"])!,
                        address
                    );



                    result = new Adoption(
                        new Guid(Convert.ToString(reader["Id"])!),
                        MyEnumHelper.GetEnumFromDescription<ApplicationStatus>( Convert.ToString(reader["Status"])!),
                        Convert.ToDateTime(reader["DateCreated"])!,
                        dateStart,
                        dateEnd,
                        contactInDb,
                        animalInDb
                    );

                }

                reader.Close();
            }
            catch (Exception ex)
            {
                if (reader != null) reader.Close();

                if (Debugger.IsAttached)
                    Debug.WriteLine($"Error while retrieving an adoption. Error : {ex.Message}. Exception : {ex}");

                if (sqlCmd != null)
                    throw new AccessDbException(sqlCmd.CommandText, $"Error while retrieving an adoption in DB. Error : {ex.Message}. Exception : {ex}.");
                else
                    throw new AccessDbException("GetAdoption : sqlCmd is null", $"Error while retrieving an adoption in DB. Error : {ex.Message}. Exception : {ex}.");
            }

            if (result == null) throw new Exception("Unknown error while retrieving an adoption.");

            return result;
        }

        public bool CreateAdoption(Adoption adoption)
        {
            bool result = false;
            NpgsqlCommand? sqlCmd = null;

            try
            {
                sqlCmd = new NpgsqlCommand(
                     """
                    INSERT INTO public."Adoptions" ("Id", "Status", "DateCreated", "DateStart", "DateEnd", "AnimalId", "ContactId")
                    VALUES (:id, :status, :dateCreated, :dateStart, :dateEnd, :animalId, :contactId)
                    """,
                    this.SqlConn
                );

                sqlCmd.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Uuid));
                sqlCmd.Parameters.Add(new NpgsqlParameter("status", NpgsqlTypes.NpgsqlDbType.Text));
                sqlCmd.Parameters.Add(new NpgsqlParameter("dateCreated", NpgsqlTypes.NpgsqlDbType.TimestampTz));
                sqlCmd.Parameters.Add(new NpgsqlParameter("dateStart", NpgsqlTypes.NpgsqlDbType.Date));
                sqlCmd.Parameters.Add(new NpgsqlParameter("dateEnd", NpgsqlTypes.NpgsqlDbType.Date));
                sqlCmd.Parameters.Add(new NpgsqlParameter("animalId", NpgsqlTypes.NpgsqlDbType.Varchar));
                sqlCmd.Parameters.Add(new NpgsqlParameter("contactId", NpgsqlTypes.NpgsqlDbType.Uuid));

                sqlCmd.Prepare();

                sqlCmd.Parameters["id"].Value = adoption.Id;
                sqlCmd.Parameters["status"].Value = adoption.Status;
                sqlCmd.Parameters["dateCreated"].Value = adoption.DateCreated.ToUniversalTime();
                sqlCmd.Parameters["dateStart"].Value = adoption.DateStart;
                sqlCmd.Parameters["dateEnd"].Value = adoption.DateEnd == null ? DBNull.Value : adoption.DateEnd;
                sqlCmd.Parameters["animalId"].Value = adoption.AnimalId;
                sqlCmd.Parameters["contactId"].Value = adoption.ContactId;


                int nbRowAffected = sqlCmd.ExecuteNonQuery();

                if (nbRowAffected == 0)
                    throw new Exception($"Unable to create an adoption in DB.");

                result = true;
            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached)
                    Debug.WriteLine($"Error while creating an adoption in DB. Error : {ex.Message}. Exception : {ex}.");

                if (sqlCmd != null)
                    throw new AccessDbException(sqlCmd.CommandText, $"Error while creating an adoption in DB. Error : {ex.Message}. Exception : {ex}.");
                else
                    throw new AccessDbException("CreateAdoption : sqlCmd is null", $"Error while creating an adoption in DB. Error : {ex.Message}. Exception : {ex}.");

            }


            return result;
        }

        /**
         * 
         */ 
        public bool CreateReleaseForAdoption(Adoption adoption, Release release)
        {
            bool result = false;
            NpgsqlTransaction transaction = this.SqlConn.BeginTransaction();

            try
            {
                bool releaseSaved = this.CreateRelease(release, transaction);

                bool adoptionSaved = this.UpdateAdoption(adoption, transaction);

                // Commit transaction to DB
                if (releaseSaved && adoptionSaved)
                    transaction.Commit();


            }
            catch (Exception ex)
            {
                if (transaction != null && transaction.Connection != null) transaction.Rollback();

                if (Debugger.IsAttached)
                    Debug.WriteLine($"Error while creating an adoption. Error : {ex.Message}. Exception : {ex}");


                throw;
            }

            return result;
        }

        public bool UpdateAdoption(Adoption adoption, NpgsqlTransaction? transaction = null)
        {
            bool result = false;
            NpgsqlCommand? sqlCmd = null;

            try
            {
                sqlCmd = new NpgsqlCommand(
                    """
                    UPDATE public."Adoptions"
                    SET "Status" = :status,
                        "DateStart" = :dateStart,
                        "DateEnd" = :dateEnd
                    WHERE "Id" = :id
                    """,
                    this.SqlConn
                );

                sqlCmd.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Uuid));
                sqlCmd.Parameters.Add(new NpgsqlParameter("status", NpgsqlTypes.NpgsqlDbType.Text));
                sqlCmd.Parameters.Add(new NpgsqlParameter("dateStart", NpgsqlTypes.NpgsqlDbType.Date));
                sqlCmd.Parameters.Add(new NpgsqlParameter("dateEnd", NpgsqlTypes.NpgsqlDbType.Date));

                sqlCmd.Prepare();

                sqlCmd.Parameters["id"].Value = adoption.Id;
                sqlCmd.Parameters["status"].Value = adoption.Status;
                sqlCmd.Parameters["dateStart"].Value = adoption.DateStart;
                sqlCmd.Parameters["dateEnd"].Value = adoption.DateEnd != null ? adoption.DateEnd : DBNull.Value;


                int nbRowAffected = sqlCmd.ExecuteNonQuery();

                if (nbRowAffected == 0) throw new Exception("Unable to update adoption's status.");

                result = true;


            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached)
                    Debug.WriteLine($"Error while updating an adoption in DB. Error : {ex.Message}. Exception : {ex}.");

                if (sqlCmd != null)
                    throw new AccessDbException(sqlCmd.CommandText, $"Error while updating an adoption in DB. Error : {ex.Message}. Exception : {ex}.");
                else
                    throw new AccessDbException("CreateVaccine : sqlCmd is null", $"Error while updating an adoption in DB. Error : {ex.Message}. Exception : {ex}.");
            }

            return result;
        }

        public Vaccine? GetVaccine(string name)
        {
            Vaccine? result = null;
            NpgsqlCommand? sqlCmd = null;
            NpgsqlDataReader? reader = null;

            try
            {
                sqlCmd = new NpgsqlCommand(
                    """
                    SELECT *
                    FROM public."Vaccines"
                    WHERE "Name" = :name
                    """,
                    this.SqlConn
                );

                sqlCmd.Parameters.Add(new NpgsqlParameter("name", NpgsqlTypes.NpgsqlDbType.Text));

                sqlCmd.Prepare();

                sqlCmd.Parameters["name"].Value = name;

                reader = sqlCmd.ExecuteReader();

                if (reader.Read())
                {
                    result = new Vaccine(new Guid(Convert.ToString(reader["Id"])!), Convert.ToString(reader["Name"])!);
                }


                reader.Close();


            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached)
                    Debug.WriteLine($"Error while updating an adoption in DB. Error : {ex.Message}. Exception : {ex}.");

                if (sqlCmd != null)
                    throw new AccessDbException(sqlCmd.CommandText, $"Error while updating an adoption in DB. Error : {ex.Message}. Exception : {ex}.");
                else
                    throw new AccessDbException("CreateVaccine : sqlCmd is null", $"Error while updating an adoption in DB. Error : {ex.Message}. Exception : {ex}.");
            }

            return result;
        }

        public Vaccine CreateVaccine(Vaccine vaccine)
        {
            Vaccine? result = null;
            NpgsqlCommand? sqlCmd = null;

            try
            {
                sqlCmd = new NpgsqlCommand(
                    """
                    INSERT INTO public."Vaccines" ("Id", "Name")
                    VALUES (:id, :name)
                    """,
                    this.SqlConn
                );

                sqlCmd.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Uuid));
                sqlCmd.Parameters.Add(new NpgsqlParameter("name", NpgsqlTypes.NpgsqlDbType.Text));


                sqlCmd.Prepare();

                sqlCmd.Parameters["id"].Value = vaccine.Id;
                sqlCmd.Parameters["name"].Value = vaccine.Name;


                int nbRowAffected = sqlCmd.ExecuteNonQuery();

                if (nbRowAffected == 0)
                    throw new Exception($"");

                result = this.GetVaccine(vaccine.Name);
            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached)
                    Debug.WriteLine($"Error while creating a vaccine in DB. Error : {ex.Message}. Exception : {ex}.");

                if (sqlCmd != null)
                    throw new AccessDbException(sqlCmd.CommandText, $"Error while creating a vaccine in DB. Error : {ex.Message}. Exception : {ex}.");
                else
                    throw new AccessDbException("CreateVaccine : sqlCmd is null", $"Error while creating a vaccine in DB. Error : {ex.Message}. Exception : {ex}.");

            }

            if (result == null)
                throw new Exception("Unable to create a vaccine.");


            return result;
        }

        public bool CreateVaccination(Vaccination vaccination)
        {
            bool result = false;
            NpgsqlCommand? sqlCmd = null;

            try
            {
                sqlCmd = new NpgsqlCommand(
                    """
                    INSERT INTO public."Vaccinations" ("Id", "DateCreated", "DateVaccination", "Done", "AnimalId", "VaccineId")
                    VALUES (:id, :dateCreated, :dateVaccination, :done, :animalId, :vaccineId)
                    """,
                    this.SqlConn
                );

                sqlCmd.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Uuid));
                sqlCmd.Parameters.Add(new NpgsqlParameter("dateCreated", NpgsqlTypes.NpgsqlDbType.TimestampTz));
                sqlCmd.Parameters.Add(new NpgsqlParameter("dateVaccination", NpgsqlTypes.NpgsqlDbType.Date));
                sqlCmd.Parameters.Add(new NpgsqlParameter("done", NpgsqlTypes.NpgsqlDbType.Boolean));
                sqlCmd.Parameters.Add(new NpgsqlParameter("animalId", NpgsqlTypes.NpgsqlDbType.Varchar));
                sqlCmd.Parameters.Add(new NpgsqlParameter("vaccineId", NpgsqlTypes.NpgsqlDbType.Uuid));


                sqlCmd.Prepare();

                sqlCmd.Parameters["id"].Value = vaccination.Id;
                sqlCmd.Parameters["dateCreated"].Value = vaccination.DateCreated.ToUniversalTime();
                sqlCmd.Parameters["dateVaccination"].Value = vaccination.DateVaccination;
                sqlCmd.Parameters["done"].Value = vaccination.Done;
                sqlCmd.Parameters["animalId"].Value = vaccination.AnimalId;
                sqlCmd.Parameters["vaccineId"].Value = vaccination.VaccineId;

                int nbRowAffected = sqlCmd.ExecuteNonQuery();

                if (nbRowAffected == 0)
                    throw new Exception($"Unable to create a vaccination.");

                result = true;
            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached)
                    Debug.WriteLine($"Error while creating a release in DB. Error : {ex.Message}. Exception : {ex}.");

                if (sqlCmd != null)
                    throw new AccessDbException(sqlCmd.CommandText, $"Error while creating a release in DB. Error : {ex.Message}. Exception : {ex}.");
                else
                    throw new AccessDbException("CreateRelease : sqlCmd is null", $"Error while creating a release in DB. Error : {ex.Message}. Exception : {ex}.");

            }


            return result;
        }
    }
}

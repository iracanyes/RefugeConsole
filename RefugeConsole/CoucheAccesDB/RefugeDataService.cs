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
        /**
         * <summary>
         *  Gère l'ajout d'une admission pour un animal
         * </summary>
         */ 
        public bool HandleCreateAdmission(Admission admission)
        {
            bool result = false;
            NpgsqlTransaction transaction = this.SqlConn.BeginTransaction();

            try
            {
                // Si c'est un retour adoption, enregistrement de la date de fin de l'adoption

                if(admission.Reason == MyEnumHelper.GetEnumDescription<AdmissionType>(AdmissionType.ReturnAdoption))
                {
                    Adoption adoption = this.GetAdoption(admission.Contact, admission.Animal);
                    adoption.DateEnd = DateOnly.FromDateTime(DateTime.Now);

                    result = this.UpdateAdoption(adoption, transaction);
                }

                // Si c'est un retour de famille d'accueil, enregistrer la date de fin de l'accueil
                if(admission.Reason == MyEnumHelper.GetEnumDescription<AdmissionType>(AdmissionType.ReturnFosterFamily))
                {
                    FosterFamily ff = this.GetFosterFamily(admission.Contact, admission.Animal);

                    if (ff == null)
                        throw new Exception("Foster family not found!");

                    ff.DateEnd = DateOnly.FromDateTime(DateTime.Now);

                    result = this.UpdateFosterFamily(ff, transaction);
                }

                // Sauvegarder la nouvelle admission 
                result = this.CreateAdmission(admission, transaction);

                // Clôture de la transaction avec la base de donnée
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
                // Sauvegarde de la sortie
                bool releaseSaved = this.CreateRelease(release, transaction);

                // Sauvegarde de la famille d'accueil
                bool fosterFamilySaved = this.CreateFosterFamily(fosterFamily, transaction);

                // Valider la transaction en DB
                if(releaseSaved && fosterFamilySaved)
                {
                    transaction.Commit();
                    result = true;
                }
                    


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

        public bool CreateRelease(Release release, NpgsqlTransaction? transaction = null)
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
                // Sauvegarde de la sortie
                bool releaseSaved = this.CreateRelease(release, transaction);

                // Sauvegarde de l'adoption
                bool adoptionSaved = this.UpdateAdoption(adoption, transaction);

                // Commit transaction to DB
                if (releaseSaved && adoptionSaved)
                {
                    transaction.Commit();
                    result = true;
                }
                    


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

        /**
         * Créer une vaccination 
         */ 
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
                sqlCmd.Parameters["dateVaccination"].Value = vaccination.DateVaccination != null ? vaccination.DateVaccination : DBNull.Value ;
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

        /**
         * Vérifie la contrainte  : Date_entree: Un animal ne peut être entrée plus d’une fois depuis une sortie
         */
        public bool IsAdmittedSinceLastRelease(string animalId) {
            bool result = false;
            DateTime? dateLastRelease = null;

            try
            {
                // Get Date of last release if exists
                dateLastRelease = this.GetDateLastRelease(animalId, DateTime.Now);

                if (dateLastRelease == null)
                    return false;

                // admission exists since date of last release
                result = this.AdmissionExistsSinceLastRelease(animalId, (DateTime) dateLastRelease);


            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());

                if (Debugger.IsAttached)
                    Debug.WriteLine($"Error while checking if an animal's admission already exists since last release in DB. Error : {ex.Message}. Exception : {ex}.");

            }


            return result;
            
        }

        /**
         * Récupère la date de la dernière sortie de l'animal si elle existe 
         * 
         */ 
        private DateTime? GetDateLastRelease(string animalId, DateTime dateAdmission, ReleaseType? reason = null)
        {
            DateTime? result = null;
            NpgsqlCommand? sqlCmd = null;
            NpgsqlDataReader? reader = null;
            StringBuilder sb = new StringBuilder();

            sb.Append("SELECT \"DateCreated\" ")
                .Append("FROM public.\"Releases\" ")
                .Append("WHERE \"AnimalId\" = :animalId AND \"DateCreated\" < :dateAdmission ");

            if (reason != null)
                sb.Append("AND \"Reason\" = :reason ");

            sb.Append("ORDER BY \"DateCreated\" DESC ")
                .Append("LIMIT 1");


            try
            {
                
                sqlCmd = new NpgsqlCommand(
                    sb.ToString(),
                    this.SqlConn
                );

                sqlCmd.Parameters.Add(new NpgsqlParameter("animalId", NpgsqlTypes.NpgsqlDbType.Varchar));
                sqlCmd.Parameters.Add(new NpgsqlParameter("dateAdmission", NpgsqlTypes.NpgsqlDbType.TimestampTz));

                if (reason != null)
                    sqlCmd.Parameters.Add(new NpgsqlParameter("reason", NpgsqlTypes.NpgsqlDbType.Text));

                sqlCmd.Prepare();

                sqlCmd.Parameters["animalId"].Value = animalId;
                sqlCmd.Parameters["dateAdmission"].Value = dateAdmission.ToUniversalTime();

                if (reason != null)
                    sqlCmd.Parameters["reason"].Value = MyEnumHelper.GetEnumDescription<ReleaseType>( (ReleaseType) reason);

                reader = sqlCmd.ExecuteReader();

                if (reader.Read())
                {
                    result = Convert.ToDateTime(reader["DateCreated"])!;
                }

                reader.Close();



            }
            catch (Exception ex)
            {
                if (reader != null) { reader.Close(); }

                if (Debugger.IsAttached)
                    Debug.WriteLine($"Error while checking if an animal's admission already exists since last release in DB. Error : {ex.Message}. Exception : {ex}.");

                if (sqlCmd != null)
                    throw new AccessDbException(sqlCmd.CommandText, $"Error while checking if an animal's admission already exists since last release in DB. Error : {ex.Message}. Exception : {ex}.");
                else
                    throw new AccessDbException("CreateRelease : sqlCmd is null", $"Error while checking if an animal's admission already exists since last release in DB. Error : {ex.Message}. Exception : {ex}.");
            }


            return result;
        }

        /**
         * Check if a admission exists for the animal since last release date
         */ 
        private bool AdmissionExistsSinceLastRelease(string animalId, DateTime dateLastRelease)
        {
            bool result = false;
            NpgsqlCommand? sqlCmd1 = null;
            NpgsqlDataReader? reader1 = null;


            try
            {
                // Get Date of last release if exists
                sqlCmd1 = new NpgsqlCommand(
                    """
                    SELECT EXISTS(
                        SELECT 1 
                        FROM public."Admissions"
                        WHERE "AnimalId" = :animalId AND "DateCreated" > :dateLastRelease
                        LIMIT 1
                    )
                    """,
                    this.SqlConn
                );

                sqlCmd1.Parameters.Add(new NpgsqlParameter("animalId", NpgsqlTypes.NpgsqlDbType.Varchar));
                sqlCmd1.Parameters.Add(new NpgsqlParameter("dateLastRelease", NpgsqlTypes.NpgsqlDbType.TimestampTz));

                sqlCmd1.Prepare();

                sqlCmd1.Parameters["animalId"].Value = animalId;
                sqlCmd1.Parameters["dateLastRelease"].Value = dateLastRelease.ToUniversalTime();

                reader1 = sqlCmd1.ExecuteReader();

                if (reader1.Read())
                {
                    result = Convert.ToBoolean(reader1["exists"])!;
                }

                reader1.Close();


            }
            catch (Exception ex)
            {
                if (reader1 != null) { reader1.Close(); }

                Console.Error.WriteLine(ex.Message);

                if (Debugger.IsAttached)
                    Debug.WriteLine($"Error while checking if an animal's admission already exists since last release in DB. Error : {ex.Message}. Exception : {ex}.");

                if (sqlCmd1 != null)
                    throw new AccessDbException(sqlCmd1.CommandText, $"Error while checking if an animal's admission already exists since last release in DB. Error : {ex.Message}. Exception : {ex}.");
                else
                    throw new AccessDbException("AdmissionExistsSinceLastRelease : sqlCmd is null", $"Error while checking if an animal's admission already exists since last release in DB. Error : {ex.Message}. Exception : {ex}.");
            }


            return result;
        }

        /**
         * Check constraint : Release exists since last admission's date
         * 
         * 
         */
        public bool IsReleasedSinceLastAdmission(string animalId)
        {
            bool result = false;
            DateTime? dateLastAdmission = null;

            try
            {
                // Retrieve date of last admission 
                dateLastAdmission = this.GetDateLastAdmission(animalId, DateTime.Now);

                if (dateLastAdmission == null)
                    throw new Exception($"No admission exists for the animal!");

                // Check if a release exists since last admission
                result = this.ReleaseExistsSinceLastAdmission(animalId, (DateTime) dateLastAdmission);


            }
            catch (Exception ex)
            {

                Console.Error.WriteLine(ex.Message);

                if (Debugger.IsAttached)
                    Debug.WriteLine($"Error while checking if an animal's admission already exists since last release in DB. Error : {ex.Message}. Exception : {ex}.");
            }

            return result;

        }

        /**
         * Retrieve the last admission's date since a datetime provided for an animal identified by its ID 
         */ 
        private DateTime? GetDateLastAdmission(string animalId, DateTime dateAdmission)
        {
            DateTime? result = null;
            NpgsqlCommand? sqlCmd = null;
            NpgsqlDataReader? reader = null;

            try
            {
                sqlCmd = new NpgsqlCommand(
                    """
                    SELECT "DateCreated" 
                    FROM public."Admissions"
                    WHERE "AnimalId" = :animalId AND "DateCreated" < :dateRelease
                    ORDER BY "DateCreated" DESC
                    LIMIT 1
                    """,
                    this.SqlConn
                );

                sqlCmd.Parameters.Add(new NpgsqlParameter("animalId", NpgsqlTypes.NpgsqlDbType.Varchar));
                sqlCmd.Parameters.Add(new NpgsqlParameter("dateRelease", NpgsqlTypes.NpgsqlDbType.TimestampTz));


                sqlCmd.Prepare();

                sqlCmd.Parameters["animalId"].Value = animalId;
                sqlCmd.Parameters["dateRelease"].Value = dateAdmission.ToUniversalTime();


                reader = sqlCmd.ExecuteReader();

                if (reader.Read())
                {
                    result = Convert.ToDateTime(reader["DateCreated"]);
                }

                reader.Close();


            }
            catch (Exception ex)
            {
                if (reader != null) { reader.Close(); }

                Console.Error.WriteLine(ex.Message);

                if (Debugger.IsAttached)
                    Debug.WriteLine($"Error while retrieving last admission's date for an animal in DB. Error : {ex.Message}. Exception : {ex}.");

                if (sqlCmd != null)
                    throw new AccessDbException(sqlCmd.CommandText, $"Error while retrieving last admission's date for an animal in DB. Error : {ex.Message}. Exception : {ex}.");
                else
                    throw new AccessDbException("AdmissionExistsSinceLastRelease : sqlCmd is null", $"Error while retrieving last admission's date for an animal in DB. Error : {ex.Message}. Exception : {ex}.");
            }

            return result;
        }

        /**
         * Check if a release exists since last admission's date
         */ 
        private bool ReleaseExistsSinceLastAdmission(string animalId, DateTime dateLastAdmission)
        {
            bool result = false;
            NpgsqlCommand? sqlCmd = null;
            NpgsqlDataReader? reader = null;

            try
            {
                sqlCmd = new NpgsqlCommand(
                    """
                    SELECT EXISTS(
                        SELECT 1
                        FROM public."Releases"
                        WHERE "AnimalId" = :animalId AND "DateCreated" > :dateLastAdmission
                    )
                    """,
                    this.SqlConn
                );

                sqlCmd.Parameters.Add(new NpgsqlParameter("animalId", NpgsqlTypes.NpgsqlDbType.Varchar));
                sqlCmd.Parameters.Add(new NpgsqlParameter("dateLastAdmission", NpgsqlTypes.NpgsqlDbType.TimestampTz));

                sqlCmd.Prepare();

                sqlCmd.Parameters["animalId"].Value = animalId;
                sqlCmd.Parameters["dateLastAdmission"].Value = dateLastAdmission;

                reader = sqlCmd.ExecuteReader();

                if (reader.Read())
                {
                    result = Convert.ToBoolean(reader["exists"]);
                }

                reader.Close();



            }
            catch (Exception ex)
            {
                if (reader != null) { reader.Close(); }

                Console.Error.WriteLine(ex.Message);

                if (Debugger.IsAttached)
                    Debug.WriteLine($"Error while checking if an animal's release already exists since last admission in DB. Error : {ex.Message}. Exception : {ex}.");

                if (sqlCmd != null)
                    throw new AccessDbException(sqlCmd.CommandText, $"Error while checking if an animal's release already exists since last admission in DB. Error : {ex.Message}. Exception : {ex}.");
                else
                    throw new AccessDbException("AdmissionExistsSinceLastRelease : sqlCmd is null", $"Error while checking if an animal's release already exists since last admission in DB. Error : {ex.Message}. Exception : {ex}.");
            }

            return result;

        }


        /**
         * Check if an animal is already in a foster family 
         */ 
        public bool IsInFosterFamily(string animalId)
        {
            bool result = false;
            NpgsqlCommand? sqlCmd = null;
            NpgsqlDataReader? reader = null;

            try
            {
                sqlCmd = new NpgsqlCommand(
                    """
                    SELECT EXISTS(
                        SELECT 1
                        FROM public."FosterFamilies"
                        WHERE "AnimalId" = :animalId AND "DateEnd" IS NULL
                        LIMIT 1
                    )
                    """,
                    this.SqlConn
                );

                sqlCmd.Parameters.Add(new NpgsqlParameter("animalId", NpgsqlTypes.NpgsqlDbType.Varchar));
                
                sqlCmd.Prepare();

                sqlCmd.Parameters["animalId"].Value = animalId; 

                reader = sqlCmd.ExecuteReader();

                if (reader.Read())
                {
                    result = Convert.ToBoolean(reader["exists"]);
                }

                reader.Close();  



            }
            catch (Exception ex)
            {
                if (reader != null) { reader.Close(); }

                Console.Error.WriteLine(ex.Message);

                if (Debugger.IsAttached)
                    Debug.WriteLine($"Error while checking if an animal is already in a foster family in DB. Error : {ex.Message}. Exception : {ex}.");

                if (sqlCmd != null)
                    throw new AccessDbException(sqlCmd.CommandText, $"Error while checking if an animal is already in a foster family in DB. Error : {ex.Message}. Exception : {ex}.");
                else
                    throw new AccessDbException("AdmissionExistsSinceLastRelease : sqlCmd is null", $"Error while checking if an animal is already in a foster family in DB. Error : {ex.Message}. Exception : {ex}.");
            }

            return result;
        }

    }
}

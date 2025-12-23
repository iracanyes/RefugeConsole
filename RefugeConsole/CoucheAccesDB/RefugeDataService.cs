using Npgsql;
using RefugeConsole.ClassesMetiers.Exceptions;
using RefugeConsole.ClassesMetiers.Helper;
using RefugeConsole.ClassesMetiers.Model.Entities;
using RefugeConsole.ClassesMetiers.Model.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace RefugeConsole.CoucheAccesDB
{
    internal class RefugeDataService: AccessDb, IRefugeDataService
    {
        public bool CreateAdmission(Admission admission)
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
                    this.SqlConn
                );

                sqlCmd.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Uuid);
                sqlCmd.Parameters.Add("reason", NpgsqlTypes.NpgsqlDbType.Text);
                sqlCmd.Parameters.Add("dateCreated", NpgsqlTypes.NpgsqlDbType.TimestampTz);
                sqlCmd.Parameters.Add("contactId", NpgsqlTypes.NpgsqlDbType.Uuid);
                sqlCmd.Parameters.Add("animalId", NpgsqlTypes.NpgsqlDbType.Varchar);

                sqlCmd.Prepare();

                sqlCmd.Parameters["id"].Value = admission.Id;
                sqlCmd.Parameters["reason"].Value = admission.Reason;
                sqlCmd.Parameters["dateCreated"].Value = admission.DateCreated;
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
            throw new NotImplementedException();
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

        public HashSet<Adoption> GetAdoptions()
        {
            throw new NotImplementedException();
        }

        public bool CreateAdoption(Adoption adoption, NpgsqlTransaction transaction)
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
                    this.SqlConn,
                    transaction
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
                sqlCmd.Parameters["dateCreated"].Value = adoption.DateCreated;
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

        public bool UpdateAdoption(Adoption adoption)
        {
            throw new NotImplementedException();
        }

        public bool CreateVaccination(Vaccination vaccination)
        {
            bool result = false;
            NpgsqlCommand? sqlCmd = null;

            try
            {
                sqlCmd = new NpgsqlCommand(
                    """

                    """,
                    this.SqlConn
                );

                sqlCmd.Prepare();

                int nbRowAffected = sqlCmd.ExecuteNonQuery();

                if (nbRowAffected == 0)
                    throw new Exception($"");

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

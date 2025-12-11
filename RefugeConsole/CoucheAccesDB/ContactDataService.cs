using Microsoft.Extensions.Logging;
using Npgsql;
using RefugeConsole.ClassesMetiers.Exceptions;
using RefugeConsole.ClassesMetiers.Model.Entities;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Text;

namespace RefugeConsole.CoucheAccesDB
{
    internal class ContactDataService : IContactDataService
    {
        private static readonly ILogger MyLogger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger(nameof(AnimalDataService));
        private readonly NpgsqlConnection SqlConn;
        public ContactDataService()
        {
            try
            {
                SqlConn = new NpgsqlConnection(Environment.GetEnvironmentVariable("REFUGE_DB_CONNECTION_STRING"));
                SqlConn.Open();

            }
            catch (Exception ex)
            {
                MyLogger.LogError("Error while connecting to database. Reason : {0}", ex.Message);
                throw new AccessDbException(ex.Message, "Error while connecting to database");
            }
        }

        public Contact CreateContact(Contact contact)
        {
            Contact? result = null;
            NpgsqlCommand? sqlCmd = null;

            try
            {
                sqlCmd = new NpgsqlCommand(
                    $"""
                    INSERT INTO public."Contacts" ("Id", "Firstname", "Lastname", "RegistryNumber", "Email", "PhoneNumber", "MobileNumber", "AddressId")
                    VALUES (:id, :firstname, :lastname, :registryNumber, :email, :phoneNumber, :mobileNumber, :addressId)
                    """,
                    this.SqlConn
                );

                sqlCmd.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Uuid));
                sqlCmd.Parameters.Add(new NpgsqlParameter("firstname", NpgsqlTypes.NpgsqlDbType.Text));
                sqlCmd.Parameters.Add(new NpgsqlParameter("lastname", NpgsqlTypes.NpgsqlDbType.Text));
                sqlCmd.Parameters.Add(new NpgsqlParameter("registryNumber", NpgsqlTypes.NpgsqlDbType.Text));
                sqlCmd.Parameters.Add(new NpgsqlParameter("email", NpgsqlTypes.NpgsqlDbType.Text));
                sqlCmd.Parameters.Add(new NpgsqlParameter("phoneNumber", NpgsqlTypes.NpgsqlDbType.Text));
                sqlCmd.Parameters.Add(new NpgsqlParameter("mobileNumber", NpgsqlTypes.NpgsqlDbType.Text));
                sqlCmd.Parameters.Add(new NpgsqlParameter("addressId", NpgsqlTypes.NpgsqlDbType.Uuid));

                sqlCmd.Prepare();

                sqlCmd.Parameters["id"].Value = contact.Id;
                sqlCmd.Parameters["firstname"].Value = contact.Firstname;
                sqlCmd.Parameters["lastname"].Value = contact.Lastname;
                sqlCmd.Parameters["registryNumber"].Value = contact.RegistryNumber;
                sqlCmd.Parameters["email"].Value = contact.Email;
                sqlCmd.Parameters["phoneNumber"].Value = contact.PhoneNumber;
                sqlCmd.Parameters["mobileNumber"].Value = contact.MobileNumber;
                sqlCmd.Parameters["addressId"].Value = contact.AddressId;

                int nbRowAffected = sqlCmd.ExecuteNonQuery();

                if(nbRowAffected == 0) throw new AccessDbException(sqlCmd.CommandText, $"Unable to create a Contact instance in DB!\nObject:\n{contact}");

                result = this.GetContactByRegistryNumber(contact.RegistryNumber);
            }
            catch (Exception ex)
            {

                Debug.WriteLine($"Unable to create a contact info instance.\nObject : \n{contact}\nReason : \n{ex.Message}\nException : \n{ex}");
            }

            if (result == null) throw new Exception($"Unable to create a contact info instance.\nObject : \n{contact}");

            return result!;

        }

        public Contact GetContactByRegistryNumber(string registryNumber)
        {
            Contact? result = null;
            NpgsqlCommand? sqlCmd = null;
            NpgsqlDataReader? reader = null;

            try
            {
                sqlCmd = new NpgsqlCommand(
                    $"""
                    SELECT c."Id" AS "Id"
                           c."Firstname" AS "Firstname",
                           c."Lastname" AS "Lastname",
                           c."RegistryNumber" AS "RegistryNumber",
                           c."Email" AS "Email",
                           c."MobileNumber" AS "MobileNumber",
                           c."PhoneNumber" AS "PhoneNumber",
                           a."Street" AS "Street",
                           a."City" AS "City",
                           a."State" AS "State" ,
                           a."ZipCode" AS "ZipCode",
                           a."Country" AS "Country", 
                    FROM public."Contacts" c
                    INNER JOIN public."Addresses" a
                        ON c."AddressId" = a."Id"
                    WHERE c."RegistryNumber" = :registryNumber
                    """,
                    this.SqlConn
                );

                sqlCmd.Parameters.Add(new NpgsqlParameter("registryNumber", NpgsqlTypes.NpgsqlDbType.Text));

                sqlCmd.Prepare();

                sqlCmd.Parameters["registryNumber"].Value = registryNumber;

                reader = sqlCmd.ExecuteReader();

                if(reader.Read())
                {
                    Address address = new Address(
                        new Guid(Convert.ToString(reader["Id"])!),
                        Convert.ToString(reader["Street"])!,
                        Convert.ToString(reader["City"])!,
                        Convert.ToString(reader["State"])!,
                        Convert.ToString(reader["ZipCode"])!,
                        Convert.ToString(reader["Country"])!
                    );

                    result = new Contact(
                        new Guid(Convert.ToString(reader["Id"])!),
                        Convert.ToString(reader["Firstname"])!,
                        Convert.ToString(reader["Lastname"])!,
                        Convert.ToString(reader["RegistryNumber"])!,                        
                        Convert.ToString(reader["Email"])!,
                        Convert.ToString(reader["PhoneNumber"])!,
                        Convert.ToString(reader["MobileNumber"])!,
                        address
                    );
                }

                reader.Close();


            }
            catch (Exception ex)
            {
                reader?.Close();

                Debug.WriteLine($"Unable to create a contact info instance.\nRegistryNumber : \n{registryNumber}\nReason : \n{ex.Message}\nException : \n{ex}");
                if(sqlCmd != null)
                    throw new AccessDbException(sqlCmd.CommandText, ex.Message);
                else 
                    throw new AccessDbException($"Unable to create a contact info instance.\nRegistryNumber : \n{registryNumber}", ex.Message);

            }

            if (result == null) throw new Exception($"Unable to retrieve a contact info instance with registry number : {registryNumber}");

            return result;
        }

        public Contact UpdateContact(Contact contact) {
            Contact? result = null;
            NpgsqlCommand? sqlCmd = null;

            try
            {
                sqlCmd = new NpgsqlCommand(
                    $"""
                    UPDATE public."Contacts" cin
                    SET "Firstname" = :firstname,
                        "Lastname" = :lastname,
                        "RegistryNumber" = :registryNumber,
                        "Address" = :address,
                        "Email" = :email,
                        "PhoneNumber" = :phoneNumber,
                        "MobileNumber" = :mobileNumber,
                        "AddressId" = :addressId
                    WHERE "Id" = :id
                    """,
                    this.SqlConn
                );

                sqlCmd.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Uuid));
                sqlCmd.Parameters.Add(new NpgsqlParameter("firstname", NpgsqlTypes.NpgsqlDbType.Text));
                sqlCmd.Parameters.Add(new NpgsqlParameter("lastname", NpgsqlTypes.NpgsqlDbType.Text));
                sqlCmd.Parameters.Add(new NpgsqlParameter("registryNumber", NpgsqlTypes.NpgsqlDbType.Text));
                sqlCmd.Parameters.Add(new NpgsqlParameter("address", NpgsqlTypes.NpgsqlDbType.Text));
                sqlCmd.Parameters.Add(new NpgsqlParameter("email", NpgsqlTypes.NpgsqlDbType.Text));
                sqlCmd.Parameters.Add(new NpgsqlParameter("phoneNumber", NpgsqlTypes.NpgsqlDbType.Text));
                sqlCmd.Parameters.Add(new NpgsqlParameter("mobileNumber", NpgsqlTypes.NpgsqlDbType.Text));
                sqlCmd.Parameters.Add(new NpgsqlParameter("addressId", NpgsqlTypes.NpgsqlDbType.Uuid));


                sqlCmd.Prepare();

                sqlCmd.Parameters["id"].Value = contact.Id;
                sqlCmd.Parameters["firstname"].Value = contact.Firstname;
                sqlCmd.Parameters["lastname"].Value = contact.Lastname;
                sqlCmd.Parameters["registryNumber"].Value = contact.RegistryNumber;
                sqlCmd.Parameters["address"].Value = contact.Address;
                sqlCmd.Parameters["email"].Value = contact.Email;
                sqlCmd.Parameters["phoneNumber"].Value = contact.PhoneNumber;
                sqlCmd.Parameters["mobileNumber"].Value = contact.MobileNumber;
                sqlCmd.Parameters["addressId"].Value = contact.AddressId;

                int nbRowAffected = sqlCmd.ExecuteNonQuery();

                if (nbRowAffected == 0) throw new AccessDbException(sqlCmd.CommandText, $"Unable to create a Contact instance in DB!\nObject:\n{contact}");

                result = this.GetContactByRegistryNumber(contact.RegistryNumber);


            }
            catch (Exception ex)
            {

                Debug.WriteLine($"Unable to retrieve a contact instance with registry number : {contact.RegistryNumber}.\nReason :\n{ex.Message}\nException : \n{ex}");

                if (sqlCmd != null)
                    throw new AccessDbException(sqlCmd.CommandText, ex.Message);
                else
                    throw new AccessDbException($"Unable to retrieve a contact instance with registry number : {contact.RegistryNumber}.", ex.Message);
            }

            if (result == null) throw new Exception($"Unable to retrieve a contact instance with registry number : {contact.RegistryNumber}");

            return result;
        }

        public bool DeleteContact(Contact contact)
        {
            bool result = false;
            NpgsqlCommand? sqlCmd = null;

            try
            {
                sqlCmd = new NpgsqlCommand(
                    $"""
                    DELETE FROM public."Contacts"
                    WHERE "Id" = :id
                    """    
                );

                sqlCmd.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Uuid));
                 
                sqlCmd.Prepare();

                sqlCmd.Parameters["id"].Value = contact.Id;

                int nbRowAffected = sqlCmd.ExecuteNonQuery();

                if (nbRowAffected == 0) throw new AccessDbException(sqlCmd.CommandText, $"Error while deleting contact info : \n{contact}");

                result = true;
            }
            catch (Exception ex)
            {

                Debug.WriteLine($"Unable to retrieve a contact instance with registry number : {contact.RegistryNumber}.\nReason :\n{ex.Message}\nException : \n{ex}");

                if (sqlCmd != null)
                    throw new AccessDbException(sqlCmd.CommandText, ex.Message);
                else
                    throw new AccessDbException($"Unable to retrieve a contact instance with registry number : {contact.RegistryNumber}.", ex.Message);
            }
            

            return result;
        }

        

        public Contact GetContactById(Guid id)
        {
            Contact? result = null;
            NpgsqlCommand? sqlCmd = null;
            NpgsqlDataReader? reader = null;

            try
            {
                sqlCmd = new NpgsqlCommand(
                    $"""
                    SELECT c."Id" AS "Id"
                           c."Firstname" AS "Firstname",
                           c."Lastname" AS "Lastname",
                           c."RegistryNumber" AS "RegistryNumber",
                           c."Email" AS "Email",
                           c."MobileNumber" AS "MobileNumber",
                           c."PhoneNumber" AS "PhoneNumber",
                           a."Street" AS "Street",
                           a."City" AS "City",
                           a."State" AS "State" ,
                           a."ZipCode" AS "ZipCode",
                           a."Country" AS "Country", 
                    FROM public."Contacts" c
                    INNER JOIN public."Addresses" a
                        ON c."AddressId" = a."Id"
                    WHERE c."Id" = :id
                    """,
                    this.SqlConn
                );

                sqlCmd.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Uuid));

                sqlCmd.Prepare();

                sqlCmd.Parameters["id"].Value = id;

                reader = sqlCmd.ExecuteReader();

                if (reader.Read())
                {

                    Address address = new Address(
                        new Guid(Convert.ToString(reader["Id"])!),
                        Convert.ToString(reader["Street"])!,
                        Convert.ToString(reader["City"])!,
                        Convert.ToString(reader["State"])!,
                        Convert.ToString(reader["ZipCode"])!,
                        Convert.ToString(reader["Country"])!
                    );
                    
                    result = new Contact(
                        new Guid(Convert.ToString(reader["ContactId"])!),
                        Convert.ToString(reader["Fistname"])!,
                        Convert.ToString(reader["Lastname"])!,
                        Convert.ToString(reader["RegistryNumber"])!,
                        Convert.ToString(reader["Email"])!,
                        Convert.ToString(reader["PhoneNumber"])!,
                        Convert.ToString(reader["MobileNumber"])!,
                        address
                    );
                }

                reader.Close();
            }
            catch (Exception ex)
            {

                Debug.WriteLine($"Unable to retrieve a contact instance with ID : {id}.\nReason :\n{ex.Message}\nException : \n{ex}");

                if (sqlCmd != null)
                    throw new AccessDbException(sqlCmd.CommandText, ex.Message);
                else
                    throw new AccessDbException($"Unable to retrieve a contact instance with ID : {id}.", ex.Message);
            }

            if (result == null) throw new Exception($"Unable to retrieve a contact instance with ID: {id}");

            return result;

        }

    }
}

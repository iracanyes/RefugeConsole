using Microsoft.Extensions.Logging;
using Npgsql;
using RefugeConsole.ClassesMetiers.Exceptions;
using RefugeConsole.ClassesMetiers.Model.Entities;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace RefugeConsole.CoucheAccesDB
{
    internal class ContactDataService : AccessDb, IContactDataService
    {
        private static readonly ILogger MyLogger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger(nameof(AnimalDataService));
        public ContactDataService()
            : base()
        { }

        public bool CreateAddress(Address address, NpgsqlTransaction transaction)
        {
            bool result = false;
            NpgsqlCommand? sqlCmd = null;

            try
            {
                sqlCmd = new NpgsqlCommand(
                    """"
                    INSERT INTO public."Addresses" ("Id", "Street", "City", "State", "ZipCode", "Country")
                    VALUES (:id, :street, :city, :state, :zipCode, :country)
                    """",
                    this.SqlConn

                );

                sqlCmd.Transaction = transaction;

                sqlCmd.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Uuid));
                sqlCmd.Parameters.Add(new NpgsqlParameter("street", NpgsqlTypes.NpgsqlDbType.Text));
                sqlCmd.Parameters.Add(new NpgsqlParameter("city", NpgsqlTypes.NpgsqlDbType.Text));
                sqlCmd.Parameters.Add(new NpgsqlParameter("state", NpgsqlTypes.NpgsqlDbType.Text));
                sqlCmd.Parameters.Add(new NpgsqlParameter("zipCode", NpgsqlTypes.NpgsqlDbType.Text));
                sqlCmd.Parameters.Add(new NpgsqlParameter("country", NpgsqlTypes.NpgsqlDbType.Text));

                sqlCmd.Prepare();

                sqlCmd.Parameters["id"].Value = address.Id;
                sqlCmd.Parameters["street"].Value = address.Street;
                sqlCmd.Parameters["city"].Value = address.City;
                sqlCmd.Parameters["state"].Value = address.State;
                sqlCmd.Parameters["zipCode"].Value = address.ZipCode;
                sqlCmd.Parameters["country"].Value = address.Country;

                int nbRowAffected = sqlCmd.ExecuteNonQuery();

                if (nbRowAffected == 0) throw new AccessDbException(sqlCmd.CommandText, $"Unable to create an address instance. Object : \n{address}");

                result = true;

            }
            catch (Exception)
            {

                throw;
            }

            return result;
        }

        public bool CreateContactRole(ContactRole contactRole, NpgsqlTransaction transaction)
        {
            bool result = false;
            NpgsqlCommand? sqlCmd = null;

            try
            {
                sqlCmd = new NpgsqlCommand(
                    """
                    INSERT INTO public."ContactRoles" ("Id", "ContactId", "RoleId")
                    VALUES (:id, :contactId, :roleId)
                    """,
                    this.SqlConn,
                    transaction
                );

                sqlCmd.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Uuid));
                sqlCmd.Parameters.Add(new NpgsqlParameter("contactId", NpgsqlTypes.NpgsqlDbType.Uuid));
                sqlCmd.Parameters.Add(new NpgsqlParameter("roleId", NpgsqlTypes.NpgsqlDbType.Uuid));


                sqlCmd.Prepare();

                sqlCmd.Parameters["id"].Value = contactRole.Id;
                sqlCmd.Parameters["contactId"].Value = contactRole.ContactId;
                sqlCmd.Parameters["roleId"].Value = contactRole.RoleId;

                int nbRowAffected = sqlCmd.ExecuteNonQuery();

                if (nbRowAffected == 0) throw new Exception($"Unable to create an ContactRole instance with data : {contactRole}.");

                result = true;

            }
            catch (Exception ex)
            {
                if(transaction != null)
                    transaction.Rollback();

                if (Debugger.IsAttached)
                    Debug.WriteLine($"Unable to create a ContactRole instance. \nMessage :\n {ex.Message}.\nException :\n{ex}");
                
                if (sqlCmd != null)
                    throw new AccessDbException(sqlCmd.CommandText, $"Unable to create a ContactRole instance. \nMessage :\n {ex.Message}.\nException :\n{ex}");
                else
                    throw new AccessDbException("Unable to create an sqlCommand for inserting a ContactRole instance", $"Unable to insert a ContactRole instance. \nMessage :\n {ex.Message}.\nException :\n{ex}");
            }

            return result;
            
        }
        
        public Contact CreateContact(Contact contact)
        {
            Contact? result = null;
            NpgsqlCommand? sqlCmd = null;
            NpgsqlTransaction transaction = this.SqlConn.BeginTransaction();

            try
            {
                // Insert the address 
                bool addressCreated = this.CreateAddress(contact.Address, transaction);
                // Throw an exception in case nothing happen!
                if (!addressCreated) throw new AccessDbException("Unable to create an address instance", $"Error while creating an address instance with object : {contact.Address}");

                               

                sqlCmd = new NpgsqlCommand(
                    """
                    INSERT INTO public."Contacts" ("Id", "Firstname", "Lastname", "RegistryNumber", "Email", "PhoneNumber", "MobileNumber", "AddressId")
                    VALUES (:id, :firstname, :lastname, :registryNumber, :email, :phoneNumber, :mobileNumber, :addressId)
                    """,
                    this.SqlConn,
                    transaction
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

                // Insert all roles for the contact
                foreach (ContactRole contactRole in contact.ContactRoles)
                {
                    bool contactRoleCreated = this.CreateContactRole(contactRole, transaction);

                    if (!contactRoleCreated) throw new Exception($"Unable to create a ContactRole instance with data : {contactRole}.");
                }

                result = this.GetContactByRegistryNumber(contact.RegistryNumber);
            }
            catch (Exception ex)
            {
                if(Debugger.IsAttached)
                    Debug.WriteLine($"Unable to create a contact info instance.\nObject : \n{contact}\nReason : \n{ex.Message}\nException : \n{ex}");
                
                if(transaction != null) transaction.Rollback();

                if (sqlCmd != null)
                    throw new AccessDbException(sqlCmd.CommandText, $"Unable to update a Contact instance. Object : {contact}.");
                else
                    throw new AccessDbException("SqlCommand is null", $"Unable to update a Contact instance. Object : {contact}.");


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

        public bool UpdateAddress(Address address, NpgsqlTransaction transaction)
        {
            bool result = false;
            NpgsqlCommand? sqlCmd = null;

            try
            {
                sqlCmd = new NpgsqlCommand(
                    """
                    UPDATE public."Addresses"
                    SET "Street" = :street,
                        "City" = :city,
                        "State" = :state,
                        "ZipCode" = :zipCode,
                        "Country" = :country
                    WHERE "Id" = :id
                    """,
                    this.SqlConn,
                    transaction
                );

                sqlCmd.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Uuid));
                sqlCmd.Parameters.Add(new NpgsqlParameter("street", NpgsqlTypes.NpgsqlDbType.Text));
                sqlCmd.Parameters.Add(new NpgsqlParameter("city", NpgsqlTypes.NpgsqlDbType.Text));
                sqlCmd.Parameters.Add(new NpgsqlParameter("state", NpgsqlTypes.NpgsqlDbType.Text));
                sqlCmd.Parameters.Add(new NpgsqlParameter("zipCode", NpgsqlTypes.NpgsqlDbType.Text));
                sqlCmd.Parameters.Add(new NpgsqlParameter("country", NpgsqlTypes.NpgsqlDbType.Text));

                sqlCmd.Prepare();

                sqlCmd.Parameters["id"].Value = address.Id;
                sqlCmd.Parameters["street"].Value = address.Street;
                sqlCmd.Parameters["city"].Value = address.Country;
                sqlCmd.Parameters["state"].Value = address.State;
                sqlCmd.Parameters["zipCode"].Value = address.ZipCode;
                sqlCmd.Parameters["country"].Value = address.Country;


                int nbRowAffected = sqlCmd.ExecuteNonQuery();

                if (nbRowAffected == 0) throw new AccessDbException(sqlCmd.CommandText, $"Unable to update an address instance with info : {address}.");

                result = true;


            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to update an address instance. Object : {address}. Exception message: {ex.Message}.\nException : {ex}");
                if (sqlCmd != null)
                    throw new AccessDbException(sqlCmd.CommandText, $"Unable to update an address instance. Object : {address}.");
                else
                    throw new AccessDbException("SqlCommand is null", $"Unable to update an address instance. Object : {address}.");
            }


            return result;
        }

        public Contact UpdateContact(Contact contact) {
            Contact? result = null;
            NpgsqlCommand? sqlCmd = null;
            NpgsqlTransaction transaction = this.SqlConn.BeginTransaction();

            try
            {
                // First, update the address
                bool addressUpdated = this.UpdateAddress(contact.Address, transaction);

                if (!addressUpdated) throw new AccessDbException("sqlCmd - updateAddress", $"Unexpected error while updating the address with info {contact.Address}."); 

                sqlCmd = new NpgsqlCommand(
                    $"""
                    UPDATE public."Contacts" c
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
                    this.SqlConn,
                    transaction
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

        public HashSet<Role> GetRoles()
        {
            HashSet<Role> roles = new HashSet<Role>();
            NpgsqlCommand? sqlCmd = null;
            NpgsqlDataReader? reader = null;

            try
            {
                sqlCmd = new NpgsqlCommand(
                    """
                    SELECT *
                    FROM public."Roles"
                    """,
                    this.SqlConn
                );

                sqlCmd.Prepare();

                reader = sqlCmd.ExecuteReader();

                if (reader.Read())
                {
                    roles.Add(new Role(
                       new Guid(Convert.ToString(reader["Id"])!),
                       Convert.ToString(reader["Name"])!
                    ));
                }

                reader.Close();

            }
            catch (Exception)
            {
                if (reader != null) reader.Close();


                throw;
            }

            return roles;

        }



    }
}

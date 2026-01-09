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

        public bool CreateAddress(Address address, NpgsqlTransaction? transaction = null)
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
                sqlCmd.Parameters["email"].Value = contact.Email != null ? contact.Email : DBNull.Value;
                sqlCmd.Parameters["phoneNumber"].Value = contact.PhoneNumber != null ? contact.PhoneNumber : DBNull.Value;
                sqlCmd.Parameters["mobileNumber"].Value = contact.MobileNumber != null ? contact.MobileNumber : DBNull.Value;
                sqlCmd.Parameters["addressId"].Value = contact.AddressId;

                int nbRowAffected = sqlCmd.ExecuteNonQuery();

                if(nbRowAffected == 0) throw new AccessDbException(sqlCmd.CommandText, $"Unable to create a Contact instance in DB!\nObject:\n{contact}");

                // Insert all roles for the contact
                foreach (ContactRole contactRole in contact.ContactRoles)
                {
                    bool contactRoleCreated = this.CreateContactRole(contactRole, transaction);

                    if (!contactRoleCreated) throw new AccessDbException($"", $"Unable to create a ContactRole instance with data : {contactRole}.");
                }

                // Commit transaction
                transaction.Commit();

                result = this.GetContactByRegistryNumber(contact.RegistryNumber);
            }
            catch (Exception ex)
            {
                if(Debugger.IsAttached)
                    Debug.WriteLine($"Unable to create a contact info instance.\nObject : \n{contact}\nReason : \n{ex.Message}\nException : \n{ex}");
                
                if(transaction != null && transaction.Connection != null) transaction.Rollback();

                if (sqlCmd != null)
                    throw new AccessDbException(sqlCmd.CommandText, $"Unable to create a Contact instance. \nError: {ex.Message}.\nException: {ex}");
                else
                    throw new AccessDbException("SqlCommand is null", $"Unable to create a Contact instance. \nError: {ex.Message}.\nException: {ex}.");


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
                    SELECT c."Id" AS "Id",
                           c."Firstname" AS "Firstname",
                           c."Lastname" AS "Lastname",
                           c."RegistryNumber" AS "RegistryNumber",
                           c."Email" AS "Email",
                           c."MobileNumber" AS "MobileNumber",
                           c."PhoneNumber" AS "PhoneNumber",
                           a."Id" AS "AddressId",
                           a."Street" AS "Street",
                           a."City" AS "City",
                           a."State" AS "State" ,
                           a."ZipCode" AS "ZipCode",
                           a."Country" AS "Country" 
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
                        new Guid(Convert.ToString(reader["AddressId"])!),
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

                // Attach contact's roles to contact
                this.GetContactRoles(result!);


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



        public bool UpdateAddress(Address address, NpgsqlTransaction? transaction = null)
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

                if (nbRowAffected == 0) throw new AccessDbException(sqlCmd.CommandText, $"Unknown error while updating an address instance with info.");

                result = true;


            }
            catch (Exception ex)
            {                

                if(Debugger.IsAttached)
                    Debug.WriteLine($"Unable to update an address instance.\n Exception message: {ex.Message}.\nException : {ex}");

                if (sqlCmd != null)
                    throw new AccessDbException(sqlCmd.CommandText, $"Unable to update an address instance.\n Exception message: {ex.Message}.\nException : {ex}");
                else
                    throw new AccessDbException("SqlCommand - UpdateAddress", $"Unable to update an address instance.\n Exception message: {ex.Message}.\nException : {ex}");
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
                sqlCmd.Parameters.Add(new NpgsqlParameter("registryNumber", NpgsqlTypes.NpgsqlDbType.Text));;
                sqlCmd.Parameters.Add(new NpgsqlParameter("email", NpgsqlTypes.NpgsqlDbType.Text));
                sqlCmd.Parameters.Add(new NpgsqlParameter("phoneNumber", NpgsqlTypes.NpgsqlDbType.Text));
                sqlCmd.Parameters.Add(new NpgsqlParameter("mobileNumber", NpgsqlTypes.NpgsqlDbType.Text));
                sqlCmd.Parameters.Add(new NpgsqlParameter("addressId", NpgsqlTypes.NpgsqlDbType.Uuid));


                sqlCmd.Prepare();

                sqlCmd.Parameters["id"].Value = contact.Id;
                sqlCmd.Parameters["firstname"].Value = contact.Firstname;
                sqlCmd.Parameters["lastname"].Value = contact.Lastname;
                sqlCmd.Parameters["registryNumber"].Value = contact.RegistryNumber;
                sqlCmd.Parameters["email"].Value = contact.Email != null ? contact.Email : DBNull.Value;
                sqlCmd.Parameters["phoneNumber"].Value = contact.PhoneNumber != null ? contact.PhoneNumber : DBNull.Value;
                sqlCmd.Parameters["mobileNumber"].Value = contact.MobileNumber != null ? contact.MobileNumber : DBNull.Value;
                sqlCmd.Parameters["addressId"].Value = contact.AddressId;

                int nbRowAffected = sqlCmd.ExecuteNonQuery();

                if (nbRowAffected == 0) throw new AccessDbException(sqlCmd.CommandText, $"Unable to update a Contact instance in DB!\nObject:\n{contact}");

                // Commit transaction
                transaction.Commit();

                result = this.GetContactByRegistryNumber(contact.RegistryNumber);


            }
            catch (Exception ex)
            {
                if(transaction != null && transaction.Connection != null) transaction.Rollback();

                if(Debugger.IsAttached)
                    Debug.WriteLine($"Unable to update a contact instance with registry number : {contact.RegistryNumber}.\nReason :\n{ex.Message}\nException : \n{ex}");

                if (sqlCmd != null)
                    throw new AccessDbException(sqlCmd.CommandText, $"Unable to update a contact instance with registry number : {contact.RegistryNumber}.\nReason :\n{ex.Message}\nException : \n{ex}");
                else
                    throw new AccessDbException("sqlCmd is null", $"Unable to update a contact instance with registry number : {contact.RegistryNumber}.\nReason :\n{ex.Message}\nException : \n{ex}");
            }

            if (result == null) throw new Exception($"Unable to update a contact instance with registry number : {contact.RegistryNumber}");

            return result;
        }

        public bool DeleteContactRole(ContactRole contactRole, NpgsqlTransaction? transaction = null)
        {
            bool result = false;
            NpgsqlCommand? sqlCmd = null;

            try
            {
                sqlCmd = new NpgsqlCommand(
                    $"""
                    DELETE FROM public."ContactRoles"
                    WHERE "Id" = :id
                    """,
                    this.SqlConn,
                    transaction
                );

                sqlCmd.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Uuid));

                sqlCmd.Prepare();

                sqlCmd.Parameters["id"].Value = contactRole.Id;

                int nbRowAffected = sqlCmd.ExecuteNonQuery();

                if (nbRowAffected == 0) throw new AccessDbException(sqlCmd.CommandText, $"Error while deleting contact's role info : \n{contactRole.Id}");

                result = true;
            }
            catch (Exception ex)
            {
                if(Debugger.IsAttached)
                    Debug.WriteLine($"Unable to delete a contact's role instance : {contactRole.Id}.\nReason :\n{ex.Message}\nException : \n{ex}");

                if (sqlCmd != null)
                    throw new AccessDbException(sqlCmd.CommandText, $"Unable to delete a contact's role instance : {contactRole.Id}.\nReason :\n{ex.Message}\nException : \n{ex}");
                else
                    throw new AccessDbException($"DeleteContact : qlCmd is null", $"Unable to delete a contact's role instance : {contactRole.Id}.\nReason :\n{ex.Message}\nException : \n{ex}");
            }


            return result;
        }

        public bool DeleteContact(Contact contact)
        {
            bool result = false;
            NpgsqlCommand? sqlCmd = null;
            NpgsqlTransaction transaction = this.SqlConn.BeginTransaction();

            try
            {

                // Delete all contact's roles
                foreach (ContactRole contactRole in contact.ContactRoles)
                    this.DeleteContactRole(contactRole, transaction);

                sqlCmd = new NpgsqlCommand(
                    $"""
                    DELETE FROM public."Contacts"
                    WHERE "Id" = :id
                    """,
                    this.SqlConn, 
                    transaction
                );

                sqlCmd.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Uuid));
                 
                sqlCmd.Prepare();

                sqlCmd.Parameters["id"].Value = contact.Id;

                int nbRowAffected = sqlCmd.ExecuteNonQuery();

                if (nbRowAffected == 0) throw new AccessDbException(sqlCmd.CommandText, $"Error while deleting contact info : \n{contact}");

                transaction.Commit();

                result = true;
            }
            catch (Exception ex)
            {
                if(transaction != null && transaction.Connection != null) transaction.Rollback();

                if(Debugger.IsAttached)
                    Debug.WriteLine($"Unable to delete a contact instance with registry number : {contact.RegistryNumber}.\nReason :\n{ex.Message}\nException : \n{ex}");

                if (sqlCmd != null)
                    throw new AccessDbException(sqlCmd.CommandText, $"Unable to delete a contact instance with registry number : {contact.RegistryNumber}.\nReason :\n{ex.Message}\nException : \n{ex}");
                else
                    throw new AccessDbException($"DeleteContact : sqlCmd is null", $"Unable to delete a contact instance with registry number : {contact.RegistryNumber}.\nReason :\n{ex.Message}\nException : \n{ex}");
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

                while (reader.Read())
                {
                    roles.Add(new Role(
                       new Guid(Convert.ToString(reader["Id"])!),
                       Convert.ToString(reader["Name"])!
                    ));
                }

                reader.Close();

            }
            catch (Exception ex)
            {
                if (reader != null) reader.Close();

                if (Debugger.IsAttached)
                    Debug.WriteLine($"Error while retrieving roles from DB. Error : {ex.Message}. Exception: {ex}");

                if (sqlCmd != null)
                    throw new AccessDbException(sqlCmd.CommandText, $"Error while retrieving roles from DB. Error : {ex.Message}. Exception: {ex}");
                else
                    throw new AccessDbException("sqlCmd is NULL", $"Error while retrieving roles from DB. Error : {ex.Message}. Exception: {ex}");

            }

            return roles;

        }


        public HashSet<ContactRole> GetContactRoles(Contact contact) { 
            NpgsqlCommand? sqlCmd = null;
            NpgsqlDataReader? reader = null;

            try
            {
                sqlCmd = new NpgsqlCommand(
                    """
                    SELECT cr."Id" AS "Id",
                            cr."ContactId" AS "ContactId",
                            cr."RoleId" AS "RoleId",
                            r."Name" AS "RoleName"
                    FROM public."ContactRoles" cr
                    INNER JOIN public."Roles" r
                        ON cr."RoleId" = r."Id"
                    WHERE cr."ContactId" = :contactId                
                    """,
                    this.SqlConn
                );

                sqlCmd.Parameters.Add(new NpgsqlParameter("contactId", NpgsqlTypes.NpgsqlDbType.Uuid));

                sqlCmd.Prepare();

                sqlCmd.Parameters["contactId"].Value = contact.Id;

                reader = sqlCmd.ExecuteReader();

                while (reader.Read())
                {
                    Role role = new Role(new Guid(Convert.ToString(reader["RoleId"])!), Convert.ToString(reader["RoleName"])!);

                    contact.ContactRoles.Add(  new ContactRole(new Guid(Convert.ToString(reader["Id"])!), contact, role) );
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);

                if (reader != null) reader.Close();

                if (Debugger.IsAttached)
                    Debug.WriteLine($"Error while retrieving contact's roles from DB. Error : {ex.Message}. Exception: {ex}");

                if (sqlCmd != null)
                    throw new AccessDbException(sqlCmd.CommandText, $"Error while retrieving contact's roles from DB. Error : {ex.Message}. Exception: {ex}");
                else
                    throw new AccessDbException("sqlCmd is NULL", $"Error while retrieving contact's roles from DB. Error : {ex.Message}. Exception: {ex}");
            }

            return contact.ContactRoles;

        }

        /**
         * 
         * 
         */
        public bool RegistryNumberExists(string registryNumber)
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
                        FROM public."Contacts"
                        WHERE "RegistryNumber" = :registryNumber
                        LIMIT 1
                    )
                    """,
                    this.SqlConn
                );

                sqlCmd.Parameters.Add(new NpgsqlParameter("registryNumber", NpgsqlTypes.NpgsqlDbType.Text));

                sqlCmd.Prepare();

                sqlCmd.Parameters["registryNumber"].Value = registryNumber;

                reader = sqlCmd.ExecuteReader();

                if (reader.Read())
                {
                    result = Convert.ToBoolean(reader["exists"]);
                }

                // Close reader 
                reader.Close();


            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);

                if (reader != null) reader.Close();

                if (Debugger.IsAttached)
                    Debug.WriteLine($"Error while retrieving contact's roles from DB. Error : {ex.Message}. Exception: {ex}");

                if (sqlCmd != null)
                    throw new AccessDbException(sqlCmd.CommandText, $"Error while retrieving contact's roles from DB. Error : {ex.Message}. Exception: {ex}");
                else
                    throw new AccessDbException("sqlCmd is NULL", $"Error while retrieving contact's roles from DB. Error : {ex.Message}. Exception: {ex}");
            }

            return result;
        }



    }
}

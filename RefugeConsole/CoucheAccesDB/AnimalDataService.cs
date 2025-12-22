using Microsoft.Extensions.Logging;
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
    internal class AnimalDataService : AccessDb, IAnimalDataService
    {
        private static readonly ILogger MyLogger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger(nameof(AnimalDataService));
        

        public AnimalDataService()
            : base() { }
        


        /**
         * Create an animal record in database
         */ 
        public Animal CreateAnimal(Animal animal)
        {
            Animal? result = null;
            NpgsqlCommand? sqlCmd = null;

            try
            {
                sqlCmd = new NpgsqlCommand(
                    $"""
                    INSERT INTO public."Animals" ("Id", "Name", "Type", "Gender", "BirthDate", "DeathDate", "IsSterilized","DateSterilization", "Particularity", "Description")
                    VALUES (:id, :name, :type, :gender, :dateBirth, :dateDeath, :isSterilized, :dateSterilization, :particularity, :description )
                    """,
                    this.SqlConn
                );

                sqlCmd.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Text));
                sqlCmd.Parameters.Add(new NpgsqlParameter("name", NpgsqlTypes.NpgsqlDbType.Text));
                sqlCmd.Parameters.Add(new NpgsqlParameter("type", NpgsqlTypes.NpgsqlDbType.Text));
                sqlCmd.Parameters.Add(new NpgsqlParameter("gender", NpgsqlTypes.NpgsqlDbType.Text));
                sqlCmd.Parameters.Add(new NpgsqlParameter("dateBirth", NpgsqlTypes.NpgsqlDbType.Date));
                sqlCmd.Parameters.Add(new NpgsqlParameter("dateDeath", NpgsqlTypes.NpgsqlDbType.Date));
                sqlCmd.Parameters.Add(new NpgsqlParameter("isSterilized", NpgsqlTypes.NpgsqlDbType.Boolean));
                sqlCmd.Parameters.Add(new NpgsqlParameter("dateSterilization", NpgsqlTypes.NpgsqlDbType.Date));
                sqlCmd.Parameters.Add(new NpgsqlParameter("particularity", NpgsqlTypes.NpgsqlDbType.Text));
                sqlCmd.Parameters.Add(new NpgsqlParameter("description", NpgsqlTypes.NpgsqlDbType.Text));

                // Prepare the parametized statement
                sqlCmd.Prepare();

                
                // Parameters value
                sqlCmd.Parameters["id"].Value = animal.Id;
                sqlCmd.Parameters["name"].Value = animal.Name;
                sqlCmd.Parameters["type"].Value = animal.Type;
                sqlCmd.Parameters["gender"].Value = animal.Gender;
                sqlCmd.Parameters["dateBirth"].Value = animal.BirthDate;
                sqlCmd.Parameters["dateDeath"].Value = animal.DeathDate != null ? animal.DeathDate : DBNull.Value;
                sqlCmd.Parameters["isSterilized"].Value = animal.IsSterilized;
                sqlCmd.Parameters["dateSterilization"].Value = animal.DateSterilization != null ? animal.DateSterilization : DBNull.Value;
                sqlCmd.Parameters["particularity"].Value = animal.Particularity != null ? animal.Particularity : DBNull.Value;
                sqlCmd.Parameters["description"].Value = animal.Description != null ? animal.Description : DBNull.Value;
                
                // Parametized statement execution
                int createOp = sqlCmd.ExecuteNonQuery();

                if (createOp == 0) throw new Exception("Impossible d'ajouter l'animal!");

                result = GetAnimalById(animal.Id);

            }
            catch (Exception ex)
            {
                if (sqlCmd != null)
                    throw new AccessDbException(sqlCmd.CommandText, ex.Message);
                else
                    throw new AccessDbException("Error while creating an animal", ex.Message);
            }

            if (result == null) throw new Exception($"Unable to find newly created animal with name {animal.Name}");

            return result;
        }

        
        public List<Animal> GetAnimalByName(string name)
        {
            List<Animal> result = [];
            NpgsqlCommand? sqlCmd = null;
            NpgsqlDataReader? reader = null;

            try
            {
                sqlCmd = new NpgsqlCommand(
                    $"""
                    SELECT *
                    FROM public."Animals" a
                    WHERE "Name" = :name
                    """,
                    SqlConn
                );

                sqlCmd.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Text);

                sqlCmd.Prepare();

                sqlCmd.Parameters["name"].Value = name;

                reader = sqlCmd.ExecuteReader();

                while (reader.Read())
                {
                    Debug.WriteLine($"reader - birthDate : {reader["BirthDate"]}");
                    DateOnly? birthDate = reader["BirthDate"] != DBNull.Value ? (DateOnly) reader["BirthDate"] : null; 
                    DateOnly? deathDate = reader["DeathDate"] != DBNull.Value ? (DateOnly) reader["DeathDate"] : null;
                    DateOnly? dateSterilization = reader["dateSterilization"] != DBNull.Value ? (DateOnly)  reader["dateSterilization"] : null;

                    Animal animal = new Animal(
                        Convert.ToString(reader["Id"])!,
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

                    

                    result.Add( animal );                    


                }

                reader.Close();

                foreach(Animal a in result)
                {
                    // Retrieve animal's colors from DB
                    this.GetAnimalColors(a);
                }

            }
            catch (Exception ex)
            {
                if(reader != null) reader.Close();

                if(sqlCmd != null)
                    throw new AccessDbException(sqlCmd.CommandText, ex.ToString());
                else
                    throw new AccessDbException("Error while retrieving an animal", ex.ToString());
            }

            

            return result;
        }

        public Animal? GetAnimalById(string id)
        {
            Animal? result = null;
            NpgsqlCommand? sqlCmd = null;
            NpgsqlDataReader? reader = null;

            try
            {
                sqlCmd = new NpgsqlCommand(
                    $"""
                    SELECT *
                    FROM public."Animals" a
                    WHERE "Id" = :id
                    """,
                    SqlConn
                );

                sqlCmd.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Varchar);

                sqlCmd.Prepare();

                sqlCmd.Parameters["id"].Value = id;

                reader = sqlCmd.ExecuteReader();

                if (reader.Read())
                {
                    Debug.WriteLine($"reader - birthDate : {reader["BirthDate"]}");
                    DateOnly? birthDate = reader["BirthDate"] != DBNull.Value ? (DateOnly)reader["BirthDate"] : null;
                    DateOnly? deathDate = reader["DeathDate"] != DBNull.Value ? (DateOnly)reader["DeathDate"] : null;
                    DateOnly? dateSterilization = reader["dateSterilization"] != DBNull.Value ? (DateOnly)reader["dateSterilization"] : null;


                    result = new Animal(
                        Convert.ToString(reader["Id"])!,
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

                    


                }

                reader.Close();

                if (result == null)
                    throw new AccessDbException(sqlCmd.CommandText, $"Unknown error while retrieving animal.");

                // Retrieve animal's colors from DB
                this.GetAnimalColors(result);
            }
            catch (Exception ex)
            {
                reader?.Close();

                if (sqlCmd != null)
                    throw new AccessDbException(sqlCmd.CommandText, ex.ToString());
                else
                    throw new AccessDbException("Error while retrieving an animal", ex.ToString());
            }



            return result;
        }

        public HashSet<AnimalColor> GetAnimalColors(Animal animal)
        {
            NpgsqlCommand? sqlCmd = null;
            NpgsqlDataReader? reader = null;

            try
            {
                sqlCmd = new NpgsqlCommand(
                    """
                    SELECT ac."Id" as "Id",
                            ac."AnimalId" AS "AnimalId",
                            ac."ColorId" AS "ColorId",
                            c."Name" AS "ColorName"
                    FROM public."AnimalColors" ac
                    INNER JOIN public."Colors" c
                        ON ac."ColorId" = c."Id"
                    WHERE ac."AnimalId" = :animalId
                    """,
                    this.SqlConn
                );

                sqlCmd.Parameters.Add(new NpgsqlParameter("animalId", NpgsqlTypes.NpgsqlDbType.Varchar));

                sqlCmd.Prepare();

                sqlCmd.Parameters["animalId"].Value = animal.Id;

                reader = sqlCmd.ExecuteReader();

                while (reader.Read()) {
                    Color color = new Color(new Guid(Convert.ToString(reader["ColorId"])!), Convert.ToString(reader["ColorName"])!);

                    animal.AnimalColors.Add(
                        new AnimalColor(
                            new Guid(Convert.ToString(reader["Id"])!),
                            animal,
                            new Color(
                                new Guid(Convert.ToString(reader["Id"])!),
                                Convert.ToString(reader["ColorName"])!
                            )
                        )
                    );
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                if(reader != null) { reader.Close(); }

                if (Debugger.IsAttached)
                    Debug.WriteLine($"Error while retrieving animal's colors for {animal.Name}. Message: {ex.Message}. Exception : {ex}");

                if (sqlCmd != null)
                    throw new AccessDbException(sqlCmd.CommandText, $"Error while retrieving animal's colors for {animal.Name}. Message: {ex.Message}. Exception : {ex}");
                else
                    throw new AccessDbException("sqlCmd.CommandText is null", $"Error while retrieving animal's colors for {animal.Name}. Message: {ex.Message}. Exception : {ex}");

            }

            return animal.AnimalColors;

        }

        public bool RemoveAnimal(Animal animal)
        {
            bool result = false;
            NpgsqlCommand? sqlCmd = null;

            try
            {
                sqlCmd = new NpgsqlCommand(
                    $"""
                    DELETE FROM public."Animals" a
                    WHERE "Id" = :id
                    """, 
                    this.SqlConn
                );

                sqlCmd.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Varchar));

                sqlCmd.Prepare();

                sqlCmd.Parameters["id"].Value = animal.Id;

                int response = sqlCmd.ExecuteNonQuery();

                if (response == 0) throw new AccessDbException(sqlCmd.CommandText, $"Error while deleting an animal with ID({animal.Id})");

                result = true;
            }
            catch (Exception ex)
            {
                if(sqlCmd != null)
                    throw new AccessDbException(sqlCmd.CommandText, ex.Message);
                else
                    throw new AccessDbException("Error while creating the SQL command instance!", ex.Message);
            }

            return result;
        }

        public Animal UpdateAnimal(Animal animal)
        {
            Animal? result = null;
            NpgsqlCommand? sqlCmd = null;

            try
            {
                sqlCmd = new NpgsqlCommand(
                    $"""
                    UPDATE public."Animals"
                    SET "Name" = :name,
                        "Type" = :type,
                        "Gender" = :gender,
                        "BirthDate" = :dateBirth,
                        "DeathDate" = :dateDeath,
                        "IsSterilized" = :isSterilized,
                        "DateSterilization" = :dateSterilization,
                        "Particularity" = :particularity,
                        "Description" = :description
                    WHERE "Id" = :id
                    """,
                    this.SqlConn
                );

                sqlCmd.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Text));
                sqlCmd.Parameters.Add(new NpgsqlParameter("name", NpgsqlTypes.NpgsqlDbType.Text));
                sqlCmd.Parameters.Add(new NpgsqlParameter("type", NpgsqlTypes.NpgsqlDbType.Text));
                sqlCmd.Parameters.Add(new NpgsqlParameter("gender", NpgsqlTypes.NpgsqlDbType.Varchar));
                sqlCmd.Parameters.Add(new NpgsqlParameter("dateBirth", NpgsqlTypes.NpgsqlDbType.Date));
                sqlCmd.Parameters.Add(new NpgsqlParameter("dateDeath", NpgsqlTypes.NpgsqlDbType.Date));
                sqlCmd.Parameters.Add(new NpgsqlParameter("isSterilized", NpgsqlTypes.NpgsqlDbType.Boolean));
                sqlCmd.Parameters.Add(new NpgsqlParameter("dateSterilization", NpgsqlTypes.NpgsqlDbType.Date));
                sqlCmd.Parameters.Add(new NpgsqlParameter("particularity", NpgsqlTypes.NpgsqlDbType.Text));
                sqlCmd.Parameters.Add(new NpgsqlParameter("description", NpgsqlTypes.NpgsqlDbType.Text));

                // Prepare the parametized statement
                sqlCmd.Prepare();


                // Parameters value
                sqlCmd.Parameters["id"].Value = animal.Id;
                sqlCmd.Parameters["name"].Value = animal.Name;
                sqlCmd.Parameters["type"].Value = animal.Type;
                sqlCmd.Parameters["gender"].Value = animal.Gender;
                sqlCmd.Parameters["dateBirth"].Value = animal.BirthDate;
                sqlCmd.Parameters["dateDeath"].Value = animal.DeathDate != null ? animal.DeathDate : DBNull.Value;
                sqlCmd.Parameters["isSterilized"].Value = animal.IsSterilized;
                sqlCmd.Parameters["dateSterilization"].Value = animal.DateSterilization != null ? animal.DateSterilization : DBNull.Value;
                sqlCmd.Parameters["particularity"].Value = animal.Particularity != null ? animal.Particularity : DBNull.Value;
                sqlCmd.Parameters["description"].Value = animal.Description != null ? animal.Description : DBNull.Value;

                // Parametized statement execution
                int nbRowAffected = sqlCmd.ExecuteNonQuery();

                if (nbRowAffected == 0) throw new AccessDbException(sqlCmd.CommandText, $"Unable to update animal with id {animal.Id}! No row affected!");

                result = GetAnimalById(animal.Id);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AnimalDataService : Error while updating an animal with name {animal.Name} in DB!\nException :\n{ex.Message}\nException:\n{ex}");

                if (sqlCmd != null)
                    throw new AccessDbException(sqlCmd.CommandText, ex.Message);
                else
                    throw new AccessDbException("Error while creating the SQL command instance!", ex.Message);

                
            }

            if (result == null) throw new Exception($"Unknown error while updating animal with name {animal.Name}.");

            return result!;
        }

        public bool CreateCompatibility(Compatibility compatibility, NpgsqlTransaction? transaction = null) {
            bool result = false;
            NpgsqlCommand? sqlCmd = null;

            try
            {
                sqlCmd = new NpgsqlCommand(
                     """
                     INSERT INTO public."Compatibilities" ("Id", "Type")
                     VALUES (:id, :type)
                     """,
                     this.SqlConn,
                     transaction
                );

                sqlCmd.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Uuid));
                sqlCmd.Parameters.Add(new NpgsqlParameter("type", NpgsqlTypes.NpgsqlDbType.Text));

                sqlCmd.Prepare();

                sqlCmd.Parameters["id"].Value = compatibility.Id;
                sqlCmd.Parameters["type"].Value = compatibility.Type;

                int nbRowAffected = sqlCmd.ExecuteNonQuery();

                if (nbRowAffected > 0) {
                    throw new AccessDbException(sqlCmd.CommandText, $"Unable to create a compatibility instance with type name : {compatibility.Type}");
                }

                result = true;

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to create a compatibility instance with type name : {compatibility.Type}.\nException Message: {ex.Message}.\nException : {ex}");
                
                if(transaction != null) transaction.Rollback();
            }

            return result;
        }

        public bool CreateAnimalCompatibility(AnimalCompatibility animalCompatibility)
        {
            bool result = false;
            NpgsqlCommand? sqlCmd = null;

            try
            {
                sqlCmd = new NpgsqlCommand(
                    $"""
                    INSERT INTO public."AnimalCompatibilities" ("Id", "Value", "Description", "AnimalId", "CompatibilityId")
                    VALUES ( :id, :value, :description, :animalId, :compatibilityId )
                    """,
                    this.SqlConn
                );

                sqlCmd.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Uuid));
                sqlCmd.Parameters.Add(new NpgsqlParameter("value", NpgsqlTypes.NpgsqlDbType.Boolean));
                sqlCmd.Parameters.Add(new NpgsqlParameter("description", NpgsqlTypes.NpgsqlDbType.Text));
                sqlCmd.Parameters.Add(new NpgsqlParameter("animalId", NpgsqlTypes.NpgsqlDbType.Text));
                sqlCmd.Parameters.Add(new NpgsqlParameter("compatibilityId", NpgsqlTypes.NpgsqlDbType.Uuid));

                sqlCmd.Prepare();

                sqlCmd.Parameters["id"].Value = animalCompatibility.Id;
                sqlCmd.Parameters["value"].Value = animalCompatibility.Value != null ? animalCompatibility.Value : DBNull.Value;
                sqlCmd.Parameters["description"].Value = animalCompatibility.Description != null ? animalCompatibility.Description : DBNull.Value;
                sqlCmd.Parameters["animalId"].Value = animalCompatibility.AnimalId;
                sqlCmd.Parameters["compatibilityId"].Value = animalCompatibility.CompatibilityId;


                int nbRowAffected = sqlCmd.ExecuteNonQuery();

                if (nbRowAffected == 0) throw new AccessDbException(sqlCmd.CommandText, $"Unable to create a compatibility with an animal in Db.\nObject\n{animalCompatibility}");

                result = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to create a compatibility with an animal.\nReason : {ex.Message}.\nObject :\n{animalCompatibility}\nException:\n{ex}");
                throw;
            }

            return result;
        }


        public HashSet<Compatibility> GetCompatibilities()
        {
            HashSet<Compatibility> result = new HashSet<Compatibility>();
            NpgsqlCommand? sqlCmd = null;
            NpgsqlDataReader? reader = null;

            try
            {
                sqlCmd = new NpgsqlCommand(
                    """
                    SELECT *
                    FROM public."Compatibilities"
                    """,
                    this.SqlConn
                );

                sqlCmd.Prepare();

                reader = sqlCmd.ExecuteReader();

                while (reader.Read())
                {
                    result.Add(new Compatibility(
                        new Guid(Convert.ToString(reader["Id"])!),
                        Convert.ToString(reader["Type"])!
                    )); 
                }

                // Close the reader 
                reader.Close();

            }
            catch (Exception ex)
            {
                if (reader != null) reader.Close();

                if (Debugger.IsAttached)
                    Debug.WriteLine($"Unable to retrieve compatibilities instance! Message: {ex.Message}.\nException : {ex}");

                if (sqlCmd != null)
                    throw new AccessDbException(sqlCmd.CommandText, $"Unable to retrieve compatibilities instance! Message: {ex.Message}.\nException : {ex}");
                else
                    throw new AccessDbException("SqlCommand is null!", $"Unable to retrieve compatibilities instance! Message: {ex.Message}.\nException : {ex}");
            }

            return result;

        }

        public HashSet<Color> GetColors()
        {
            HashSet<Color> colors = new HashSet<Color>();
            NpgsqlCommand? sqlCmd = null;
            NpgsqlDataReader? reader = null;

            try
            {
                sqlCmd = new NpgsqlCommand(
                    """
                    SELECT "Id", "Name"
                    FROM public."Colors"
                    """,
                    this.SqlConn
                );

                sqlCmd.Prepare();

                reader = sqlCmd.ExecuteReader();

                while (reader.Read()) {
                    colors.Add(new Color(
                        new Guid(Convert.ToString(reader["Id"])!),
                        Convert.ToString(reader["Name"])!
                    ));
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                if(reader != null) reader.Close();

                if (Debugger.IsAttached)
                    Debug.WriteLine($"Error while retrieving animal's colors in database. Message : {ex.Message}. Error : {ex} ");
                if (sqlCmd != null)
                    throw new AccessDbException(sqlCmd.CommandText, $"Error while retrieving animal's colors in database. Message : {ex.Message}. Error : {ex} ");
                else
                    throw new AccessDbException("SQL Command is null", $"Error while retrieving animal's colors in database. Message : {ex.Message}. Error : {ex} ");
            }

            return colors;
        }

        public bool CreateAnimalColor(AnimalColor animalColor, NpgsqlTransaction? transaction = null)
        {
            bool result = false;
            NpgsqlCommand? sqlCmd = null;

            try
            {
                sqlCmd = new NpgsqlCommand(
                    """
                    INSERT INTO public."AnimalColors" ("Id", "AnimalId", "ColorId")
                    VALUES (:id, :animalId, :colorId)
                    """,
                    this.SqlConn,
                    transaction
                );

                sqlCmd.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Uuid));
                sqlCmd.Parameters.Add(new NpgsqlParameter("animalId", NpgsqlTypes.NpgsqlDbType.Varchar));
                sqlCmd.Parameters.Add(new NpgsqlParameter("colorId", NpgsqlTypes.NpgsqlDbType.Uuid));


                sqlCmd.Prepare();

                sqlCmd.Parameters["id"].Value = animalColor.Id;
                sqlCmd.Parameters["animalId"].Value = animalColor.AnimalId;
                sqlCmd.Parameters["colorId"].Value = animalColor.ColorId;

                int nbRowAffected = sqlCmd.ExecuteNonQuery();

                if(nbRowAffected == 0)
                    throw new AccessDbException(sqlCmd.CommandText, $"Error while creating animal's colors in database.");

                result = true;


            }
            catch (Exception ex)
            {

                if (Debugger.IsAttached)
                    Debug.WriteLine($"Error while creating animal's colors in database. Message : {ex.Message}. Error : {ex} ");

                if (sqlCmd != null)
                    throw new AccessDbException(sqlCmd.CommandText, $"Error while creating animal's colors in database. Message : {ex.Message}. Error : {ex} ");
                else
                    throw new AccessDbException("SQL Command is null", $"Error while creating animal's colors in database. Message : {ex.Message}. Error : {ex} ");
            }


            return result;
        }


    }
}

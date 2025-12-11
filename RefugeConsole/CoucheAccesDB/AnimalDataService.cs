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
    internal class AnimalDataService : IAnimalDataService
    {
        private static readonly ILogger MyLogger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger(nameof(AnimalDataService));
        private readonly NpgsqlConnection SqlConn;

        public AnimalDataService()
        {
            try
            {
                SqlConn = new NpgsqlConnection(Environment.GetEnvironmentVariable("REFUGE_DB_CONNECTION_STRING"));
                SqlConn.Open();

            }
            catch (Exception ex) {
                MyLogger.LogError("Error while connecting to database. Reason : {0}", ex.Message);
                throw new AccessDbException(ex.Message, "Error while connecting to database");
            }
        }
        public Animal CreateAnimal(Animal animal)
        {
            Animal? result = null;
            NpgsqlCommand? sqlCmd = null;

            try
            {
                sqlCmd = new NpgsqlCommand(
                    $"""
                    INSERT INTO public."Animals" ("Id", "Name", "Type", "Gender", "Color", "BirthDate", "DeathDate", "IsSterilized","DateSterilization", "Particularity", "Description")
                    VALUES (:id, :name, :type, :gender, :color, :dateBirth, :dateDeath, :isSterilized, :dateSterilization, :particularity, :description )
                    """,
                    this.SqlConn
                );

                sqlCmd.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Text));
                sqlCmd.Parameters.Add(new NpgsqlParameter("name", NpgsqlTypes.NpgsqlDbType.Text));
                sqlCmd.Parameters.Add(new NpgsqlParameter("type", NpgsqlTypes.NpgsqlDbType.Text));
                sqlCmd.Parameters.Add(new NpgsqlParameter("gender", NpgsqlTypes.NpgsqlDbType.Text));
                sqlCmd.Parameters.Add(new NpgsqlParameter("color", NpgsqlTypes.NpgsqlDbType.Text));
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
                sqlCmd.Parameters["color"].Value = animal.Color;
                sqlCmd.Parameters["dateBirth"].Value = animal.BirthDate;
                sqlCmd.Parameters["dateDeath"].Value = animal.DeathDate != null ? animal.DeathDate : DBNull.Value;
                sqlCmd.Parameters["isSterilized"].Value = animal.IsSterilized;
                sqlCmd.Parameters["dateSterilization"].Value = animal.DateSterilization != null ? animal.DateSterilization : DBNull.Value;
                sqlCmd.Parameters["particularity"].Value = animal.Particularity != null ? animal.Particularity : DBNull.Value;
                sqlCmd.Parameters["description"].Value = animal.Description != null ? animal.Description : DBNull.Value;
                
                // Parametized statement execution
                int createOp = sqlCmd.ExecuteNonQuery();

                if (createOp == 0) throw new Exception("Impossible d'ajouter l'animal!");

                result = GetAnimal(animal.Name);

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

        public Animal CreateCompatibility(Animal animal, Compatibility compatibility)
        {
            throw new NotImplementedException();
        }

        public Animal? GetAnimal(string name)
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
                    WHERE "Name" = :name
                    """,
                    SqlConn
                );

                sqlCmd.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Text);

                sqlCmd.Prepare();

                sqlCmd.Parameters["name"].Value = name;

                reader = sqlCmd.ExecuteReader();

                if (reader.Read())
                {
                    Debug.WriteLine($"reader - birthDate : {reader["BirthDate"]}");
                    DateOnly? birthDate = reader["BirthDate"] != DBNull.Value ? (DateOnly) reader["BirthDate"] : null; 
                    DateOnly? deathDate = reader["DeathDate"] != DBNull.Value ? (DateOnly) reader["DeathDate"] : null;
                    DateOnly? dateSterilization = reader["dateSterilization"] != DBNull.Value ? (DateOnly)  reader["dateSterilization"] : null;

                    //bool birthDateCorrect = DateOnly.TryParse((string) reader["BirthDate"], out birthDate);                    


                    //bool deathDateCorrect = DateOnly.TryParse((string) reader["DeathDate"], out deathDate);

                    //bool dateSterilizationCorrect = DateOnly.TryParse((string) reader["dateSterilization"], out dateSterilization);
                    

                    result = new Animal(
                        Convert.ToString(reader["Id"])!,
                        Convert.ToString(reader["Name"])!,
                        MyEnumHelper.GetEnumFromDescription<AnimalType>(Convert.ToString(reader["Type"])!),
                        MyEnumHelper.GetEnumFromDescription<GenderType>(Convert.ToString(reader["Gender"])!),
                        Convert.ToString(reader["Color"])!,
                        birthDate,
                        deathDate,
                        Convert.ToBoolean(reader["IsSterilized"])!,
                        dateSterilization,
                        Convert.ToString(reader["Particularity"])!,
                        Convert.ToString(reader["Description"])!

                    );

                    reader.Close();
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
                        "Color" = :color,
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
                sqlCmd.Parameters.Add(new NpgsqlParameter("gender", NpgsqlTypes.NpgsqlDbType.Text));
                sqlCmd.Parameters.Add(new NpgsqlParameter("color", NpgsqlTypes.NpgsqlDbType.Text));
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
                sqlCmd.Parameters["color"].Value = animal.Color;
                sqlCmd.Parameters["dateBirth"].Value = animal.BirthDate;
                sqlCmd.Parameters["dateDeath"].Value = animal.DeathDate != null ? animal.DeathDate : DBNull.Value;
                sqlCmd.Parameters["isSterilized"].Value = animal.IsSterilized;
                sqlCmd.Parameters["dateSterilization"].Value = animal.DateSterilization != null ? animal.DateSterilization : DBNull.Value;
                sqlCmd.Parameters["particularity"].Value = animal.Particularity != null ? animal.Particularity : DBNull.Value;
                sqlCmd.Parameters["description"].Value = animal.Description != null ? animal.Description : DBNull.Value;

                // Parametized statement execution
                int nbRowAffected = sqlCmd.ExecuteNonQuery();

                if (nbRowAffected == 0) throw new AccessDbException(sqlCmd.CommandText, $"Unable to update animal with id {animal.Id}! No row affected!");

                result = GetAnimal(animal.Name);
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


        public bool CreateCompatibility(Compatibility compatibility)
        {
            bool result = false;
            NpgsqlCommand? sqlCmd = null;

            try
            {
                sqlCmd = new NpgsqlCommand(
                    $"""
                    INSERT INTO public."Compatibilities" ("Id", "Type", "Value", "Description", "AnimalId")
                    VALUES (:id, :type, :value, :description, :animalId)

                    """,
                    this.SqlConn
                );

                sqlCmd.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Uuid));
                sqlCmd.Parameters.Add(new NpgsqlParameter("type", NpgsqlTypes.NpgsqlDbType.Text));
                sqlCmd.Parameters.Add(new NpgsqlParameter("value", NpgsqlTypes.NpgsqlDbType.Text));
                sqlCmd.Parameters.Add(new NpgsqlParameter("description", NpgsqlTypes.NpgsqlDbType.Text));
                sqlCmd.Parameters.Add(new NpgsqlParameter("animalId", NpgsqlTypes.NpgsqlDbType.Text));

                sqlCmd.Prepare();

                sqlCmd.Parameters["id"].Value = compatibility.Id;
                sqlCmd.Parameters["type"].Value = compatibility.Type;
                sqlCmd.Parameters["value"].Value = compatibility.Value;
                sqlCmd.Parameters["description"].Value = compatibility.Description;
                sqlCmd.Parameters["animalId"].Value = compatibility.AnimalId;

                int nbRowAffected = sqlCmd.ExecuteNonQuery();

                if (nbRowAffected == 0) throw new AccessDbException(sqlCmd.CommandText, $"Unable to create a compatibility with an animal in Db.\nObject\n{compatibility}");

                result = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to create a compatibility with an animal.\nReason : {ex.Message}.\nObject :\n{compatibility}\nException:\n{ex}");
                throw;
            }

            return result;
        }
    }
}

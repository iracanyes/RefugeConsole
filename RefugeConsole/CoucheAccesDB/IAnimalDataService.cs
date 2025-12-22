using Npgsql;
using RefugeConsole.ClassesMetiers.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace RefugeConsole.CoucheAccesDB
{
    internal interface IAnimalDataService
    {
        Animal CreateAnimal(Animal animal);

        List<Animal> GetAnimalByName(string name);

        Animal? GetAnimalById(string id);

        Animal UpdateAnimal(Animal animal);

        bool RemoveAnimal(Animal animal);

        bool CreateCompatibility(Compatibility compatibility, NpgsqlTransaction transaction);

        bool CreateAnimalCompatibility(AnimalCompatibility animalCompatibility);

        HashSet<Compatibility> GetCompatibilities();

        HashSet<Color> GetColors();

        bool CreateAnimalColor(AnimalColor animalColor, NpgsqlTransaction? transaction = null);
    }
}

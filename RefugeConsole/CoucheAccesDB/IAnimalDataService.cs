using RefugeConsole.ClassesMetiers.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace RefugeConsole.CoucheAccesDB
{
    internal interface IAnimalDataService
    {
        Animal CreateAnimal(Animal animal);

        Animal? GetAnimal(string name);

        Animal UpdateAnimal(Animal animal);

        bool RemoveAnimal(Animal animal);

        Animal CreateCompatibility(Animal animal, Compatibility compatibility);

        
    }
}

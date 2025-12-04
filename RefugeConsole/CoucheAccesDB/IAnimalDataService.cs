using RefugeConsole.ClassesMetiers.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace RefugeConsole.CoucheAccesDB
{
    internal interface IAnimalDataService
    {
        Animal CreateAnimal(Animal animal);

        Animal GetAnimal(string name);

        bool UpdateAnimal(Animal animal);

        bool RemoveAnimal(Animal animal);

        Animal AddCompatibility(Animal animal, Compatibility compatibility);

        
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace RefugeConsole.CouchePresentation.ViewModel.Enums
{
    internal enum AnimalMenu
    {
        [Description("Consulter un animal")]
        ViewAnimal = 1,
        [Description("Ajouter un animal")] 
        AddAnimal = 2,        
        [Description("Supprimer un animal")]
        DeleteAnimal = 3,
        [Description("Mettre à jour un animal")]
        UpdateAnimal = 4,
        [Description("Retour menu principal")]
        Exit = 5,
        Unkown = 0,


    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace RefugeConsole.CouchePresentation.ViewModel.Enums
{
    internal enum ContactMenu
    {
        [Description("Consulter une personne de contact")]
        ViewContact = 1,
        [Description("Ajouter une personne de contact")]
        AddContact = 2,        
        [Description("Mettre à jour une personne de contact")]
        UpdateContact = 3,
        [Description("Supprimer une personne de contact")]
        DeleteContact = 4,
        [Description("Retour menu principal")]
        Exit = 5,
        Unknown = 0
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace RefugeConsole.CouchePresentation.ViewModel.Enums
{
    internal enum RefugeMenu
    {
        [Description("Lister les animaux présents au refuge (toutes les entrées)")]
        ListAdmissions = 1,
        [Description("Lister les familles d'accueil par où l'animal est passé")]
        ListAnimalPastFosterFamilies = 2,
        [Description("Lister les animaux accueillis par une famille d'accueil")]
        ListReleaseInFosterFamily = 3,
        [Description("Ajouter une famille d'accueil à un animal")]
        ReleaseToFosterFamily = 4,
        [Description("Lister les adoptions et leur statut")]
        ListAdoptionWithStatus = 5,
        [Description("Ajouter une adoption (Candidature)")]
        AddAdoption = 6,
        [Description("Valider une adoption")]
        ReleaseForAdoption = 7,
        [Description("Modifier le statut d'une adoption")]
        UpdateAdoptionStatus = 8,
        [Description("Ajouter un vaccin à un animal")]
        AddVaccination = 9,
        [Description("Retour menu principal")]
        Exit = 10,
        Unkown = 0,

    }
}

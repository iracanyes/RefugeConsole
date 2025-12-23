using RefugeConsole.ClassesMetiers.Helper;
using RefugeConsole.CoucheAccesDB;
using RefugeConsole.CouchePresentation.ViewModel;
using RefugeConsole.CouchePresentation.ViewModel.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace RefugeConsole.CouchePresentation.View
{
    internal class MenuView
    {
        private static readonly AnimalViewModel animalViewModel;
        private static readonly ContactViewModel contactViewModel;
        private static readonly RefugeViewModel refugeViewModel;

        static MenuView()
        {
            AnimalDataService animalDataService = new AnimalDataService();
            ContactDataService contactDataService = new ContactDataService();
            animalViewModel = new AnimalViewModel(animalDataService);
            contactViewModel = new ContactViewModel(contactDataService);
            refugeViewModel = new RefugeViewModel(animalViewModel, contactViewModel, animalDataService, contactDataService, new RefugeDataService());
        }

        public static void Display()
        {
            TopMenu topMenuSelection;
            bool exit = false;

            do
            {
                Console.Clear();
                MenuHelper.DisplayMenu<TopMenu>();

                topMenuSelection = MenuHelper.GetMenuChoices<TopMenu>();

                Console.WriteLine("Vous avez choisi ({0})", MyEnumHelper.GetEnumDescription(topMenuSelection));

                if (topMenuSelection == TopMenu.Unkown) continue;

                switch (topMenuSelection) {
                    case TopMenu.Animal:
                        ShowAnimalMenu();
                        break;
                    case TopMenu.Contact:
                        ShowContactMenu();
                        break;
                    case TopMenu.Refuge:
                        ShowRefugeMenu();
                        break;
                    default:
                        return;
                }
            } while (topMenuSelection != TopMenu.Exit || exit);
        }

        public static void ShowAnimalMenu() {
            // Afficher le menu
            Console.Clear();

            MenuHelper.DisplayMenu<AnimalMenu>();
            AnimalMenu selection = MenuHelper.GetMenuChoices<AnimalMenu>();

            switch (selection) {
                case AnimalMenu.AddAnimal:
                    refugeViewModel.AddAdmission();
                    break;
                case AnimalMenu.ViewAnimal:
                    animalViewModel.ViewAnimal();
                    break;
                case AnimalMenu.UpdateAnimal:
                    animalViewModel.UpdateAnimal();
                    break;
                case AnimalMenu.DeleteAnimal:
                    animalViewModel.RemoveAnimal();
                    break;
                case AnimalMenu.Exit:
                default:
                    return;
            }
        }

        public static void ShowContactMenu() {
            // Afficher le menu
            Console.Clear();

            MenuHelper.DisplayMenu<ContactMenu>();
            ContactMenu selection = MenuHelper.GetMenuChoices<ContactMenu>();

            switch (selection)
            {
                case ContactMenu.AddContact:
                    contactViewModel.AddContact();
                    break;
                case ContactMenu.ViewContact:
                    contactViewModel.ViewContact();
                    break;
                case ContactMenu.UpdateContact:
                    contactViewModel.UpdateContact();
                    break;
                case ContactMenu.DeleteContact:
                    contactViewModel.RemoveContact();
                    break;
                case ContactMenu.Exit:
                default:
                    return;
            }
        }

        public static void ShowRefugeMenu() {
            // Afficher le menu
            Console.Clear();

            MenuHelper.DisplayMenu<RefugeMenu>();
            RefugeMenu selection = MenuHelper.GetMenuChoices<RefugeMenu>();

            switch (selection)
            {
                case RefugeMenu.ListAdmissions:
                    refugeViewModel.ListAdmissions();
                    break;
                case RefugeMenu.ListAnimalPastFosterFamilies:
                    refugeViewModel.ListAnimalPastFosterFamilies();
                    break;
                case RefugeMenu.ListReleaseInFosterFamily:
                    refugeViewModel.ListReleaseInFosterFamily();
                    break;
                case RefugeMenu.AddCandidate:
                    refugeViewModel.AddCandidate();
                    break;
                case RefugeMenu.ReleaseToFosterFamily:
                    refugeViewModel.ReleaseToFosterFamily();
                    break;
                case RefugeMenu.ListReleaseAdoptionWithStatus:
                    refugeViewModel.ListReleaseAdoptionWithStatus();
                    break;
                case RefugeMenu.ReleaseForAdoption:
                    refugeViewModel.ReleaseForAdoption();
                    break;
                case RefugeMenu.UpdateReleaseForAdoption:
                    refugeViewModel.UpdateReleaseForAdoption();
                    break;
                case RefugeMenu.AddVaccination:
                    refugeViewModel.AddVaccination();
                    break;
                case RefugeMenu.Exit:
                default:
                    return;
            }
        }
    }
}

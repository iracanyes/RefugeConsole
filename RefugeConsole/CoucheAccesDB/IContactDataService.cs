using Npgsql;
using RefugeConsole.ClassesMetiers.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace RefugeConsole.CoucheAccesDB
{
    internal interface IContactDataService
    {

        /**
         * <summary>
         *  Ajouter une adresse
         * </summary>
         */
        bool CreateAddress(Address address, NpgsqlTransaction? transaction = null);

        /**
         * <summary>
         *  Ajouter un rôle à la personne de contact
         * </summary>
         */
        bool CreateContactRole(ContactRole contactRole, NpgsqlTransaction transaction);

        /**
         * <summary>
         *  Récupérer une personne de contact par son numéro de registre national
         * </summary>
         */
        Contact GetContactByRegistryNumber(string registryNumber);

        /**
         * <summary>
         *  Ajouter une personne de contact
         * </summary>
         */
        Contact CreateContact(Contact contact);

        /**
          * <summary>
          *   Mettre à jour une adresse
          * </summary>
          */
        bool UpdateAddress(Address address, NpgsqlTransaction? transaction = null);

        /**
         * <summary>
         *  Mettre à jour les informations de contact 
         * </summary>
         */
        Contact UpdateContact(Contact contact);

        /**
         * <summary>
         *  Supprimer un rôle pour une personne de contact
         * </summary>
         */
        bool DeleteContactRole(ContactRole contactRole, NpgsqlTransaction? transaction = null);

        /**
         * <summary>
         *  Supprimer une personne de contact
         * </summary>
         */
        bool DeleteContact(Contact contact);

        /**
          * <summary>
          *     Récupérer la liste des rôles pour les personnes de contact
          * </summary>
          */
        HashSet<Role> GetRoles();

        /**
          * <summary>
          *    Récupérer la liste des rôles pour un contact
          * </summary>
          */
        HashSet<ContactRole> GetContactRoles(Contact contact);

        /**
          * <summary>
          *     Vérifie que le numéro de registre national existe en base de donnée
          * </summary>
          */
        bool RegistryNumberExists(string registryNumber);

        

    }
}

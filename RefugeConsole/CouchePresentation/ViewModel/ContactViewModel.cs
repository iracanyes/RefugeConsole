using Microsoft.Extensions.Logging;
using RefugeConsole.ClassesMetiers.Model.Entities;
using RefugeConsole.CoucheAccesDB;
using RefugeConsole.CouchePresentation.View;
using System;
using System.Collections.Generic;
using System.Text;

namespace RefugeConsole.CouchePresentation.ViewModel
{
    internal class ContactViewModel
    {
        private static readonly ILogger MyLogger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger(nameof(ContactViewModel));
        private readonly IContactDataService contactDataService;

        public ContactViewModel(IContactDataService contactDataService)
        {
            this.contactDataService = contactDataService;
        }

        public void ViewContact() {
            
        }

        public void AddContact() {
            try
            {
                Contact contact = ContactView.AddContact();


            }catch(Exception ex)
            {
                MyLogger.LogError("Error while creating a contact! Reason : {0}", ex.Message);
            }

            
        }

        public void UpdateContact() { }

        public void RemoveContact() { }
    }
}

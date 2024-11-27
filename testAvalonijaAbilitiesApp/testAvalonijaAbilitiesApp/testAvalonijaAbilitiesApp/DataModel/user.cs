using System;

namespace testAvalonijaAbilitiesApp.DataModel
{
    public class user // mainly created for future additions
    {
        public string Username { get; set; }
        public int User_ID { get; set; }

        public DateTime createdAt { get; set; } 

        public user(string currentUsername, int currentUserID, DateTime dateOfCreation)
        {
            Username = currentUsername;
            User_ID = currentUserID;
            createdAt = dateOfCreation;
        }
    }
}

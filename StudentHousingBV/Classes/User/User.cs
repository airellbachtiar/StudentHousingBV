using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StudentHousingBV.Classes.Scheduler;

namespace StudentHousingBV.Classes.User
{
    public abstract class User
    {
        private static int currentID = 1;

        public string name { get; set; } 
        public int userID { get; }
        public string password { get; set; }
        public string displayName { get; set; }

        public User(string name, string password)
        {
            this.name = name;
            this.password = password;
            this.userID = currentID++; // Ups the id
            this.displayName = $"{this.name} #{userID}";
        }

        // Using the id would be better instead of using the name. Try to change this later.
        // Later let's find a solution for users for the same name.
        // Such as first and last name or id before the name and splitting the string.
        public bool CheckLogin(string displayName, string password)
        {
            if (this.displayName.Equals(displayName) && this.password.Equals(password))
                return true;
            else
                return false;
        }

        public bool Equals(User user)
        {
            if (this.userID == user.userID)
                return true;
            return false;
        }

        public bool isStudent()
        {
            if (this.GetType() == typeof(Student))
                return true;
            return false;
        }

        // Checking if the new name is okay to be saved.
        public bool changeName(string newName)
        {
            bool containsIn = newName.Any(char.IsDigit);   // If there is a digit, bool = true
            if ((string.IsNullOrWhiteSpace(newName) == true) || (containsIn == true)) // The name can't contain digits and can't be empty
            {
                return false;
            }
            else
            {
                this.name = newName;
                this.displayName = $"{name} #{userID}";
                return true;
            }
        }
    }
}

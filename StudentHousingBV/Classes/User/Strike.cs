using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentHousingBV.Classes.House
{
    public class Strike
    {
        private static int currentID = 1;

        public User.Student student { get; }
        public strikeCategory category { get; set; }
        public string description { get; set; }
        public int id { get; }

        public Strike(strikeCategory category, string description, User.Student student)
        {
            this.student = student;
            this.category = category;
            this.description = description;
            this.id = UpId();
        }

        private int UpId()
        {
            return currentID++;
        }

        public string GetInfo()
        {
            string output = $"Student: {this.student.name}\nCategory: {this.category}\nDescription: {this.description}";
            return output;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StudentHousingBV.Classes.User;
using StudentHousingBV.Classes.House;

namespace StudentHousingBV.Classes.Report
{
    // Maybe we can get rid of strikes and just use the complaints as strikes as well.
    // Assigning complaints to students which count as strikes. Too many complaints will lead to the same consequences
    // as too many strikes.
    // The class is exactly the same.

    public class Complaint
    {
        private static int currentID = 1;

        public string comment { get; set; }
        public complaintCategory category { get; set; }
        public int id { get; }
        public bool anonymous { get; set; }
        public Student student { get; set; }
        public Student targetStudent { get; set; }
        public char type { get; set; }  // Type A: a general complaint , Type B: A complaint regarding a student


        public Complaint(string comment, complaintCategory category, bool anonymous, Student student, Student targetStudent)
        {
            this.student = student;
            this.comment = comment;
            this.category = category;
            this.anonymous = anonymous;
            this.targetStudent = targetStudent;
            this.id = UpId();

            this.type = 'B';
            if(this.targetStudent == null)
                this.type = 'A';
        }

        public Complaint(string comment, complaintCategory category, bool anonymous, Student student) : this(comment, category, anonymous,student, null)
        {

        }

        private int UpId()
        {
            return currentID++;
        }
    }
}

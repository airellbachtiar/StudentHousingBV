using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StudentHousingBV.Classes.User;

namespace StudentHousingBV.Classes.Report
{
    public class Report
    {
        public List<Complaint> complaints { get; set; }

        public Report()
        {
            this.complaints = new List<Complaint>();
        }

        // Change this later so that you check if the strings empty or not before.
        public bool AddCompaint(Complaint complaint)
        {
            if (CheckComplaint(complaint) == true)
            {
                this.complaints.Add(complaint);
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool CheckComplaint(Complaint complaint)
        {
            if (string.IsNullOrWhiteSpace(complaint.comment))
                return false; // The comment is empty, which is not allowed
            else
                return true; // Everything is okay
        }

        public void RemoveComplaint(Complaint complaint)
        {
            this.complaints.Remove(complaint);
        }

        public Complaint GetComplaint(int id)
        {
            //using indexOf maybe easier
            foreach(Complaint c in complaints)
            {
                if (id == c.id)
                    return c;
            }
            return null;
        }

        // Send true if the logged user is a targetstudent or if the complaint is a general complaint
        public bool ComplaintVisibility(Complaint complaint, Student loggedUser)
        {
            bool visible = true;
            if (complaint.type.Equals('B') && (complaint.targetStudent.Equals(loggedUser) == false))
                visible = false;
            return visible;
        }
    }
}

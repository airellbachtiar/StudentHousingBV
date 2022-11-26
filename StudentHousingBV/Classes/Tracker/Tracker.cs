using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using StudentHousingBV.Classes.User;

namespace StudentHousingBV.Classes.Tracker
{
    public static class Tracker
    {
        public static void UpdateBalance(List<User.User> users, List<Student> deactivevatedStudents)
        {
            List<Student> allStudents = new List<Student>();
            foreach(User.User u in users)
            {
                if (u.isStudent())
                {
                    allStudents.Add((Student)u);
                }
            }
            allStudents.AddRange(deactivevatedStudents);

            foreach(Student student in allStudents)
            {
                student.balance = 0;
            }

            foreach(Student s in allStudents)
            {
                foreach(Item i in s.items)
                {
                    double balanceChange = Math.Round((i.price * i.amount) / (double)(i.studentNeedToPay.Count() + 1), 2);
                    s.balance += balanceChange * (double)(i.studentNeedToPay.Count());
                        
                    foreach (Student studentThatOwes in i.studentNeedToPay)
                    {
                        studentThatOwes.balance -= balanceChange;
                    }
                }
                s.balance += s.paidBalance;
            }
        }
        
        public static List<Student> GetStudentsNeedToPay(Student itemOwner, List<User.User> users)
        {
            List<Student> studentsNeedToPay = new List<Student>();
            foreach(User.User user in users)
            {
                if(user.isStudent() && !user.Equals(itemOwner))
                {
                    studentsNeedToPay.Add((Student)user);
                }
            }
            return studentsNeedToPay;
        }

        // You can use the getStudent method. use this method before you use this method
        public static bool PayUser(Student payer , double money, Student userToPay)
        {
            bool isPaid = false; // Paying the user didn't work

            if(userToPay.balance >= money)
            {
                userToPay.paidBalance -= money;
                payer.paidBalance += money;
                isPaid = true;
            }

            return isPaid; // The user is paid or not
        }

        public static ListViewItem CreateItemLVI(Item item, Student student)
        {
            ListViewItem temp = new ListViewItem(item.id.ToString());
            temp.SubItems.Add(item.name);
            temp.SubItems.Add(item.price.ToString());
            temp.SubItems.Add(item.amount.ToString());
            temp.SubItems.Add(student.displayName);

            return temp;
        }

    }
}

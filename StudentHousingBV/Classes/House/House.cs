using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StudentHousingBV.Classes.User;
using StudentHousingBV.Classes.Tracker;
using StudentHousingBV.Classes.Scheduler;
using StudentHousingBV.Classes.Report;

namespace StudentHousingBV.Classes.House
{
    public class House
    {
        public List<Room> rooms { get; }
        public List<User.User> users { get; }
        public List<Student> deactivatedStudents { get; }

        public int numOfStudents { get; set; }

        public House()
        {
            rooms = new List<Room>();
            users = new List<User.User>();
            deactivatedStudents = new List<Student>();
            this.numOfStudents = 0;
        }

        public void AddUser(User.User user)
        {
            users.Add(user);
            if (user.isStudent())
                this.numOfStudents++;
        }

        //Remove User method
        public void RemoveUser(Student student)
        {
            users.Remove(student);
            this.deactivatedStudents.Add(student);
            this.numOfStudents--;
        }

        //Room is different.
        public void AddRoom(Room room)
        {
            rooms.Add(room);
        }

        public Student GetStudent(string displayName)
        {
            foreach(User.User u in this.users)
            {
                if (u.displayName.Equals(displayName))
                {
                    return ((Student)u);
                }
            }
            return null;
        }

        public Room GetRoom(RoomType roomName)
        {
            foreach(Room r in this.rooms)
            {
                if (r.name.Equals(roomName))
                {
                    return r;
                }
            }
            return null;
        }

        public Room GetRoom(int roomId)
        {
            foreach(Room r in this.rooms)
            {
                if(r.id == roomId)
                {
                    return r;
                }
            }
            return null;
        }

        // Won't work if you add a toilet, storage room
        public List<Room> AvailableBedroom()
        {
            List<Room> available = new List<Room>();
            foreach (Room r in rooms)
            {
                if (!r.isReserveable && !r.isOccupied)
                {
                    available.Add(r);
                }
            }
            return available;
        }
    }
}

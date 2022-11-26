using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using StudentHousingBV.Classes.House;
using StudentHousingBV.Classes.User;
using StudentHousingBV.Classes.Tracker;
using StudentHousingBV.Classes.Scheduler;
using StudentHousingBV.Classes.Report;
using System.Globalization;

namespace StudentHousingBV
{
    public partial class AdminForm : Form
    {
        private House house;
        private Admin loggedUser;
        private Scheduler scheduler;
        private Report report;

        private Complaint curSelComplaint; //Current selected complaint
        private Student curStudentInfo; // Current shown student in info
        private Strike curSelStrike; // Curren selected Strike

        private bool showAll = true; // Show all dates
        private DateTime currentShownDate; // Show only the reservations with this date

        public AdminForm(House house, Admin loggedUser, Scheduler scheduler, Report report)
        {
            InitializeComponent();


            this.house = house;
            this.loggedUser = loggedUser;
            this.scheduler = scheduler;
            this.report = report;

            reportSetup();  // Report
            infoSetup();    // Info
            strikeSetup(); // Strike
            schedulerSetup(); //Scheduler
            AddSetup(); // add setup
            trackerSetup(); // tracker setup
        }

        #region Report System

        public void reportSetup()
        {
            updateComplaint();
        }

        //Copy paste from Tenant form
        // Possibility to use the method from that form?
        public void updateComplaint()
        {
            lsvReportComplaints.Items.Clear();
            rtxtComplaintSelectedComment.Clear();

            foreach (Complaint c in report.complaints)
            {
                string targetStudentName = "None";
                if (c.type.Equals('B')) // B: complaint with target student
                    targetStudentName = c.targetStudent.displayName;

                string studentName = c.student.displayName;
                if (c.anonymous) // Anon or not
                    studentName = "Anonymous";

                ListViewItem complaint = new ListViewItem(c.id.ToString()); // First subitem is the id for the complaint
                complaint.SubItems.Add(studentName);
                complaint.SubItems.Add(targetStudentName);
                complaint.SubItems.Add(c.category.ToString());

                lsvReportComplaints.Items.Add(complaint);
            }
        }

        // The same method as in tenantform
        private void lsvReportComplaints_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lsvReportComplaints.SelectedItems.Count > 0)
            {
                int selectedIndex = lsvReportComplaints.SelectedIndices[0]; // The index of the current selected item
                int cId = int.Parse(lsvReportComplaints.Items[selectedIndex].SubItems[0].Text); // id of the Complaint

                rtxtComplaintSelectedComment.Text = report.GetComplaint(cId).comment;
                this.curSelComplaint = report.GetComplaint(cId); // Used for removing complaints
            }
        }

        private void btnReportRemoveComplaint_Click(object sender, EventArgs e) // Remove a complaint
        {
            int messageCode = 11; // Succesfull message code
            if (curSelComplaint != null)
            {
                report.RemoveComplaint(curSelComplaint);
                rtxtComplaintSelectedComment.Clear();
                updateComplaint();
            }
            else
            {
                messageCode = 10; // Error message code: No complaint selected
            }

            Messages(messageCode);
        }

        #endregion

        #region Info System

        public void infoSetup()
        {
            // Adding the users in selection
            updateInfoUserSelection(); // Sends a bool to say this is the start up

            if (house.numOfStudents > 0)
            {
                cboxInfoUserSelection.SelectedIndex = 0; // If state where you check if there are any students in the program
                curStudentInfo = house.GetStudent(cboxInfoUserSelection.Text); // Saving the current student in the info page

                updateInfo();
            }
            else
            {
                txtInfoName.ReadOnly = true;
                txtInfoRoom.ReadOnly = true;
                btnInfoChange.Enabled = false;
                btnInfoSave.Enabled = false;
                btnInfoRemoveStrike.Enabled = false;
                btnInfoRemoveUser.Enabled = false;

                txtInfoName.Clear();
                txtInfoRoom.Clear();
                txtInfoBalance.Clear();
                cboxInfoStrike.Items.Clear();
                rtxtInfoStrike.Clear();
            }
        }

        //Maybe just only change the string instead of clearing and then adding everything anew
        public void updateInfoUserSelection()
        {
            int tempUserIndex = cboxInfoUserSelection.SelectedIndex; // Saving the index at which student you were at
            int tempUserCount = cboxInfoUserSelection.Items.Count;

            // Adding the users in selection
            cboxInfoUserSelection.Items.Clear();
            foreach (User u in this.house.users)
            {
                if (u.isStudent())
                    cboxInfoUserSelection.Items.Add(u.displayName);
            }

            // Maybe if else with if non in the house then clear it.
            int currentUserCount = house.numOfStudents;
            if (currentUserCount == 0)
            {
                // Nothing happens, because the combobox should be empty
            }
            else if (currentUserCount != tempUserCount)
            {
                cboxInfoUserSelection.SelectedIndex = 0; // User count changes so go to the first student in the list
            }
            else
            {
                cboxInfoUserSelection.SelectedIndex = tempUserIndex; // No changes to the user count, so the combobox should stay at the current shown Student
            }
        }

        public void updateInfo()
        {
            txtInfoName.Text = curStudentInfo.name;
            txtInfoRoom.Text = curStudentInfo.roomId.ToString();
            txtInfoBalance.Text = curStudentInfo.balance.ToString();

            updateInfoStrikeSelection();

            if (this.curStudentInfo.strikes.Count() < 1)
                btnInfoRemoveStrike.Enabled = false;
            else
                btnInfoRemoveStrike.Enabled = true;
        }

        public void updateInfoStrikeSelection()
        {
            cboxInfoStrike.Items.Clear();
            rtxtInfoStrike.Clear();
            if (this.curStudentInfo.strikes.Count() > 0)
            {
                foreach (Strike s in this.curStudentInfo.strikes) // Adding the strikes from the corresponding Student
                {
                    cboxInfoStrike.Items.Add(s.id.ToString()); // Change this later to show a name or "Strike: {this.id}"
                }
                cboxInfoStrike.SelectedIndex = 0;
            }
        }

        public void updateInfoStrikeComment()
        {
            if (this.curStudentInfo.strikes.Count() > 0)
            {
                this.curSelStrike = this.curStudentInfo.GetStrike(int.Parse(cboxInfoStrike.Text));

                rtxtInfoStrike.Text = curSelStrike.GetInfo();
            }

        }

        private void btnInfoChange_Click(object sender, EventArgs e)
        {
            txtInfoName.ReadOnly = false;
            txtInfoRoom.ReadOnly = false;
            btnInfoSave.Enabled = true;
        }

        // If we have time, let's make this monster cleaner
        private void btnInfoSave_Click(object sender, EventArgs e)
        {
            int messageCode = 20; // Succes code: All information is aved properly

            int roomId = -1;    // Saving the changed room number to compare later

            if (this.curStudentInfo.changeName(txtInfoName.Text) && ((string.IsNullOrWhiteSpace(txtInfoRoom.Text) == false) && int.TryParse(txtInfoRoom.Text, out roomId) == true)) // Changing the name and checkind the roomId field
            {
                if (this.house.AvailableBedroom().Contains(this.house.GetRoom(roomId)) == true || this.curStudentInfo.roomId == roomId) // If it's true it means the room is not taken
                {
                    int oldRoomId = ((Student)house.users[house.users.IndexOf(curStudentInfo)]).roomId;
                    if (roomId != oldRoomId)
                    {
                        house.GetRoom(oldRoomId).isOccupied = false; // The old room is now avaiable for other to get in
                        ((Student)house.users[house.users.IndexOf(curStudentInfo)]).roomId = roomId; // The new room is assigned to that user
                        house.GetRoom(roomId).isOccupied = true; // The new room isn't available to other anymore
                    }
                }
                else
                {
                    messageCode = 22; // Error code: Room taken or the room doesn't exist or is not occupiedable
                }
                updateInfoStrikeComment();  // Updating the name in the strike box
                updateInfoUserSelection();  // Updating the name in the user selection

                txtInfoName.ReadOnly = true;
                txtInfoRoom.ReadOnly = true;
                btnInfoSave.Enabled = false;
            }
            else
            {
                messageCode = 21; // Error code: Name and roomId has to be formatted right
            }

            //Update the program
            updateProgram();

            Messages(messageCode);
        }

        private void cboxInfoUserSelection_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.house.numOfStudents > 0)
            {
                curStudentInfo = house.GetStudent(cboxInfoUserSelection.Text); // Update the current shown student

                txtInfoName.ReadOnly = true;
                txtInfoRoom.ReadOnly = true;
                btnInfoSave.Enabled = false;
                btnInfoRemoveStrike.Enabled = true;
                btnInfoRemoveUser.Enabled = true;
                btnInfoChange.Enabled = true;

                updateInfo();
            }
        }

        private void cboxInfoStrike_SelectedIndexChanged(object sender, EventArgs e)
        {
            updateInfoStrikeComment(); // Also saves the new selected info to curSelStrike
        }

        private void btnInfoRemoveStrike_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete this Strike?", "Confirmation", MessageBoxButtons.YesNo);

            if (dialogResult == DialogResult.Yes)
            {
                this.curStudentInfo.RemoveStrike(curSelStrike);
                rtxtInfoStrike.Clear();
                updateInfo();
                Messages(24); // Succesfull code: Strike is removed
            }
            else if (dialogResult == DialogResult.No) { }
        }

        private void btnInfoRemoveUser_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show($"Are you sure you want to remove {curStudentInfo.displayName}?", "Remove User", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);

            if (dialogResult == DialogResult.Yes)
            {
                DialogResult dialogResult2 = MessageBox.Show($"Are you really sure you want to remove {curStudentInfo.displayName}? This cannot be undone.", "Remove User", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);

                if (dialogResult2 == DialogResult.Yes)
                {
                    Messages(25); // Succesfull code: User is removed
                    house.GetRoom(curStudentInfo.roomId).isOccupied = false;
                    house.RemoveUser(curStudentInfo);

                    // Information in the program have to be reloaded/refreshed
                    updateProgram();

                    // Resetup everything
                }
                else if (dialogResult2 == DialogResult.No) { }
            }
            else if (dialogResult == DialogResult.No) { }
        }

        #endregion

        #region Scheduler System

        public void schedulerSetup()
        {
            //cboxSchedulerCategory.Items.AddRange(Enum.GetValues(typeof(SchedulerCategory)).Cast<SchedulerCategory>().Select(x => x.ToString()).ToArray()); 
            foreach (SchedulerCategory s in Enum.GetValues(typeof(SchedulerCategory)))
            {
                cboxSchedulerCategory.Items.Add(s);
            }

            cboxSchedulerCategory.SelectedIndex = 0;

            cboxHour.SelectedIndex = 0;

            //Scheduler Room list
            foreach (Room r in house.rooms)
            {
                if (r.isReserveable == true) // Bedrooms shouldn't be able to be reserved
                    cboxSchedulerRoom.Items.Add(r.name);
            }
            cboxSchedulerRoom.SelectedIndex = 0;

            UpdateSchedulerListBox();
        }

        // Show the reservations in the listbox
        public void UpdateSchedulerListBox()
        {
            //update listbox
            lsvSchedulerReservations.Items.Clear();
            foreach (Reservation r in scheduler.reservations)
            {
                if (showAll == false)
                {
                    if (r.date.ToString(this.scheduler.DateFormat).Equals(currentShownDate.ToString(this.scheduler.DateFormat)))
                    {
                        lsvSchedulerReservations.Items.Add(createReservationItem(r)); // Adding only the reservations that are the same as the selected date to be shown
                    }
                }
                else
                {
                    lsvSchedulerReservations.Items.Add(createReservationItem(r)); // Adding every reservation to the listbox
                }
            }
        }

        // Maybe move this to the scheduler class for when you actually create the reservation
        public ListViewItem createReservationItem(Reservation r)
        {
            ListViewItem ReservationItem = new ListViewItem(r.id.ToString());
            ReservationItem.SubItems.Add(r.date.ToString(this.scheduler.DateFormat));
            ReservationItem.SubItems.Add(r.date.ToString("HH:mm"));
            ReservationItem.SubItems.Add(r.date.AddHours(r.duration).ToString("HH:mm"));
            ReservationItem.SubItems.Add(r.user.displayName.ToString());
            ReservationItem.SubItems.Add(r.room.name.ToString());
            ReservationItem.SubItems.Add(r.category.ToString());
            ReservationItem.SubItems.Add(r.status);

            return ReservationItem;
        }

        private void btnAddReservation_Click(object sender, EventArgs e)
        {

            DateTime schedulerDate = DateTime.ParseExact($"{dateTimePicker.Value.ToString(this.scheduler.DateFormat)} {cboxHour.SelectedItem}", $"{this.scheduler.DateFormat} HH:mm", System.Globalization.CultureInfo.InvariantCulture);//culture info can be null

            RoomType roomName = (RoomType)cboxSchedulerRoom.SelectedItem;
            Room room = this.house.GetRoom(roomName);
            SchedulerCategory schedulerCategory = (SchedulerCategory)cboxSchedulerCategory.SelectedItem;
            string schedulerDescription = txtSchedulerDescription.Text;
            int schedulerDuration = Convert.ToInt32(nudSchedulerDuration.Value);

            //If possible the reservation is added and a succes code will be given, else an errorCode
            int messageCode = this.scheduler.AddReservation(schedulerDate, this.loggedUser, schedulerCategory, schedulerDescription, room, schedulerDuration);
            txtSchedulerDescription.Clear();

            Messages(messageCode); // Show the right message to the user
            UpdateSchedulerListBox(); //The whole box should be updated.
        }

        private void btnSchedulerRemoveReservation_Click(object sender, EventArgs e)
        {
            int messageCode = 36; // Succesfull message code

            if (lsvSchedulerReservations.SelectedIndices.Count > 0)
            {
                int reservationId = int.Parse(lsvSchedulerReservations.Items[lsvSchedulerReservations.SelectedIndices[0]].SubItems[0].Text);
                if (scheduler.RemoveCheck(reservationId, loggedUser)) // True if it's your reservation
                {
                    scheduler.RemoveReservation(reservationId);
                    UpdateSchedulerListBox();
                }
                else
                {
                    messageCode = 35; // Error message: it's not the user's reservation
                }
            }
            else
            {
                messageCode = 34; // Error message: No reservation is selected to be removed
            }

            Messages(messageCode);
        }

        private void btnSchedulerShowAll_Click(object sender, EventArgs e)
        {
            this.showAll = true;
            scheduler.Refresh();
            UpdateSchedulerListBox();
        }

        private void lsvSchedulerReservations_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lsvSchedulerReservations.SelectedItems.Count > 0)
            {
                int id = int.Parse(lsvSchedulerReservations.Items[lsvSchedulerReservations.SelectedIndices[0]].SubItems[0].Text);
                rtbxSchedulerDescription.Text = scheduler.GetReservation(id).description;
            }
            else
            {
                rtbxSchedulerDescription.Clear();
            }
        }

        private void dateTimePicker_ValueChanged(object sender, EventArgs e)//show picked date
        {
            this.showAll = false;
            this.currentShownDate = dateTimePicker.Value;
            scheduler.Refresh();
            UpdateSchedulerListBox();
        }

        #endregion

        #region Strike System

        public void strikeSetup()
        {
            cboxStrikeStudent.Items.Clear();
            foreach (User u in house.users)
            {
                if (u.isStudent())
                {
                    cboxStrikeStudent.Items.Add(u.displayName);
                }
            }

            cboxStrikeCategory.Items.Clear();
            //cboxStrikeCategory.Items.AddRange(Enum.GetValues(typeof(strikeCategory)).Cast<strikeCategory>().Select(x => x.ToString()).ToArray());
            foreach (strikeCategory s in Enum.GetValues(typeof(strikeCategory)))
            {
                cboxStrikeCategory.Items.Add(s);
            }
            cboxStrikeCategory.SelectedIndex = 0;

            if (this.house.numOfStudents > 0)
            {
                cboxStrikeStudent.SelectedIndex = 0;
                cboxStrikeCategory.SelectedIndex = 0;
                cboxStrikeStudent.Enabled = true;
                cboxStrikeCategory.Enabled = true;
                rtxtStrikeCom.ReadOnly = false;
                btnAddStrike.Enabled = true;
            }
            else
            {
                cboxStrikeStudent.Enabled = false;
                cboxStrikeCategory.Enabled = false;
                rtxtStrikeCom.ReadOnly = true;
                btnAddStrike.Enabled = false;
            }
        }

        public void createStrike(strikeCategory category, String studentName, string comment)
        {
            DialogResult dialogResult = MessageBox.Show($"Are you sure you want to add a strike to: {studentName} with category: {category}", "Strike confirmation", MessageBoxButtons.YesNo);

            if (dialogResult == DialogResult.Yes)
            {
                Student targetStudent = this.house.GetStudent(studentName);
                Strike strike = new Strike(category, comment, targetStudent);
                targetStudent.AddStrike(strike);

                rtxtStrikeCom.Clear();
                updateInfo();
            }
            else if (dialogResult == DialogResult.No) { }
        }

        public bool checkStrikeCommentField(string comment)
        {
            if (string.IsNullOrWhiteSpace(comment))
                return true;
            else
                return false;
        }

        private void btnAddStrike_Click(object sender, EventArgs e)
        {
            int messageCode = 40; // Succesfull code: Everything worked fine
            strikeCategory category = (strikeCategory)cboxStrikeCategory.SelectedItem;
            string studentName = cboxStrikeStudent.Text;
            string comment = rtxtStrikeCom.Text;

            if (checkStrikeCommentField(comment) == false)
            {
                createStrike(category, studentName, comment);
            }
            else
            {
                messageCode = 41; // Error code : Comment is empty
            }

            Messages(messageCode);
        }

        #endregion

        #region Adding Student

        public void AddSetup()
        {
            updateAvailableBedroom();
        }

        //Airell added this monstrousity
        public void updateAvailableBedroom()
        {
            List<Room> availableRooms = this.house.AvailableBedroom();
            cboxAddStudentRoom.Items.Clear();

            if (availableRooms.Count() == 0)//No available bedroom
            {
                txtAddStudentName.Enabled = false;
                txtAddStudentPassword.Enabled = false;
                cboxAddStudentRoom.Enabled = false;
                btnAddStudentAdd.Enabled = false;
            }
            else //There's avaiable bedroom
            {
                txtAddStudentName.Enabled = true;
                txtAddStudentPassword.Enabled = true;
                cboxAddStudentRoom.Enabled = true;
                btnAddStudentAdd.Enabled = true;

                foreach (Room r in availableRooms)
                {
                    cboxAddStudentRoom.Items.Add(r.id);
                }
               // cboxAddStudentRoom.Items.AddRange(availableRooms.ToArray());

                cboxAddStudentRoom.SelectedIndex = 0;
            }
        }

        private void btnAddStudentAdd_Click(object sender, EventArgs e)
        {
            int messageCode = 50; // Succesfull code: User added

            String name = txtAddStudentName.Text;
            string password = txtAddStudentPassword.Text;

            bool containsIn = name.Any(char.IsDigit); // Checks if name contains numbers

            if ((string.IsNullOrWhiteSpace(name) == false) || (containsIn == false))
            {
                if (string.IsNullOrWhiteSpace(password) == false)
                {
                    int roomId = Convert.ToInt32(cboxAddStudentRoom.SelectedItem);

                    // If a user has the name same, some things are not going to work
                    house.AddUser(new Student(name, password, roomId));
                    house.GetRoom(roomId).isOccupied = true;

                    //Reset textbox and cbox
                    txtAddStudentName.Clear();
                    txtAddStudentPassword.Clear();
                    cboxAddStudentRoom.SelectedItem = null;

                    // Update everything
                    updateProgram();
                }
                else
                {
                    messageCode = 52; // Error code: Password can't be empty
                }
            }
            else
            {
                messageCode = 51; // Error code: Name is formatted incorrect
            }

            Messages(messageCode);
        }

        #endregion

        #region Tracker

        public void trackerSetup()
        {
            updateTrackerList();
        }
        void updateTrackerList()//display all item and users
        {
            Tracker.UpdateBalance(this.house.users, this.house.deactivatedStudents);

            // Adding the bought items to the list
            lsvTrackerItems.Items.Clear();
            lsbTrackerBalance.Items.Clear();

            foreach (User u in house.users)
            {
                if (u.isStudent())
                {
                    foreach (Item i in ((Student)u).items)
                    {
                        ListViewItem itemToStore = Tracker.CreateItemLVI(i, (Student)u);
                        lsvTrackerItems.Items.Add(itemToStore);
                    }
                    lsbTrackerBalance.Items.Add($"{u.displayName} - €{Convert.ToString(((Student)u).balance)}");    // Adding the balances of the users
                }
            }

            foreach (Student s in this.house.deactivatedStudents)
            {
                lsbTrackerDeactiveBalance.Items.Add($"{s.displayName} - €{Convert.ToString(s.balance)}");
            }

            if (this.house.numOfStudents > 0)
            {
                btnRemoveItem.Enabled = true;
            }
            else
            {
                btnRemoveItem.Enabled = false;
            }
        }

        private void btnRemoveItem_Click(object sender, EventArgs e)
        {
            int messageCode = 60; // Succesfull code: Item(amount) is removed

            if (lsvTrackerItems.SelectedItems.Count > 0)
            {
                int amountToRemove = Convert.ToInt32(nudRemoveAmount.Value);

                int selectedIndex = lsvTrackerItems.SelectedIndices[0]; // The index of the current selected item in the ListView
                int itemId = int.Parse(lsvTrackerItems.Items[selectedIndex].SubItems[0].Text); // The ID of the item

                Student owner = this.house.GetStudent(lsvTrackerItems.Items[selectedIndex].SubItems[4].Text);

                Item itemToRemove = owner.GetItem(itemId);

                if (amountToRemove <= itemToRemove.amount)
                {
                   // tracker.updateBalance(itemToRemove.price * amountToRemove * -1, owner, this.house.users, this.house.numOfStudents);

                    if (itemToRemove.amount == amountToRemove)
                    {
                        owner.RemoveItem(itemToRemove);
                    }
                    else
                    {
                        itemToRemove.amount -= amountToRemove;
                    }
                    updateTrackerList();
                }
                else
                {
                    messageCode = 62; // Error code: Amount is much to remove
                }
            }
            else
            {
                messageCode = 61; // Error Code: Select an item first
            }

            Messages(messageCode);
        }

        #endregion

        private void btnLogout_Click(object sender, EventArgs e)
        {
            int index = house.users.IndexOf(loggedUser);

            this.Hide();
            LoginForm form1 = new LoginForm(house, scheduler, report);
            form1.Closed += (s, args) => this.Close();
            form1.Show();
        }

        public void Messages(int i)
        {
            // Maybe make it so that in the 10s we get good messages and 20s error messages 
            // OR
            // 10-19 are for info messages, 20-29 for report and so forth
            string message = "";

            switch (i)
            {
                // Report Messages 10-19
                case 10:
                    message = "You need to select a complaint first in order to remove one!";
                    break;
                case 11:
                    message = "The complaint is succesfully removed!";
                    break;

                // Info Messages 20-29
                case 20:
                    message = "The information is succesfully saved!";
                    break;
                case 21:
                    message = "The name can't contain any numbers and the roomId has to be numeric.";
                    break;
                case 22:
                    message = "The room is either already occupied by someone else, doesn't exist or it isn't a bedroom!";
                    break;
                case 24:
                    message = "Strike is succcesfully removed!";
                    break;
                case 25:
                    message = $"{curStudentInfo.name} has been removed from the house!";
                    break;

                // Scheduler Messages 30-39
                case 30:
                    message = "Your reservation is added!";
                    break;
                case 31:
                    message = "This room already booked at this hour!";
                    break;
                case 32:
                    message = "Can't assign this category to this room!";
                    break;
                //case 33:
                //    message = "You have reach maximum strikes to book this place!";
                //    break;
                case 34:
                    message = "You have to select a reservation first in order to remove one!";
                    break;
                //case 35:
                //    message = "You can't remove this reservation! This one is not yours!";
                //    break;
                case 36:
                    message = "The reservation is succesfully removed!";
                    break;

                // Strike Messages 40-49
                case 40:
                    message = "Strike was succesfully added!";
                    break;
                case 41:
                    message = "The comment box can't be empty!";
                    break;

                // Add Students & Room Messages 50-59
                case 50:
                    message = "The student is succesfully added!";
                    break;
                case 51:
                    message = "The Student name can't be empty or numeric.";
                    break;
                case 52:
                    message = "The password can't empty!";
                    break;

                // Tracker Messages 60-69
                case 60:
                    message = "The item is succesfully removed!";
                    break;
                case 61:
                    message = "You need to select an item before trying to remove one!";
                    break;
                case 62:
                    message = "Trying to remove more than there actually is, is not possible!";
                    break;
            }

            MessageBox.Show(message);
        }

        public void updateProgram()
        {
            infoSetup();
            strikeSetup();
            updateComplaint();
            updateTrackerList();
            UpdateSchedulerListBox();
            updateAvailableBedroom();
        }

        #region Button Tabs
        private void btnShowScheduler_Click(object sender, EventArgs e)
        {
            pnlScheduler.BringToFront();
            lblTitle.Text = "SCHEDULER";
            btnShowScheduler.BackColor = Color.FromArgb(62, 76, 89);
            btnShowTracker.BackColor = Color.FromArgb(97, 110, 124);
            btnShowReport.BackColor = Color.FromArgb(97, 110, 124);
            btnShowInfo.BackColor = Color.FromArgb(97, 110, 124);
            btnShowStrike.BackColor = Color.FromArgb(97, 110, 124);
            btnShowAddStudent.BackColor = Color.FromArgb(97, 110, 124);
        }

        private void btnShowTracker_Click(object sender, EventArgs e)
        {
            pnlTracker.BringToFront();
            lblTitle.Text = "TRACKER";
            btnShowScheduler.BackColor = Color.FromArgb(97, 110, 124);
            btnShowTracker.BackColor = Color.FromArgb(62, 76, 89);
            btnShowReport.BackColor = Color.FromArgb(97, 110, 124);
            btnShowInfo.BackColor = Color.FromArgb(97, 110, 124);
            btnShowStrike.BackColor = Color.FromArgb(97, 110, 124);
            btnShowAddStudent.BackColor = Color.FromArgb(97, 110, 124);
        }

        private void btnShowReport_Click(object sender, EventArgs e)
        {
            pnlReport.BringToFront();
            lblTitle.Text = "REPORT";
            btnShowScheduler.BackColor = Color.FromArgb(97, 110, 124);
            btnShowTracker.BackColor = Color.FromArgb(97, 110, 124);
            btnShowReport.BackColor = Color.FromArgb(62, 76, 89);
            btnShowInfo.BackColor = Color.FromArgb(97, 110, 124);
            btnShowStrike.BackColor = Color.FromArgb(97, 110, 124);
            btnShowAddStudent.BackColor = Color.FromArgb(97, 110, 124);
        }

        private void btnShowInfo_Click(object sender, EventArgs e)
        {
            pnlInfo.BringToFront();
            lblTitle.Text = "INFO";
            btnShowScheduler.BackColor = Color.FromArgb(97, 110, 124);
            btnShowTracker.BackColor = Color.FromArgb(97, 110, 124);
            btnShowReport.BackColor = Color.FromArgb(97, 110, 124);
            btnShowInfo.BackColor = Color.FromArgb(62, 76, 89);
            btnShowStrike.BackColor = Color.FromArgb(97, 110, 124);
            btnShowAddStudent.BackColor = Color.FromArgb(97, 110, 124);
        }

        private void btnShowStrike_Click(object sender, EventArgs e)
        {
            pnlStrike.BringToFront();
            lblTitle.Text = "STRIKE";
            btnShowScheduler.BackColor = Color.FromArgb(97, 110, 124);
            btnShowTracker.BackColor = Color.FromArgb(97, 110, 124);
            btnShowReport.BackColor = Color.FromArgb(97, 110, 124);
            btnShowInfo.BackColor = Color.FromArgb(97, 110, 124);
            btnShowStrike.BackColor = Color.FromArgb(62, 76, 89);
            btnShowAddStudent.BackColor = Color.FromArgb(97, 110, 124);
        }

        private void btnShowAddStudent_Click(object sender, EventArgs e)
        {
            pnlAddStudent.BringToFront();
            lblTitle.Text = "ADD STUDENT";
            btnShowScheduler.BackColor = Color.FromArgb(97, 110, 124);
            btnShowTracker.BackColor = Color.FromArgb(97, 110, 124);
            btnShowReport.BackColor = Color.FromArgb(97, 110, 124);
            btnShowInfo.BackColor = Color.FromArgb(97, 110, 124);
            btnShowStrike.BackColor = Color.FromArgb(97, 110, 124);
            btnShowAddStudent.BackColor = Color.FromArgb(62, 76, 89);
        }

        private void lblCompany_Click(object sender, EventArgs e)
        {
            pnlHome.BringToFront();
            lblTitle.Text = "HOME";
            btnShowScheduler.BackColor = Color.FromArgb(97, 110, 124);
            btnShowTracker.BackColor = Color.FromArgb(97, 110, 124);
            btnShowReport.BackColor = Color.FromArgb(97, 110, 124);
            btnShowInfo.BackColor = Color.FromArgb(97, 110, 124);
            btnShowStrike.BackColor = Color.FromArgb(97, 110, 124);
            btnShowAddStudent.BackColor = Color.FromArgb(97, 110, 124);
        }
        #endregion

    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using MySql.Data.MySqlClient;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace CISv4
{
    public partial class CIS : Form
    {
        //------------------------------------------------------------------------------REGISTRATION--------------------------------------------------------------------------------------
        private bool isFormLoaded = false;

        public CIS()
        {
            InitializeComponent();
            YearLevel.SelectedIndexChanged += YearLevel_SelectedIndexChanged;
            Course.SelectedIndexChanged += Course_SelectedIndexChanged;
            Province.SelectedIndexChanged += Province_SelectedIndexChanged;
            Municipality.SelectedIndexChanged += Municipality_SelectedIndexChanged;

        }

        private void CIS_Load(object sender, EventArgs e)
        {
            LoadYearLevels();
            LoadCourses();
            LoadSections();
            LoadProvinces();
            StudentNumber.Leave += StudentNumber_Leave;
            DateofEnlistment.Value = DateTime.Today;
            DateofEnlistment.Enabled = false;

            LastName.TextChanged += LastName_TextChanged;
            FirstName.TextChanged += FirstName_TextChanged;
            MiddleName.TextChanged += MiddleName_TextChanged;
            Street.TextChanged += Street_TextChanged;
            StreetName.TextChanged += StreetName_TextChanged;
            Religion.TextChanged += Religion_TextChanged;
            PlaceofBirth.TextChanged += PlaceofBirth_TextChanged;
            Complexion.TextChanged += Complexion_TextChanged;
            FatherName.TextChanged += FatherName_TextChanged;
            MotherName.TextChanged += MotherName_TextChanged;
            FatherJob.TextChanged += FatherJob_TextChanged;
            MotherJob.TextChanged += MotherJob_TextChanged;
            Relationship.TextChanged += Relationship_TextChanged;
            EmergencyPerson.TextChanged += EmergencyPerson_TextChanged;
            FBName.Text = "fb.com/";
            FBName.SelectionStart = 7;

            LastName.KeyPress += OnlyLetters_KeyPress;
            FirstName.KeyPress += OnlyLetters_KeyPress;
            MiddleName.KeyPress += OnlyLetters_KeyPress;
            PlaceofBirth.KeyPress += OnlyLetters_KeyPress;
            Religion.KeyPress += OnlyLetters_KeyPress;
            Complexion.KeyPress += OnlyLetters_KeyPress;
            FatherName.KeyPress += OnlyLetters_KeyPress;
            FatherJob.KeyPress += OnlyLetters_KeyPress;
            MotherName.KeyPress += OnlyLetters_KeyPress;
            MotherJob.KeyPress += OnlyLetters_KeyPress;
            EmergencyPerson.KeyPress += OnlyLetters_KeyPress;
            Relationship.KeyPress += OnlyLetters_KeyPress;

            StudentNumber.KeyPress += StudentNumber_KeyPress;
            TelephoneCellNo.KeyPress += NumericTextBox_KeyPress;
            Height.KeyPress += NumericTextBox_KeyPress;
            Weight.KeyPress += NumericTextBox_KeyPress;
            EmergencyPersonNo.KeyPress += NumericTextBox_KeyPress;

            TelephoneCellNo.Leave += TelephoneCellNo_Leave;
            EmergencyPersonNo.Leave += EmergencyPersonNo_Leave;
            EmailAddress.Leave += EmailAddress_Leave;

            Birthday.MaxDate = DateTime.Today;
            Birthday.ValueChanged += Birthday_ValueChanged;

            LoadDepartments();
            isFormLoaded = true;

        }
        private void LoadYearLevels()
        {
            try
            {
                using (MySqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT DISTINCT year_level FROM section ORDER BY year_level ASC";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            YearLevel.Items.Clear();
                            while (reader.Read())
                            {
                                YearLevel.Items.Add(reader["year_level"].ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading Year Levels: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadCourses()
        {
            try
            {
                if (guna2ComboBox3.SelectedItem == null)
                {
                    if (isFormLoaded)
                    {
                        MessageBox.Show("Please select a department first.", "Selection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    return;
                }

                string selectedDepartment = guna2ComboBox3.SelectedItem.ToString();

                using (MySqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT course_id, code FROM course WHERE department = @department ORDER BY code ASC";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@department", selectedDepartment);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            Dictionary<int, string> courseList = new Dictionary<int, string>();

                            while (reader.Read())
                            {
                                courseList.Add(reader.GetInt32("course_id"), reader.GetString("code"));
                            }

                            Course.DataSource = null;
                            Course.Items.Clear();

                            if (courseList.Count > 0)
                            {
                                Course.DataSource = new BindingSource(courseList, null);
                                Course.DisplayMember = "Value";
                                Course.ValueMember = "Key";
                            }

                            Course.SelectedIndex = -1;
                            Course.Text = string.Empty;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading Courses: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSections()
        {
            if (YearLevel.SelectedItem == null || Course.SelectedValue == null)
            {
                Section.DataSource = null;
                Section.Items.Clear();
                return;
            }

            string selectedYearLevel = YearLevel.SelectedItem.ToString();

            if (Course.SelectedValue is int selectedCourseId)
            {
                try
                {
                    using (MySqlConnection conn = Database.GetConnection())
                    {
                        conn.Open();
                        string query = "SELECT section_id, campus FROM section WHERE year_level = @yearLevel AND course_id = @courseId ORDER BY campus ASC";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@yearLevel", selectedYearLevel);
                            cmd.Parameters.AddWithValue("@courseId", selectedCourseId);

                            using (MySqlDataReader reader = cmd.ExecuteReader())
                            {
                                Dictionary<int, string> sectionList = new Dictionary<int, string>();
                                while (reader.Read())
                                {
                                    sectionList.Add(reader.GetInt32("section_id"), reader.GetString("campus"));
                                }

                                if (sectionList.Count > 0)
                                {
                                    Section.DataSource = new BindingSource(sectionList, null);
                                    Section.DisplayMember = "Value";
                                    Section.ValueMember = "Key";
                                    Section.SelectedIndex = -1;
                                }
                                else
                                {
                                    Section.DataSource = null;
                                    Section.Items.Clear();
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading Sections: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                Section.DataSource = null;
                Section.Items.Clear();
            }
        }


        private void LoadProvinces()
        {
            try
            {
                using (MySqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT province_id, province_name FROM table_province ORDER BY province_name ASC";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            Province.Items.Clear();
                            while (reader.Read())
                            {
                                Province.Items.Add(new KeyValuePair<int, string>(
                                    Convert.ToInt32(reader["province_id"]),
                                    reader["province_name"].ToString()
                                ));
                            }
                        }
                    }
                }
                Province.DisplayMember = "Value";
                Province.ValueMember = "Key";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading Provinces: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void LoadMunicipalities(int provinceId)
        {
            try
            {
                using (MySqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT municipality_id, municipality_name FROM table_municipality WHERE province_id = @provinceId ORDER BY municipality_name ASC";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@provinceId", provinceId);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            Municipality.Items.Clear();
                            while (reader.Read())
                            {
                                Municipality.Items.Add(new KeyValuePair<int, string>(
                                    Convert.ToInt32(reader["municipality_id"]),
                                    reader["municipality_name"].ToString()
                                ));
                            }
                        }
                    }
                }

                Municipality.DisplayMember = "Value";
                Municipality.ValueMember = "Key";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading Municipalities: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadBarangays(int municipalityId)
        {
            try
            {
                using (MySqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT barangay_id, barangay_name FROM table_barangay WHERE municipality_id = @municipalityId ORDER BY barangay_name ASC";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@municipalityId", municipalityId);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            Barangay.Items.Clear();
                            while (reader.Read())
                            {
                                Barangay.Items.Add(new KeyValuePair<int, string>(
                                    Convert.ToInt32(reader["barangay_id"]),
                                    reader["barangay_name"].ToString()
                                ));
                            }
                        }
                    }
                }

                Barangay.DisplayMember = "Value";
                Barangay.ValueMember = "Key";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading Barangays: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void LoadDepartments()
        {
            try
            {
                using (MySqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT DISTINCT department FROM course"; // Query to fetch distinct departments

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            guna2ComboBox3.Items.Clear();

                            while (reader.Read())
                            {
                                guna2ComboBox3.Items.Add(reader["department"].ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading departments: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void YearLevel_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadSections();
        }

        private void Course_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadSections();
        }

        private void Province_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Province.SelectedItem is KeyValuePair<int, string> selectedProvince)
            {
                int provinceId = selectedProvince.Key;
                LoadMunicipalities(provinceId);
            }
        }

        private void Municipality_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Municipality.SelectedItem is KeyValuePair<int, string> selectedMunicipality)
            {
                int municipalityId = selectedMunicipality.Key;
                LoadBarangays(municipalityId);
            }
        }

        private void UploadBTN_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                ofd.Title = "Select an Image";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    Picture.Image = Image.FromFile(ofd.FileName);
                    string fileName = System.IO.Path.GetFileName(ofd.FileName);
                    guna2HtmlLabel28.Text = fileName;
                }
            }
        }

        private void SubmitBTN_Click(object sender, EventArgs e)
        {

            try
            {

                if (string.IsNullOrWhiteSpace(StudentNumber.Text) ||
                    string.IsNullOrWhiteSpace(LastName.Text) ||
                    string.IsNullOrWhiteSpace(FirstName.Text) ||
                    Gender.SelectedItem == null ||
                    Birthday.Value == null ||
                    Section.SelectedItem == null ||
                    string.IsNullOrWhiteSpace(Height.Text) ||
                    string.IsNullOrWhiteSpace(Weight.Text) ||
                    string.IsNullOrWhiteSpace(Complexion.Text) ||
                    string.IsNullOrWhiteSpace(PlaceofBirth.Text) ||
                    string.IsNullOrWhiteSpace(Street.Text) ||
                    string.IsNullOrWhiteSpace(StreetName.Text) ||
                    Province.SelectedItem == null ||
                    Municipality.SelectedItem == null ||
                    Barangay.SelectedItem == null ||
                    string.IsNullOrWhiteSpace(TelephoneCellNo.Text) ||
                    string.IsNullOrWhiteSpace(EmailAddress.Text) ||

                    string.IsNullOrWhiteSpace(FBName.Text) ||
                    FBName.Text.Trim() == "fb.com/" ||
                    string.IsNullOrWhiteSpace(EmergencyPerson.Text) ||
                    string.IsNullOrWhiteSpace(Relationship.Text) ||
                    string.IsNullOrWhiteSpace(EmergencyPersonNo.Text) ||
                    Picture.Image == null)

                {
                    MessageBox.Show("Please fill in all required fields.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (string.IsNullOrEmpty(EmailAddress.Text.Trim()) || !Regex.IsMatch(EmailAddress.Text.Trim(), @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
                {
                    MessageBox.Show("Please check the valid email address!", "Invalid Email", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }


                string cadetId = StudentNumber.Text.Trim();
                string lastName = LastName.Text.Trim();
                string firstName = FirstName.Text.Trim();
                string middleName = MiddleName.Text.Trim();
                string suffix = Suffix.Text.Trim();
                string gender = Gender.SelectedItem.ToString();
                string height = Height.Text.Trim();
                string weight = Weight.Text.Trim();
                string complexion = Complexion.Text.Trim();
                string bloodType = BloodType.Text.Trim();
                string religion = Religion.Text.Trim();
                DateTime birthdate = Birthday.Value;
                string birthplace = PlaceofBirth.Text.Trim();
                string street = Street.Text.Trim();
                string streetName = StreetName.Text.Trim();
                string province = Province.SelectedItem is KeyValuePair<int, string> selectedProvince ? selectedProvince.Value : "";
                string municipality = Municipality.SelectedItem is KeyValuePair<int, string> selectedMunicipality ? selectedMunicipality.Value : "";
                string barangay = Barangay.SelectedItem is KeyValuePair<int, string> selectedBarangay ? selectedBarangay.Value : "";
                string address = $"{street}, {streetName}, {barangay}, {municipality}, {province}";
                string contactNumber = TelephoneCellNo.Text.Trim();
                string email = EmailAddress.Text.Trim();
                string facebookAccount = FBName.Text.Trim();
                int classYear = DateTime.Now.Year;
                int sectionId = 0;

                if (Section.SelectedValue != null)
                {
                    int.TryParse(Section.SelectedValue.ToString(), out sectionId);
                   
                }

                if (sectionId == 0)
                {
                    MessageBox.Show("Invalid section selection.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                byte[] profilePicture = null;
                if (Picture.Image != null)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        Picture.Image.Save(ms, Picture.Image.RawFormat);
                        profilePicture = ms.ToArray();
                    }
                }

                using (MySqlConnection conn = Database.GetConnection())
                {
                    conn.Open();

                    MySqlTransaction transaction = conn.BeginTransaction();

                    try
                    {
                        string query1 = @"INSERT INTO cadet_info 
                (cadet_id, last_name, first_name, middle_name, suffix, gender, height, weight, complexion, blood_type, 
                religion, birthdate, birthplace, address, contact_number, email, facebook_account, rank, class_year, 
                profile_picture, section_id, created_at) 
                VALUES 
                (@cadetId, @lastName, @firstName, @middleName, @suffix, @gender, @height, @weight, @complexion, @bloodType, 
                @religion, @birthdate, @birthplace, @address, @contactNumber, @email, @facebookAccount, NULL, @classYear, 
                @profilePicture, @sectionId, NOW())";

                        using (MySqlCommand cmd1 = new MySqlCommand(query1, conn, transaction))
                        {
                            cmd1.Parameters.AddWithValue("@cadetId", cadetId);
                            cmd1.Parameters.AddWithValue("@lastName", lastName);
                            cmd1.Parameters.AddWithValue("@firstName", firstName);
                            cmd1.Parameters.AddWithValue("@middleName", middleName);
                            cmd1.Parameters.AddWithValue("@suffix", suffix);
                            cmd1.Parameters.AddWithValue("@gender", gender);
                            cmd1.Parameters.AddWithValue("@height", height);
                            cmd1.Parameters.AddWithValue("@weight", weight);
                            cmd1.Parameters.AddWithValue("@complexion", complexion);
                            cmd1.Parameters.AddWithValue("@bloodType", bloodType);
                            cmd1.Parameters.AddWithValue("@religion", religion);
                            cmd1.Parameters.AddWithValue("@birthdate", birthdate);
                            cmd1.Parameters.AddWithValue("@birthplace", birthplace);
                            cmd1.Parameters.AddWithValue("@address", address);
                            cmd1.Parameters.AddWithValue("@contactNumber", contactNumber);
                            cmd1.Parameters.AddWithValue("@email", email);
                            cmd1.Parameters.AddWithValue("@facebookAccount", facebookAccount);
                            cmd1.Parameters.AddWithValue("@classYear", classYear);
                            cmd1.Parameters.AddWithValue("@profilePicture", profilePicture);
                            cmd1.Parameters.AddWithValue("@sectionId", sectionId);

                            cmd1.ExecuteNonQuery();
                        }

                        string query2 = @"INSERT INTO secondary_info 
                (father_name, mother_name, father_occupation, mother_occupation, 
                emergency_contact_name, emergency_contact_relationship, emergency_contact_number, cadet_id, created_at)
                VALUES 
                (@father_name, @mother_name, @father_occupation, @mother_occupation, 
                @emergency_contact_name, @emergency_contact_relationship, @emergency_contact_number, @cadet_id, NOW())";

                        using (MySqlCommand cmd2 = new MySqlCommand(query2, conn, transaction))
                        {
                            cmd2.Parameters.AddWithValue("@father_name", FatherName.Text.Trim());
                            cmd2.Parameters.AddWithValue("@mother_name", MotherName.Text.Trim());
                            cmd2.Parameters.AddWithValue("@father_occupation", FatherJob.Text.Trim());
                            cmd2.Parameters.AddWithValue("@mother_occupation", MotherJob.Text.Trim());
                            cmd2.Parameters.AddWithValue("@emergency_contact_name", EmergencyPerson.Text.Trim());
                            cmd2.Parameters.AddWithValue("@emergency_contact_relationship", Relationship.Text.Trim());
                            cmd2.Parameters.AddWithValue("@emergency_contact_number", EmergencyPersonNo.Text.Trim());
                            cmd2.Parameters.AddWithValue("@cadet_id", cadetId);

                            cmd2.ExecuteNonQuery();
                        }

                        transaction.Commit();

                        MessageBox.Show("Cadet information saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        ClearForm();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error saving cadet information: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unexpected error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CancelBTN_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to cancel?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                ClearForm();
            }
        }


        private void ClearForm()
        {
            StudentNumber.Clear();
            LastName.Clear();
            FirstName.Clear();
            MiddleName.Clear();
            Height.Clear();
            Weight.Clear();
            Complexion.Clear();
            Religion.Clear();
            PlaceofBirth.Clear();
            Street.Clear();
            TelephoneCellNo.Clear();
            EmailAddress.Clear();
            FBName.Clear();
            FatherName.Clear();
            FatherJob.Clear();
            MotherName.Clear();
            MotherJob.Clear();
            EmergencyPerson.Clear();
            Relationship.Clear();
            EmergencyPersonNo.Clear();
            Age.Clear();
            Gender.SelectedItem = null;
            BloodType.SelectedItem = null;
            Province.SelectedItem = null;
            Municipality.SelectedItem = null;
            Barangay.SelectedItem = null;
            Section.SelectedItem = null;
            Suffix.SelectedItem = null;
            YearLevel.SelectedItem = null;
            Course.SelectedIndex = -1;
            Course.Text = string.Empty;
            Course.SelectedItem = null;
            Course.DataSource = null;
            Picture.Image = null;
            LoadCourses();
            Age.Clear();
        }

        private void StudentNumber_Leave(object sender, EventArgs e)
        {
            string cadetId = StudentNumber.Text.Trim();
            string studentNumberPattern = @"^\d{2}-\d{4}$";

            if (string.IsNullOrEmpty(cadetId))
                return;

            if (!Regex.IsMatch(cadetId, studentNumberPattern))
            {
                MessageBox.Show("Please input valid student number (XX-XXXX).",
                                "Invalid Format", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                StudentNumber.Clear();
                StudentNumber.Focus();
                return;
            }

            try
            {
                using (MySqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM cadet_info WHERE cadet_id = @cadetId";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@cadetId", cadetId);
                        int count = Convert.ToInt32(cmd.ExecuteScalar());

                        if (count > 0)
                        {
                            MessageBox.Show("Student Number already exists. Please enter a unique Student Number.",
                                "Duplicate Entry", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                            StudentNumber.Clear();
                            StudentNumber.Focus();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error checking Student Number: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void StudentNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != '-' && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
                return;
            }
            if (char.IsDigit(e.KeyChar) && StudentNumber.Text.Length == 2)
            {
                StudentNumber.Text += "-";
                StudentNumber.SelectionStart = StudentNumber.Text.Length;
            }
        }
        private void EmergencyPersonNo_Leave(object sender, EventArgs e)
        {
            string input = EmergencyPersonNo.Text.Trim();

            if (string.IsNullOrEmpty(input))
                return;

            if (input.Length == 11)
            {
                if (!input.StartsWith("09"))
                {
                    MessageBox.Show("Mobile numbers must start with '09'.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    EmergencyPersonNo.Clear();
                    EmergencyPersonNo.Focus();
                    return;
                }
            }
            else if (input.Length != 8)
            {
                MessageBox.Show("Please enter a valid contact number.\n\n- 11 digits for mobile numbers (must start with '09')\n- 8 digits for landline numbers",
                                "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                EmergencyPersonNo.Clear();
                EmergencyPersonNo.Focus();
                return;
            }
        }
        private void TelephoneCellNo_Leave(object sender, EventArgs e)
        {
            string contactNumber = TelephoneCellNo.Text.Trim();

            if (string.IsNullOrEmpty(contactNumber))
                return;
            if (contactNumber.Length == 11)
            {
                if (!contactNumber.StartsWith("09"))
                {
                    MessageBox.Show("Mobile numbers must start with '09'.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    TelephoneCellNo.Clear();
                    TelephoneCellNo.Focus();
                    return;
                }
            }
            else if (contactNumber.Length != 8)
            {
                MessageBox.Show("Please enter a valid contact number.\n\n- 11 digits for mobile numbers (must start with '09')\n- 8 digits for landline numbers",
                                "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                TelephoneCellNo.Clear();
                TelephoneCellNo.Focus();
                return;
            }

            try
            {
                using (MySqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM cadet_info WHERE contact_number = @contactNumber";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@contactNumber", contactNumber);
                        int count = Convert.ToInt32(cmd.ExecuteScalar());

                        if (count > 0)
                        {
                            MessageBox.Show("This telephone number already exists in the database.", "Duplicate Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            TelephoneCellNo.Clear();
                            TelephoneCellNo.Focus();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error checking Telephone Number: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void EmailAddress_Leave(object sender, EventArgs e)
        {
            string email = EmailAddress.Text.Trim();
            string gmailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";


            if (string.IsNullOrEmpty(email))
                return;

            if (!Regex.IsMatch(email, gmailPattern))
            {
                MessageBox.Show("Please enter a valid Email Address.",
                                "Invalid Email", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (MySqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM cadet_info WHERE email = @email";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@email", email);
                        int count = Convert.ToInt32(cmd.ExecuteScalar());

                        if (count > 0)
                        {
                            MessageBox.Show("This email address already exists in the database.", "Duplicate Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            EmailAddress.Clear();
                            EmailAddress.Focus();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error checking Email Address: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Birthday_ValueChanged(object sender, EventArgs e)
        {
            if (Birthday.Value > DateTime.Today)
            {
                MessageBox.Show("Future dates are not allowed for Birthdate.",
                                "Invalid Date", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Birthday.Value = DateTime.Today;
            }

            int age = CalculateAge(Birthday.Value);
            Age.Text = age.ToString();
        }

        private int CalculateAge(DateTime birthDate)
        {
            DateTime today = DateTime.Today;
            int age = today.Year - birthDate.Year;

            if (birthDate.Date > today.AddYears(-age))
            {
                age--;
            }

            return age;
        }

        private void CapitalizeWords(Guna.UI2.WinForms.Guna2TextBox textBox)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
                return;

            string[] words = textBox.Text.Split(' ');
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 0)
                {
                    words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower();
                }
            }

            string formattedText = string.Join(" ", words);
            int selectionStart = textBox.SelectionStart;
            textBox.Text = formattedText;
            textBox.SelectionStart = selectionStart;
        }

        private void NumericTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }
        private void OnlyLetters_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsLetter(e.KeyChar) && e.KeyChar != ' ' && e.KeyChar != '-' && e.KeyChar != (char)Keys.Back)
            {
                e.Handled = true;
            }
        }

        private void EmailAddress_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (EmailAddress.SelectionStart == 0 && e.KeyChar == (char)Keys.Back)
            {
                e.Handled = true;
            }
        }
        private void FBName_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (FBName.SelectionStart < 7 && e.KeyChar == (char)Keys.Back)
            {
                e.Handled = true;
            }
        }
       


        private void LastName_TextChanged(object sender, EventArgs e)
        {
            CapitalizeWords((Guna.UI2.WinForms.Guna2TextBox)sender);
            UpdateFullNameLabel();
        }

        private void FirstName_TextChanged(object sender, EventArgs e)
        {
            CapitalizeWords((Guna.UI2.WinForms.Guna2TextBox)sender);
            UpdateFullNameLabel();
        }

        private void MiddleName_TextChanged(object sender, EventArgs e)
        {
            CapitalizeWords((Guna.UI2.WinForms.Guna2TextBox)sender);
            UpdateFullNameLabel();
        }

        private void Street_TextChanged(object sender, EventArgs e)
        {
            CapitalizeWords((Guna.UI2.WinForms.Guna2TextBox)sender);
        }

        private void PlaceofBirth_TextChanged(object sender, EventArgs e)
        {
            CapitalizeWords((Guna.UI2.WinForms.Guna2TextBox)sender);
        }

        private void Religion_TextChanged(object sender, EventArgs e)
        {
            CapitalizeWords((Guna.UI2.WinForms.Guna2TextBox)sender);
        }

        private void FatherName_TextChanged(object sender, EventArgs e)
        {
            CapitalizeWords((Guna.UI2.WinForms.Guna2TextBox)sender);
        }

        private void FatherJob_TextChanged(object sender, EventArgs e)
        {
            CapitalizeWords((Guna.UI2.WinForms.Guna2TextBox)sender);
        }

        private void MotherName_TextChanged(object sender, EventArgs e)
        {
            CapitalizeWords((Guna.UI2.WinForms.Guna2TextBox)sender);
        }

        private void MotherJob_TextChanged(object sender, EventArgs e)
        {
            CapitalizeWords((Guna.UI2.WinForms.Guna2TextBox)sender);
        }

        private void EmergencyPerson_TextChanged(object sender, EventArgs e)
        {
            CapitalizeWords((Guna.UI2.WinForms.Guna2TextBox)sender);
        }

        private void Relationship_TextChanged(object sender, EventArgs e)
        {
            CapitalizeWords((Guna.UI2.WinForms.Guna2TextBox)sender);
        }

        private void Complexion_TextChanged(object sender, EventArgs e)
        {
            CapitalizeWords((Guna.UI2.WinForms.Guna2TextBox)sender);
        }

        private void FBName_TextChanged(object sender, EventArgs e)
        {
            if (!FBName.Text.StartsWith("fb.com/"))
            {
                FBName.Text = "fb.com/" + FBName.Text.Replace("fb.com/", "");
                FBName.SelectionStart = FBName.Text.Length;
            }
        }
        private void CancelBTN_Click_1(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to cancel?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                ClearForm();
            }
        }
        private void UpdateFullNameLabel()
        {
            string lastName = LastName.Text.Trim();
            string firstName = FirstName.Text.Trim();
            string middleName = MiddleName.Text.Trim();
            string suffix = Suffix.Text.Trim();

            string middleInitial = string.IsNullOrEmpty(middleName) ? "" : middleName[0] + ".";
            string fullName = $"{lastName}, {firstName} {middleInitial} {suffix}".Trim();

            guna2HtmlLabel28.Text = fullName;
        }
        private void NextPageBTN_Click(object sender, EventArgs e)
        {
            guna2TabControl1.SelectedTab = RegistrationP2;
        }

        private void PreviousPageBTN_Click(object sender, EventArgs e)
        {
            guna2TabControl1.SelectedTab = Registration;
        }

        private void RegistrationBTN_Click(object sender, EventArgs e)
        {
            guna2TabControl1.SelectedTab = Registration;
        }

        private void EnlistmentBTN_Click(object sender, EventArgs e)
        {
            guna2TabControl1.SelectedTab = Enlistment;
        }

        private void ModifyPageBTN_Click(object sender, EventArgs e)
        {
            guna2TabControl1.SelectedTab = ModifyPage;
        }

        private void PreviousPageViewBTN_Click(object sender, EventArgs e)
        {
            guna2TabControl1.SelectedTab = ListofCadets2;
        }

        private void BackViewListBTN_Click(object sender, EventArgs e)
        {
            guna2TabControl1.SelectedTab = ListofCadets;
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            guna2TabControl1.SelectedTab = Certificate;
        }

        private void AccountCreateBTN_Click(object sender, EventArgs e)
        {
            guna2TabControl1.SelectedTab = CreateAccount;
        }

        private void guna2Button4_Click(object sender, EventArgs e)
        {
            guna2TabControl1.SelectedTab = ProfReg;
        }

        private void guna2Button5_Click(object sender, EventArgs e)
        {
            guna2TabControl1.SelectedTab = Assign;
        }

        private void HomePageBTN_Click(object sender, EventArgs e)
        {
            guna2TabControl1.SelectedTab = HomePage;
        }

        private void Suffix_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateFullNameLabel();
        }

        private void StudentNumber_TextChanged(object sender, EventArgs e)
        {
            guna2HtmlLabel103.Text = StudentNumber.Text.Trim();
        }

        private void Section_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Section.SelectedItem is KeyValuePair<int, string> selectedSection)
            {
                string campus = selectedSection.Value;
                guna2HtmlLabel102.Text = $"{campus}";
            }

        }

        private void guna2TextBox2_TextChanged(object sender, EventArgs e)
        {
            string input = guna2TextBox2.Text.Replace("-", string.Empty); 
            string formatted = "";

            if (input.Length > 0)
            {
                formatted += input.Substring(0, Math.Min(2, input.Length)); 
            }

            if (input.Length > 2)
            {
                formatted += "-" + input.Substring(2, Math.Min(2, input.Length - 2)); 
            }

            if (input.Length > 4)
            {
                formatted += "-" + input.Substring(4, Math.Min(4, input.Length - 4));
            }
            guna2TextBox2.TextChanged -= guna2TextBox2_TextChanged;
            guna2TextBox2.Text = formatted;
            guna2TextBox2.SelectionStart = guna2TextBox2.Text.Length;
            guna2TextBox2.TextChanged += guna2TextBox2_TextChanged;

            if (DateTime.TryParseExact(formatted, "dd-MM-yyyy",
                                       System.Globalization.CultureInfo.InvariantCulture,
                                       System.Globalization.DateTimeStyles.None,
                                       out DateTime parsedDate))
            {

                if (parsedDate <= DateTime.Today)
                {
                    Birthday.Value = parsedDate;
                }
                else
                {
                    MessageBox.Show("Future dates are not allowed.", "Invalid Date", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    guna2TextBox2.Text = "";
                }
            }
        }

        private void guna2ComboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadCourses();
        }

        private void StreetName_TextChanged(object sender, EventArgs e)
        {
            CapitalizeWords((Guna.UI2.WinForms.Guna2TextBox)sender);
        }

        //------------------------------------------------------------------------------END OF REGISTRATION--------------------------------------------------------------------------------------

        //------------------------------------------------------------------------------LIST OF CADETS-------------------------------------------------------------------------------------------

        private void ListCadetsBTN_Click(object sender, EventArgs e)
        {
            guna2TabControl1.SelectedTab = ListofCadets;
            LoadCadets();
        }

        private void NextPageViewBTN_Click(object sender, EventArgs e)
        {
            ViewSecondaryDetails(); 
        }

        private void SearchTXT_TextChanged(object sender, EventArgs e)
        {
            SearchCadets();
        }

        private void ViewCadetDetailsBTN_Click(object sender, EventArgs e)
        {
            ViewDetails();
        }

        private void LoadCadets()
        {
            DataTable dataTable = new DataTable();

            try
            {
                using (MySqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT cadet_id AS `Student No`, " +
                           "TRIM(CONCAT(IFNULL(last_name, ''), ', ', " + 
                           "IFNULL(first_name, ''), ' ', " + 
                           "IFNULL(middle_name, ''), ' ', " + 
                           "IFNULL(suffix, ''))) AS `Full Name`, " + 
                           "email AS `Email Address`, " + 
                           "contact_number AS `Contact No` " + 
                           "FROM cadet_info";

                    using (MySqlDataAdapter dataAdapter = new MySqlDataAdapter(query, conn))
                    {
                        dataAdapter.Fill(dataTable);
                    }
                }

                CadetListDGV.DataSource = dataTable;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading cadet data: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SearchCadets()
        {
            string searchTerm = SearchTXT.Text.Trim().ToLower();
            string filter = string.Empty;
            string selectedCriteria = SearchCB.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(selectedCriteria))
            {
                if (string.IsNullOrEmpty(searchTerm))
                {
                    LoadCadets(); 
                }
                else
                {
                    (CadetListDGV.DataSource as DataTable).DefaultView.RowFilter = string.Format(
                        "Convert([Student No], 'System.String') LIKE '%{0}%' OR [Full Name] LIKE '%{0}%'",
                        searchTerm);
                }
                return;
            }

            switch (selectedCriteria)
            {
                case "Name":
                    filter = $"[Full Name] LIKE '%{searchTerm}%'";
                    break;
                case "Student No":
                    filter = $"Convert([Student No], 'System.String') LIKE '%{searchTerm}%'";
                    break;
                case "Section":
                    string query = "SELECT ci.cadet_id AS `Student No`, " +
                                   "TRIM(CONCAT(IFNULL(ci.last_name, ''), ', ', " +
                                   " IFNULL(ci.first_name, ''), ' ', " +
                                   "IFNULL(ci.middle_name, ''), ' ', " +
                                   "IFNULL(ci.suffix, ''))) AS `Full Name`, " +
                                   "ci.email AS `Email Address`, " +
                                   "ci.contact_number AS `Contact No`, " +
                                   "s.campus " + 
                                   "FROM cadet_info ci " +
                                   "JOIN section s ON ci.section_id = s.section_id " +
                                   "WHERE s.campus LIKE '%" + searchTerm + "%'";
                    try
                    {
                        using (MySqlConnection conn = Database.GetConnection())
                        {
                            conn.Open();
                            using (MySqlCommand cmd = new MySqlCommand(query, conn))
                            {
                                cmd.Parameters.AddWithValue("@searchTerm", "%" + searchTerm + "%");
                                MySqlDataAdapter dataAdapter = new MySqlDataAdapter(cmd);
                                DataTable dataTable = new DataTable();
                                dataAdapter.Fill(dataTable);
                                CadetListDGV.DataSource = dataTable;
                                if (CadetListDGV.Columns.Contains("campus"))
                                {
                                    CadetListDGV.Columns["campus"].Visible = false;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error loading cadet details: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    break;
                case "Academic Year":
                    filter = $"[Class Year] LIKE '%{searchTerm}%'";
                    break;
                case "Rank":
                    filter = $"[Rank] LIKE '%{searchTerm}%'";
                    break;
                case "Platoon":
                   
                    break;
                case "Battalion":
                    filter = $"[Battalion] LIKE '%{searchTerm}%'";
                    break;
                default:
                    if (string.IsNullOrEmpty(searchTerm))
                    {
                        LoadCadets();
                    }
                    else
                    {
                        (CadetListDGV.DataSource as DataTable).DefaultView.RowFilter = string.Format(
                            "Convert([Student No], 'System.String') LIKE '%{0}%' OR [Full Name] LIKE '%{0}%'",
                            searchTerm);
                    }
                    return;
            }
        }

        private void ViewDetails()
        {
            if (CadetListDGV.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = CadetListDGV.SelectedRows[0];
                string cadetId = selectedRow.Cells["Student No"].Value.ToString();
                string query = "SELECT ci.*, s.*, c.* " +
                  "FROM cadet_info ci " +
                  "JOIN section s ON ci.section_id = s.section_id " +
                  "JOIN course c ON c.course_id = s.course_id " +
                  "WHERE ci.cadet_id = @cadetId";
                try
                {
                    using (MySqlConnection conn = Database.GetConnection())
                    {
                        conn.Open();
                        using (MySqlCommand cmd = new MySqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@cadetId", cadetId);
                            using (MySqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    EnlistmentDateTXT.Value = Convert.ToDateTime(reader["created_at"]);
                                    StudentNoTXT.Text = reader["cadet_id"].ToString();
                                    LastNameTXT.Text = reader["last_name"].ToString();
                                    FirstNameTXT.Text = reader["first_name"].ToString();
                                    MiddleNameTXT.Text = reader["middle_name"].ToString();
                                    Suffix.Text = reader["suffix"].ToString();
                                    GenderTXT.Text = reader["gender"].ToString();
                                    HeightTXT.Text = reader["height"].ToString();
                                    WeightTXT.Text = reader["weight"].ToString();
                                    ComplexionTXT.Text = reader["complexion"].ToString();
                                    BloodTypeTXT.Text = reader["blood_type"].ToString();
                                    ReligionTXT.Text = reader["religion"].ToString();
                                    BirthdateTXT.Value = Convert.ToDateTime(reader["birthdate"]);
                                    BirthplaceTXT.Text = reader["birthplace"].ToString();
                                    AddressTXT.Text = reader["address"].ToString();
                                    CellNoTXT.Text = reader["contact_number"].ToString();
                                    EmailTXT.Text = reader["email"].ToString();
                                    FacebookTXT.Text = reader["facebook_account"].ToString();

                                    if (reader["profile_picture"] != DBNull.Value)
                                    {
                                        byte[] imageData = (byte[])reader["profile_picture"];
                                        using (MemoryStream ms = new MemoryStream(imageData))
                                        {
                                            picturePB.Image = Image.FromStream(ms);
                                        }
                                    }

                                    CollegeDepartmentTXT.Text = reader["department"].ToString();
                                    CourseTXT.Text = reader["code"].ToString();
                                    SectionTXT.Text = reader["campus"].ToString();
                                    YearTXT.Text = reader["year_level"].ToString();
                                    AgeTXT.Text = CalculateAge(BirthdateTXT.Value).ToString();
                                }
                                else
                                {
                                    MessageBox.Show("No details found for the selected cadet.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                    }
                    guna2TabControl1.SelectedTab = ListofCadets2;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading cadet details: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a cadet to view details.", "No Row Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ViewSecondaryDetails()
        {
            string cadetId = ListofCadets2.Controls["StudentNoTXT"].Text;
            string query = "SELECT * FROM secondary_info WHERE cadet_id = @cadetId";
            try
            {
                using (MySqlConnection conn = Database.GetConnection())
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@cadetId", cadetId);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                guna2TabControl1.SelectedTab = ListofCadets3;
                                FatherNameTXT.Text = reader["father_name"].ToString();
                                FatherOccTXT.Text = reader["father_occupation"].ToString();
                                MotherNameTXT.Text = reader["mother_name"].ToString();
                                MotherOccTXT.Text = reader["mother_occupation"].ToString();
                                TelephoneTXT.Text = reader["emergency_contact_number"].ToString();
                                RelationTXT.Text = reader["emergency_contact_relationship"].ToString();
                                PersonTXT.Text = reader["emergency_contact_name"].ToString();
                            }
                            else
                            {
                                MessageBox.Show("No secondary details found for the selected cadet.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading cadet details: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //----------------------------------------------------------------------------END OF LIST OF CADETS--------------------------------------------------------------------------------------
    }

}

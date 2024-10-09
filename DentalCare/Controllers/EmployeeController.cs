﻿using DentalCare.Models;
using DentalCare.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;

namespace DentalCare.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly DoctorService _doctorService;
        private readonly NurseService _nurseService;
        private readonly ReceptionistService _receptionistService;
        private readonly FacultyService _facultyService;
        private readonly AccountService _accountService;

        public EmployeeController(NurseService nurseService, DoctorService doctorService, ReceptionistService receptionistService, FacultyService facultyService, AccountService accountService)
        {
            _doctorService = doctorService;
            _nurseService = nurseService;
            _receptionistService = receptionistService;
            _facultyService = facultyService;
            _accountService = accountService;
        }

        [HttpGet]
        public IActionResult Add()
        {
            var faculties = _facultyService.GetAll();
            ViewBag.Faculties = faculties;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(Employee model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.Faculty == null)
            {
                Console.WriteLine($"Không tìm thấy khoa với tên: {model.Faculty.Trim()}");
            }
            else
            {
                Console.WriteLine($"tìm thấy khoa với tên: {model.Faculty.Trim()}");
            }

            try
            {
                if (model.Avatar != null && model.Avatar.Length > 0)
                {
                    var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/avatars");
                    var fileName = Path.GetFileName(model.Avatar.FileName);
                    var filePath = Path.Combine(uploads, fileName);

                    Directory.CreateDirectory(uploads);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.Avatar.CopyToAsync(fileStream);
                    }

                    model.AvatarPath = $"/uploads/avatars/{fileName}";
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "File upload failed");
            }

            if (model.Role.Equals("Doctor", StringComparison.OrdinalIgnoreCase))
            {
                var doctor = new Doctor
                {
                    Id = _doctorService.GenerateID(),
                    Name = model.FullName,
                    Phone = model.Phone,
                    Email = model.Email,
                    Birthday = model.Birthday.Value,
                    Gender = model.Gender.Equals("Male", StringComparison.OrdinalIgnoreCase),
                    Firstdayofwork = model.JoinTime.Value,
                    Falcutyid = model.Faculty,
                    Avatar = model.AvatarPath,
                    Fired = false,
                    FacultyName = _facultyService.Get(model.Faculty).Name
                };

                var account = new Account
                {
                    Phone = doctor.Phone,
                    Password = doctor.Id,
                    Email = doctor.Email,
                    Role = "D",
                    DoctorId = doctor.Id
                };

                _doctorService.Add(doctor);
                _accountService.Add(account);
                SendAccountToEmail(doctor.Id);
            }
            else if (model.Role.Equals("Nurse", StringComparison.OrdinalIgnoreCase))
            {
                var nurse = new Nurse
                {
                    Id = _nurseService.GenerateID(),
                    Name = model.FullName,
                    Phone = model.Phone,
                    Email = model.Email,
                    Birthday = model.Birthday.Value,
                    Gender = model.Gender.Equals("Male", StringComparison.OrdinalIgnoreCase),
                    Firstdayofwork = model.JoinTime.Value,
                    Avatar = model.AvatarPath,
                    Fired = false
                };

                _nurseService.Add(nurse);
            }
            else if (model.Role.Equals("Receptionist", StringComparison.OrdinalIgnoreCase))
            {
                var receptionist = new Receptionist
                {
                    Id = _receptionistService.GenerateID(),
                    Name = model.FullName,
                    Phone = model.Phone,
                    Email = model.Email,
                    Birthday = model.Birthday.Value,
                    Gender = model.Gender.Equals("Male", StringComparison.OrdinalIgnoreCase),
                    Firstdayofwork = model.JoinTime.Value,
                    Avatar = model.AvatarPath,
                    Fired = false
                };

                var account = new Account
                {
                    Phone = receptionist.Phone,
                    Password = receptionist.Id,
                    Email = receptionist.Email,
                    Role = "R",
                    ReceptionistId = receptionist.Id
                };

                _receptionistService.Add(receptionist);
                _accountService.Add(account);
                SendAccountToEmail(receptionist.Id);
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Edit(string id)
        {
            var faculties = _facultyService.GetAll();
            ViewBag.Faculties = faculties;

            if (id.Contains("D"))
            {
                var doctor = _doctorService.Get(id);
                var employee = new Employee
                {
                    id = doctor.Id,
                    FullName = doctor.Name,
                    AvatarPath = doctor.Avatar,
                    Email = doctor.Email,
                    Faculty = doctor.FacultyName,
                    Birthday = doctor.Birthday,
                    JoinTime = doctor.Firstdayofwork,
                    Role = "Doctor",
                    Phone = doctor.Phone,
                    Gender = (doctor.Gender == true) ? "Male" : "Female",
                    Fired = doctor.Fired,
                };
                return View(employee);
            }
            else if (id.Contains("R"))
            {
                var receptionist = _receptionistService.Get(id);
                var employee = new Employee
                {
                    id = receptionist.Id,
                    FullName = receptionist.Name,
                    AvatarPath = receptionist.Avatar,
                    Email = receptionist.Email,
                    Birthday = receptionist.Birthday,
                    JoinTime = receptionist.Firstdayofwork,
                    Role = "Receptionist",
                    Phone = receptionist.Phone,
                    Gender = (receptionist.Gender == true) ? "Male" : "Female",
                    Fired = receptionist.Fired
                };
                return View(employee);
            }
            else if (id.Contains("N"))
            {
                var nurse = _nurseService.Get(id);
                var employee = new Employee
                {
                    id = nurse.Id,
                    FullName = nurse.Name,
                    AvatarPath = nurse.Avatar,
                    Email = nurse.Email,
                    Birthday = nurse.Birthday,
                    JoinTime = nurse.Firstdayofwork,
                    Role = "Nurse",
                    Phone = nurse.Phone,
                    Gender = (nurse.Gender == true) ? "Male" : "Female",
                    Fired = nurse.Fired
                };
                return View(employee);
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Employee model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                if (model.Avatar != null && model.Avatar.Length > 0)
                {
                    var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/avatars");
                    var fileName = Path.GetFileName(model.Avatar.FileName);
                    var filePath = Path.Combine(uploads, fileName);

                    Directory.CreateDirectory(uploads);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.Avatar.CopyToAsync(fileStream);
                    }

                    model.AvatarPath = $"/uploads/avatars/{fileName}";
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "File upload failed");
            }

            if (model.id.Contains("D"))
            {
                var doctor = _doctorService.Get(model.id);

                if (model.AvatarPath == null)
                {
                    model.AvatarPath = doctor.Avatar;
                }

                doctor.Name = model.FullName;
                doctor.Phone = model.Phone;
                doctor.Email = model.Email;
                doctor.Birthday = model.Birthday.Value;
                doctor.Gender = model.Gender.Equals("Male", StringComparison.OrdinalIgnoreCase);
                doctor.Firstdayofwork = model.JoinTime.Value;
                doctor.Avatar = model.AvatarPath;

                _doctorService.Update(doctor);
                return RedirectToAction("Manage", "Employee", new { role = "Doctor" });
            }
            else if (model.id.Contains("N"))
            {
                var nurse = _nurseService.Get(model.id);

                if (model.AvatarPath == null)
                {
                    model.AvatarPath = nurse.Avatar;
                }

                nurse.Name = model.FullName;
                nurse.Phone = model.Phone;
                nurse.Email = model.Email;
                nurse.Birthday = model.Birthday.Value;
                nurse.Gender = model.Gender.Equals("Male", StringComparison.OrdinalIgnoreCase);
                nurse.Firstdayofwork = model.JoinTime.Value;
                nurse.Avatar = model.AvatarPath;

                _nurseService.Update(nurse);
                return RedirectToAction("Manage", "Employee", new { role = "Nurse" });
            }
            else if (model.id.Contains("R"))
            {
                var receptionist = _receptionistService.Get(model.id);

                if (model.AvatarPath == null)
                {
                    model.AvatarPath = receptionist.Avatar;
                }

                receptionist.Name = model.FullName;
                receptionist.Phone = model.Phone;
                receptionist.Email = model.Email;
                receptionist.Birthday = model.Birthday.Value;
                receptionist.Gender = model.Gender.Equals("Male", StringComparison.OrdinalIgnoreCase);
                receptionist.Firstdayofwork = model.JoinTime.Value;
                receptionist.Avatar = model.AvatarPath;

                _receptionistService.Update(receptionist);
                return RedirectToAction("Manage", "Employee", new { role = "Receptionist" });
            }

            return RedirectToAction("Edit", new { id = model.id });
        }

        [HttpGet]
        public IActionResult Fire(string id)
        {
            if (id.Contains("D"))
            {
                var doctor = _doctorService.Get(id);
                if (doctor.Fired == false)
                {
                    doctor.Fired = true;
                }
                else
                {
                    doctor.Fired = false;
                }
                _doctorService.Update(doctor);
                return RedirectToAction("Manage", "Employee", new { role = "Doctor" });
            }
            else if (id.Contains("N"))
            {
                var nurse = _nurseService.Get(id);
                if (nurse.Fired == false)
                {
                    nurse.Fired = true;
                }
                else
                {
                    nurse.Fired = false;
                }
                _nurseService.Update(nurse);
                return RedirectToAction("Manage", "Employee", new { role = "Nurse" });
            }
            else if (id.Contains("R"))
            {
                var receptionist = _receptionistService.Get(id);
                if (receptionist.Fired == false)
                {
                    receptionist.Fired = true;
                }
                else
                {
                    receptionist.Fired = false;
                }
                _receptionistService.Update(receptionist);
                return RedirectToAction("Manage", "Employee", new { role = "Receptionist" });
            }
            return RedirectToAction("Manage", "Employee");
        }

        [HttpGet]
        public IActionResult Manage(string role)
        {
            if (string.IsNullOrEmpty(role))
            { 
                var receptionistList = _receptionistService.GetAll();
                return View(receptionistList);
            }

            if (role.ToLower().Equals("doctor"))
            {
                var doctorList = _doctorService.GetAll();
                return View(doctorList);
            }
            else if (role.ToLower().Equals("nurse"))
            {
                var nurseList = _nurseService.GetAll();
                return View(nurseList);
            }
            else
            {
                var receptionistList = _receptionistService.GetAll();
                return View(receptionistList);
            }
        }

        private async void SendAccountToEmail(string id)
        {
            if (id.Contains("D"))
            {
                Doctor? doctor = _doctorService.Get(id);
                if (doctor != null)
                {
                    string from = "n8cnpm2024@gmail.com";
                    string pass = "ymhp dxyc jgti ohne";
                    string to = doctor.Email;

                    // Nội dung email với HTML
                    string messageBody = $@"
            <html>
            <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px;'>
                <div style='max-width: 600px; margin: auto; background-color: #ffffff; border-radius: 10px; overflow: hidden;'>
                    <div style='background-color: #0066cc; color: white; padding: 20px; text-align: center;'>
                        <h1>Welcome to DentalCare</h1>
                    </div>
                    <div style='padding: 20px;'>
                        <h2 style='color: #0066cc;'>Account Information</h2>
                        <p><strong>Doctor ID:</strong> {doctor.Id}</p>
                        <p><strong>Phone:</strong> {doctor.Phone}</p>
                        <p><strong>Email:</strong> {doctor.Email}</p>
                        <p><strong>Password: is your ID</strong></p>
                        
                        <h2 style='color: #0066cc;'>Personal Information</h2>
                        <p><strong>Full Name:</strong> {doctor.Name}</p>
                        <p><strong>Role:</strong> Doctor</p>
                        <p><strong>Faculty:</strong> {_facultyService.Get(doctor.Falcutyid).Name}</p>
                        <p><strong>Birthday:</strong> {doctor.Birthday.ToString("dd/MM/yyyy")}</p>
                        <p><strong>Gender:</strong> {(doctor.Gender ? "Male" : "Female")}</p>
                        <p><strong>First Day of Work:</strong> {doctor.Firstdayofwork.ToString("dd/MM/yyyy")}</p>
                    </div>
                    <div style='background-color: #0066cc; color: white; text-align: center; padding: 10px;'>
                        <p>Thank you for joining DentalCare! If you have any questions, feel free to contact us.</p>
                        <p>&copy; 2024 DentalCare</p>
                    </div>
                </div>
            </body>
            </html>";

                    MailMessage message = new MailMessage();
                    message.To.Add(to);
                    message.From = new MailAddress(from);
                    message.Body = messageBody;
                    message.Subject = "Account Details - DentalCare";
                    message.IsBodyHtml = true; // Để kích hoạt HTML trong email

                    SmtpClient smtp = new SmtpClient("smtp.gmail.com");
                    smtp.EnableSsl = true;
                    smtp.Port = 587;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.Credentials = new NetworkCredential(from, pass);

                    try
                    {
                        smtp.Send(message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error sending email: " + ex.Message);
                    }
                }
            }
            else if (id.Contains("R"))
            {
                Receptionist? receptionist = _receptionistService.Get(id);
                if (receptionist != null)
                {
                    string from = "n8cnpm2024@gmail.com";
                    string pass = "ymhp dxyc jgti ohne";
                    string to = receptionist.Email.Trim();

                    // Nội dung email với HTML
                    string messageBody = $@"
            <html>
            <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px;'>
                <div style='max-width: 600px; margin: auto; background-color: #ffffff; border-radius: 10px; overflow: hidden;'>
                    <div style='background-color: #0066cc; color: white; padding: 20px; text-align: center;'>
                        <h1>Welcome to DentalCare</h1>
                    </div>
                    <div style='padding: 20px;'>
                        <h2 style='color: #0066cc;'>Account Information</h2>
                        <p><strong>Doctor ID:</strong> {receptionist.Id}</p>
                        <p><strong>Phone:</strong> {receptionist.Phone}</p>
                        <p><strong>Email:</strong> {receptionist.Email}</p>
                        <p><strong>Password: is your ID</strong></p

                        <h2 style='color: #0066cc;'>Personal Information</h2>
                        <p><strong>Full Name:</strong> {receptionist.Name}</p>
                        <p><strong>Role:</strong> Receptionist</p>
                        <p><strong>Birthday:</strong> {receptionist.Birthday.ToString("dd/MM/yyyy")}</p>
                        <p><strong>Gender:</strong> {(receptionist.Gender ? "Male" : "Female")}</p>
                        <p><strong>First Day of Work:</strong> {receptionist.Firstdayofwork.ToString("dd/MM/yyyy")}</p>
                    </div>
                    <div style='background-color: #0066cc; color: white; text-align: center; padding: 10px;'>
                        <p>Thank you for joining DentalCare!</p>
                        <p>&copy; 2024 DentalCare</p>
                    </div>
                </div>
            </body>
            </html>";

                    MailMessage message = new MailMessage();
                    message.To.Add(to);
                    message.From = new MailAddress(from);
                    message.Body = messageBody;
                    message.Subject = "Account Details - DentalCare";
                    message.IsBodyHtml = true; // Để kích hoạt HTML trong email

                    SmtpClient smtp = new SmtpClient("smtp.gmail.com");
                    smtp.EnableSsl = true;
                    smtp.Port = 587;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.Credentials = new NetworkCredential(from, pass);

                    try
                    {
                        smtp.Send(message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error sending email: " + ex.Message);
                    }
                }
            }
        }

    }
}
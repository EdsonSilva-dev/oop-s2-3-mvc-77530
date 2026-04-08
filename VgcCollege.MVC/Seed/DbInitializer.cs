using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Domain.Entities;
using VgcCollege.Domain.Enums;
using VgcCollege.MVC.Data;
using VgcCollege.MVC.Models;

namespace VgcCollege.MVC.Seed
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(
            IServiceProvider serviceProvider,
            IConfiguration configuration)
        {
            using var scope = serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await context.Database.MigrateAsync();

            await SeedRolesAsync(roleManager);
            await SeedUsersAndProfilesAsync(userManager, context);
            await SeedAcademicDataAsync(context);
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            var roles = new[]
            {
                UserRoles.Admin,
                UserRoles.Faculty,
                UserRoles.Student
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private static async Task SeedUsersAndProfilesAsync(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            var adminUser = await EnsureUserAsync(
                userManager,
                email: "admin@vgc.ie",
                password: "Admin123!",
                displayName: "System Admin",
                role: UserRoles.Admin);

            var facultyUser1 = await EnsureUserAsync(
                userManager,
                email: "faculty1@vgc.ie",
                password: "Faculty123!",
                displayName: "Sarah Collins",
                role: UserRoles.Faculty);

            var facultyUser2 = await EnsureUserAsync(
                userManager,
                email: "faculty2@vgc.ie",
                password: "Faculty123!",
                displayName: "Michael Byrne",
                role: UserRoles.Faculty);

            var studentUser1 = await EnsureUserAsync(
                userManager,
                email: "student1@vgc.ie",
                password: "Student123!",
                displayName: "Emma Doyle",
                role: UserRoles.Student);

            var studentUser2 = await EnsureUserAsync(
                userManager,
                email: "student2@vgc.ie",
                password: "Student123!",
                displayName: "Liam Murphy",
                role: UserRoles.Student);

            var studentUser3 = await EnsureUserAsync(
                userManager,
                email: "student3@vgc.ie",
                password: "Student123!",
                displayName: "Sophie Walsh",
                role: UserRoles.Student);

            if (!await context.FacultyProfiles.AnyAsync())
            {
                context.FacultyProfiles.AddRange(
                    new FacultyProfile
                    {
                        ApplicationUserId = facultyUser1.Id,
                        FullName = "Sarah Collins",
                        Email = "faculty1@vgc.ie",
                        Phone = "0851111111",
                        Specialization = "Software Engineering"
                    },
                    new FacultyProfile
                    {
                        ApplicationUserId = facultyUser2.Id,
                        FullName = "Michael Byrne",
                        Email = "faculty2@vgc.ie",
                        Phone = "0852222222",
                        Specialization = "Business Analytics"
                    }
                );
            }

            if (!await context.StudentProfiles.AnyAsync())
            {
                context.StudentProfiles.AddRange(
                    new StudentProfile
                    {
                        ApplicationUserId = studentUser1.Id,
                        StudentNumber = "STU001",
                        FullName = "Emma Doyle",
                        Email = "student1@vgc.ie",
                        Phone = "0861111111",
                        Address = "Dublin 1",
                        DateOfBirth = new DateTime(2001, 5, 12)
                    },
                    new StudentProfile
                    {
                        ApplicationUserId = studentUser2.Id,
                        StudentNumber = "STU002",
                        FullName = "Liam Murphy",
                        Email = "student2@vgc.ie",
                        Phone = "0862222222",
                        Address = "Cork City",
                        DateOfBirth = new DateTime(2000, 9, 20)
                    },
                    new StudentProfile
                    {
                        ApplicationUserId = studentUser3.Id,
                        StudentNumber = "STU003",
                        FullName = "Sophie Walsh",
                        Email = "student3@vgc.ie",
                        Phone = "0863333333",
                        Address = "Galway",
                        DateOfBirth = new DateTime(2002, 2, 8)
                    }
                );
            }

            await context.SaveChangesAsync();
        }

        private static async Task<ApplicationUser> EnsureUserAsync(
            UserManager<ApplicationUser> userManager,
            string email,
            string password,
            string displayName,
            string role)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    DisplayName = displayName,
                    EmailConfirmed = true,
                    IsActive = true
                };

                var createResult = await userManager.CreateAsync(user, password);

                if (!createResult.Succeeded)
                {
                    var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to create user {email}: {errors}");
                }
            }

            if (!await userManager.IsInRoleAsync(user, role))
            {
                await userManager.AddToRoleAsync(user, role);
            }

            return user;
        }

        private static async Task SeedAcademicDataAsync(ApplicationDbContext context)
        {
            if (!await context.Branches.AnyAsync())
            {
                context.Branches.AddRange(
                    new Branch { Name = "Dublin", Address = "10 O'Connell Street, Dublin" },
                    new Branch { Name = "Cork", Address = "25 Patrick Street, Cork" },
                    new Branch { Name = "Galway", Address = "8 Eyre Square, Galway" }
                );

                await context.SaveChangesAsync();
            }

            if (!await context.Courses.AnyAsync())
            {
                var dublin = await context.Branches.FirstAsync(b => b.Name == "Dublin");
                var cork = await context.Branches.FirstAsync(b => b.Name == "Cork");
                var galway = await context.Branches.FirstAsync(b => b.Name == "Galway");

                context.Courses.AddRange(
                    new Course
                    {
                        Code = "SE101",
                        Name = "Software Engineering",
                        Description = "Core software engineering concepts and practice.",
                        BranchId = dublin.Id,
                        StartDate = new DateTime(2026, 1, 15),
                        EndDate = new DateTime(2026, 6, 30),
                        IsActive = true
                    },
                    new Course
                    {
                        Code = "BA201",
                        Name = "Business Analytics",
                        Description = "Introduction to business analytics and reporting.",
                        BranchId = cork.Id,
                        StartDate = new DateTime(2026, 1, 20),
                        EndDate = new DateTime(2026, 6, 25),
                        IsActive = true
                    },
                    new Course
                    {
                        Code = "DF301",
                        Name = "Data Fundamentals",
                        Description = "Database and data handling fundamentals.",
                        BranchId = galway.Id,
                        StartDate = new DateTime(2026, 2, 1),
                        EndDate = new DateTime(2026, 7, 15),
                        IsActive = true
                    }
                );

                await context.SaveChangesAsync();
            }

            if (!await context.FacultyCourseAssignments.AnyAsync())
            {
                var faculty1 = await context.FacultyProfiles.FirstAsync(f => f.Email == "faculty1@vgc.ie");
                var faculty2 = await context.FacultyProfiles.FirstAsync(f => f.Email == "faculty2@vgc.ie");

                var seCourse = await context.Courses.FirstAsync(c => c.Code == "SE101");
                var baCourse = await context.Courses.FirstAsync(c => c.Code == "BA201");
                var dfCourse = await context.Courses.FirstAsync(c => c.Code == "DF301");

                context.FacultyCourseAssignments.AddRange(
                    new FacultyCourseAssignment
                    {
                        FacultyProfileId = faculty1.Id,
                        CourseId = seCourse.Id,
                        IsTutor = true,
                        AssignedDate = DateTime.UtcNow
                    },
                    new FacultyCourseAssignment
                    {
                        FacultyProfileId = faculty1.Id,
                        CourseId = dfCourse.Id,
                        IsTutor = false,
                        AssignedDate = DateTime.UtcNow
                    },
                    new FacultyCourseAssignment
                    {
                        FacultyProfileId = faculty2.Id,
                        CourseId = baCourse.Id,
                        IsTutor = true,
                        AssignedDate = DateTime.UtcNow
                    }
                );

                await context.SaveChangesAsync();
            }

            if (!await context.CourseEnrolments.AnyAsync())
            {
                var student1 = await context.StudentProfiles.FirstAsync(s => s.StudentNumber == "STU001");
                var student2 = await context.StudentProfiles.FirstAsync(s => s.StudentNumber == "STU002");
                var student3 = await context.StudentProfiles.FirstAsync(s => s.StudentNumber == "STU003");

                var seCourse = await context.Courses.FirstAsync(c => c.Code == "SE101");
                var baCourse = await context.Courses.FirstAsync(c => c.Code == "BA201");
                var dfCourse = await context.Courses.FirstAsync(c => c.Code == "DF301");

                context.CourseEnrolments.AddRange(
                    new CourseEnrolment
                    {
                        StudentProfileId = student1.Id,
                        CourseId = seCourse.Id,
                        EnrolDate = new DateTime(2026, 1, 10),
                        Status = EnrolmentStatus.Active
                    },
                    new CourseEnrolment
                    {
                        StudentProfileId = student1.Id,
                        CourseId = dfCourse.Id,
                        EnrolDate = new DateTime(2026, 1, 10),
                        Status = EnrolmentStatus.Active
                    },
                    new CourseEnrolment
                    {
                        StudentProfileId = student2.Id,
                        CourseId = baCourse.Id,
                        EnrolDate = new DateTime(2026, 1, 12),
                        Status = EnrolmentStatus.Active
                    },
                    new CourseEnrolment
                    {
                        StudentProfileId = student3.Id,
                        CourseId = seCourse.Id,
                        EnrolDate = new DateTime(2026, 1, 14),
                        Status = EnrolmentStatus.Active
                    }
                );

                await context.SaveChangesAsync();
            }

            if (!await context.Assignments.AnyAsync())
            {
                var seCourse = await context.Courses.FirstAsync(c => c.Code == "SE101");
                var baCourse = await context.Courses.FirstAsync(c => c.Code == "BA201");
                var dfCourse = await context.Courses.FirstAsync(c => c.Code == "DF301");

                context.Assignments.AddRange(
                    new Assignment
                    {
                        CourseId = seCourse.Id,
                        Title = "MVC Project",
                        Description = "Build a layered MVC web app.",
                        MaxScore = 100,
                        DueDate = new DateTime(2026, 3, 15),
                        Weight = 40
                    },
                    new Assignment
                    {
                        CourseId = seCourse.Id,
                        Title = "Unit Testing Lab",
                        Description = "Create automated tests for business rules.",
                        MaxScore = 100,
                        DueDate = new DateTime(2026, 4, 10),
                        Weight = 20
                    },
                    new Assignment
                    {
                        CourseId = baCourse.Id,
                        Title = "Analytics Report",
                        Description = "Produce a business dashboard analysis.",
                        MaxScore = 100,
                        DueDate = new DateTime(2026, 3, 20),
                        Weight = 35
                    },
                    new Assignment
                    {
                        CourseId = dfCourse.Id,
                        Title = "Database Design",
                        Description = "Design a normalized academic database.",
                        MaxScore = 100,
                        DueDate = new DateTime(2026, 3, 25),
                        Weight = 30
                    }
                );

                await context.SaveChangesAsync();
            }

            if (!await context.Exams.AnyAsync())
            {
                var seCourse = await context.Courses.FirstAsync(c => c.Code == "SE101");
                var baCourse = await context.Courses.FirstAsync(c => c.Code == "BA201");
                var dfCourse = await context.Courses.FirstAsync(c => c.Code == "DF301");

                context.Exams.AddRange(
                    new Exam
                    {
                        CourseId = seCourse.Id,
                        Title = "SE Final Exam",
                        ExamDate = new DateTime(2026, 5, 30),
                        MaxScore = 100,
                        ResultsReleased = false,
                        Weight = 40
                    },
                    new Exam
                    {
                        CourseId = baCourse.Id,
                        Title = "BA Final Exam",
                        ExamDate = new DateTime(2026, 5, 28),
                        MaxScore = 100,
                        ResultsReleased = true,
                        Weight = 45
                    },
                    new Exam
                    {
                        CourseId = dfCourse.Id,
                        Title = "DF Final Exam",
                        ExamDate = new DateTime(2026, 6, 5),
                        MaxScore = 100,
                        ResultsReleased = true,
                        Weight = 50
                    }
                );

                await context.SaveChangesAsync();
            }

            if (!await context.AttendanceRecords.AnyAsync())
            {
                var enrolments = await context.CourseEnrolments.ToListAsync();

                foreach (var enrolment in enrolments)
                {
                    context.AttendanceRecords.AddRange(
                        new AttendanceRecord
                        {
                            CourseEnrolmentId = enrolment.Id,
                            SessionDate = new DateTime(2026, 2, 3),
                            WeekNumber = 1,
                            Present = true
                        },
                        new AttendanceRecord
                        {
                            CourseEnrolmentId = enrolment.Id,
                            SessionDate = new DateTime(2026, 2, 10),
                            WeekNumber = 2,
                            Present = enrolment.StudentProfileId % 2 == 0 ? false : true
                        },
                        new AttendanceRecord
                        {
                            CourseEnrolmentId = enrolment.Id,
                            SessionDate = new DateTime(2026, 2, 17),
                            WeekNumber = 3,
                            Present = true
                        }
                    );
                }

                await context.SaveChangesAsync();
            }

            if (!await context.AssignmentResults.AnyAsync())
            {
                var enrolments = await context.CourseEnrolments.ToListAsync();
                var assignments = await context.Assignments.ToListAsync();

                foreach (var enrolment in enrolments)
                {
                    var courseAssignments = assignments
                        .Where(a => a.CourseId == enrolment.CourseId)
                        .ToList();

                    foreach (var assignment in courseAssignments)
                    {
                        context.AssignmentResults.Add(new AssignmentResult
                        {
                            AssignmentId = assignment.Id,
                            CourseEnrolmentId = enrolment.Id,
                            Score = 70 + (enrolment.Id % 20),
                            Feedback = "Good work overall.",
                            GradedAt = DateTime.UtcNow
                        });
                    }
                }

                await context.SaveChangesAsync();
            }

            if (!await context.ExamResults.AnyAsync())
            {
                var enrolments = await context.CourseEnrolments.ToListAsync();
                var exams = await context.Exams.ToListAsync();

                foreach (var enrolment in enrolments)
                {
                    var courseExams = exams
                        .Where(e => e.CourseId == enrolment.CourseId)
                        .ToList();

                    foreach (var exam in courseExams)
                    {
                        var score = 65 + (enrolment.Id % 25);

                        context.ExamResults.Add(new ExamResult
                        {
                            ExamId = exam.Id,
                            CourseEnrolmentId = enrolment.Id,
                            Score = score,
                            Grade = GetGrade(score),
                            Feedback = "Final exam recorded.",
                            RecordedAt = DateTime.UtcNow
                        });
                    }
                }

                await context.SaveChangesAsync();
            }
        }

        private static string GetGrade(decimal score)
        {
            if (score >= 85) return "A";
            if (score >= 70) return "B";
            if (score >= 55) return "C";
            if (score >= 40) return "D";
            return "F";
        }
    }
}
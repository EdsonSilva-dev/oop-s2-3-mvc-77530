using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Domain.Entities;
using VgcCollege.MVC.Models;

namespace VgcCollege.MVC.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Branch> Branches => Set<Branch>();
        public DbSet<Course> Courses => Set<Course>();
        public DbSet<StudentProfile> StudentProfiles => Set<StudentProfile>();
        public DbSet<FacultyProfile> FacultyProfiles => Set<FacultyProfile>();
        public DbSet<FacultyCourseAssignment> FacultyCourseAssignments => Set<FacultyCourseAssignment>();
        public DbSet<CourseEnrolment> CourseEnrolments => Set<CourseEnrolment>();
        public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
        public DbSet<Assignment> Assignments => Set<Assignment>();
        public DbSet<AssignmentResult> AssignmentResults => Set<AssignmentResult>();
        public DbSet<Exam> Exams => Set<Exam>();
        public DbSet<ExamResult> ExamResults => Set<ExamResult>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Course>()
                .HasIndex(c => c.Code)
                .IsUnique();

            builder.Entity<StudentProfile>()
                .HasIndex(s => s.StudentNumber)
                .IsUnique();

            builder.Entity<CourseEnrolment>()
                .HasIndex(e => new { e.StudentProfileId, e.CourseId })
                .IsUnique();

            builder.Entity<StudentProfile>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(s => s.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<FacultyProfile>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(f => f.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Course>()
                .HasOne(c => c.Branch)
                .WithMany(b => b.Courses)
                .HasForeignKey(c => c.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CourseEnrolment>()
                .HasOne(e => e.StudentProfile)
                .WithMany(s => s.Enrolments)
                .HasForeignKey(e => e.StudentProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CourseEnrolment>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Enrolments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<FacultyCourseAssignment>()
                .HasOne(fca => fca.FacultyProfile)
                .WithMany(fp => fp.CourseAssignments)
                .HasForeignKey(fca => fca.FacultyProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<FacultyCourseAssignment>()
                .HasOne(fca => fca.Course)
                .WithMany(c => c.FacultyAssignments)
                .HasForeignKey(fca => fca.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<AttendanceRecord>()
                .HasOne(a => a.CourseEnrolment)
                .WithMany(e => e.AttendanceRecords)
                .HasForeignKey(a => a.CourseEnrolmentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Assignment>()
                .HasOne(a => a.Course)
                .WithMany(c => c.Assignments)
                .HasForeignKey(a => a.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<AssignmentResult>()
                .HasOne(ar => ar.Assignment)
                .WithMany(a => a.Results)
                .HasForeignKey(ar => ar.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AssignmentResult>()
                .HasOne(ar => ar.CourseEnrolment)
                .WithMany(e => e.AssignmentResults)
                .HasForeignKey(ar => ar.CourseEnrolmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Exam>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Exams)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExamResult>()
                .HasOne(er => er.Exam)
                .WithMany(e => e.Results)
                .HasForeignKey(er => er.ExamId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ExamResult>()
                .HasOne(er => er.CourseEnrolment)
                .WithMany(e => e.ExamResults)
                .HasForeignKey(er => er.CourseEnrolmentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
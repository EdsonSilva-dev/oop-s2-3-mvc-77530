using VgcCollege.Domain.Entities;
using VgcCollege.Domain.Services;
using Xunit;

namespace VgcCollege.Tests.Services
{
    public class FacultyAccessServiceTests
    {
        [Fact]
        public void FilterEnrolmentsForFaculty_ShouldReturnOnlyAssignedCourses()
        {
            var service = new FacultyAccessService();

            var enrolments = new List<CourseEnrolment>
            {
                new CourseEnrolment { Id = 1, CourseId = 10, StudentProfileId = 1 },
                new CourseEnrolment { Id = 2, CourseId = 20, StudentProfileId = 2 },
                new CourseEnrolment { Id = 3, CourseId = 10, StudentProfileId = 3 }
            };

            var assignedCourseIds = new List<int> { 10 };

            var result = service.FilterEnrolmentsForFaculty(enrolments, assignedCourseIds).ToList();

            Assert.Equal(2, result.Count);
            Assert.All(result, r => Assert.Equal(10, r.CourseId));
        }

        [Fact]
        public void CanFacultyAccessCourse_ShouldReturnTrue_WhenCourseIsAssigned()
        {
            var service = new FacultyAccessService();

            var result = service.CanFacultyAccessCourse(10, new List<int> { 10, 20 });

            Assert.True(result);
        }

        [Fact]
        public void CanFacultyAccessCourse_ShouldReturnFalse_WhenCourseIsNotAssigned()
        {
            var service = new FacultyAccessService();

            var result = service.CanFacultyAccessCourse(30, new List<int> { 10, 20 });

            Assert.False(result);
        }
    }
}
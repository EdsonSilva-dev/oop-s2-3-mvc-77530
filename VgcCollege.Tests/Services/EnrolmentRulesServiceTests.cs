using VgcCollege.Domain.Entities;
using VgcCollege.Domain.Enums;
using VgcCollege.Domain.Services;
using Xunit;

namespace VgcCollege.Tests.Services
{
    public class EnrolmentRulesServiceTests
    {
        [Fact]
        public void CanCreateEnrolment_ShouldReturnTrue_WhenNoDuplicateActiveEnrolmentExists()
        {
            var service = new EnrolmentRulesService();

            var enrolments = new List<CourseEnrolment>
            {
                new CourseEnrolment
                {
                    StudentProfileId = 1,
                    CourseId = 1,
                    Status = EnrolmentStatus.Completed
                }
            };

            var result = service.CanCreateEnrolment(enrolments, 1, 1);

            Assert.True(result);
        }

        [Fact]
        public void CanCreateEnrolment_ShouldReturnFalse_WhenDuplicateActiveEnrolmentExists()
        {
            var service = new EnrolmentRulesService();

            var enrolments = new List<CourseEnrolment>
            {
                new CourseEnrolment
                {
                    StudentProfileId = 1,
                    CourseId = 1,
                    Status = EnrolmentStatus.Active
                }
            };

            var result = service.CanCreateEnrolment(enrolments, 1, 1);

            Assert.False(result);
        }
    }
}
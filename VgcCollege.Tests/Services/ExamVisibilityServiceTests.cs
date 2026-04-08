using VgcCollege.Domain.Entities;
using VgcCollege.Domain.Services;
using Xunit;

namespace VgcCollege.Tests.Services
{
    public class ExamVisibilityServiceTests
    {
        [Fact]
        public void CanStudentSeeExamResult_ShouldReturnTrue_WhenResultsAreReleased()
        {
            var service = new ExamVisibilityService();

            var examResult = new ExamResult
            {
                Exam = new Exam
                {
                    ResultsReleased = true
                }
            };

            var result = service.CanStudentSeeExamResult(examResult);

            Assert.True(result);
        }

        [Fact]
        public void CanStudentSeeExamResult_ShouldReturnFalse_WhenResultsAreNotReleased()
        {
            var service = new ExamVisibilityService();

            var examResult = new ExamResult
            {
                Exam = new Exam
                {
                    ResultsReleased = false
                }
            };

            var result = service.CanStudentSeeExamResult(examResult);

            Assert.False(result);
        }

        [Fact]
        public void CanStudentSeeExamResult_ShouldReturnFalse_WhenExamIsNull()
        {
            var service = new ExamVisibilityService();

            var examResult = new ExamResult
            {
                Exam = null
            };

            var result = service.CanStudentSeeExamResult(examResult);

            Assert.False(result);
        }
    }
}
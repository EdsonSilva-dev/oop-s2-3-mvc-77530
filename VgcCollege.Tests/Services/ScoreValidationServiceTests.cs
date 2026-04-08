using VgcCollege.Domain.Services;
using Xunit;

namespace VgcCollege.Tests.Services
{
    public class ScoreValidationServiceTests
    {
        [Theory]
        [InlineData(0, 100, true)]
        [InlineData(50, 100, true)]
        [InlineData(100, 100, true)]
        [InlineData(-1, 100, false)]
        [InlineData(101, 100, false)]
        public void IsValidScore_ShouldValidateScoreCorrectly(decimal score, decimal maxScore, bool expected)
        {
            var service = new ScoreValidationService();

            var result = service.IsValidScore(score, maxScore);

            Assert.Equal(expected, result);
        }
    }
}
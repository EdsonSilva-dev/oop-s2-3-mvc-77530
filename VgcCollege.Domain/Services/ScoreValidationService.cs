using System;
using System.Collections.Generic;
using System.Text;

namespace VgcCollege.Domain.Services
{
    public class ScoreValidationService
    {
        public bool IsValidScore(decimal score, decimal maxScore)
        {
            return score >= 0 && score <= maxScore;
        }
    }
}

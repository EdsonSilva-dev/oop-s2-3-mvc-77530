using System;
using System.Collections.Generic;
using System.Text;
using VgcCollege.Domain.Entities;

namespace VgcCollege.Domain.Services
{
    public class ExamVisibilityService
    {
        public bool CanStudentSeeExamResult(ExamResult examResult)
        {
            return examResult.Exam != null && examResult.Exam.ResultsReleased;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using VgcCollege.Domain.Entities;
using VgcCollege.Domain.Enums;

namespace VgcCollege.Domain.Services
{
    public class EnrolmentRulesService
    {
        public bool CanCreateEnrolment(
            IEnumerable<CourseEnrolment> existingEnrolments,
            int studentProfileId,
            int courseId)
        {
            return !existingEnrolments.Any(e =>
                e.StudentProfileId == studentProfileId &&
                e.CourseId == courseId &&
                e.Status == EnrolmentStatus.Active);
        }
    }
}

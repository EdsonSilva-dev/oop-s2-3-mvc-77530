using System;
using System.Collections.Generic;
using System.Text;

using VgcCollege.Domain.Entities;

namespace VgcCollege.Domain.Services
{
    public class FacultyAccessService
    {
        public IEnumerable<CourseEnrolment> FilterEnrolmentsForFaculty(
            IEnumerable<CourseEnrolment> enrolments,
            IEnumerable<int> assignedCourseIds)
        {
            var assignedSet = assignedCourseIds.ToHashSet();

            return enrolments.Where(e => assignedSet.Contains(e.CourseId));
        }

        public bool CanFacultyAccessCourse(int courseId, IEnumerable<int> assignedCourseIds)
        {
            return assignedCourseIds.Contains(courseId);
        }
    }
}

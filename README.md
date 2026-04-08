# oop-s2-3-mvc-77530

# VGC College Management System

ASP.NET Core MVC application for managing branches, courses, students, enrolments, attendance, assignments, exams, and academic progress across multiple college branches.

## Tech Stack

- ASP.NET Core MVC (.NET 10)
- Entity Framework Core
- SQL Server
- ASP.NET Core Identity
- xUnit
- GitHub Actions

## Features

### Admin
- Manage branches
- Manage courses
- Manage student profiles
- Manage enrolments
- Manage attendance
- Manage assignments and assignment results
- Manage exams and exam results
- Release / hide exam results

### Faculty
- View only assigned courses
- View only assigned students
- Manage attendance for assigned courses
- Manage assignment results for assigned courses
- Manage exam results for assigned courses

### Student
- View own profile
- View own courses
- View own attendance
- View own assignment results
- View only released exam results

## Project Structure

```text
VgcCollege.Domain/
VgcCollege.MVC/
VgcCollege.Tests/
.github/workflows/
README.md

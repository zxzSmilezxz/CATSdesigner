﻿using Application.Core.Data;

namespace LMPlatform.Models
{
    public class StudentLabMark : ModelBase
    {
        public int LabId { get; set; }

        public int StudentId { get; set; }

        public string Mark { get; set; }

        public string Comment { get; set; }

        public string Date { get; set; }

        public bool ShowForStudent { get; set; }

        public Labs Lab { get; set; }

        public Student Student { get; set; }

        public int? LecturerId { get; set; }

        public Lecturer Lecturer { get; set; }

        public StudentLabMark()
        {

        }

        public StudentLabMark(int labId, int studentId, int lecturerId, string mark, string comment, string date, int id, bool showForStudent)
        {
            LabId = labId;
            StudentId = studentId;
            LecturerId = lecturerId;
            Mark = mark;
            Comment = comment;
            Date = date;
            Id = id;
            ShowForStudent = showForStudent;
        }
    }
}
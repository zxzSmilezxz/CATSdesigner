namespace Application.ElasticDataModels
{
    using Application.Core.Data;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Lecturer : ModelBase
    {
        public User User { get; set; }

        public string Skill { get; set; }

        [NotMapped]
        public string FullName => $"{LastName} {FirstName} {MiddleName}";

       
        public string FirstName { get; set; }
       
        public string LastName { get; set; }
        
        public string MiddleName { get; set; }

        [JsonIgnore]
        public bool IsSecretary { get; set; }

       
        [JsonIgnore]
        public bool? IsActive { get; set; }

        [JsonIgnore]
        public virtual ICollection<Group> Groups { get; set; }

        [JsonIgnore]
        public bool IsLecturerHasGraduateStudents { get; set; }




    }
}
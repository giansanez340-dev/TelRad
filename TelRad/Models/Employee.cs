using System.ComponentModel.DataAnnotations;

namespace TelRad.Models
{
    public class Employee
    {
        public int Id { get; set; }

        [Required]
        public string? FullName { get; set; } = "";

        [Required]
        public string? Branch { get; set; } = "";

        [Required]
        public string? Department { get; set; } = "";

        [Required]
        public string? AssignedTelrad { get; set; } = "";

        public string? NearestTelrad { get; set; } = "";

        public bool IsMainHandler { get; set; } = false;
    }
}
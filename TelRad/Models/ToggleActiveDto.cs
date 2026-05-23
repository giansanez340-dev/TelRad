namespace TelRad.Models
{
    public class ToggleActiveDto
    {
        public int EmployeeId { get; set; }
        public bool IsActive { get; set; }
        public string? AssignedTelrad { get; set; }  // <-- add this
    }
}

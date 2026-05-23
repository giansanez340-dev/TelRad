namespace TelRad.Models
{
    public class UpdateActiveStatusRequest
    {
        public int EmployeeId { get; set; }
        public bool IsActive { get; set; }
        public string AssignedTelrad { get; set; } = "";
    }
}

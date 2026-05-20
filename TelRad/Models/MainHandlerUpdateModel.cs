namespace TelRad.Models
{
    public class MainHandlerUpdateModel
    {
        public int EmployeeId { get; set; }
        public bool IsMainHandler { get; set; }
        public string? AssignedTelrad { get; set; }
    }
}

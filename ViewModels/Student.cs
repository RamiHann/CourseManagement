namespace CourseManagement.ViewModels
{
    public class Student
    {
        public string Name { get; set; }
        public string LastName { get; set; }
        public string ZehutNumber { get; set; }
        public int Year { get; set; }
        public List<TaskViewModel> Tasks { get; set; }
        public string FullName { get; set; }
    }

}

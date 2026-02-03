namespace ExaminationSystem.ViewModel
{
    public class CourseTopicVm
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Order { get; set; }
        public bool IsRequired { get; set; }
    }
}

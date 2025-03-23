namespace WebBaiGiang.Models
{
    public class HomeViewModel
    {
        public List<Course> FeaturedCourses { get; set; }
        public List<ClassCourse> Courses { get; set; }
        public List<Course> Recommendations { get; set; }
        public List<Feedback> Feedbacks { get; set; }
    }
}

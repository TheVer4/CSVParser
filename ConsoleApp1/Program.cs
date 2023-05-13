using Secret;

namespace ConsoleApp1
{
    class Program
    {
        public static void Main(string[] args)
        {
            List<Student> students = CsvParser.Parse<Student>("students.csv");
            foreach (Student student in students)
            {
                Console.WriteLine(student.ToString());
            }
        }
    }
}

namespace Secret {

    class Student
    {
        private int id;
        public string studentName, groupName;
        public int age;

        public override string ToString()
        {
            return
                $"{nameof(id)}: {id}, {nameof(studentName)}: {studentName}, {nameof(groupName)}: {groupName}, {nameof(age)}: {age}";
        }
    }
}
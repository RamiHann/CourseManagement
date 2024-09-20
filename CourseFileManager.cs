using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Dynamic;
using CourseManagement.ViewModels;
using CsvHelper;
using System.Globalization;
using CsvHelper.Configuration;

namespace CourseManagement
{

    // Manages data storage and retrieval for courses in a file-based system
    public class CourseFileManager
    {
        // Path to the directory where course data files are stored
        private string _directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CourseStorage");

        // Constructor
        public CourseFileManager()
        {
            // Ensure the directory exists; if not, create it
            if (!Directory.Exists(_directoryPath))
            {
                Directory.CreateDirectory(_directoryPath);
            }
        }

        // Retrieves all available courses from the file system
        public ObservableCollection<dynamic> GetAllCourses()
        {
            var courses = new ObservableCollection<dynamic>();

            // Retrieve all files in the directory
            var files = Directory.GetFiles(_directoryPath);

            // Deserialize each file into a dynamic object and add it to the collection
            foreach (var file in files)
            {
                var serializedData = File.ReadAllText(file);

                dynamic records = JsonConvert.DeserializeObject<dynamic>(serializedData);
                courses.Add(records);
            }

            return courses;
        }

        // Retrieves course data from a specific file
        public dynamic GetCourse(string fileName)
        {
            // Check if the file exists
            var filePath = Path.Combine(_directoryPath, fileName);
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("The specified course file was not found.", fileName);
            }
            var serializedData = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<dynamic>(serializedData);
        }

        public void InitSave(dynamic course,  string fileName)
        {

                if (course is null)
                {
                    return;
                }
                dynamic newCourse = new ExpandoObject();
                newCourse.FileName = fileName;
                foreach(dynamic c in course)
                {
                    var data = c as IDictionary<string, object>;
                    if (data != null && data.Count > 0)
                    {
                        var grades = data.Skip(4)
                                            .Select(kv => new { Name = kv.Key, Value = kv.Value })
                                            .ToList();
                        foreach(var grade in grades)
                        {
                            data.Remove(grade.Name);
                        }

                        c.Tasks = grades;
                    }
                    c.FullName = $"{c.Name} {c.LastName}";
                  
                 }

            newCourse.Students = course;
                // Serialize the course object into JSON
                var serializedData = JsonConvert.SerializeObject(newCourse);

                // Determine the file path based on the course name
                var filePath = Path.Combine(_directoryPath, $"{fileName}.json");

                // Write the serialized data to the file
                File.WriteAllText(filePath, serializedData);
            
        }


        // Stores course data to a file and updates the file name with a timestamp after saving
        public void SaveCourse(dynamic course, string filename)
        {
            if (course == null)
            {
                return;
            }

            // Determine the base file path without timestamp
            var baseFilePath = Path.Combine(_directoryPath, $"{filename}.json");

            // Find the most recent file matching the base filename pattern
            var files = Directory.GetFiles(_directoryPath, $"{filename}*.json");
            var mostRecentFile = files.OrderByDescending(f => f).FirstOrDefault() ?? baseFilePath;

            // Create a new course object to be saved
            dynamic newCourse = new ExpandoObject();
            newCourse.FileName = filename;
            newCourse.Students = course.Students;

            // Serialize the course object into JSON
            var serializedData = JsonConvert.SerializeObject(newCourse);

            // Write the serialized data to the most recent file found or the base file
            File.WriteAllText(mostRecentFile, serializedData);

            // Generate a new file name with the current timestamp
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HHmmss");
            var newFileName = $"{filename}{timestamp}.json";
            var newFilePath = Path.Combine(_directoryPath, newFileName);

            // Rename the existing file with the timestamp
            if (File.Exists(newFilePath))
            {
                File.Delete(newFilePath);  // Ensures there is no name conflict by deleting the target if it exists
            }
            if (mostRecentFile != newFilePath) // Prevent renaming if the filename would be the same
            {
                File.Move(mostRecentFile, newFilePath);
            }
        }


        public object ReadCsv(string filePath)
        {
            var students = new List<Student>();
            CsvConfiguration conf = new CsvConfiguration(CultureInfo.InvariantCulture);

            conf.HasHeaderRecord = true;
            conf.MissingFieldFound = null;

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, conf))
            {
              
                csv.Read();
                csv.ReadHeader();
                var headers = csv.HeaderRecord;
                while (csv.Read())
                {
                    var student = new Student
                    {
                        Name = csv.GetField<string>("Name"),
                        LastName = csv.GetField<string>("LastName"),
                        ZehutNumber = csv.GetField<string>("ZehutNumber"),
                        Year = csv.GetField<int>("Year"),
                        FullName = csv.GetField<string>("Name") + " " +  csv.GetField<string>("LastName"),
                        Tasks = new List<TaskViewModel>()
                    };

                    foreach (var header in headers.Where(h => h != "Name" && h != "LastName" && h != "ZehutNumber" && h != "Year"))
                    {
                        var taskGrade = csv.GetField<int>(header);
                        student.Tasks.Add(new TaskViewModel(header, taskGrade));
                    }

                    students.Add(student);
                }
            }
            students = students.OrderBy(s => s.Name).ToList();
            return new { FileName = Path.GetFileNameWithoutExtension(filePath), Students = students };
        }

        public void SaveToJson(object data)
        {
            var serializedData = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(Path.Combine(_directoryPath, ((dynamic)data).FileName + ".json"), serializedData);
        }
    }

}


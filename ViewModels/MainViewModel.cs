using CommunityToolkit.Mvvm.Input;
using CsvHelper.Configuration;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CourseManagement.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private CourseFileManager _manager;
        public ICommand LoadCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand FactoryCommand { get; }

        public MainViewModel()
        {
            _manager = new CourseFileManager();
            LoadCommand = new RelayCommand(LoadData);
            SaveCommand = new RelayCommand<FrameworkElement>(SaveData, CanSaveData);
            FactoryCommand = new RelayCommand(OpenFactor);

            Courses = _manager.GetAllCourses();
            if (Courses.Count > 0)
            {
                SelectedCourse = Courses[0];
            }
        }

        private void OpenFactor()
        {
            var vm = new FactorialViewModel(CourseName, SelectedCourse);
            var f = new FactorialWindow()
            {
                DataContext = vm
            };

            vm.SaveEvent += Model_SaveEvent;
            vm.OnRequestClose += (s, e) => f.Close();
            f.Show();
        }

        private void Model_SaveEvent(object? sender, EventArgs e)
        {
            if (sender is FactorialViewModel viewModel)
            {
                SaveData(null); // Pass null if not checking for validation here
                viewModel.SaveEvent -= Model_SaveEvent;
            }
        }

        private string _courseName = string.Empty;

        public string CourseName
        {
            get => _courseName;
            set
            {
                _courseName = value;
                OnPropertyChanged();
            }
        }

        private string _centerText = "Course Name (Final Grades Average)";

        public string CenterText
        {
            get => _centerText;
            set
            {
                _centerText = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<dynamic> _courses;

        public ObservableCollection<dynamic> Courses
        {
            get => _courses;
            set
            {
                _courses = value;
                OnPropertyChanged();
                try
                {
                    var student = value[0]?.Students["Students"];
                    SelectedStudent = value.Count == 0 ? new ExpandoObject() : student[0] ?? new ExpandoObject();
                }
                catch (Exception)
                {
                    return;
                }
            }
        }

        private dynamic _selectedCourse;

        public dynamic SelectedCourse
        {
            get => _selectedCourse;
            set
            {
                _selectedCourse = value;
                OnPropertyChanged();
                CenterText = value is null ? "Course" : value.FileName + ": AVG Grade: " + CalculateAverageGrade().ToString(".0");

                try
                {
                    CourseName = value?.FileName ?? "No Course loaded";
                }
                catch { }
            }
        }

        private string _filePath;

        public string FilePath
        {
            get => _filePath;
            set
            {
                _filePath = value;
                OnPropertyChanged();
            }
        }

        private dynamic _selectedStudent;

        public dynamic SelectedStudent
        {
            get => _selectedStudent;
            set
            {
                _selectedStudent = value;
                FinalGrade = nameof(FinalGrade) + ": " + CalculateFinalGrade();
                OnPropertyChanged();
            }
        }

        private void SaveData(FrameworkElement element)
        {
            if (element != null && HasValidationError(element))
            {
                MessageBox.Show("Please fix validation errors before saving.");
                return;
            }

            _manager.SaveCourse(SelectedCourse, CourseName);
            MessageBox.Show("Save successful!");
        }

        private bool CanSaveData(FrameworkElement element)
        {
            return element == null || !HasValidationError(element);
        }

        private bool HasValidationError(DependencyObject element)
        {
            if (Validation.GetHasError(element))
                return true;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(element, i);
                if (HasValidationError(child))
                    return true;
            }
            return false;
        }

        private void LoadData()
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog
                {
                    Title = "Open CSV File",
                    Filter = "CSV Files (*.csv)|*.csv"
                };

                if (dialog.ShowDialog().Value)
                {
                    var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        HasHeaderRecord = true,
                        MissingFieldFound = null
                    };

                    var studentModel = _manager.ReadCsv(dialog.FileName);
                    FilePath = dialog.FileName;
                    _manager.SaveToJson(studentModel);

                    // Initialize and set the newly loaded course as the selected course
                    int newCourseIndex = _manager.GetAllCourses().Count;
                    Init(newCourseIndex);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Init(int selectedIndex = 0)
        {
            Courses = _manager.GetAllCourses();
            if (Courses.Count > selectedIndex)
            {
                SelectedCourse = Courses[selectedIndex];
            }
            else if (Courses.Count > 0)
            {
                SelectedCourse = Courses[0];
            }
        }

        public double CalculateAverageGrade()
        {
            if (Courses == null || Courses.Count == 0)
                return 0.0;

            int totalGrades = 0;
            double sumGrades = 0.0;

            foreach (var course in Courses)
            {
                if (course.Students != null)
                {
                    foreach (var student in course.Students)
                    {
                        if (student.Tasks != null)
                        {
                            foreach (dynamic task in student.Tasks)
                            {
                                try
                                {
                                    dynamic value = task.Value.ToString();
                                    sumGrades += double.Parse(value);
                                    totalGrades++;
                                }
                                catch (Exception)
                                {
                                    // Ignore
                                }
                            }
                        }
                    }
                }
            }

            return totalGrades > 0 ? sumGrades / totalGrades : 0.0;
        }

        private string _finalGrade;
        public string FinalGrade
        {
            get { return _finalGrade; }
            set { _finalGrade = value; OnPropertyChanged(); }
        }

        private string CalculateFinalGrade()
        {
            try
            {
                if (SelectedStudent is null)
                {
                    return string.Empty;
                }
                double totalGrade = 0;
                double totalWeight = 0;

                foreach (dynamic task in SelectedStudent.Tasks)
                {
                    string taskName = task.Name;
                    double taskWeight = ParseTaskWeight(taskName);
                    var value = task.Value.ToString();
                    if (!double.TryParse(value, out double newValue))
                    {
                        return string.Empty;
                    }
                    double taskGrade = newValue;

                    totalGrade += taskWeight * taskGrade;
                    totalWeight += taskWeight;
                }

                double finalGrade = totalWeight > 0 ? totalGrade / totalWeight : 0;
                return finalGrade.ToString(".0");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return string.Empty;
            }
        }

        private double ParseTaskWeight(string taskName)
        {
            var parts = taskName.Split('-');
            if (parts.Length == 2)
            {
                string weightString = parts[1].Trim('%');
                if (double.TryParse(weightString, out double weight))
                {
                    return weight / 100;
                }
            }
            // Default weight if parsing fails
            return 0.0;
        }
    }
}

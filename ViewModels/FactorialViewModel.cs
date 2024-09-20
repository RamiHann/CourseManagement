using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CourseManagement.ViewModels
{
    public class FactorialViewModel : BaseViewModel
    {
        private dynamic _course;

        public dynamic Course
        {
            get { return _course; }
            set
            {
                _course = value;
            }
        }

        private ObservableCollection<dynamic> _tasks;

        public ObservableCollection<dynamic> Tasks
        {
            get { return _tasks; }
            set
            {
                _tasks = value;
                initialized = true;
                OnPropertyChanged();
            }
        }
        private dynamic _selectedTask;

        public dynamic SelectedTask
        {
            get { return _selectedTask; }
            set
            {
                _selectedTask = value;
                OnPropertyChanged();
            }
        }

        private int _factor = 0;

        public int Factor
        {
            get { return _factor; }
            set
            {
                if(value < 0 || value > 100)
                {
                    return;
                }
                _factor = value;

                OnPropertyChanged();
            }
        }


        private bool initialized = false;
        public string CourseName { get; }
        public ICommand SaveCommand { get; set; }
        public event EventHandler SaveEvent;
        public event EventHandler OnRequestClose;
        public FactorialViewModel(string courseName, dynamic selectedCourse)
        {
            Course = selectedCourse;
            CourseName = courseName;
            SaveCommand = new RelayCommand(Save);

            Tasks = new ObservableCollection<dynamic>();
            HashSet<string> uniqueTaskNames = new HashSet<string>();
            foreach (var student in selectedCourse.Students)
            {

                foreach (var task in student.Tasks)
                {
                    string taskName = task.Name;
                    if (uniqueTaskNames.Add(taskName))
                    {
                        Tasks.Add(task);
                    }

                }
            }

        }

        private void Save()
        {

            if (SelectedTask != null && Factor != 0)
            {
                string selectedTaskName = SelectedTask.Name.ToString();
                foreach (var student in Course.Students)
                {
                    foreach (var task in student.Tasks)
                    {
                        if (task.Name == selectedTaskName)
                        {
                            int currentValue = int.Parse(task.Value.ToString());
                            int newValue = currentValue + Factor;
                            newValue = Math.Min(newValue, 100);
                            task.Value = newValue.ToString();
                            break;
                        }
                    }
                }
                SaveEvent?.Invoke(this, EventArgs.Empty);
                OnRequestClose?.Invoke(this, EventArgs.Empty);
            
            }
        }
    }
}

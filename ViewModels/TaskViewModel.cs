namespace CourseManagement.ViewModels
{
    public class TaskViewModel : BaseViewModel
    {
        public TaskViewModel(string name, int value)
        {
            Name = name;
       
            Value = value;
           
        }
        public TaskViewModel()
        {
            Name = string.Empty;
        }
        private string _name;

        public string Name
        {
            get { return _name; }
            set 
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        private int _value;

        public int Value
        {
            get { return _value; }
            set 
            {
                _value = value; 
                OnPropertyChanged();
            }
        }


    }
}

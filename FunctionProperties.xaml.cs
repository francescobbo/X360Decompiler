using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace X360Decompiler
{
    /// <summary>
    /// Interaction logic for Function.xaml
    /// </summary>
    public partial class FunctionProperties : Window
    {
        Function func;

        public class ListViewArgument
        {
            public ListViewArgument(String name, CType t)
            {
                Name = name;
                TypeClass = t;
            }

            public String Type { get { return TypeClass.ToString(); } }
            public String Name { get; set; }

            public CType TypeClass;
        };

        ObservableCollection<ListViewArgument> _ArgCollection = new ObservableCollection<ListViewArgument>();
        public ObservableCollection<ListViewArgument> ArgCollection { get { return _ArgCollection; } }

        public FunctionProperties(Function f)
        {
            func = f;
            InitializeComponent();

            FuncName.Text = f.Name;
            Address.Text = "0x" + f.Address.ToString("X8");

            if (func.ArgCount != -1)
            {
                ArgUnknown.IsChecked = false;
                ArgKnown.IsChecked = true;

                foreach (Variable a in func.Arguments)
                {
                    _ArgCollection.Add(new ListViewArgument(a.Name, a.Type));
                }
            }

            if (func.Returns.Kind == CType.TypeKind.Pointer  || func.Returns.Kind == CType.TypeKind.UnknownPointer)
                RetPointer.SelectedIndex = 1;
            else
                RetPointer.SelectedIndex = 0;

            for (int i = 0; i < RetType.Items.Count; i++)
            {
                ComboBoxItem item = RetType.Items[i] as ComboBoxItem;
                if (((String) item.Content) == func.Returns.Name)
                    RetType.SelectedIndex = i;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            func.ListViewEntry.FuncName = func.Name = FuncName.Text;
            if (ArgUnknown.IsChecked == true)
            {
                func.ArgCount = -1;
                func.Arguments = null;
            }
            else
            {
                func.ArgCount = ArgListView.Items.Count;
                func.Arguments = new List<Variable>();

                foreach (ListViewArgument a in _ArgCollection)
                {
                    func.Arguments.Add(new Variable(a.Name, a.TypeClass));
                }
            }

            ComboBoxItem typeItem = RetType.SelectedItem as ComboBoxItem;
            String type = typeItem.Content as String;
            CType.TypeKind kind = CType.TypeKind.ValueType;
            if (RetPointer.SelectedIndex == 1)
                kind = CType.TypeKind.Pointer;

            func.Returns = new CType(kind, type);

            Close();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            if (NewArgName.Text.Length < 1)
            {
                MessageBox.Show("Choose a name for the parameter!");
                return;
            }

            ComboBoxItem typeItem = NewArgType.SelectedItem as ComboBoxItem;
            String type = typeItem.Content as String;

            if (type == "void" && NewArgPointer.SelectedIndex != 1)
            {
                MessageBox.Show("A void argument must be a pointer!");
                return;
            }

            CType.TypeKind kind = CType.TypeKind.ValueType;
            if (type == "_unknown_")
            {
                if (NewArgPointer.SelectedIndex == 1)
                    kind = CType.TypeKind.UnknownPointer;
                else
                    kind = CType.TypeKind.Unknown;
            }
            else if (NewArgPointer.SelectedIndex == 1)
                kind = CType.TypeKind.Pointer;

            String name = NewArgName.Text.Trim();
            if (!Regex.IsMatch(name, @"^[a-zA-Z_][a-zA-Z0-9_]*$"))
            {
                MessageBox.Show("A parameter must be made of only letters, digits and underscores!");
                return;
            }

            foreach (ListViewArgument a in _ArgCollection)
            {
                if (a.Name == name)
                {
                    MessageBox.Show("Parameter name collision!");
                    return;
                }
            }

            ListViewArgument arg = new ListViewArgument(NewArgName.Text, new CType(kind, type));
            _ArgCollection.Add(arg);
        }
    }
}

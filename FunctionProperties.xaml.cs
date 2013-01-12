using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public FunctionProperties(Function f)
        {
            func = f;
            InitializeComponent();

            Name.Text = f.Name;
            Address.Text = "0x" + f.Address.ToString("X8");
        }
    }
}

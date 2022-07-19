﻿using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

using RevitOpeningPlacement.ViewModels.OpeningConfig.MepCategories;

namespace RevitOpeningPlacement.Views {
    /// <summary>
    /// Interaction logic for CategoryView.xaml
    /// </summary>
    public partial class CategoryView : UserControl {
        public static DependencyProperty SelectedItemProperty = DependencyProperty.Register(nameof(SelectedItem), typeof(MepCategoryViewModel), typeof(CategoryView));

        internal MepCategoryViewModel SelectedItem {
            get {
                return (MepCategoryViewModel) GetValue(SelectedItemProperty);
            }
            set {
                SetValue(SelectedItemProperty, value);
            }
        }


        public CategoryView() {
            InitializeComponent();
        }
    }
}
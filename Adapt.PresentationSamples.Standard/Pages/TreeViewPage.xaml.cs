﻿
using Adapt.Presentation.Controls.TreeView;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;


namespace Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TreeViewPage : ContentPage
    {
        private bool _IsLoaded;

        protected override void OnAppearing()
        {
            if (_IsLoaded)
            {
                return;
            }

            _IsLoaded = true;



            var node = new TreeViewNode();
            node.Content = new Label { Text = "Content", VerticalOptions = LayoutOptions.Center };
            node.BackgroundColor = Color.Purple;

            var node2 = new TreeViewNode();
            node2.Content = new Label { Text = "Content 2", VerticalOptions = LayoutOptions.Center };
            node2.BackgroundColor = Color.Green;

            var node3 = new TreeViewNode();
            node3.Content = new Label { Text = "Content 3", VerticalOptions = LayoutOptions.Center };
            node3.BackgroundColor = Color.Red;

            var node4 = new TreeViewNode();
            node4.Content = new Label { Text = "Content 4", VerticalOptions = LayoutOptions.Center };
            node4.BackgroundColor = Color.Pink;

            node.Children.Add(node2);
            node2.Children.Add(node3);

            TheTreeView.Children.Add(node);
            TheTreeView.Children.Add(node4);

            base.OnAppearing();
        }

        public TreeViewPage()
        {
            InitializeComponent();
        }
    }
}
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System;
using System.Threading;
using ToDoList.DataModel;
using ToDoList.ViewModels;

namespace ToDoList.Views;

public partial class ToDoListView : UserControl
{
    public ToDoListView()
    {
        InitializeComponent();

        DateTimeOffset currentDate = new(DateTime.Now);

        DueDatePicker.MinYear = currentDate;

    }



}
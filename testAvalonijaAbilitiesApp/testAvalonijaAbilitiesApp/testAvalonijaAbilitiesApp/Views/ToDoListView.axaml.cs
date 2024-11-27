using Avalonia.Controls;
using System;

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
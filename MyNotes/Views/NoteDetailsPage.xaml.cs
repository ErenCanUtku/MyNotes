﻿using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using MyNotes.ViewModels;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using MyNotes.Helpers;

namespace MyNotes.Views;

public sealed partial class NoteDetailsPage : Page
{
    private readonly StorageFolder notesFolder = ApplicationData.Current.LocalFolder;
    private DispatcherTimer? dispatcherTimer;

    public NoteDetailsViewModel ViewModel
    {
        get;
    }
    public NoteDetailsPage()
    {
        ViewModel = App.GetService<NoteDetailsViewModel>();
        InitializeComponent();
        LoadDocument();
        noteName.Title = "NoteNameFlyoutTitle".GetLocalized();
    }
    private async void LoadDocument()
    {
        // Open a text file.
        if (ShellPage.NoteName != null)
        {
            var directory = notesFolder.Path.ToString() + @"\Notes\" + ShellPage.NoteName + ".rtf";
            StorageFile file = await StorageFile.GetFileFromPathAsync(directory);

            if (file != null)
            {
                using IRandomAccessStream randAccStream = await file.OpenAsync(FileAccessMode.Read);
                NoteEditor.Document.LoadFromStream(TextSetOptions.FormatRtf, randAccStream);
                noteName.Message = ShellPage.NoteName;
            }
        }
    }
    private async void SaveFile()
    {
        var directory = notesFolder.Path.ToString() + @"\Notes\" + ShellPage.NoteName + ".rtf";
        StorageFile file = await StorageFile.GetFileFromPathAsync(directory);
        if (file != null)
        {
            try
            {
                using IRandomAccessStream randAccStream = await file.OpenAsync(FileAccessMode.ReadWrite);
                NoteEditor.Document.SaveToStream(TextGetOptions.FormatRtf, randAccStream);
                InfoBar("Success".GetLocalized(), InfoBarSeverity.Success, "Success_Message".GetLocalized());
            }
            catch (Exception ex)
            {
                InfoBar("Error".GetLocalized(), InfoBarSeverity.Error, "Error_Message".GetLocalized() + ex.Message);
            }
        }
    }
    private void DispatcherTimer_Tick(object sender, object e)
    {
        //hide the contentdialog
        infobar.IsOpen = false;
        // release the timer
        dispatcherTimer.Stop();
        dispatcherTimer = null;
    }
    private void InfoBar(string title, InfoBarSeverity type, string message)
    {
        infobar.Title = title;
        infobar.Severity = type;
        infobar.Message = message;
        infobar.IsOpen = true;
        infobar.Background.Opacity = 1;
        CreateTimer();
    }
    private void CreateTimer()
    {
        // get a timer to close the ContentDialog after 4s
        dispatcherTimer = new DispatcherTimer();
        dispatcherTimer.Tick += DispatcherTimer_Tick;
        dispatcherTimer.Interval = new TimeSpan(0, 0, 4);
        dispatcherTimer.Start();
    }
    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        SaveFile();
    }
    private void NoteEditor_ProcessKeyboardAccelerators(UIElement sender, ProcessKeyboardAcceleratorEventArgs args)
    {
        var selectedtext = NoteEditor.Document.Selection.Text;
        if (selectedtext.Length > 0)
        {
            findBox.Text = selectedtext;
            findBox.Focus(FocusState.Programmatic);
        }
    }
    private void FindBoxHighlightMatches()
    {
        FindBoxRemoveHighlights();

        Color highlightBackgroundColor = (Color)App.Current.Resources["SystemColorHighlightColor"];
        Color highlightForegroundColor = (Color)App.Current.Resources["SystemColorHighlightTextColor"];

        var textToFind = findBox.Text;
        if (textToFind != null)
        {
            ITextRange searchRange = NoteEditor.Document.GetRange(0, 0);
            while (searchRange.FindText(textToFind, TextConstants.MaxUnitCount, FindOptions.None) > 0)
            {
                searchRange.CharacterFormat.BackgroundColor = highlightBackgroundColor;
                searchRange.CharacterFormat.ForegroundColor = highlightForegroundColor;
            }
        }
    }
    private void FindBoxRemoveHighlights()
    {
        ITextRange documentRange = NoteEditor.Document.GetRange(0, TextConstants.MaxUnitCount);
        SolidColorBrush? defaultBackground = NoteEditor.Background as SolidColorBrush;
        SolidColorBrush? defaultForeground = NoteEditor.Foreground as SolidColorBrush;

        documentRange.CharacterFormat.BackgroundColor = defaultBackground.Color;
        documentRange.CharacterFormat.ForegroundColor = defaultForeground.Color;
    }
}

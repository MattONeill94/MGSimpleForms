﻿using MGSimpleForms.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Controls;
using MGSimpleForms.Tools;

namespace MGSimpleForms.MVVM
{
    public class ViewModel : INotifyPropertyChanging, INotifyPropertyChanged
    {
        Dictionary<string, object> properties = new Dictionary<string, object>();

        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;

        public void OnPropertyChanging([CallerMemberName] string propertyName = null) => PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
        public void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected void SetProperty<T>(T value, [CallerMemberName] string propertyName = null)
        {
            properties[propertyName] = value;
            OnPropertyChanged(propertyName);
        }

        protected T GetProperty<T>([CallerMemberName] string propertyName = null)
        {
            if (properties.ContainsKey(propertyName))
                return (T)properties[propertyName];
            return default;
        }
    }


    public class FormViewModel<T> : FormViewModel
    {
        public T Item { get => GetProperty<T>(); set => SetProperty(value); }
    }

    public class FormViewModel : ViewModel
    {
        //add Events...
        internal List<FrameworkElement> ToDisableElements = new List<FrameworkElement>();
        internal Dispatcher Dispatcher { get; set; }

        internal UserControl Parent { get; set; }
        public bool StopClose { get; private set; }

        /// <summary>
        /// Gets the Window this ViewModel is Attached to
        /// </summary>
        /// <returns></returns>
        public Window GetWindow()
        {
            return Parent.GetParent<Window>();
        }

        /// <summary>
        /// Gets the DataContext of the Window this ViewModel is attached to, and tried to Type it.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetParent<T>()
        {
            var parent = GetWindow();
            if (parent != null)
                return (T)parent.DataContext;
            return default(T);
        }

        /// <summary>
        /// Disables all buttons on the From
        /// </summary>
        public void DisableForm()
        {
            Dispatcher.Invoke(() =>
            {
                foreach (var element in ToDisableElements)
                {
                    element.IsEnabled = false;
                }
            });

        }

        /// <summary>
        /// Enables all buttons on the From
        /// </summary>
        public void EnableForm()
        {
            Dispatcher.Invoke(() =>
            {
                foreach (var element in ToDisableElements)
                {
                    element.IsEnabled = true;
                }
            });
        }

        internal void AddItemToDisable(FrameworkElement element)
        {
            ToDisableElements.Add(element);
        }

        public void Close(bool? Result = true)
        {
            if(StopClose) return;
            var window = GetWindow();

            window.DialogResult = Result;
            window.Close();
        }

        public virtual void OnFormLoaded()
        { 

        }
        public void DisableFormButtons()
        {
            Dispatcher.Invoke(() =>
            {
                foreach (var element in ToDisableElements)
                {
                    element.IsEnabled = false;
                }
            });

        }
        public void EnableFormButtons()
        {
            Dispatcher.Invoke(() =>
            {
                foreach (var element in ToDisableElements)
                {
                    element.IsEnabled = true;
                }
            });
        }


        protected Task RunAsync(Action action, Action<Exception> ExceptionHandler = null)
        {
            return Task.Run(() =>
            {
                StopClose = true;
                DisableFormButtons();
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    if (ExceptionHandler != null)
                        ExceptionHandler(ex);
                    else
                        MessageBox.Show(ex.ToString());
                }
                EnableFormButtons();
                StopClose = false;
            });
        }

    }



    [Form(Border = Attributes.Border.TopBottomPadding)]
    class Example : FormViewModel
    {

        public string TestString { get => GetProperty<string>(); set => SetProperty(value); }
    }

}

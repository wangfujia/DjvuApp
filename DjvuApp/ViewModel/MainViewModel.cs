﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using DjvuApp.Dialogs;
using DjvuApp.Model.Books;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using JetBrains.Annotations;

namespace DjvuApp.ViewModel
{
    public sealed class MainViewModel : ViewModelBase
    {
        private ObservableCollection<IBook> _books;
        private bool _isProgressVisible;
        private readonly IBookProvider _bookProvider;

        public ObservableCollection<IBook> Books
        {
            get { return _books; }

            private set
            {
                if (_books == value)
                {
                    return;
                }

                _books = value;
                RaisePropertyChanged();
            }
        }

        public bool IsProgressVisible
        {
            get { return _isProgressVisible; }

            private set
            {
                if (_isProgressVisible == value)
                {
                    return;
                }

                _isProgressVisible = value;
                RaisePropertyChanged();
            }
        }

        public ICommand RenameBookCommand { get; private set; }

        public ICommand RemoveBookCommand { get; private set; }

        public ICommand AddBookCommand { get; private set; }

        [UsedImplicitly]
        public MainViewModel(IBookProvider bookProvider)
        {
            _bookProvider = bookProvider;

            RenameBookCommand = new RelayCommand<IBook>(RenameBook);
            RemoveBookCommand = new RelayCommand<IBook>(RemoveBook);
            AddBookCommand = new RelayCommand(AddBook);

            RefreshBooks();
        }

        private void AddBook()
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".djvu");
            picker.PickSingleFileAndContinue();
        }

        private async void RenameBook(IBook book)
        {
            var name = await RenameDialog.ShowAsync(book.Title);

            if (name != book.Title)
            {
                await _bookProvider.ChangeTitleAsync(book, name);
            }
        }

        private async void RemoveBook(IBook book)
        {
            var dialog = new MessageDialog("If you delete this document, you won't be able to recover it later." +
                " All the progress will also be deleted from your phone.", string.Format("Delete {0}?", book.Title));
            dialog.Commands.Add(new UICommand("delete", async command =>
            {
                Books.Remove(book);
                await _bookProvider.RemoveBookAsync(book);
            }));
            dialog.Commands.Add(new UICommand("cancel"));

            await dialog.ShowAsync();
        }

        private async void RefreshBooks()
        {
            IsProgressVisible = true;

            var books = 
                from book in await _bookProvider.GetBooksAsync()
                orderby book.CreationTime descending 
                select book;

            Books = new ObservableCollection<IBook>(books);

            IsProgressVisible = false;
        }
    }
}
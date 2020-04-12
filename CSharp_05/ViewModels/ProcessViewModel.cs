using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using CSharp_05.Model;
using CSharp_05.Tools;
using CSharp_05.Tools.Managers;

namespace CSharp_05.ViewModels
{
    internal class ProcessViewModel : BaseViewModel
    {
        #region Constructors

        public ProcessViewModel()
        {
            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;
            Load();
            StartWorkingThread();
            StationManager.StopThreads += StopWorkingThread;
            ViewSource.Source = _processMap;
            ViewSource.View.Filter = ShowOnlyBargainsFilter;

        }

        #endregion


        #region Fields

        private static ConcurrentDictionary<int, MyProcess>
            _processMap = new ConcurrentDictionary<int, MyProcess>();

        private KeyValuePair<int, MyProcess> _selectedProcess;
        private readonly CollectionViewSource _viewSource = new CollectionViewSource();

        private RelayCommand<object> _openCommand;
        private RelayCommand<object> _filterCommand;
        private RelayCommand<object> _terminateCommand;


        private Thread _workingThread;
        private Thread _workingThread2;

        private CancellationToken _token;
        private readonly CancellationTokenSource _tokenSource;

        #endregion

        #region Properties

        public CollectionViewSource ViewSource
        {
            get
            {
                KeyValuePair<int, MyProcess> t = _selectedProcess;
                _viewSource?.View?.Refresh();
                SelectedProcess = t;

                return _viewSource;
            }
        }

        public string FilterText { get; set; }

        public string[] FilterBy { get; } = { "Id", "Name" };

        public string[] SortBy { get; } = { "Id", "Name", "Memory Usage" };


        public int FilterByIndex { get; set; } = 1;

        public KeyValuePair<int, MyProcess> SelectedProcess
        {
            get => _selectedProcess;
            set
            {
                _selectedProcess = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(ProcessModules));
                OnPropertyChanged(nameof(ProcessThreads));
                OnPropertyChanged(nameof(ThreadsNumber));
            }
        }


        public RelayCommand<object> OpenCommand => _openCommand ??= new RelayCommand<object>(OpenImplementation, CanDoWithProcess);

        public RelayCommand<object> TerminateCommand =>
            _terminateCommand ??= new RelayCommand<object>(
                TerminateImplementation, CanDoWithProcess);

        public RelayCommand<object> FilterCommand
        {
            get
            {
                return _filterCommand ??= new RelayCommand<object>(
                    (o =>
                    {
                        _viewSource.View.Refresh();
                        OnPropertyChanged(nameof(ViewSource));
                    }));
            }
        }

        #endregion

        #region ProcessProps

        public ProcessModuleCollection ProcessModules => SelectedProcess.Value?.Modules;

        public ProcessThreadCollection ProcessThreads => SelectedProcess.Value?.Threads;

        public int ThreadsNumber => SelectedProcess.Value?.Threads.Count ?? 0;

        #endregion


        private async void OpenImplementation(object obj)
        {
            LoaderManager.Instance.ShowLoader();
            await Task.Run((() =>
            {
                if (String.IsNullOrWhiteSpace(SelectedProcess.Value.FilePath))
                {
                    MessageBox.Show("Access denied");
                    return;
                }

                string argument = "/select, \"" + SelectedProcess.Value.FilePath + "\"";
                Process.Start("explorer.exe", argument);
            }), _token);
            LoaderManager.Instance.HideLoader();
        }

        private async void TerminateImplementation(object obj)
        {
            LoaderManager.Instance.ShowLoader();
            await Task.Run(() =>
            {
                try
                {
                    SelectedProcess.Value.TerminateProcess();
                    OnPropertyChanged(nameof(ViewSource));
                }
                catch (Exception e)
                {
                    MessageBox.Show("Access denied");
                }
            }, _token);
            LoaderManager.Instance.HideLoader();
        }

        private bool CanDoWithProcess(object obj)
        {
            return SelectedProcess.Value != null;
        }

        private void StartWorkingThread()
        {
            _workingThread = new Thread(WorkingThreadProcess);
            _workingThread2 = new Thread(WorkingThreadProcess2);
            _workingThread.Start();
            _workingThread2.Start();
        }


        private void WorkingThreadProcess()
        {
            int i = 0;
            while (!_token.IsCancellationRequested)
            {

                Process[] processes = Process.GetProcesses();

                var old = new HashSet<int>(_processMap.Keys);

                foreach (var process in processes)
                {
                    _processMap.GetOrAdd(process.Id, new MyProcess(process));
                    old.Remove(process.Id);
                    if (_token.IsCancellationRequested)
                        return;
                }

                foreach (var o in old)
                {
                    MyProcess s;
                    _processMap.TryRemove(o, out s);
                    if (_token.IsCancellationRequested)
                        return;
                }

                OnPropertyChanged(nameof(ViewSource));


                for (int j = 0; j < 10; j++)
                {
                    Thread.Sleep(500);

                    if (_token.IsCancellationRequested)
                        break;
                }

                i++;
            }
        }

        private void WorkingThreadProcess2()
        {
            int i = 0;
            while (!_token.IsCancellationRequested)
            {
                foreach (var process in _processMap.Values)
                {
                    process.Update();
                    if (_token.IsCancellationRequested)
                        return;
                }

                OnPropertyChanged(nameof(ViewSource));

                for (int j = 0; j < 4; j++)
                {
                    Thread.Sleep(500);

                    if (_token.IsCancellationRequested)
                        break;
                }

                i++;
            }
        }


        private bool ShowOnlyBargainsFilter(object item)
        {
            KeyValuePair<int, MyProcess> process = (KeyValuePair<int, MyProcess>)item;
            if (process.Value != null && !String.IsNullOrWhiteSpace(FilterText))
            {
                MyProcess l = process.Value;

                switch (FilterByIndex)
                {
                    case 0:
                        return l.Id.ToString().Contains(FilterText);
                    case 1:
                        return l.Name.Contains(FilterText);
                    default:
                        return true;
                }
            }

            return true;
        }

        private async void Load()
        {
            LoaderManager.Instance.ShowLoader();
            await Task.Run(() =>
            {
                Process[] processes = Process.GetProcesses();

                var old = new HashSet<int>(_processMap.Keys);

                foreach (var process in processes)
                {
                    _processMap.GetOrAdd(process.Id, new MyProcess(process));
                    old.Remove(process.Id);
                    if (_token.IsCancellationRequested)
                        return;
                }

                foreach (var o in old)
                {
                    MyProcess s;
                    _processMap.TryRemove(o, out s);
                    if (_token.IsCancellationRequested)
                        return;
                }

                OnPropertyChanged(nameof(ViewSource));
            }, _token);


            LoaderManager.Instance.HideLoader();
        }

        internal void StopWorkingThread()
        {
            _tokenSource.Cancel();
            _workingThread.Join(2000);
            _workingThread = null;

            _workingThread2.Join(2000);
            _workingThread2 = null;
        }
    }

}
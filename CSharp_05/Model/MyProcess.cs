using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Principal;


namespace CSharp_05.Model
{
    class MyProcess : INotifyPropertyChanged
    {
        public MyProcess(Process process)
        {
            _process = process;
            Name = process.ProcessName;
            Id = process.Id;
            _counter = new PerformanceCounter("Process", "% Processor Time", process.ProcessName, process.MachineName);

            try
            {
                _startTime = _process.StartTime;
            }
            catch (Exception)
            {
                // ignored
            }

            try
            {
                if (_process.MainModule != null) FilePath = _process.MainModule.FileName;
            }
            catch (Exception)
            {
                // ignored
            }
        }

        #region Fields

        private Process _process;
        private PerformanceCounter _counter;
        private long _lastTime = -1;
        private long _workingSet = 0;
        private float _cpu;
        private DateTime _startTime = new DateTime();

        #endregion


        #region Properties

        public string Name { get; set; }
        public int Id { get; set; }
        public string FilePath { get; set; }

        public float Cpu => _cpu;

        public long WorkingSet => _workingSet;

        public string MemoryPercent => "Not available";

        public string MemoryMb => (_workingSet / (1024.0 * 1024.0)).ToString("0.00");

        public bool IsActive => _process.Responding;

        public string CPU => _cpu.ToString("0.00");

        public string StartTime => _startTime.ToString(" dd/MM/yyyy HH:mm:ss") == " 01.01.0001 00:00:00" ? "Access denied" : _startTime.ToString(" dd/MM/yyyy HH:mm:ss");

        public ProcessModuleCollection Modules
        {
            get
            {
                try
                {
                    ProcessModuleCollection a = _process.Modules;
                    return _process.Modules;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public ProcessThreadCollection Threads
        {
            get
            {
                try
                {
                    return _process.Threads;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        #endregion

        public void TerminateProcess()
        {
            _process.Kill();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Update()
        {
            _workingSet = _process.WorkingSet64;
            try
            {
                if (_lastTime == -1)
                {
                    _lastTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    _cpu = 0;
                    _counter.NextValue();
                }
                else if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _lastTime > 1000)
                {
                    _lastTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    _cpu = _counter.NextValue() / Environment.ProcessorCount;
                }

            }
            catch (Exception)
            {
                // ignored
            }

            OnPropertyChanged(nameof(WorkingSet));
            OnPropertyChanged(nameof(MemoryMb));
            OnPropertyChanged(nameof(MemoryPercent));
            OnPropertyChanged(nameof(CPU));

        }
    }
}
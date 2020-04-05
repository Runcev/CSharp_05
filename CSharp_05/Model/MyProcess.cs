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


        #region Fields

        private readonly Process _process;
        private readonly PerformanceCounter _cpuUsage;
        private long _lastTime = -1;
        private long _workingSet = 0;
        private float _cpu;
        private readonly DateTime _startTime = new DateTime();

        #endregion


        #region Properties

        public string Name { get; set; }

        public int Id { get; set; }

        public string FilePath { get; set; }

        public int ThrCount => _process.Threads.Count;

        public bool IsActive => _process.Responding;

        public float Cpu => _cpu;

        public long WorkingSet => _workingSet;

        public string MemoryPercent => "Not available";
        public string MemoryMb => (_workingSet / (1024.0 * 1024.0)).ToString("0.00");

        public string CPU => _cpu.ToString("0.00");

        public string StartTime
        {
            get
            {
                try
                {
                    return _process.StartTime.ToString(" dd/MM/yyyy HH:mm:ss");
                }
                catch (Exception)
                {
                    return "Access denied";
                }
            }
        }

        public string User
        {
            get
            {
                try
                {
                    OpenProcessToken(_process.Handle, 8, out var processHandle);
                    var wi = new WindowsIdentity(processHandle);
                    var user = wi.Name;
                    return user;
                }
                catch
                {
                    return "Unknown user";
                }
            }
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool OpenProcessToken(IntPtr processHandle, uint desiredAccess, out IntPtr tokenHandle);
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);
        #endregion


        public MyProcess(Process process)
        {
            _process = process;
            Name = _process.ProcessName;
            Id = _process.Id;

            _cpuUsage = new PerformanceCounter("Process", "% Processor Time", _process.ProcessName, _process.MachineName);

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
                    _cpuUsage.NextValue();
                }
                else if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _lastTime > 1000)
                {
                    _lastTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    _cpu = _cpuUsage.NextValue() / Environment.ProcessorCount;
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
using HrefParser;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks.Dataflow;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HrefParser
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class MainWindowViewModel() : BaseViewModel
    {
        public string Href
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged(nameof(Href));
            }
        }
        public int Progress
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged(nameof(Progress));
            }
        }

        public int MaxProgress
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged(nameof(MaxProgress));
            }
        }

        public bool InProgress
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged(nameof(InProgress));
            }
        } = false;

        public bool IsPaused
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged(nameof(IsPaused));
            }
        } = false;

        private readonly SemaphoreSlim _pauseSemaphore = new(1, 1);

        public static int Timeout { get; set; } = 5;

        private CancellationTokenSource _cts = new CancellationTokenSource();

        public ObservableCollection<HrefDataModel> Data { get; set; } = new ObservableCollection<HrefDataModel>();

        public ICommand StartButtonClick => field ??= new RelayCommand(ClickRun);
        public ICommand PauseButtonClick => field ??= new RelayCommand(ClickPause);

        public ICommand StopButtonClick => field ??= new RelayCommand(ClickStop);
        private async void ClickRun()
        {
            InProgress = true;
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            try
            {
                Data.Clear();
                Progress = 0;

                var result = await ParserService.Parse(new Uri(Href), token);
                MaxProgress = result.Count;

                var pipeline = DataFlowService.CreatePipeline<HrefDataModel, HrefDataModel>(
                    pause: async item =>
                    {
                        await _pauseSemaphore.WaitAsync(token);
                        _pauseSemaphore.Release();
                        return item;
                    },
                    processor: async el =>
                    {
                        try
                        {
                            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token);
                            linkedCts.CancelAfter(TimeSpan.FromSeconds(Timeout));
                            el.Status = Status.InProgress;
                            el.SiteName = await ParserService.ParseTitle(el.Href, linkedCts.Token);
                            el.Status = Status.Completed;
                            return el;
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(ex.Message);
                            el.Status = Status.Failed;
                            return el;
                        }

                    },
                    onItemProcessed: el =>
                    {
                        try
                        {
                            Application.Current.Dispatcher.Invoke(() => Progress++);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(ex.Message);
                        }

                    },
                    token: token
                );

                foreach (var res in result)
                {
                    Data.Add(res);
                    res.Status = Status.InProgress;
                    var accepted = await pipeline.SendAsync(res, token);

                    if (!accepted) break;
                }
            }
            catch (OperationCanceledException) { InProgress = false; }
            catch (Exception ex) { InProgress = false; }
            finally
            {
                InProgress = false;
            }
        }

        private void ClickPause()
        {
            if (!IsPaused)
            {
                if (_pauseSemaphore.CurrentCount > 0)
                {
                    _pauseSemaphore.Wait();
                    IsPaused = true;
                }
            }
            else
            {
                if (_pauseSemaphore.CurrentCount == 0)
                {
                    _pauseSemaphore.Release();
                }
                IsPaused = false;
            }
        }

        private void ClickStop()
        {
            try
            {
                _cts?.Cancel();
                if (_pauseSemaphore.CurrentCount == 0)
                {
                    _pauseSemaphore.Release();
                }

                IsPaused = false;
                InProgress = false;
            }
            catch (Exception ex) { }
            finally
            {
                InProgress = false;
            }
        }
    }
}
﻿using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using ReactiveUI;
using TagLib;
using System.Collections.ObjectModel;
using AudioPlayer.Models;
using AudioPlayer.ViewModels;

namespace AudioPlayer.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        private readonly AudioPlayerModel _player;
        private string _currentFile = "No file selected";
        private string _currentTimeFormatted = "00:00";
        private string _durationFormatted = "00:00";
        private double _volume = 1.0;
        private double _progress = 0.0;
        private double _duration = 1.0;
        private Bitmap _coverImage;
        private readonly string _defaultCoverPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "default_cover.png");
        private readonly string _fallbackCoverPath = Path.Combine(Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)!, "..", "..", "..", "Assets", "default_cover.png");
        private ObservableCollection<TrackViewModel> _musicFiles;
        private string _musicFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Music");
        private FileSystemWatcher _fileSystemWatcher;

        public string CurrentFile
        {
            get => _currentFile;
            set => this.RaiseAndSetIfChanged(ref _currentFile, value);
        }

        public string CurrentTimeFormatted
        {
            get => _currentTimeFormatted;
            set => this.RaiseAndSetIfChanged(ref _currentTimeFormatted, value);
        }

        public string DurationFormatted
        {
            get => _durationFormatted;
            set => this.RaiseAndSetIfChanged(ref _durationFormatted, value);
        }

        public double Volume
        {
            get => _volume;
            set
            {
                this.RaiseAndSetIfChanged(ref _volume, value);
                _player.SetVolume((float)value);
            }
        }

        public double Progress
        {
            get => _progress;
            set => this.RaiseAndSetIfChanged(ref _progress, value);
        }

        public double Duration
        {
            get => _duration;
            set => this.RaiseAndSetIfChanged(ref _duration, value);
        }

        public Bitmap CoverImage
        {
            get => _coverImage;
            set
            {
                this.RaiseAndSetIfChanged(ref _coverImage, value);
                Console.WriteLine($"CoverImage set to: {(value != null ? "valid image" : "null")}");
            }
        }

        public bool HasCoverImage => true;

        public ObservableCollection<TrackViewModel> MusicFiles
        {
            get => _musicFiles;
            set => this.RaiseAndSetIfChanged(ref _musicFiles, value);
        }

        public ReactiveCommand<Unit, Unit> OpenFileCommand { get; }
        public ReactiveCommand<Unit, Unit> PlayPauseCommand { get; }
        public ReactiveCommand<Unit, Unit> StopCommand { get; }
        public ReactiveCommand<Unit, Unit> MinimizeCommand { get; }
        public ReactiveCommand<Unit, Unit> CloseCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenFolderCommand { get; }
        public ReactiveCommand<string, Unit> PlayTrackCommand { get; }

        public MainWindowViewModel()
        {
            _player = new AudioPlayerModel();
            MusicFiles = new ObservableCollection<TrackViewModel>();
            OpenFileCommand = ReactiveCommand.CreateFromTask(OpenFile);
            PlayPauseCommand = ReactiveCommand.Create(_player.PlayPause);
            StopCommand = ReactiveCommand.Create(_player.Stop);
            MinimizeCommand = ReactiveCommand.Create(MinimizeWindow);
            CloseCommand = ReactiveCommand.Create(CloseWindow);
            OpenFolderCommand = ReactiveCommand.CreateFromTask(OpenFolder);
            PlayTrackCommand = ReactiveCommand.CreateFromTask<string>(PlayTrack);

            CoverImage = LoadDefaultCoverImage();

            Observable.Interval(TimeSpan.FromMilliseconds(100))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ =>
                {
                   
                    if (_player.IsPlaying)
                    {
                        Progress = _player.GetCurrentTime();
                        Duration = _player.GetDuration();
                        CurrentTimeFormatted = TimeSpan.FromSeconds(Progress).ToString(@"mm\:ss");
                        DurationFormatted = TimeSpan.FromSeconds(Duration).ToString(@"mm\:ss");
                     
                    }
                });

            InitializeMusicFolder();
        }

        private void InitializeMusicFolder()
        {
            try
            {
                if (!Directory.Exists(_musicFolder))
                {
                    Directory.CreateDirectory(_musicFolder);
            
                }

                LoadMusicFiles();

                _fileSystemWatcher = new FileSystemWatcher(_musicFolder)
                {
                    Filter = "*.mp3",
                    EnableRaisingEvents = true
                };
                _fileSystemWatcher.Changed += (s, e) => LoadMusicFiles();
                _fileSystemWatcher.Created += (s, e) => LoadMusicFiles();
                _fileSystemWatcher.Deleted += (s, e) => LoadMusicFiles();
                _fileSystemWatcher.Renamed += (s, e) => LoadMusicFiles();
               
            }
            catch (Exception ex)
            {
            
            }
        }

        private void LoadMusicFiles()
        {
            try
            {
                var files = Directory.GetFiles(_musicFolder, "*.mp3", SearchOption.TopDirectoryOnly);
                MusicFiles.Clear();
                for (int i = 0; i < files.Length; i++)
                {
                    MusicFiles.Add(new TrackViewModel(i + 1, files[i]));
                }
                Console.WriteLine($"Loaded {MusicFiles.Count} music files from {_musicFolder}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load music files: {ex.Message}");
            }
        }

        private Bitmap? LoadDefaultCoverImage()
        {
            try
            {
                if (System.IO.File.Exists(_defaultCoverPath))
                {
                    var bitmap = new Bitmap(_defaultCoverPath);
                    Console.WriteLine("Default cover image loaded successfully from output directory.");
                    return bitmap;
                }

                Console.WriteLine($"Default cover image not found at: {_defaultCoverPath}");
                Console.WriteLine($"Attempting to load default cover image from fallback: {_fallbackCoverPath}");
                if (System.IO.File.Exists(_fallbackCoverPath))
                {
                    var bitmap = new Bitmap(_fallbackCoverPath);
                    Console.WriteLine("Default cover image loaded successfully from fallback path.");
                    return bitmap;
                }

                Console.WriteLine($"Default cover image not found at: {_fallbackCoverPath}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load default cover image: {ex.Message}");
                return null;
            }
        }

        private async Task OpenFile()
        {
            var dialog = new OpenFileDialog
            {
                Filters = { new FileDialogFilter { Name = "MP3 Files", Extensions = { "mp3" } } }
            };
            var window = (App.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (window != null)
            {
                var result = await dialog.ShowAsync(window);
                if (result != null && result.Length > 0)
                {
                    Console.WriteLine($"Loading file: {result[0]}");
                    await LoadAndPlayTrack(result[0]);
                }
            }
        }

        private async Task OpenFolder()
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Select Music Folder"
            };
            var window = (App.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (window != null)
            {
                var result = await dialog.ShowAsync(window);
                if (!string.IsNullOrEmpty(result))
                {
                    _musicFolder = result;
                    _fileSystemWatcher.Path = _musicFolder;
                    LoadMusicFiles();
                }
            }
        }

        private async Task LoadAndPlayTrack(string filePath)
        {
            _player.LoadFile(filePath);
            CurrentFile = System.IO.Path.GetFileName(filePath);
            Duration = _player.GetDuration();
            Progress = 0;
            CurrentTimeFormatted = "00:00";
            DurationFormatted = TimeSpan.FromSeconds(Duration).ToString(@"mm\:ss");
            Console.WriteLine($"File loaded. Duration: {Duration}, DurationFormatted: {DurationFormatted}");

            try
            {
                Console.WriteLine($"Attempting to load cover for {CurrentFile}...");
                using (var file = TagLib.File.Create(filePath))
                {
                    var pictures = file.Tag.Pictures;
                    if (pictures != null && pictures.Length > 0)
                    {
                        var picture = pictures[0];
                        using (Stream stream = new MemoryStream(picture.Data.Data))
                        {
                            Console.WriteLine($"Stream type for embedded cover: {stream.GetType().Name}");
                            CoverImage = new Bitmap(stream);
                            Console.WriteLine("Embedded cover image loaded successfully.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("No embedded cover found, loading default cover...");
                        CoverImage = LoadDefaultCoverImage();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load cover image: {ex.Message}");
                Console.WriteLine("Attempting to load default cover image as fallback...");
                CoverImage = LoadDefaultCoverImage();
            }
        }

        private async Task PlayTrack(string trackPath)
        {
            Console.WriteLine($"Playing track: {trackPath}");
            await LoadAndPlayTrack(trackPath);
            _player.PlayPause();
        }

        private void MinimizeWindow()
        {
            var window = (App.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (window != null)
            {
                window.WindowState = WindowState.Minimized;
            }
        }

        private void CloseWindow()
        {
            var window = (App.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (window != null)
            {
                window.Close();
            }
        }
    }
}

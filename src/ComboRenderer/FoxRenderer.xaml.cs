using ComboInterpreter;
using Newtonsoft.Json;
using Slippi.NET.Console.Types;
using Slippi.NET.Stats.Types;
using Slippi.NET.Types;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using WindowUtils;

namespace ComboRenderer;

public partial class FoxRenderer : Window
{
    private BaseComboRenderer _comboRenderer;
    private DolphinWindowTracker? _dolphinTracker = null;
    private FoxComboInterpreter? _comboBot;

    [SupportedOSPlatform("windows10.0")]
    public FoxRenderer()
    {
        InitializeComponent();

        string[] args = Environment.GetCommandLineArgs();
        if (args.Length == 1)
        {
            _comboRenderer = new LiveComboRenderer();
            _comboRenderer.OnNewGame += (_, comboBot) =>
            {
                _comboBot?.Dispose();
                _comboBot = comboBot;

                _ = Task.Run(() =>
                {
                    _ = Task.Run(async () => await comboBot.WaitForLiveGameEndAsync());

                    ProcessInterpretedCombos();
                });

                _dolphinTracker = new DolphinWindowTracker(isPlaybackDolphin: false);
                _dolphinTracker.OnDolphinMoved += OnDolphinMoved;

                Dispatcher.BeginInvoke(() =>
                {
                    AdjustWindowToDolphin(_dolphinTracker.GetDolphinWindowInfo());
                });
            };
        }
        else
        {
            // replay
            
            if (args.Length > 2)
            {
                if (args[1] == "queue")
                {
                    string launchArgsPath = args[2];
                    var dolphinArgs = JsonConvert.DeserializeObject<DolphinLaunchArgs>(System.IO.File.ReadAllText(launchArgsPath));

                    _comboRenderer = new ReplayComboRenderer(this, dolphinArgs?.Queue ?? throw new ArgumentException());

                    // TODO record with OBS
                }
                else
                {
                    string replayPath = args[1];
                    int startFrame = args.Length > 2 ? int.Parse(args[2]) : (int)Frames.FIRST;
                    _comboRenderer = new ReplayComboRenderer(this, replayPath, startFrame);
                }
            }
            else
            {
                string replayPath = args[1];
                _comboRenderer = new ReplayComboRenderer(this, replayPath);
            }
            
            _comboRenderer.OnNewGame += (_, comboBot) =>
            {
                Dispatcher.Invoke(() =>
                {
                    ComboRow.Children.Clear();
                });

                _comboBot?.Dispose();
                _comboBot = comboBot;

                _ = Task.Run(() => ProcessInterpretedCombos());

                _dolphinTracker = new DolphinWindowTracker();
                _dolphinTracker.OnDolphinMoved += OnDolphinMoved;

                Dispatcher.BeginInvoke(() =>
                {
                    AdjustWindowToDolphin(_dolphinTracker.GetDolphinWindowInfo());
                });
            };
        }

        _comboRenderer.Begin();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);

        _comboBot?.Dispose();
        _dolphinTracker?.Dispose();
    }

    private void OnDolphinMoved(object? sender, EventArgs args)
    {
        if (_dolphinTracker is not null)
        {
            AdjustWindowToDolphin(_dolphinTracker.GetMovedDolphinWindowInfo());
        }
    }

    private void AdjustWindowToDolphin(WindowInfo? dolphinWindow)
    {
        if (dolphinWindow is not null)
        {
            this.Left = dolphinWindow.Left + 100;
            this.Top = dolphinWindow.Top + 45;
            this.Width = dolphinWindow.Width - 200;
        }
    }

    private void ProcessInterpretedCombos()
    {
        CancellationToken cancellation = _comboRenderer.CancellationToken;

        bool activeLine = false;
        bool continuation = false;
        string currentLine = string.Empty;

        bool queueFlush = false;
        Image? queueRender = null;

        Actions previousAction = Actions.None; 

        Stopwatch s = new Stopwatch();
        s.Start();
        while (!cancellation.IsCancellationRequested)
        {
            var combo = _comboBot?.ComboStream.Take(cancellation) ?? throw new Exception("No combo interpreter set up");
            s.Stop();

            Dispatcher.Invoke(() =>
            {
                if (queueFlush || (s.ElapsedMilliseconds >= 450 && activeLine))
                {
                    if (!continuation && previousAction != Actions.FirefoxStartup)
                    {
                        ComboRow.Children.Clear();

                        currentLine = string.Empty;
                        activeLine = false;
                    }

                    s.Restart();

                    queueFlush = false;
                    if (queueRender is not null)
                    {
                        ComboRow.Children.Add(queueRender);
                        queueRender = null;
                    }
                }

                StringBuilder sb = new StringBuilder();

                sb.Append(combo.DisplayName);

                string result = sb.ToString();

                StackPanel newImage = ComboImageBuilder.CreateImage(this, combo.ActionEvent, combo.Buttons);
                newImage.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                if (ComboRow.Children.Count > 0)
                {
                    if ((previousAction == Actions.DashDance && combo.ActionEvent.Action == Actions.DashDance) ||
                        (previousAction == Actions.Wavedash && combo.ActionEvent.Action == Actions.Wavedash) ||
                        (previousAction == Actions.FirefoxStartup && combo.ActionEvent.Action == Actions.Firefox))
                    {
                        ComboRow.Children.RemoveAt(ComboRow.Children.Count - 1);
                    }
                }

                double childWidth = 0;
                foreach (var child in ComboRow.Children)
                {
                    childWidth += ((FrameworkElement)child)!.DesiredSize.Width;
                }

                if (childWidth + newImage.DesiredSize.Width >= this.ActualWidth)
                {
                    ComboRow.Children.Clear();
                }

                Grid imageTextGrid = new Grid()
                {
                    Margin = combo.HasContinuation ? new Thickness(0) : new Thickness(0, 0, 10, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };

                imageTextGrid.RowDefinitions.Add(new RowDefinition()
                {
                    Height = new GridLength(1, GridUnitType.Star)
                });

                imageTextGrid.RowDefinitions.Add(new RowDefinition()
                {
                    Height = GridLength.Auto
                });

                imageTextGrid.Children.Add(newImage);
                Grid.SetRow(newImage, 0);

                var text = ComboImageBuilder.GetStrokeText(this, combo.DisplayName);
                imageTextGrid.Children.Add(text);
                Grid.SetRow(text, 1);

                ComboRow.Children.Add(imageTextGrid);

                if (combo.HasContinuation)
                {
                    continuation = true;
                }
                else
                {
                    continuation = false;
                }

                if (combo.EndsCombo || (combo.DisplayName == "Dash" && ComboRow.Children.Count == 1))
                {
                    queueFlush = true;

                    currentLine = string.Empty;
                    activeLine = false;
                    s.Restart();
                }
                else
                {
                    activeLine = true;
                    s.Restart();
                }

                previousAction = combo.ActionEvent.Action;
            });
        }
    }
}
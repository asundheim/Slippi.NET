using ComboInterpreter;
using Slippi.NET.Stats.Types;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ComboRenderer;

public static class ComboImageBuilder
{
    private static Thickness RightMargin = new Thickness(0, 0, 0, 10);

    public static StackPanel CreateImage(Window window, ActionEvent hintAction, SimpleButtons buttons)
    {
        const int BUTTON_WIDTH = 541 / 6;

        List<SimpleButtons> eachButton = new List<SimpleButtons>();
        foreach (var buttonToCheck in Enum.GetValues<SimpleButtons>())
        {
            if (buttonToCheck == SimpleButtons.NONE)
            {
                continue;
            }

            if (buttons.HasFlag(buttonToCheck))
            {
                eachButton.Add(buttonToCheck);
            }
        }

        int totalWidth = BUTTON_WIDTH * eachButton.Count;
        StackPanel imagePanel = new StackPanel() 
        { 
            Orientation = Orientation.Horizontal, 
            HorizontalAlignment = HorizontalAlignment.Center 
        };

        if (hintAction.Action == Actions.Waveland)
        {
            imagePanel.Children.Add(ImageForButton(buttons));
            imagePanel.Children.Add(PlusText(window));
            imagePanel.Children.Add(ImageForButton(SimpleButtons.RT));
        }
        else if (hintAction.Action == Actions.Wavedash)
        {
            imagePanel.Children.Add(ImageForButton(SimpleButtons.Y));
            imagePanel.Children.Add(PlusText(window));
            imagePanel.Children.Add(ImageForButton(buttons));
            imagePanel.Children.Add(PlusText(window));
            imagePanel.Children.Add(ImageForButton(SimpleButtons.RT));
        }
        else if (hintAction.Action == Actions.DashDance)
        {
            imagePanel.Children.Add(ImageForDashDance(buttons));
        }
        else if (eachButton.Count == 1)
        {
            imagePanel.Children.Add(ImageForButton(buttons));
        }
        else
        {
            eachButton = eachButton.OrderByDescending(x => x, SimpleButtonsComparer.Instance).ToList();
            int nonStickIndex = eachButton.Index().First(x => !ButtonIsControlStick(x.Item)).Index;
            SimpleButtons stick = SimpleButtons.NONE;
            for (int i = 0; i < nonStickIndex; i++)
            {
                stick |= eachButton[i];
            }

            if (stick != SimpleButtons.NONE)
            {
                imagePanel.Children.Add(ImageForButton(stick));
            }

            if (nonStickIndex < eachButton.Count)
            {
                imagePanel.Children.Add(PlusText(window));
            }

            for (int i = nonStickIndex; i < eachButton.Count; i++)
            {
                imagePanel.Children.Add(ImageForButton(eachButton[i]));
                if (i < eachButton.Count - 1)
                {
                    imagePanel.Children.Add(PlusText(window));
                }
            }
        }

        if (hintAction.HasContinuation)
        {
            imagePanel.Children.Add(ArrowText(window));
        }

        RenderOptions.SetBitmapScalingMode(imagePanel, BitmapScalingMode.HighQuality);
        return imagePanel;
    }

    private class SimpleButtonsComparer : IComparer<SimpleButtons>
    {
        public static SimpleButtonsComparer Instance = new SimpleButtonsComparer();

        public int Compare(SimpleButtons x, SimpleButtons y)
        {
            bool xIsStick = ButtonIsControlStick(x);
            bool yIsStick = ButtonIsControlStick(y);
            if (xIsStick && yIsStick)
            {
                return 0;
            }
            else if (xIsStick)
            {
                return 1; // stick has highest priority
            }
            else if (yIsStick)
            {
                return -1;
            }

            bool xIsTrigger = ButtonIsTrigger(x);
            bool yIsTrigger = ButtonIsTrigger(y);

            if (xIsTrigger && yIsTrigger)
            {
                return 0;
            }
            else if (xIsTrigger)
            {
                return 1; // 
            }
            else if (yIsTrigger)
            {
                return -1;
            }

            return 0; // both regular buttons
        }
    }

    private static bool ButtonIsControlStick(SimpleButtons button)
    {
        return button switch
        {
            SimpleButtons.STICK_RIGHT or SimpleButtons.STICK_LEFT or SimpleButtons.STICK_DOWN or SimpleButtons.STICK_UP => true,
            _ => false
        };
    }

    private static bool ButtonIsTrigger(SimpleButtons button)
    {
        return button switch
        {
            SimpleButtons.LT or SimpleButtons.RT => true,
            _ => false
        };
    }

    private static Uri ImageSourceForButton(SimpleButtons button)
    {
        return button switch
        {
            SimpleButtons.A => new Uri(@"Assets\a.png", UriKind.Relative),
            SimpleButtons.B => new Uri(@"Assets\b.png", UriKind.Relative),
            SimpleButtons.X => new Uri(@"Assets\x.png", UriKind.Relative),
            SimpleButtons.Y => new Uri(@"Assets\y.png", UriKind.Relative),
            SimpleButtons.RT => new Uri(@"Assets\R.png", UriKind.Relative),
            SimpleButtons.LT => new Uri(@"Assets\L.png", UriKind.Relative),
            SimpleButtons.Z => new Uri(@"Assets\z.png", UriKind.Relative),

            SimpleButtons.CSTICK_DOWN => new Uri(@"Assets\c-down.png", UriKind.Relative),
            SimpleButtons.CSTICK_LEFT => new Uri(@"Assets\c-left.png", UriKind.Relative),
            SimpleButtons.CSTICK_RIGHT => new Uri(@"Assets\c-right.png", UriKind.Relative),
            SimpleButtons.CSTICK_UP => new Uri(@"Assets\c-up.png", UriKind.Relative),

            SimpleButtons.STICK_DOWN | SimpleButtons.STICK_RIGHT => new Uri(@"Assets\analog-downright.png", UriKind.Relative),
            SimpleButtons.STICK_DOWN | SimpleButtons.STICK_LEFT => new Uri(@"Assets\analog-downleft.png", UriKind.Relative),

            SimpleButtons.STICK_UP | SimpleButtons.STICK_RIGHT => new Uri(@"Assets\analog-upright.png", UriKind.Relative),
            SimpleButtons.STICK_UP | SimpleButtons.STICK_LEFT => new Uri(@"Assets\analog-upleft.png", UriKind.Relative),

            SimpleButtons.STICK_DOWN => new Uri(@"Assets\analog-down.png", UriKind.Relative),
            SimpleButtons.STICK_UP => new Uri(@"Assets\analog-up.png", UriKind.Relative),
            SimpleButtons.STICK_LEFT => new Uri(@"Assets\analog-left.png", UriKind.Relative),
            SimpleButtons.STICK_RIGHT => new Uri(@"Assets\analog-right.png", UriKind.Relative),
            _ => throw new ArgumentException()
        };
    }

    private static Image ImageForButton(SimpleButtons button)
    {
        const int SCALE = 10;

        BitmapImage bmp = new BitmapImage(ImageSourceForButton(button));

        Image img = new Image()
        {
            Source = bmp,
            Width = bmp.Width / SCALE,
            Height = bmp.Height / SCALE,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        return img;
    }

    private static Image ImageForDashDance(SimpleButtons button)
    {
        const int SCALE = 10;

        BitmapImage bmp = new BitmapImage(
            button == SimpleButtons.STICK_LEFT ? new Uri(@"Assets\analog-dd-left.png", UriKind.Relative) : new Uri(@"Assets\analog-dd-right.png", UriKind.Relative)
        );

        Image img = new Image()
        {
            Source = bmp,
            Width = bmp.Width / SCALE,
            Height = bmp.Height / SCALE,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        return img;
    }

    private static Path PlusText(Window window)
    {
        var p = GetStrokeText(window, "+");
        p.Margin = new Thickness(5, 0, 5, 0);
        
        return p;
    }

    private static Path ArrowText(Window window)
    {
        var p = GetStrokeText(window, "➜", bold: false);
        p.Margin = new Thickness(5, 0, 5, 0);

        return p;
    }

    public static Path GetStrokeText(Window window, string text, int fontSize = 32, bool bold = true)
    {
        FormattedText f = new FormattedText(
            text,
            CultureInfo.CurrentUICulture,
            FlowDirection.LeftToRight,
            new Typeface("Tahoma"),
            fontSize,
            Brushes.White,
            VisualTreeHelper.GetDpi(window).PixelsPerDip
        );
        
        if (bold)
        {
            f.SetFontWeight(FontWeights.Bold);
        }

        Geometry g = f.BuildGeometry(new Point(0, 0));
        Path p = new Path();
        p.Fill = Brushes.White;
        p.Stroke = Brushes.Black;
        p.StrokeThickness = 1;
        p.Data = g;
        p.HorizontalAlignment = HorizontalAlignment.Center;
        p.VerticalAlignment = VerticalAlignment.Center;

        return p;
    }
}

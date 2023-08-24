using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Navigation.App.Services;
using SkiaSharp;
using System.Collections.ObjectModel;

namespace Navigation.App.ViewModels.Sensors;

public partial class AccelerometerViewModel : ObservableObject
{
    private static readonly SKColor s_blue = new(25, 118, 210);
    private static readonly SKColor s_red = new(229, 57, 53);
    private static readonly SKColor s_yellow = new(198, 167, 0);

    private static readonly SKColor s_gray = new(195, 195, 195);
    private static readonly SKColor s_gray1 = new(160, 160, 160);
    private static readonly SKColor s_gray2 = new(90, 90, 90);
    private static readonly SKColor s_dark3 = new(60, 60, 60);

    public ObservableCollection<ISeries> Series { get; set; }

    public AccelerometerViewModel()
    {
        var service = Services.ServiceProvider.GetService<SensorService>();

        Series = new ObservableCollection<ISeries> {
            new LineSeries<ObservableValue>
            {
                LineSmoothness = 1,
                Name = "X",
                Values = service.AccelerometerX,
                Stroke = new SolidColorPaint(s_blue, 2),
                GeometrySize = 10,
                GeometryStroke = new SolidColorPaint(s_blue, 2),
                Fill = null,
                ScalesYAt = 0
            },
            new LineSeries<ObservableValue>
            {
                Name = "Y",
                Values = service.AccelerometerY,
                Stroke = new SolidColorPaint(s_red, 2),
                GeometrySize = 10,
                GeometryStroke = new SolidColorPaint(s_red, 2),
                Fill = null,
                ScalesYAt = 0
            },
            new LineSeries<ObservableValue>
            {
                Name = "Z",
                Values = service.AccelerometerZ,
                Stroke = new SolidColorPaint(s_yellow, 2),
                GeometrySize = 10,
                GeometryStroke = new SolidColorPaint(s_yellow, 2),
                Fill = null,
                ScalesYAt = 0
            }
        };
    }

    public ICartesianAxis[] YAxes { get; set; } =
    {
        new Axis // the "units" and "tens" series will be scaled on this axis
        {
            Name = "G",
            NameTextSize = 14,
            NamePaint = new SolidColorPaint(s_blue),
            NamePadding = new LiveChartsCore.Drawing.Padding(0, 20),
            Padding =  new LiveChartsCore.Drawing.Padding(0, 0, 20, 0),
            TextSize = 12,
            LabelsPaint = new SolidColorPaint(s_blue),
            TicksPaint = new SolidColorPaint(s_blue),
            SubticksPaint = new SolidColorPaint(s_blue),
            DrawTicksPath = true
        }
    };

    public DrawMarginFrame Frame { get; set; } =
    new()
    {
        Fill = new SolidColorPaint(s_dark3),
        Stroke = new SolidColorPaint
        {
            Color = s_gray,
            StrokeThickness = 1
        }
    };
}
